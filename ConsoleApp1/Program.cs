using ConsoleApp1;
using ConsoleApp1.AST;
using ConsoleApp1.IR;
using ConsoleApp1.Lexer;
using ConsoleApp1.Semantic;
using ConsoleApp1.VM;

// ============================================================
// PONTO DE ENTRADA DO COMPILADOR DOXLUQI
// ------------------------------------------------------------
// Uso:
//   doxluqi <arquivo.dox>           compila e EXECUTA o programa
//   doxluqi <arquivo.dox> --debug   mostra também tokens, AST,
//                                    TAC e bytecode no caminho
//
// Este arquivo amarra todas as fases na ORDEM do pipeline:
//   código fonte
//     -> Lexer    (tokens)
//     -> Parser   (AST)
//     -> Semantic (validação)
//     -> TAC      (código intermediário)
//     -> Bytecode (código final)
//     -> VM       (execução)
// ============================================================

if (args.Length == 0)
{
    Console.WriteLine("Uso: doxluqi <arquivo.dox> [--debug]");
    return;
}

string path = args[0];
bool debug = args.Contains("--debug");

if (!File.Exists(path))
{
    Console.WriteLine($"Arquivo não encontrado: {path}");
    return;
}

string source = File.ReadAllText(path);

try
{
    // 1) ANÁLISE LÉXICA: caracteres -> tokens
    List<Token> tokens = new Lexer(source).ScanTokens();
    if (debug)
    {
        Console.WriteLine("=== TOKENS ===");
        foreach (Token tk in tokens) Console.WriteLine("  " + tk);
        Console.WriteLine();
    }

    // 2) ANÁLISE SINTÁTICA: tokens -> AST
    List<Stmt> ast = new Parser(tokens).Parse();
    if (debug)
    {
        Console.WriteLine("=== AST ===");
        Console.Write(AstPrinter.Print(ast));
        Console.WriteLine();
    }

    // 3) ANÁLISE SEMÂNTICA: tipos, escopo e declarações
    new SemanticAnalyzer().Analyze(ast);
    if (debug) Console.WriteLine("=== SEMÂNTICA: OK ===\n");

    // 4) CÓDIGO INTERMEDIÁRIO: AST -> TAC
    List<Tac> tac = new TacGenerator().Generate(ast);
    if (debug)
    {
        Console.WriteLine("=== TAC (Código de Três Endereços) ===");
        foreach (Tac t in tac) Console.WriteLine(t);
        Console.WriteLine();
    }

    // 5) CÓDIGO FINAL: TAC -> bytecode
    List<Instruction> bytecode = new BytecodeGenerator().Generate(tac);
    if (debug)
    {
        Console.WriteLine("=== BYTECODE ===");
        for (int i = 0; i < bytecode.Count; i++)
            Console.WriteLine($"  {i,3}: {bytecode[i]}");
        Console.WriteLine();
    }

    // 6) EXECUÇÃO na máquina virtual
    if (debug) Console.WriteLine("=== SAÍDA DO PROGRAMA ===");
    new VirtualMachine(bytecode).Run();
}
catch (CompilerException ex)
{
    // Erros previstos do compilador (léxico/sintático/semântico): mensagem amigável.
    Console.Error.WriteLine($"[Erro {ex.Fase}] linha {ex.Linha}: {ex.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    // Erros em tempo de execução (ex.: divisão por zero na VM).
    Console.Error.WriteLine($"[Erro de execução] {ex.Message}");
    Environment.Exit(1);
}
