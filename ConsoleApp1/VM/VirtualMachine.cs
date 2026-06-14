namespace ConsoleApp1.VM;

// ============================================================
// MÁQUINA VIRTUAL (executa o bytecode)
// ------------------------------------------------------------
// É um interpretador simples baseado em pilha:
//   - _stack: pilha onde os cálculos acontecem;
//   - _vars : memória das variáveis (nome -> valor inteiro);
//             temporárias t0, t1... também moram aqui;
//   - _pc   : "program counter", o índice da instrução atual.
//
// O laço principal: pega a instrução em _pc, avança _pc e
// executa. Saltos simplesmente mudam o valor de _pc.
// ============================================================
public class VirtualMachine
{
    private readonly List<Instruction> _code;
    private readonly Stack<int> _stack = new();
    private readonly Dictionary<string, int> _vars = new();
    private int _pc = 0;

    public VirtualMachine(List<Instruction> code) => _code = code;

    public void Run()
    {
        while (_pc < _code.Count)
        {
            Instruction inst = _code[_pc];
            _pc++; // avança por padrão; saltos sobrescrevem _pc abaixo

            switch (inst.Op)
            {
                case OpCode.PushConst: _stack.Push(inst.IntArg); break;
                case OpCode.Load:      _stack.Push(_vars[inst.Name!]); break;
                case OpCode.Store:     _vars[inst.Name!] = _stack.Pop(); break;

                // Aritmética. Atenção à ORDEM: o 2º operando foi
                // empilhado por último, então sai primeiro no Pop.
                case OpCode.Add: { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a + b); break; }
                case OpCode.Sub: { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a - b); break; }
                case OpCode.Mul: { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a * b); break; }
                case OpCode.Div: { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a / b); break; }

                // Comparações empilham 1 (verdadeiro) ou 0 (falso).
                case OpCode.Eq:  { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a == b ? 1 : 0); break; }
                case OpCode.Neq: { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a != b ? 1 : 0); break; }
                case OpCode.Lt:  { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a <  b ? 1 : 0); break; }
                case OpCode.Gt:  { int b = _stack.Pop(), a = _stack.Pop(); _stack.Push(a >  b ? 1 : 0); break; }

                case OpCode.Jmp:        _pc = inst.IntArg; break;
                case OpCode.JmpIfFalse: if (_stack.Pop() == 0) _pc = inst.IntArg; break;

                case OpCode.Print:    Console.WriteLine(_stack.Pop()); break;
                case OpCode.PrintStr: Console.WriteLine(inst.Name); break;

                case OpCode.Read: ReadInto(inst.Name!); break;

                case OpCode.Halt: return;
            }
        }
    }

    // Lê uma linha do teclado e guarda um inteiro na variável.
    // Para ser robusto, mantemos apenas dígitos e um sinal de menos
    // inicial (isso descarta espaços e caracteres invisíveis como o
    // BOM que alguns terminais inserem). Se nada sobrar, vale 0.
    private void ReadInto(string name)
    {
        Console.Write($"{name}? ");
        string? raw = Console.ReadLine();

        string cleaned = "";
        if (raw is not null)
        {
            foreach (char c in raw)
            {
                if (char.IsDigit(c) || (c == '-' && cleaned.Length == 0))
                    cleaned += c;
            }
        }

        _vars[name] = int.TryParse(cleaned, out int value) ? value : 0;
    }
}