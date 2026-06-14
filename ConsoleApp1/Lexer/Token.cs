namespace ConsoleApp1.Lexer;

// Um Token é a menor unidade com significado do código.
// Exemplo: o texto "int x = 10;" vira 5 tokens:
//   [Int "int"] [Identifier "x"] [Assign "="] [Number "10" =10] [Semicolon ";"]
public class Token
{
    public TokenType Type { get; }   // a categoria do token (ver TokenType)
    public string Lexeme { get; }    // o texto exato que apareceu no código fonte
    public int Line { get; }         // a linha onde apareceu (para mensagens de erro)
    public object? Literal { get; }  // valor já convertido: int para Number, string para String

    public Token(TokenType type, string lexeme, int line, object? literal = null)
    {
        Type = type;
        Lexeme = lexeme;
        Line = line;
        Literal = literal;
    }

    // Usado só para imprimir os tokens de forma legível no modo --debug.
    public override string ToString()
        => Literal is null ? $"{Type} '{Lexeme}'" : $"{Type} '{Lexeme}' = {Literal}";
}