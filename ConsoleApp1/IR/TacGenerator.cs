namespace ConsoleApp1.IR;

// ============================================================
// GERAÇÃO DE CÓDIGO INTERMEDIÁRIO (AST -> TAC)
// ------------------------------------------------------------
// Percorre a AST e produz uma LISTA de instruções TAC.
//
// Duas ideias centrais:
//   - NewTemp()  cria variáveis temporárias (t0, t1, ...) para
//     guardar resultados parciais de expressões.
//   - NewLabel() cria rótulos (L0, L1, ...) para implementar
//     if e while com saltos (goto / if_false).
//
// GenExpr devolve o "endereço" (nome de temp, de variável, ou
// uma constante) onde o resultado da expressão ficou.
// ============================================================
public class TacGenerator
{
    private readonly List<Tac> _code = new();
    private int _tempCount = 0;
    private int _labelCount = 0;

    public List<Tac> Generate(List<Stmt> program)
    {
        foreach (var stmt in program) GenStmt(stmt);
        return _code;
    }

    private string NewTemp() => $"t{_tempCount++}";
    private string NewLabel() => $"L{_labelCount++}";
    private void Emit(Tac instruction) => _code.Add(instruction);

    // -------- COMANDOS --------
    private void GenStmt(Stmt stmt)
    {
        switch (stmt)
        {
            case VarDeclStmt d:
            {
                string value = GenExpr(d.Initializer);
                Emit(new Tac("=", arg1: value, result: d.Name)); // x = value
                break;
            }

            case AssignStmt a:
            {
                string value = GenExpr(a.Value);
                Emit(new Tac("=", arg1: value, result: a.Name)); // x = value
                break;
            }

            case PrintStmt p:
                if (p.Value is StringLiteral s)
                    Emit(new Tac("print_str", arg1: s.Value));   // print "texto"
                else
                {
                    string value = GenExpr(p.Value);
                    Emit(new Tac("print", arg1: value));         // print value
                }
                break;

            case ReadStmt r:
                Emit(new Tac("read", result: r.Name));           // read x
                break;

            case IfStmt iff:
                GenIf(iff);
                break;

            case WhileStmt w:
                GenWhile(w);
                break;

            case BlockStmt b:
                foreach (var child in b.Statements) GenStmt(child);
                break;
        }
    }

    // if SEM else:                  | if COM else:
    //     c = <cond>                |     c = <cond>
    //     if_false c goto Lend      |     if_false c goto Lelse
    //     <then>                    |     <then>
    // Lend:                         |     goto Lend
    //                               | Lelse:
    //                               |     <else>
    //                               | Lend:
    private void GenIf(IfStmt iff)
    {
        string cond = GenExpr(iff.Condition);

        if (iff.ElseBranch is null)
        {
            string end = NewLabel();
            Emit(new Tac("if_false", arg1: cond, result: end));
            GenStmt(iff.ThenBranch);
            Emit(new Tac("label", result: end));
        }
        else
        {
            string elseLabel = NewLabel();
            string end = NewLabel();
            Emit(new Tac("if_false", arg1: cond, result: elseLabel));
            GenStmt(iff.ThenBranch);
            Emit(new Tac("goto", result: end));
            Emit(new Tac("label", result: elseLabel));
            GenStmt(iff.ElseBranch);
            Emit(new Tac("label", result: end));
        }
    }

    // Lstart:
    //     c = <cond>
    //     if_false c goto Lend
    //     <body>
    //     goto Lstart
    // Lend:
    private void GenWhile(WhileStmt w)
    {
        string start = NewLabel();
        string end = NewLabel();

        Emit(new Tac("label", result: start));
        string cond = GenExpr(w.Condition);
        Emit(new Tac("if_false", arg1: cond, result: end));
        GenStmt(w.Body);
        Emit(new Tac("goto", result: start));
        Emit(new Tac("label", result: end));
    }

    // -------- EXPRESSÕES --------
    // Gera o código da expressão e devolve onde o resultado está.
    private string GenExpr(Expr expr)
    {
        switch (expr)
        {
            // Constantes viram operandos diretos (não precisam de temp).
            case NumberLiteral n: return n.Value.ToString();
            case BoolLiteral b:   return b.Value ? "1" : "0"; // bool é 1/0 na VM
            case VariableExpr v:  return v.Name;

            case UnaryExpr u:
            {
                string operand = GenExpr(u.Operand);
                string temp = NewTemp();
                Emit(new Tac("-", arg1: "0", arg2: operand, result: temp)); // -x  =>  t = 0 - x
                return temp;
            }

            case BinaryExpr bin:
            {
                string left = GenExpr(bin.Left);
                string right = GenExpr(bin.Right);
                string temp = NewTemp();
                Emit(new Tac(OpSymbol(bin.Op.Type), arg1: left, arg2: right, result: temp));
                return temp;
            }

            default:
                throw new Exception("Expressão inesperada na geração de TAC.");
        }
    }

    // Converte o TokenType do operador para o símbolo textual usado no TAC.
    private static string OpSymbol(TokenType type) => type switch
    {
        TokenType.Plus       => "+",
        TokenType.Minus      => "-",
        TokenType.Star       => "*",
        TokenType.Slash      => "/",
        TokenType.EqualEqual => "==",
        TokenType.BangEqual  => "!=",
        TokenType.Less       => "<",
        TokenType.Greater    => ">",
        _ => throw new Exception($"Operador inesperado: {type}")
    };
}