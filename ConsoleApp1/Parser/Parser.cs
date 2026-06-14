using ConsoleApp1.AST;
using ConsoleApp1.Lexer;

namespace ConsoleApp1;

// ============================================================
// ANÁLISE SINTÁTICA (Parser Descendente Recursivo)
// ------------------------------------------------------------
// Recebe a LISTA DE TOKENS e constrói a AST (Árvore de Sintaxe
// Abstrata), verificando se a ORDEM dos tokens respeita a
// gramática da linguagem.
//
// "Descendente recursivo" significa: para cada regra da
// gramática existe UM MÉTODO. Regras que se referem a outras
// regras viram CHAMADAS DE MÉTODO (às vezes recursivas).
//
// GRAMÁTICA (precedência cresce de cima para baixo):
//
//   program     -> statement* EOF
//   statement   -> varDecl | assign | ifStmt | whileStmt
//                | printStmt | readStmt | block
//   varDecl     -> ("int"|"bool") IDENT "=" expression ";"
//   assign      -> IDENT "=" expression ";"
//   ifStmt      -> "if" "(" expression ")" statement ("else" statement)?
//   whileStmt   -> "while" "(" expression ")" statement
//   printStmt   -> "print" "(" expression ")" ";"
//   readStmt    -> "read" "(" IDENT ")" ";"
//   block       -> "{" statement* "}"
//
//   expression  -> equality
//   equality    -> comparison (("=="|"!=") comparison)*
//   comparison  -> term (("<"|">") term)*
//   term        -> factor (("+"|"-") factor)*
//   factor      -> unary (("*"|"/") unary)*
//   unary       -> "-" unary | primary
//   primary     -> NUMBER | STRING | "true" | "false"
//                | IDENT | "(" expression ")"
// ============================================================
public class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;  // índice do token que estamos olhando

    public Parser(List<Token> tokens) => _tokens = tokens;

    // program -> statement* EOF
    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
            statements.Add(Statement());
        return statements;
    }

    // ---------------- COMANDOS (statements) ----------------

    private Stmt Statement()
    {
        // Decidimos qual comando é olhando o primeiro token.
        if (Match(TokenType.Int, TokenType.Bool)) return VarDeclaration();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.Read)) return ReadStatement();
        if (Match(TokenType.LBrace)) return Block();
        if (Check(TokenType.Identifier)) return Assignment();

        throw Error(Peek(), "Esperava um comando (int/bool, if, while, print, read ou atribuição)");
    }

    // ("int"|"bool") IDENT "=" expression ";"
    private Stmt VarDeclaration()
    {
        Token typeToken = Previous(); // já foi consumido pelo Match: é Int ou Bool
        Token name = Consume(TokenType.Identifier, "Esperava o nome da variável após o tipo");
        Consume(TokenType.Assign, "Esperava '=' na declaração da variável");
        Expr initializer = Expression();
        Consume(TokenType.Semicolon, "Esperava ';' no fim da declaração");
        return new VarDeclStmt(typeToken.Type, name.Lexeme, initializer, name.Line);
    }

    // IDENT "=" expression ";"
    private Stmt Assignment()
    {
        Token name = Consume(TokenType.Identifier, "Esperava o nome da variável");
        Consume(TokenType.Assign, "Esperava '=' na atribuição");
        Expr value = Expression();
        Consume(TokenType.Semicolon, "Esperava ';' no fim da atribuição");
        return new AssignStmt(name.Lexeme, value, name.Line);
    }

    // "if" "(" expression ")" statement ("else" statement)?
    private Stmt IfStatement()
    {
        int line = Previous().Line; // linha do token 'if'
        Consume(TokenType.LParen, "Esperava '(' depois de 'if'");
        Expr condition = Expression();
        Consume(TokenType.RParen, "Esperava ')' depois da condição");
        Stmt thenBranch = Statement();

        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
            elseBranch = Statement();

        return new IfStmt(condition, thenBranch, elseBranch, line);
    }

    // "while" "(" expression ")" statement
    private Stmt WhileStatement()
    {
        int line = Previous().Line; // linha do token 'while'
        Consume(TokenType.LParen, "Esperava '(' depois de 'while'");
        Expr condition = Expression();
        Consume(TokenType.RParen, "Esperava ')' depois da condição");
        Stmt body = Statement();
        return new WhileStmt(condition, body, line);
    }

    // "print" "(" expression ")" ";"
    private Stmt PrintStatement()
    {
        Consume(TokenType.LParen, "Esperava '(' depois de 'print'");
        Expr value = Expression();
        Consume(TokenType.RParen, "Esperava ')' depois do valor");
        Consume(TokenType.Semicolon, "Esperava ';' depois de print(...)");
        return new PrintStmt(value);
    }

    // "read" "(" IDENT ")" ";"
    private Stmt ReadStatement()
    {
        Consume(TokenType.LParen, "Esperava '(' depois de 'read'");
        Token name = Consume(TokenType.Identifier, "Esperava o nome da variável dentro de read");
        Consume(TokenType.RParen, "Esperava ')' depois do nome");
        Consume(TokenType.Semicolon, "Esperava ';' depois de read(...)");
        return new ReadStmt(name.Lexeme, name.Line);
    }

    // "{" statement* "}"
    private Stmt Block()
    {
        var statements = new List<Stmt>();
        while (!Check(TokenType.RBrace) && !IsAtEnd())
            statements.Add(Statement());
        Consume(TokenType.RBrace, "Esperava '}' para fechar o bloco");
        return new BlockStmt(statements);
    }

    // ---------------- EXPRESSÕES ----------------
    // A "escada" de métodos abaixo implementa a PRECEDÊNCIA dos
    // operadores. O método chamado PRIMEIRO (Equality) tem a
    // MENOR precedência; o último (Primary), a maior.

    private Expr Expression() => Equality();

    // equality -> comparison (("=="|"!=") comparison)*
    private Expr Equality()
    {
        Expr expr = Comparison();
        while (Match(TokenType.EqualEqual, TokenType.BangEqual))
        {
            Token op = Previous();
            Expr right = Comparison();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    // comparison -> term (("<"|">") term)*
    private Expr Comparison()
    {
        Expr expr = Term();
        while (Match(TokenType.Less, TokenType.Greater))
        {
            Token op = Previous();
            Expr right = Term();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    // term -> factor (("+"|"-") factor)*
    private Expr Term()
    {
        Expr expr = Factor();
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            Token op = Previous();
            Expr right = Factor();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    // factor -> unary (("*"|"/") unary)*
    private Expr Factor()
    {
        Expr expr = Unary();
        while (Match(TokenType.Star, TokenType.Slash))
        {
            Token op = Previous();
            Expr right = Unary();
            expr = new BinaryExpr(expr, op, right);
        }
        return expr;
    }

    // unary -> "-" unary | primary
    private Expr Unary()
    {
        if (Match(TokenType.Minus))
        {
            Token op = Previous();
            Expr operand = Unary(); // recursivo: permite --x (raro, mas correto)
            return new UnaryExpr(op, operand);
        }
        return Primary();
    }

    // primary -> NUMBER | STRING | "true" | "false" | IDENT | "(" expression ")"
    private Expr Primary()
    {
        if (Match(TokenType.Number))
            return new NumberLiteral((int)Previous().Literal!);

        if (Match(TokenType.String))
            return new StringLiteral((string)Previous().Literal!);

        if (Match(TokenType.True)) return new BoolLiteral(true);
        if (Match(TokenType.False)) return new BoolLiteral(false);

        if (Match(TokenType.Identifier))
        {
            Token id = Previous();
            return new VariableExpr(id.Lexeme, id.Line);
        }

        if (Match(TokenType.LParen))
        {
            Expr expr = Expression();
            Consume(TokenType.RParen, "Esperava ')' para fechar a expressão");
            return expr;
        }

        throw Error(Peek(), "Esperava uma expressão (número, variável, true/false, string ou '(')");
    }

    // ---------------- FUNÇÕES AUXILIARES ----------------

    // Se o token atual for de algum dos tipos dados, consome e devolve true.
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    // O token atual é do tipo dado? (sem consumir)
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;

    // Consome o token atual e o devolve.
    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EOF;
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];

    // Espera um token específico. Se não vier, é erro de sintaxe.
    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    // Monta um ParseError com a linha e uma dica do que foi encontrado.
    private ParseError Error(Token token, string message)
    {
        string found = token.Type == TokenType.EOF ? "o fim do arquivo" : $"'{token.Lexeme}'";
        return new ParseError(token.Line, $"{message} (encontrei {found})");
    }
}