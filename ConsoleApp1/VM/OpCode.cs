namespace ConsoleApp1.VM;

// ============================================================
// CONJUNTO DE INSTRUÇÕES DA NOSSA MÁQUINA VIRTUAL (bytecode)
// ------------------------------------------------------------
// A VM é "baseada em pilha" (stack-based): quase tudo opera
// empilhando e desempilhando valores de uma pilha de inteiros.
// Esse modelo é MUITO mais simples de gerar e executar do que
// Assembly real (que tem registradores, endereços, etc.).
// ============================================================
public enum OpCode
{
    PushConst,   // empilha um inteiro constante (usa IntArg)
    Load,        // empilha o valor de uma variável (usa Name)
    Store,       // desempilha e guarda numa variável (usa Name)

    Add, Sub, Mul, Div,   // aritmética: desempilha 2, empilha o resultado

    Eq, Neq, Lt, Gt,      // comparação: desempilha 2, empilha 1 (true) ou 0 (false)

    Jmp,         // pula para o endereço IntArg (incondicional)
    JmpIfFalse,  // desempilha 1; se for 0 (falso), pula para IntArg

    Print,       // desempilha 1 e imprime o número
    PrintStr,    // imprime a string Name (não usa a pilha)
    Read,        // lê um inteiro do teclado e guarda na variável Name

    Halt         // encerra a execução
}