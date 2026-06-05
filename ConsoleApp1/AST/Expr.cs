using ConsoleApp1.Lexer;

namespace ConsoleApp1.AST
{
    // ============================================================
    // NÓS DE EXPRESSÃO DA AST
    // ------------------------------------------------------------
    // EXPRESSÃO = qualquer coisa que PRODUZ UM VALOR.
    // Ex.: 10,  x,  a + b,  x < y,  -i
    // ============================================================
    public abstract record Expr;

    // Número inteiro literal: 10, 42, 0
    public record NumberLiteral(int Value) : Expr;

    // Booleano literal: true, false
    public record BoolLiteral(bool Value) : Expr;

    // String literal: "texto" (só é válida dentro de print)
    public record StringLiteral(string Value) : Expr;

    // Uso de uma variável: x, contador (guardamos a linha para erros)
    public record VariableExpr(string Name, int Line) : Expr;

    // Operação unária: -x  (só temos o menos unário)
    public record UnaryExpr(Token Op, Expr Operand) : Expr;

    // Operação binária: a + b, x < y, i == 0
    // Op é o token do operador (+, -, *, /, ==, !=, <, >)
    public record BinaryExpr(Expr Left, Token Op, Expr Right) : Expr;

}
