# EncurtaUrl

API de encurtamento de URLs criada para o **Desafio 4 do Halloween 2026**, da [balta.io](https://balta.io). O desafio aplica Spec-Driven Development a um projeto existente usando SpecKit.

<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/04.png" width="200" alt="Badge do Desafio 4" />

## Funcionalidades

- Cria links curtos com código aleatório de 7 caracteres, usando letras e números.
- Permite escolher um código customizado opcional ao criar o link.
- Valida códigos customizados com 3 a 32 caracteres: letras ASCII, números, hífen e underscore.
- Reserva `urls` sem distinção entre maiúsculas e minúsculas para evitar conflito com as rotas da API.
- Rejeita códigos já utilizados com HTTP 409, incluindo colisões concorrentes protegidas pelo índice único do SQLite.
- Lista links, consulta detalhes e redireciona para a URL original, contando cada clique bem-sucedido.
- Disponibiliza OpenAPI e Scalar apenas no ambiente Development.

## Tecnologias

- .NET 10 e ASP.NET Core Minimal APIs.
- Entity Framework Core com SQLite.
- OpenAPI integrado ao ASP.NET Core e interface Scalar via Scalar.AspNetCore.
- SpecKit para especificação, planejamento, tarefas e análise da funcionalidade.

## Requisitos

- .NET 10 SDK.
- VS Code e a extensão REST Client de Huachao Mao para executar os cenários do arquivo `.http` (opcional; as chamadas também podem ser feitas com outra ferramenta HTTP).

## Executar localmente

Abra um terminal PowerShell na raiz deste repositório e execute:

```powershell
dotnet restore EncurtaUrl.sln
dotnet build EncurtaUrl.sln
dotnet run --project src/EncurtaUrl.Api/EncurtaUrl.Api.csproj --launch-profile http
```

O perfil `http` inicia a API em `http://localhost:5080` no ambiente Development. A aplicação cria o banco automaticamente na primeira execução, sem migrations.

### Banco SQLite

Por padrão, o banco fica em `data/encurtaurl.db`, na raiz do repositório e fora de `src/`. O caminho relativo é resolvido a partir do content root da API, e a pasta é criada automaticamente. Os arquivos auxiliares `-wal`, `-shm` e `-journal` ficam junto ao banco e são ignorados pelo Git.

Para usar outro caminho persistente, configure `ConnectionStrings__Default` com uma connection string SQLite cujo `Data Source` seja um caminho absoluto. Por exemplo, no PowerShell:

```powershell
$env:ConnectionStrings__Default = 'Data Source=C:\AppData\encurtaurl.db'
```

Em produção, configure um caminho persistente e gravável fora dos arquivos publicados da aplicação. Não versione bancos locais com dados de execução.

### Scalar e OpenAPI

Com a API em execução no perfil `http`, abra a [interface Scalar](http://localhost:5080/scalar). O documento OpenAPI está em [http://localhost:5080/openapi/v1.json](http://localhost:5080/openapi/v1.json). Ambos só são disponibilizados em Development.

### Encerrar

Pressione **Ctrl+C** no terminal em que `dotnet run` está sendo executado.

## Endpoints

| Método | Rota               | Descrição                                        | Respostas principais |
| ------ | ------------------ | ------------------------------------------------ | -------------------- |
| `POST` | `/urls`            | Cria um link curto                               | `201`, `400`, `409`  |
| `GET`  | `/urls`            | Lista os links curtos                            | `200`                |
| `GET`  | `/urls/{code}`     | Retorna os detalhes de um link                   | `200`, `404`         |
| `GET`  | `/{code}`          | Redireciona para a URL original e conta o clique | `302`, `404`         |
| `GET`  | `/openapi/v1.json` | Documento OpenAPI                                | Development          |
| `GET`  | `/scalar`          | Interface interativa Scalar                      | Development          |

### Criar link curto

Envie somente a URL para gerar um código automaticamente:

```http
POST /urls
Content-Type: application/json

{
	"url": "https://balta.io"
}
```

Para escolher um código, informe `code`:

```http
POST /urls
Content-Type: application/json

{
	"url": "https://balta.io",
	"code": "balta-io"
}
```

O campo `code` é opcional. Ausente, nulo, vazio ou composto apenas por espaços, ele mantém o fluxo de geração automática. Um código customizado não é normalizado: maiúsculas e minúsculas são preservadas, e espaços em um valor não vazio são inválidos.

As falhas de URL ou formato/reserva retornam `400 ValidationProblem`, com mensagens em português. Código duplicado retorna `409 ProblemDetails`. Uma criação bem-sucedida retorna `201 Created`, o código, a URL curta, a URL original, a data de criação, a contagem de cliques e o cabeçalho `Location`.

## Testes manuais da API

O arquivo [EncurtaUrl.Api.http](src/EncurtaUrl.Api/EncurtaUrl.Api.http) contém chamadas para criação gerada e customizada, consulta, redirecionamento, incremento de cliques, URL inválida, códigos fora do formato, código reservado, duplicidade e código inexistente.

No VS Code com a extensão REST Client instalada, abra o arquivo e selecione **Send Request** acima de cada bloco. Antes de repetir os cenários de criação customizada e duplicidade, troque `@customCode` por um valor ainda não usado no banco local.

O projeto não possui um projeto de testes automatizados. Os cenários HTTP do arquivo `.http` e o build são os gates de validação definidos para esta aplicação:

```powershell
dotnet build EncurtaUrl.sln
```

## Estrutura

```text
.
├── Directory.Build.props               # Saídas centralizadas do SDK .NET
├── EncurtaUrl.sln                       # Solution para agrupar os projetos .NET do repositório
├── artifacts/                           # bin, obj e demais saídas geradas; ignorado pelo Git
├── data/                                # Banco SQLite local e sidecars; ignorados pelo Git
├── docs/prompts.md                      # Registro do fluxo SpecKit usado no desafio
├── specs/001-custom-url-codes/          # Spec, plano, tarefas e artefatos de design
└── src/EncurtaUrl.Api/
		├── Contracts/                       # Requests e responses
		├── Data/                            # DbContext e configuração EF Core
		├── Endpoints/                       # Rotas Minimal API
		├── Models/                          # Entidades persistidas
		├── Services/                        # Geração e validação de códigos
		├── EncurtaUrl.Api.http              # Cenários de teste manual
		├── appsettings.json                 # Connection string e logging
		└── Program.cs                       # Configuração e inicialização
```

O SDK centraliza as saídas de compilação em `artifacts/bin/` e `artifacts/obj/`, separadas por projeto e configuração. O diretório `data/` é reservado aos dados de runtime e não faz parte dos artefatos de build.
