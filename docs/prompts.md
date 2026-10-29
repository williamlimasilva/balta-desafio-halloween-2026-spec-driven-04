## Prompts

### 1.Constitution

/speckit-constitution Este é um projeto existente. A constituição deve registrar as convenções que o código JÁ segue, para que toda feature nova respeite a arquitetura atual. Leia o código em src/EncurtaUrl.Api antes de escrever.

Princípios:

1. Simplicidade: API única em .NET 10 com Minimal APIs, sem camadas extras, sem MediatR, sem repositórios. O endpoint usa o AppDbContext diretamente.
2. Organização: endpoints ficam em Endpoints/ como métodos de extensão sobre IEndpointRouteBuilder, agrupados com MapGroup. Contratos (records de request e response) ficam em Contracts/. Entidades em Models/. Serviços pequenos em Services/ com interface.
3. Respostas HTTP: sempre TypedResults com Results<...> na assinatura. Erros de validação retornam ValidationProblem (400). Demais erros retornam ProblemDetails com o status correto.
4. Dados: EF Core com SQLite. O banco é criado com EnsureCreated, não usamos migrations. Mudanças de schema precisam ser justificadas no plano.
5. Compatibilidade: contratos existentes não podem quebrar. Campos novos em requests devem ser opcionais.
6. Dependências: nenhum pacote NuGet novo sem justificativa explícita no plano.
7. Validação: não há projeto de testes. Toda feature deve adicionar cenários de sucesso e de erro no arquivo EncurtaUrl.Api.http, e o projeto deve compilar com dotnet build sem warnings novos.
8. Idioma: mensagens de erro para o usuário em português; código, nomes e comentários técnicos podem seguir o padrão atual.

### 2.Specify

/speckit-specify Permitir que o usuário informe um código customizado ao encurtar uma URL, para depois navegar por ele (ex.: /balta-io em vez de /aZ3k9Qx).

Contexto: hoje o POST /urls recebe apenas a URL e o código é sempre gerado automaticamente. Essa feature é uma extensão desse fluxo, não um sistema novo.

Comportamento esperado:

- O código customizado é opcional. Se não for informado, tudo continua funcionando exatamente como hoje.
- Se for informado e válido, a URL curta usa esse código, e a navegação por /{codigo} redireciona para a URL original, contando o clique como já acontece hoje.
- O código precisa ser único. Se já existir, a criação é recusada informando que o código está em uso.
- O código tem regras de formato: apenas letras, números, hífen e underscore, com tamanho mínimo e máximo.
- Alguns códigos não podem ser usados porque conflitam com rotas da própria API.
- A listagem e os detalhes continuam funcionando igual para URLs com código gerado ou customizado.

Fora do escopo: editar ou remover o código depois de criado, autenticação, expiração de links.

### 3. Clarify

/speckit-clarify

Nesta seção, esclareço dúvidas e detalhes adicionais sobre o projeto, garantindo que todos os requisitos e comportamentos esperados estejam bem definidos antes de iniciar o planejamento e a implementação.

### 4. Plan

/speckit-plan Use a stack e a arquitetura que já existem no projeto, conforme a constituição: .NET 10, Minimal APIs, EF Core com SQLite, TypedResults e ProblemDetails.

Diretrizes:

- Estender o record CreateShortUrlRequest com um campo opcional Code. Não criar endpoint novo.
- Concentrar as regras do código customizado (formato, tamanho, reservados) em uma classe pequena em Services/, sem pacote de validação externo.
- Não alterar o schema: a coluna Code já existe com tamanho 32 e índice único.
- Tratar a corrida entre duas requisições com o mesmo código capturando a violação do índice único no SaveChanges e devolvendo 409.
- Atualizar o EncurtaUrl.Api.http com os cenários novos.
- Não adicionar pacotes NuGet nem projeto de testes.

### 5. Tasks

/speckit-tasks
Caso não adicione no contexto o arquivo plan.md no contexto ele pega o ultimo gerado

### 6. Analize

/speckit-analyze
Nesta seção, reviso o plano e as decisões tomadas, verificando se estão alinhadas com os princípios definidos nas seções. Identifico possíveis melhorias, riscos e inconsistências, garantindo que o projeto siga as diretrizes estabelecidas.

### 7. Implement

/speckit-implement
Nesta seção, descrevo a implementação do projeto com base no plano definido anteriormente. Incluo detalhes sobre a estrutura de arquivos, endpoints, serviços de aplicação, persistência e testes, garantindo que todas as decisões estejam alinhadas com os princípios estabelecidos.
