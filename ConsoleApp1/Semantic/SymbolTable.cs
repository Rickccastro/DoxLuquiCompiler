namespace Doxluqi;

// ============================================================
// TABELA DE SÍMBOLOS
// ------------------------------------------------------------
// Guarda QUAIS variáveis existem e QUAL o tipo de cada uma.
//
// Para controlar ESCOPO, usamos uma PILHA de dicionários:
//   - cada bloco { } "empilha" um novo escopo;
//   - ao sair do bloco, "desempilhamos" (as variáveis daquele
//     bloco somem).
//
// Procuramos uma variável do escopo mais interno para o mais
// externo (é assim que o escopo léxico funciona).
// ============================================================
public class SymbolTable
{
    private readonly List<Dictionary<string, DataType>> _scopes = new();

    public SymbolTable() => EnterScope(); // começa com o escopo global

    // Entra num novo escopo (chamado ao abrir um bloco "{").
    public void EnterScope() => _scopes.Add(new Dictionary<string, DataType>());

    // Sai do escopo atual (chamado ao fechar um bloco "}").
    public void ExitScope() => _scopes.RemoveAt(_scopes.Count - 1);

    // A variável já existe em ALGUM escopo visível agora?
    // Usamos isto para PROIBIR redeclarar um nome que já existe
    // (simplificação: não permitimos "shadowing", o que mantém a
    // nossa VM com um único espaço de nomes bem simples).
    public bool IsDeclaredAnywhere(string name)
        => _scopes.Any(scope => scope.ContainsKey(name));

    // Registra uma nova variável NO ESCOPO ATUAL (o do topo da pilha).
    public void Declare(string name, DataType type)
        => _scopes[^1][name] = type;

    // Procura o tipo de uma variável, do escopo interno ao externo.
    // Devolve false se a variável não existir em nenhum escopo.
    public bool TryLookup(string name, out DataType type)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].TryGetValue(name, out type))
                return true;
        }
        type = default;
        return false;
    }
}