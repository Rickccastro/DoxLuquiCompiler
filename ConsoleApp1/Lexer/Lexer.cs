namespace ConsoleApp1.Lexer;

// ============================================================
// ANÁLISE LÉXICA (Scanner)
// ------------------------------------------------------------
// Transforma o texto bruto (uma string de caracteres) numa
// LISTA DE TOKENS. É a primeira fase do compilador.
//
// Teoria: cada categoria de token é descrita por uma EXPRESSÃO
// REGULAR (ex.: número = [0-9]+). Ao varrer caractere por
// caractere e decidir o próximo passo com base no caractere
// atual, estamos executando, na prática, um AUTÔMATO FINITO
// DETERMINÍSTICO (AFD).
// ============================================================
public class Lexer
{
    private readonly string _source;        // o código fonte inteiro
    private readonly List<Token> _tokens = new();

    private int _start = 0;    // índice onde o token atual começou
    private int _current = 0;  // índice do caractere que estamos olhando agora
    private int _line = 1;     // linha atual (para mensagens de erro)

    // Mapa de palavras reservadas. Se um identificador estiver aqui,
    // ele na verdade é uma keyword.
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["int"] = TokenType.Int,
        ["bool"] = TokenType.Bool,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["while"] = TokenType.While,
        ["print"] = TokenType.Print,
        ["read"] = TokenType.Read,
    };

    public Lexer(string source) => _source = source;

    // Ponto de entrada: varre o código inteiro e devolve a lista de tokens.
    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;  // marca o início do próximo token
            ScanToken();
        }
        // Token especial que marca o fim. O parser usa isto para saber que acabou.
        _tokens.Add(new Token(TokenType.EOF, "", _line));
        return _tokens;
    }

    // -------- FUNÇÕES BÁSICAS DE NAVEGAÇÃO --------

    // Chegamos ao fim do código?
    private bool IsAtEnd() => _current >= _source.Length;

    // Consome o caractere atual e avança o ponteiro.
    private char Advance() => _source[_current++];

    // Olha o caractere atual SEM consumir (lookahead de 1).
    private char Peek() => IsAtEnd() ? '\0' : _source[_current];

    // Se o caractere atual for o esperado, consome e devolve true. Senão, false.
    // É o que nos deixa decidir entre "=" e "==".
    private bool Match(char expected)
    {
        if (IsAtEnd() || _source[_current] != expected) return false;
        _current++;
        return true;
    }

    // Cria um token usando o texto entre _start e _current.
    private void AddToken(TokenType type, object? literal = null)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, _line, literal));
    }

    // -------- O CORAÇÃO DO SCANNER --------

    // Lê UM token a partir do caractere atual.
    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            // Pontuação de 1 caractere
            case '(': AddToken(TokenType.LParen); break;
            case ')': AddToken(TokenType.RParen); break;
            case '{': AddToken(TokenType.LBrace); break;
            case '}': AddToken(TokenType.RBrace); break;
            case ';': AddToken(TokenType.Semicolon); break;

            // Operadores aritméticos
            case '+': AddToken(TokenType.Plus); break;
            case '-': AddToken(TokenType.Minus); break;
            case '*': AddToken(TokenType.Star); break;

            // '/' pode ser divisão OU início de comentário "//"
            case '/':
                if (Match('/'))
                {
                    // Comentário de linha: ignora tudo até o fim da linha.
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else AddToken(TokenType.Slash);
                break;

            // Operadores relacionais
            case '<': AddToken(TokenType.Less); break;
            case '>': AddToken(TokenType.Greater); break;

            // '=' pode ser atribuição "=" OU comparação "=="
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Assign);
                break;

            // '!' só existe acompanhado de '=', formando "!="
            case '!':
                if (Match('=')) AddToken(TokenType.BangEqual);
                else throw new LexError(_line, "O operador '!' sozinho não existe. Você quis dizer '!='?");
                break;

            // String literal
            case '"': ScanString(); break;

            // Espaços em branco: simplesmente ignoramos.
            case ' ':
            case '\r':
            case '\t':
                break;

            // Quebra de linha: ignora mas conta a linha.
            case '\n':
                _line++;
                break;

            // Qualquer outra coisa: número, identificador/keyword ou erro.
            default:
                if (char.IsDigit(c)) ScanNumber();
                else if (char.IsLetter(c) || c == '_') ScanIdentifier();
                else throw new LexError(_line, $"Caractere inesperado: '{c}'.");
                break;
        }
    }

    // Lê um número inteiro: uma sequência de dígitos.  Regex: [0-9]+
    private void ScanNumber()
    {
        while (char.IsDigit(Peek())) Advance();
        string text = _source.Substring(_start, _current - _start);
        AddToken(TokenType.Number, int.Parse(text));
    }

    // Lê um identificador ou palavra reservada.  Regex: [a-zA-Z_][a-zA-Z0-9_]*
    private void ScanIdentifier()
    {
        while (char.IsLetterOrDigit(Peek()) || Peek() == '_') Advance();
        string text = _source.Substring(_start, _current - _start);
        // Se o texto for uma keyword, usa o TokenType dela; senão, é um Identifier.
        TokenType type = Keywords.TryGetValue(text, out var keyword) ? keyword : TokenType.Identifier;
        AddToken(type);
    }

    // Lê uma string entre aspas duplas: "..."
    private void ScanString()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') _line++; // permite string de várias linhas
            Advance();
        }
        if (IsAtEnd())
            throw new LexError(_line, "String não terminada (faltou fechar com aspas).");

        Advance(); // consome a aspa de fechamento
        // Pega o conteúdo SEM as aspas das pontas.
        string value = _source.Substring(_start + 1, _current - _start - 2);
        AddToken(TokenType.String, value);
    }
}