namespace ConsoleApp1.VM;

// Uma instrução de bytecode = um OpCode + (no máximo) um operando.
// Dependendo do OpCode, usamos IntArg (número/endereço) OU Name
// (nome de variável / texto de string / rótulo ainda não resolvido).
public class Instruction
{
    public OpCode Op;
    public int IntArg;    // constante numérica OU endereço de salto
    public string? Name;  // variável, string, ou nome do rótulo (antes de virar endereço)

    public Instruction(OpCode op, int intArg = 0, string? name = null)
    {
        Op = op;
        IntArg = intArg;
        Name = name;
    }

    // Impressão legível para o modo --debug.
    public override string ToString() => Op switch
    {
        OpCode.PushConst  => $"PUSH   {IntArg}",
        OpCode.Load       => $"LOAD   {Name}",
        OpCode.Store      => $"STORE  {Name}",
        OpCode.Jmp        => $"JMP    {IntArg}",
        OpCode.JmpIfFalse => $"JMPF   {IntArg}",
        OpCode.PrintStr   => $"PRINTS \"{Name}\"",
        OpCode.Read       => $"READ   {Name}",
        _                 => Op.ToString().ToUpper()  // ADD, SUB, EQ, PRINT, HALT...
    };
}