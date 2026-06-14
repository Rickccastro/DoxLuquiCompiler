using ConsoleApp1.IR;

namespace ConsoleApp1.VM;

// ============================================================
// GERAÇÃO DE CÓDIGO FINAL (TAC -> Bytecode)
// ------------------------------------------------------------
// Traduz cada instrução TAC para uma ou mais instruções da VM.
//
// O ponto delicado são os SALTOS: o TAC usa RÓTULOS (L0, L1),
// mas a VM usa ENDEREÇOS (índices na lista de instruções).
// Resolvemos isso em DOIS PASSOS:
//   PASSO 1 - gera o bytecode e anota em que endereço cada
//             rótulo "cai"; saltos guardam o NOME do rótulo.
//   PASSO 2 - troca o nome do rótulo pelo endereço real.
//
// Exemplo de tradução de "t0 = a + b":
//     LOAD a
//     LOAD b
//     ADD
//     STORE t0
// ============================================================
public class BytecodeGenerator
{
    private readonly List<Instruction> _code = new();
    private readonly Dictionary<string, int> _labelAddresses = new();

    public List<Instruction> Generate(List<Tac> tacCode)
    {
        // PASSO 1
        foreach (var instruction in tacCode)
            Translate(instruction);
        Emit(new Instruction(OpCode.Halt));

        // PASSO 2: resolve os saltos (nome do rótulo -> endereço).
        foreach (var inst in _code)
        {
            if (inst.Op is OpCode.Jmp or OpCode.JmpIfFalse && inst.Name is not null)
            {
                inst.IntArg = _labelAddresses[inst.Name];
                inst.Name = null;
            }
        }
        return _code;
    }

    private void Emit(Instruction instruction) => _code.Add(instruction);

    // Empilha um operando do TAC:
    //   - se for um número (constante), usa PUSH;
    //   - senão, é uma variável/temporária, então usa LOAD.
    private void EmitOperand(string operand)
    {
        if (int.TryParse(operand, out int value))
            Emit(new Instruction(OpCode.PushConst, intArg: value));
        else
            Emit(new Instruction(OpCode.Load, name: operand));
    }

    private void Translate(Tac t)
    {
        switch (t.Op)
        {
            case "label":
                // O rótulo "cai" no próximo endereço a ser emitido.
                _labelAddresses[t.Result!] = _code.Count;
                break;

            case "=": // Result = Arg1
                EmitOperand(t.Arg1!);
                Emit(new Instruction(OpCode.Store, name: t.Result));
                break;

            case "goto":
                Emit(new Instruction(OpCode.Jmp, name: t.Result));
                break;

            case "if_false":
                EmitOperand(t.Arg1!);
                Emit(new Instruction(OpCode.JmpIfFalse, name: t.Result));
                break;

            case "print":
                EmitOperand(t.Arg1!);
                Emit(new Instruction(OpCode.Print));
                break;

            case "print_str":
                Emit(new Instruction(OpCode.PrintStr, name: t.Arg1));
                break;

            case "read":
                Emit(new Instruction(OpCode.Read, name: t.Result));
                break;

            default: // operação binária: Result = Arg1 OP Arg2
                EmitOperand(t.Arg1!);
                EmitOperand(t.Arg2!);
                Emit(new Instruction(BinaryOpCode(t.Op)));
                Emit(new Instruction(OpCode.Store, name: t.Result));
                break;
        }
    }

    private static OpCode BinaryOpCode(string op) => op switch
    {
        "+"  => OpCode.Add,
        "-"  => OpCode.Sub,
        "*"  => OpCode.Mul,
        "/"  => OpCode.Div,
        "==" => OpCode.Eq,
        "!=" => OpCode.Neq,
        "<"  => OpCode.Lt,
        ">"  => OpCode.Gt,
        _ => throw new Exception($"Operador TAC desconhecido: {op}")
    };
}