# Documentação técnica do projeto **Doxluqi**: um compilador didático completo


### Arquitetura adotada

Arquitetura de **pipeline linear de fases** (também chamada de *pipes and filters*):
cada fase é uma classe independente que recebe a saída da fase anterior e produz a
entrada da próxima. **O tipo de dado trocado entre as fases é o contrato.** Não há
acoplamento entre fases além desses tipos.


### Responsabilidades de cada módulo

| Módulo (namespace)          | Responsabilidade                                      | Entrada        | Saída                              |
|-----------------------------|------------------------------------------------------|---------------|------------------------------------|
| `Lexer`                     | Análise léxica (scanner)                             | `string`      | `List<Token>`                      |
| `Parser` (`ConsoleApp1`)    | Análise sintática (constrói a AST)                   | `List<Token>` | `List<Stmt>`                       |
| `AST`                       | Definição dos nós da árvore e impressora de depuração | —             | —                                  |
| `Semantic`                  | Verificação de tipos e escopo                        | `List<Stmt>`  | Validação (sem transformação)      |
| `IR`                        | Geração de código intermediário (TAC)               | `List<Stmt>`  | `List<Tac>`                        |
| `VM`                        | Geração de bytecode e execução                       | `List<Tac>`   | `List<Instruction>` + execução     |
| `CompilerException`         | Hierarquia de erros previstos                        | —             | —                                  |
| `Program.cs`                | Driver: orquestra o pipeline e trata erros           | `args[]`      | Código de saída                    |

### Estrutura de Diretórios

A raiz do repositório git é a pasta `DoxLuquiCompilerSolution/`. **Não existe arquivo
`.sln`**; o código vive em um único projeto, `ConsoleApp1/`.

```text
DoxLuquiCompilerSolution/            (raiz do repositorio git)
└── ConsoleApp1/                     PROJETO unico (.csproj)
    ├── ConsoleApp1.csproj           configuracao do projeto (.NET 10)
    ├── Program.cs                   ponto de entrada / driver do pipeline
    ├── CompilerException.cs         hierarquia de excecoes do compilador
    ├── Lexer/                       FASE 1 - analise lexica
    │   ├── Lexer.cs                 scanner (caracteres -> tokens)
    │   ├── Token.cs                 estrutura de um token
    │   └── TokenType.cs             enum com as categorias de token
    ├── Parser/                      FASE 2 - analise sintatica
    │   └── Parser.cs                parser descendente recursivo (tokens -> AST)
    ├── AST/                         Arvore de Sintaxe Abstrata
    │   ├── Expr.cs                  nos de EXPRESSAO (records)
    │   ├── Stmt.cs                  nos de COMANDO (records)
    │   └── AstPrinter.cs            impressao indentada da AST (apenas --debug)
    ├── Semantic/                    FASE 3 - analise semantica
    │   ├── SemanticAnalyzer.cs      verificacao de tipos e escopo
    │   ├── SymbolTable.cs           tabela de simbolos (pilha de escopos)
    │   └── DataType.cs              enum dos tipos (Int, Bool, String)
    ├── IR/                          FASE 4 - codigo intermediario
    │   ├── Tac.cs                   instrucao de tres enderecos
    │   └── TacGenerator.cs          AST -> TAC
    ├── VM/                          FASES 5 e 6 - bytecode + execucao
    │   ├── OpCode.cs                conjunto de instrucoes da VM
    │   ├── Instruction.cs           uma instrucao de bytecode
    │   ├── BytecodeGenerator.cs     TAC -> bytecode (com backpatching)
    │   └── VirtualMachine.cs        interpretador de bytecode (pilha)
    ├── Properties/
    │   └── launchSettings.json      perfil de execucao da IDE ("Doxluqi")
    └── Examples/                    programas de exemplo (.dox)
        ├── exemplo1_basico.dox      declaracao + if/else + print
        ├── exemplo2_while.dox       laco while
        ├── exemplo3_completo.dox    while + aritmetica + strings + if/else
        ├── exemplo4_io.dox          entrada (read) e saida (print)
        ├── exemplo5_aninhado.dox    if dentro de while + menos unario + escopo
        └── erros/                   programas que DEVEM falhar (testes manuais)
            ├── erro_sintatico.dox       falta ';'
            ├── erro_nao_declarada.dox   variavel nao declarada
            ├── erro_tipo.dox            int + bool
            ├── erro_condicao.dox        condicao de if nao-booleana
            └── erro_escopo.dox          uso de variavel fora do bloco
```

### Como cada pasta se relaciona com as demais

- `Lexer/` é a porta de entrada: só depende de si mesma e de `CompilerException`.
- `Parser/` consome `Lexer/` (tokens) e produz `AST/`.
- `Semantic/`, `IR/` e `VM/` (geração) consomem `AST/`.
- `VM/` (execução) consome o bytecode produzido por `VM/` (geração).
- `CompilerException` é transversal: usada por `Lexer`, `Parser` e `Semantic`.
- `Examples/` é insumo de teste manual (ver seção [12](#12-testes)).

### Tecnologias Utilizadas

| Tecnologia              | Versão / Detalhe                                                                 | Finalidade                         |
|-------------------------|------------------------------------------------------------------------------------|-------------------------------------|
| **C#**                  | Recursos modernos: `record`, *pattern matching*, *top-level statements*, *nullable reference types* | Linguagem de implementação |
| **.NET**                | `net10.0` (TFM); SDK validado: **10.0.301**                                       | Runtime e ferramenta de build |
| **CLI `dotnet`**        | `dotnet build`, `dotnet run`                                                      | Compilar e executar o projeto |
| **MSBuild / `.csproj`** | `Microsoft.NET.Sdk`                                                               | Sistema de build |


### Componentes e Classes Principais

#### Lexer (`Lexer/Lexer.cs`)

**Responsabilidade.** Transformar a string de código em `List<Token>`.

**Principais métodos.** `ScanTokens()` (público); auxiliares privados `ScanToken`,
`ScanNumber`, `ScanIdentifier`, `ScanString`, `Advance`, `Peek`, `Match`,
`AddToken`, `IsAtEnd`.

**Fluxo interno.** Laço sobre os caracteres; cada `ScanToken` produz um token a
partir do caractere atual, usando lookahead onde necessário; ao final, anexa `EOF`.

**Dependências.** `Token`, `TokenType`, `LexError`.

#### Token (`Lexer/Token.cs`) e TokenType (`Lexer/TokenType.cs`)

**Responsabilidade.** `Token` é um objeto de dados (`Type`, `Lexeme`, `Line`,
`Literal`) com `ToString()` para debug. `TokenType` é o enum de categorias.

#### Parser (`Parser/Parser.cs`)

**Responsabilidade.** Construir a AST a partir dos tokens, validando a gramática.

**Principais métodos.** `Parse()` (público); um método por regra: `Statement`,
`VarDeclaration`, `Assignment`, `IfStatement`, `WhileStatement`, `PrintStatement`,
`ReadStatement`, `Block`, e a escada de expressões `Expression → Equality →
Comparison → Term → Factor → Unary → Primary`.

**Fluxo interno.** Descida recursiva guiada pelo token atual; constrói nós `Stmt`/
`Expr`; aborta com `ParseError` no primeiro problema.

**Dependências.** `Token`, `TokenType`, todos os nós de `AST`, `ParseError`.

#### Nós da AST (`AST/Expr.cs`, `AST/Stmt.cs`)

**Responsabilidade.** Representar o programa como árvore. Implementados como
**`record`** (imutáveis, concisos).

#### AstPrinter (`AST/AstPrinter.cs`)

**Responsabilidade.** Imprimir a AST indentada (somente para `--debug`; **não faz
parte da compilação**). Usa `switch` por tipo de nó. Método público estático
`Print(List<Stmt>)`.

#### SemanticAnalyzer (`Semantic/SemanticAnalyzer.cs`)

**Responsabilidade.** Type checking + verificação de escopo/declaração.

**Principais métodos.** `Analyze` (público); `CheckStmt`, `CheckVarDecl`,
`CheckAssign`, `CheckExpr` (devolve `DataType`), `CheckBinary`, `RequireBool`.

**Fluxo interno.** Percorre os comandos; para expressões, calcula e devolve o tipo,
lançando `SemanticError` quando uma regra é violada. Blocos chamam
`EnterScope`/`ExitScope`.

**Dependências.** `AST`, `SymbolTable`, `DataType`, `TokenType`, `SemanticError`.

#### SymbolTable (`Semantic/SymbolTable.cs`)

**Responsabilidade.** Guardar quais variáveis existem e seus tipos, com controle de
escopo via pilha de dicionários.

**Principais métodos.** `EnterScope`, `ExitScope`, `Declare`, `TryLookup`,
`IsDeclaredAnywhere`.

#### TacGenerator (`IR/TacGenerator.cs`) e Tac (`IR/Tac.cs`)

**Responsabilidade.** Converter a AST em `List<Tac>`.

**Principais métodos.** `Generate` (público); `GenStmt`, `GenIf`, `GenWhile`,
`GenExpr`, `NewTemp`, `NewLabel`, `Emit`, `OpSymbol`. `Tac` é o objeto de dados das
instruções intermediárias (com `ToString()` legível).

**Dependências.** `AST`, `TokenType`.

#### BytecodeGenerator (`VM/BytecodeGenerator.cs`)

**Responsabilidade.** Converter TAC em `List<Instruction>` (bytecode), resolvendo
rótulos em endereços (backpatching em 2 passos).

**Principais métodos.** `Generate` (público); `Translate`, `EmitOperand`,
`BinaryOpCode`, `Emit`.

**Dependências.** `Tac`, `Instruction`, `OpCode`.

#### Instruction (`VM/Instruction.cs`) e OpCode (`VM/OpCode.cs`)

**Responsabilidade.** `Instruction` = um `OpCode` + (no máximo) um operando
(`IntArg` ou `Name`) com `ToString()` mnemônico. `OpCode` é o conjunto de
instruções da VM.

**Conjunto de instruções (`OpCode`):**

| OpCode                    | Operando | Efeito                                               |
|---------------------------|----------|------------------------------------------------------|
| `PushConst`               | `IntArg` | Empilha a constante.                                 |
| `Load`                    | `Name`   | Empilha o valor da variável.                         |
| `Store`                   | `Name`   | Desempilha e grava o valor na variável.              |
| `Add`, `Sub`, `Mul`, `Div`| —        | Desempilha dois valores e empilha o resultado.       |
| `Eq`, `Neq`, `Lt`, `Gt`   | —        | Realiza comparação; empilha `1` (verdadeiro) ou `0` (falso). |
| `Jmp`                     | `IntArg` | Salto incondicional para o endereço informado.       |
| `JmpIfFalse`              | `IntArg` | Desempilha um valor; se for `0`, realiza o salto.    |
| `Print`                   | —        | Desempilha e imprime o número.                       |
| `PrintStr`                | `Name`   | Imprime uma string (não utiliza a pilha).            |
| `Read`                    | `Name`   | Lê um inteiro do teclado para a variável.            |
| `Halt`                    | —        | Encerra a execução.                                  |

#### VirtualMachine (`VM/VirtualMachine.cs`)

**Responsabilidade.** Executar o bytecode.

**Principais métodos.** `Run()` (público) — laço `fetch/execute`; `ReadInto(name)` —
lê uma linha, mantém apenas dígitos e um sinal de menos inicial (descarta espaços e
caracteres invisíveis como BOM); se nada sobrar, vale `0`.

**Detalhes importantes:**

- `_pc` avança por padrão; saltos sobrescrevem `_pc`.
- **Namespace único e plano:** `_vars` guarda **variáveis do usuário e temporárias
  juntas** (a VM não tem conceito de escopo — escopo é resolvido só na fase
  semântica). Isso é seguro porque a semântica proíbe redeclaração/shadowing.
- **Ordem dos operandos:** operações binárias fazem `b = Pop(); a = Pop();` e
  calculam `a op b` (o 2º operando foi empilhado por último). Relevante para
  operadores não comutativos (`-`, `/`, `<`, `>`).

---

### Funcionalidades Implementadas

> A linguagem é minimalista. **Não há** funções, vetores/matrizes, `for`, `string`
> como tipo de variável, operadores lógicos (`&&`, `||`, `!`), ponto-flutuante nem
> `else if` dedicado (obtém-se aninhando `if` no `else`).

#### Declaração de variáveis (com inicialização obrigatória)

- **Objetivo:** criar variáveis tipadas (`int` ou `bool`).
- **Implementação:** `VarDeclStmt`; exige inicializador; tipo do valor deve casar
  com o declarado; nome não pode já existir.
- **Exemplo:** `int x = 10;`  •  `bool ok = true;`

#### Atribuição

- **Objetivo:** alterar o valor de uma variável já declarada.
- **Implementação:** `AssignStmt`; variável deve existir; tipos devem casar.
- **Exemplo:** `x = x + 1;`

#### Operadores aritméticos `+ - * /`

- **Objetivo:** aritmética de inteiros (divisão é **inteira**).
- **Implementação:** `BinaryExpr` → `Add/Sub/Mul/Div`. Exigem `int` dos dois lados.
- **Exemplo:** `int r = i - (i / 2) * 2;`

#### Menos unário `-`

- **Objetivo:** negação de inteiro.
- **Implementação:** `UnaryExpr`; em TAC vira `t = 0 - x`.
- **Exemplo:** `int neg = -n;`

#### Operadores relacionais `< > == !=`

- **Objetivo:** comparações que produzem `bool`.
- **Implementação:** `<`/`>` exigem `int`; `==`/`!=` exigem dois operandos do mesmo
  tipo (`int` ou `bool`). Resultado `bool` (1/0 na VM).
- **Exemplo:** `bool grande = soma > 10;`

#### Condicional `if` / `else`

- **Objetivo:** execução condicional; `else` opcional.
- **Implementação:** `IfStmt`; condição deve ser `bool`; TAC com `if_false`/`goto`.
- **Exemplo:**
  ```c
  if (x < y) { print(x); } else { print(y); }
  ```

#### Laço `while`

- **Objetivo:** repetição enquanto a condição (booleana) for verdadeira.
- **Implementação:** `WhileStmt`; TAC com rótulo de início, `if_false` para sair e
  `goto` de volta.
- **Exemplo:**
  ```c
  while (i < 6) { print(i); i = i + 1; }
  ```

#### Saída `print`

- **Objetivo:** imprimir um número, um booleano (como 1/0) ou uma string literal.
- **Implementação:** `PrintStmt`. String literal vira `print_str`/`PrintStr`
  (carrega o texto na instrução); qualquer outra expressão é avaliada e impressa
  com `Print`.
- **Exemplo:** `print("Soma de 1 ate n:"); print(soma);`

#### Entrada `read`

- **Objetivo:** ler um inteiro do teclado para uma variável `int`.
- **Implementação:** `ReadStmt` → `Read`. Variável deve existir e ser `int`. A
  leitura é tolerante (extrai dígitos e um sinal; default 0).
- **Exemplo:** `read(a);`

#### Blocos e escopo `{ }`

- **Objetivo:** agrupar comandos e criar **escopo de bloco**.
- **Implementação:** `BlockStmt`; `EnterScope`/`ExitScope` na semântica. Variáveis
  declaradas no bloco não existem fora dele.
- **Exemplo:** ver `exemplo5_aninhado.dox` (variável `resto` local ao `while`).

#### Comentários e strings

- **Comentários de linha** `// ...` (ignorados pelo lexer).
- **Strings** entre aspas duplas, **válidas apenas como argumento de `print`**;
  podem ocupar várias linhas.

---
