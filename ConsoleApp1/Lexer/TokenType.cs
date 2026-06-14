namespace ConsoleApp1.Lexer;

// Todos os "tipos" de token que a nossa linguagem reconhece.
// Pense em cada valor como uma "categoria" de pedaço de código.
public enum TokenType
{
    // ----- Literais (valores escritos diretamente no código) -----
    Number,      // 10, 42, 0
    String,      // "texto"  (só usado dentro de print)
    Identifier,  // nomes de variáveis: x, contador, soma

    // ----- Palavras reservadas (keywords) -----
    Int,    // int
    Bool,   // bool
    True,   // true
    False,  // false
    If,     // if
    Else,   // else
    While,  // while
    Print,  // print
    Read,   // read

    // ----- Operadores aritméticos -----
    Plus,   // +
    Minus,  // -
    Star,   // *
    Slash,  // /

    // ----- Operadores relacionais -----
    EqualEqual,  // ==
    BangEqual,   // !=
    Less,        // <
    Greater,     // >

    // ----- Atribuição -----
    Assign,  // =

    // ----- Pontuação -----
    LParen,     // (
    RParen,     // )
    LBrace,     // {
    RBrace,     // }
    Semicolon,  // ;

    // ----- Marcador de fim de arquivo -----
    EOF
}