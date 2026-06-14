namespace ConsoleApp1.Semantic;

// ============================================================
// ANÁLISE SEMÂNTICA
// ------------------------------------------------------------
// A sintaxe já garantiu que o programa está "bem escrito".
// Agora garantimos que ele FAZ SENTIDO:
//   1. Variável usada precisa ter sido DECLARADA antes.
//   2. Não declarar a mesma variável duas vezes.
//   3. TIPOS precisam bater (type checking):
//        - aritmética (+ - * /) só com int;
//        - < e > só com int;
//        - == e != com dois int OU dois bool;
//        - condição de if/while precisa ser bool;
//        - o valor atribuído precisa ter o tipo da variável.
//
// Percorremos a AST com pattern matching (switch por tipo de nó),
// que é a alternativa mais simples ao Visitor Pattern.
// ============================================================
public class SemanticAnalyzer
{
    private readonly SymbolTable _symbols = new();

    public void Analyze(List<Stmt> program)
    {
        foreach (var stmt in program)
            CheckStmt(stmt);
    }

    // -------- Verificação de COMANDOS --------
    private void CheckStmt(Stmt stmt)
    {
        switch (stmt)
        {
            case VarDeclStmt d:
                CheckVarDecl(d);
                break;

            case AssignStmt a:
                CheckAssign(a);
                break;

            case PrintStmt p:
                // print aceita qualquer tipo (int, bool ou string),
                // mas a expressão ainda precisa ser válida.
                CheckExpr(p.Value);
                break;

            case ReadStmt r:
                if (!_symbols.TryLookup(r.Name, out var readType))
                    throw new SemanticError(r.Line, $"Variável '{r.Name}' não foi declarada.");
                if (readType != DataType.Int)
                    throw new SemanticError(r.Line, $"read só funciona com int, mas '{r.Name}' é {readType}.");
                break;

            case IfStmt iff:
                RequireBool(CheckExpr(iff.Condition), iff.Line, "if");
                CheckStmt(iff.ThenBranch);
                if (iff.ElseBranch is not null) CheckStmt(iff.ElseBranch);
                break;

            case WhileStmt w:
                RequireBool(CheckExpr(w.Condition), w.Line, "while");
                CheckStmt(w.Body);
                break;

            case BlockStmt b:
                _symbols.EnterScope();              // novo escopo
                foreach (var s in b.Statements) CheckStmt(s);
                _symbols.ExitScope();               // descarta as variáveis do bloco
                break;
        }
    }

    private void CheckVarDecl(VarDeclStmt d)
    {
        if (_symbols.IsDeclaredAnywhere(d.Name))
            throw new SemanticError(d.Line, $"Variável '{d.Name}' já foi declarada.");

        DataType declared = d.DeclaredType == TokenType.Int ? DataType.Int : DataType.Bool;
        DataType initType = CheckExpr(d.Initializer);

        if (initType != declared)
            throw new SemanticError(d.Line,
                $"Não posso inicializar '{d.Name}' ({declared}) com um valor {initType}.");

        _symbols.Declare(d.Name, declared);
    }

    private void CheckAssign(AssignStmt a)
    {
        if (!_symbols.TryLookup(a.Name, out var varType))
            throw new SemanticError(a.Line, $"Variável '{a.Name}' não foi declarada.");

        DataType valueType = CheckExpr(a.Value);
        if (valueType != varType)
            throw new SemanticError(a.Line,
                $"Não posso atribuir um valor {valueType} a '{a.Name}' (que é {varType}).");
    }

    // -------- Verificação de EXPRESSÕES --------
    // Devolve o TIPO da expressão (ou lança SemanticError).
    private DataType CheckExpr(Expr expr)
    {
        switch (expr)
        {
            case NumberLiteral: return DataType.Int;
            case BoolLiteral:   return DataType.Bool;
            case StringLiteral: return DataType.String;

            case VariableExpr v:
                if (!_symbols.TryLookup(v.Name, out var t))
                    throw new SemanticError(v.Line, $"Variável '{v.Name}' não foi declarada.");
                return t;

            case UnaryExpr u:
                if (CheckExpr(u.Operand) != DataType.Int)
                    throw new SemanticError(u.Op.Line, "O menos unário '-' só funciona com int.");
                return DataType.Int;

            case BinaryExpr b:
                return CheckBinary(b);

            default:
                throw new SemanticError(0, "Expressão desconhecida.");
        }
    }

    private DataType CheckBinary(BinaryExpr b)
    {
        DataType left = CheckExpr(b.Left);
        DataType right = CheckExpr(b.Right);
        int line = b.Op.Line;

        switch (b.Op.Type)
        {
            // Aritmética: int op int -> int
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Star:
            case TokenType.Slash:
                if (left != DataType.Int || right != DataType.Int)
                    throw new SemanticError(line,
                        $"Operação aritmética exige int dos dois lados (recebi {left} e {right}).");
                return DataType.Int;

            // Comparação de ordem: int op int -> bool
            case TokenType.Less:
            case TokenType.Greater:
                if (left != DataType.Int || right != DataType.Int)
                    throw new SemanticError(line,
                        $"'<' e '>' exigem int dos dois lados (recebi {left} e {right}).");
                return DataType.Bool;

            // Igualdade: dois tipos IGUAIS (int ou bool) -> bool
            case TokenType.EqualEqual:
            case TokenType.BangEqual:
                if (left != right || left == DataType.String)
                    throw new SemanticError(line,
                        $"'==' e '!=' exigem dois valores do mesmo tipo int/bool (recebi {left} e {right}).");
                return DataType.Bool;

            default:
                throw new SemanticError(line, "Operador binário desconhecido.");
        }
    }

    // Ajuda: a condição de if/while precisa ser bool.
    private static void RequireBool(DataType type, int line, string where)
    {
        if (type != DataType.Bool)
            throw new SemanticError(line, $"A condição do '{where}' precisa ser bool, mas é {type}.");
    }
}