namespace ConsoleApp1
{
    public abstract class CompilerException : Exception
    {
        public string Fase { get; }  // "Léxico", "Sintático" ou "Semântico"
        public int Linha { get; }

        protected CompilerException(string fase, int linha, string menssagem) : base(menssagem)
        {
            Fase = fase;
            Linha = linha;
        }
    }

    // Erro na fase do LEXER (caractere inválido, string não fechada...).
    public class LexError : CompilerException
    {
        public LexError(int linha, string menssagem) : base("Léxico", linha, menssagem) { }
    }

    // Erro na fase do PARSER (estrutura gramatical errada, falta ';'...).
    public class ParseError : CompilerException
    {
        public ParseError(int linha, string menssagem) : base("Sintático", linha, menssagem) { }
    }

    // Erro na fase SEMÂNTICA (tipo errado, variável não declarada...).
    public class SemanticError : CompilerException
    {
        public SemanticError(int linha, string menssagem) : base("Semântico", linha, menssagem) { }
    }
}
