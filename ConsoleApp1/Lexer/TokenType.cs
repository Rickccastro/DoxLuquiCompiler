namespace ConsoleApp1.Lexer
{
    public enum TokenType
    {
        // Palavras reservadas
        Int,
        Bool,
        If,
        Else,
        While,
        Print,
        Read,
        True,
        False,

        // Identificadores e literais
        Identifier, // nomes de variáveis: x, contador, soma
        Number,    // 10, 42, 0
        String,   // "texto"  (só usado dentro de print)

        // Operadores aritméticos
        Plus,     // +
        Minus,   // -
        Star,   // *
        Slash, // /     

        // Operadores relacionais
        Equal, // ==
        NotEqual, // !=
        Less,  // <
        Greater, // >

        //  Atribuição
        Assign,                              // =

        // Pontuação
        LParen, // (
        RParen, // )
        LBrace, // {
        RBrace, // }
        Semicolon,  // ;
        Bang,                    // !

        // Especial
        EOF, // fim do arquivo

    }
}
