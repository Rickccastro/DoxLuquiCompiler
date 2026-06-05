using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Lexer
{
    public class Token
    {
        public TokenType Type { get; }   // a categoria do token 
        public string Lexema { get; }    // o texto exato que apareceu no código fonte
        public int Linha { get; }         // a linha onde apareceu (para mensagens de erro)
        public object? Literal { get; }  // valor já convertido: int para Number, string para String

        public Token(TokenType type, string lexema, int linha, object? literal = null)
        {
            Type = type;
            Lexema = lexema;
            Linha = linha;
            Literal = literal;
        }


        // Usado só para imprimir os tokens de forma legível no modo --debug.
        public override string ToString()
            => Literal is null ? $"{Type} '{Lexema}'" : $"{Type} '{Lexema}' = {Literal}";
    }
}
