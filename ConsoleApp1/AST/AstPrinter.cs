namespace ConsoleApp1.AST;

using System.Text;

// ============================================================
// IMPRESSORA DA AST (só para visualização / modo --debug)
// ------------------------------------------------------------
// Percorre a árvore e imprime de forma indentada, para você
// "ver" a estrutura que o parser construiu. Não faz parte da
// compilação em si — é uma ferramenta de aprendizado/depuração.
//
// Repare na técnica usada: PATTERN MATCHING com 'switch'.
// É a alternativa mais simples ao "Visitor Pattern" clássico:
// em vez de criar uma interface IVisitor e um método Accept em
// cada nó, só perguntamos "que tipo de nó é este?" com 'case'.
// ============================================================
public static class AstPrinter
{
    public static string Print(List<Stmt> program)
    {
        var sb = new StringBuilder();
        foreach (var stmt in program) PrintStmt(stmt, 0, sb);
        return sb.ToString();
    }

    // Escreve uma linha com indentação (2 espaços por nível).
    private static void Line(StringBuilder sb, int indent, string text)
        => sb.AppendLine(new string(' ', indent * 2) + text);

    private static void PrintStmt(Stmt stmt, int indent, StringBuilder sb)
    {
        switch (stmt)
        {
            case VarDeclStmt d:
                Line(sb, indent, $"VarDecl {d.DeclaredType} {d.Name} =");
                PrintExpr(d.Initializer, indent + 1, sb);
                break;

            case AssignStmt a:
                Line(sb, indent, $"Assign {a.Name} =");
                PrintExpr(a.Value, indent + 1, sb);
                break;

            case PrintStmt p:
                Line(sb, indent, "Print");
                PrintExpr(p.Value, indent + 1, sb);
                break;

            case ReadStmt r:
                Line(sb, indent, $"Read {r.Name}");
                break;

            case IfStmt iff:
                Line(sb, indent, "If");
                Line(sb, indent + 1, "cond:");
                PrintExpr(iff.Condition, indent + 2, sb);
                Line(sb, indent + 1, "then:");
                PrintStmt(iff.ThenBranch, indent + 2, sb);
                if (iff.ElseBranch is not null)
                {
                    Line(sb, indent + 1, "else:");
                    PrintStmt(iff.ElseBranch, indent + 2, sb);
                }
                break;

            case WhileStmt w:
                Line(sb, indent, "While");
                Line(sb, indent + 1, "cond:");
                PrintExpr(w.Condition, indent + 2, sb);
                Line(sb, indent + 1, "body:");
                PrintStmt(w.Body, indent + 2, sb);
                break;

            case BlockStmt b:
                Line(sb, indent, "Block {");
                foreach (var s in b.Statements) PrintStmt(s, indent + 1, sb);
                Line(sb, indent, "}");
                break;
        }
    }

    private static void PrintExpr(Expr expr, int indent, StringBuilder sb)
    {
        switch (expr)
        {
            case NumberLiteral n: Line(sb, indent, $"Number {n.Value}"); break;
            case BoolLiteral b: Line(sb, indent, $"Bool {b.Value}"); break;
            case StringLiteral s: Line(sb, indent, $"String \"{s.Value}\""); break;
            case VariableExpr v: Line(sb, indent, $"Var {v.Name}"); break;

            case UnaryExpr u:
                Line(sb, indent, $"Unary {u.Op.Lexeme}");
                PrintExpr(u.Operand, indent + 1, sb);
                break;

            case BinaryExpr bin:
                Line(sb, indent, $"Binary {bin.Op.Lexeme}");
                PrintExpr(bin.Left, indent + 1, sb);
                PrintExpr(bin.Right, indent + 1, sb);
                break;
        }
    }
}