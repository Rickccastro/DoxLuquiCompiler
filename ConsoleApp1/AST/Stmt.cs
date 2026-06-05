using ConsoleApp1.Lexer;

namespace ConsoleApp1.AST
{
    // ============================================================
    // NÓS DE COMANDO (STATEMENT) DA AST
    // ------------------------------------------------------------
    // COMANDO = qualquer coisa que EXECUTA UMA AÇÃO (não retorna um
    // valor). Ex: declarar variável, atribuir, if, while, print.
    //
    // ============================================================
    public abstract record Stmt;

    // Declaração de variável: int x = 10;  /  bool b = true;
    // DeclaredType é TokenType.Int ou TokenType.Bool.
    public record VarDeclStmt(TokenType DeclaredType, string Name, Expr Initializer, int Line) : Stmt;

    // Atribuição a uma variável já declarada: x = x + 1;
    public record AssignStmt(string Name, Expr Value, int Line) : Stmt;

    // Saída: print(expr);  (expr pode ser número, bool ou string)
    public record PrintStmt(Expr Value) : Stmt;

    // Entrada: read(x);  (lê um inteiro do teclado para a variável)
    public record ReadStmt(string Name, int Line) : Stmt;

    // Condicional: if (cond) thenBranch [else elseBranch]
    // ElseBranch é null quando não há "else".
    public record IfStmt(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch, int Line) : Stmt;

    // Laço: while (cond) body
    public record WhileStmt(Expr Condition, Stmt Body, int Line) : Stmt;

    // Bloco: { stmt1; stmt2; ... }  — cria um novo escopo.
    public record BlockStmt(List<Stmt> Statements) : Stmt;
}
