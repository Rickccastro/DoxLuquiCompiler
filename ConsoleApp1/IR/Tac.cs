namespace ConsoleApp1.IR;

// ============================================================
// INSTRUÇÃO DE CÓDIGO DE TRÊS ENDEREÇOS (TAC)
// ------------------------------------------------------------
// TAC é uma representação intermediária onde cada instrução tem
// no MÁXIMO três "endereços" (operandos). Expressões complexas
// são quebradas em passos pequenos usando variáveis TEMPORÁRIAS
// (t0, t1, ...).
//
// Ex.:  a + b * c   vira:
//          t0 = b * c
//          t1 = a + t0
//
// Para representar TODAS as formas com uma classe só, usamos os
// campos Op / Arg1 / Arg2 / Result. O significado de cada campo
// depende de Op (ver abaixo).
// ============================================================
public class Tac
{
    public string Op;        // o que a instrução faz
    public string? Arg1;     // 1º operando
    public string? Arg2;     // 2º operando
    public string? Result;   // destino (ou nome do rótulo, no caso de saltos)

    public Tac(string op, string? arg1 = null, string? arg2 = null, string? result = null)
    {
        Op = op;
        Arg1 = arg1;
        Arg2 = arg2;
        Result = result;
    }

    // Formas que usamos (e o que cada campo significa):
    //   "+","-","*","/","==","!=","<",">"  ->  Result = Arg1 Op Arg2   (binária)
    //   "="        ->  Result = Arg1                         (cópia/atribuição)
    //   "label"    ->  Result:                               (define um rótulo)
    //   "goto"     ->  goto Result                           (salto incondicional)
    //   "if_false" ->  if_false Arg1 goto Result             (salto condicional)
    //   "print"    ->  print Arg1                            (imprime um valor)
    //   "print_str"->  print "Arg1"                          (imprime uma string)
    //   "read"     ->  read Result                           (lê para a variável)
    public override string ToString() => Op switch
    {
        "label" => $"{Result}:",
        "goto" => $"    goto {Result}",
        "if_false" => $"    if_false {Arg1} goto {Result}",
        "print" => $"    print {Arg1}",
        "print_str" => $"    print \"{Arg1}\"",
        "read" => $"    read {Result}",
        "=" => $"    {Result} = {Arg1}",
        _ => $"    {Result} = {Arg1} {Op} {Arg2}", // operação binária
    };
}