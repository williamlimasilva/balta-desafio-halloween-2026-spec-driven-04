# EncurtaUrl

API de encurtamento de URLs com .NET 10, Minimal APIs, EF Core e SQLite.

## Como rodar

```bash
cd src/EncurtaUrl.Api
dotnet run
```

A API sobe em `http://localhost:5080` e o banco `encurtaurl.db` é criado na primeira execução.

## Endpoints

| Método | Rota | Descrição |
| --- | --- | --- |
| POST | `/urls` | Cria uma URL curta com código gerado automaticamente |
| GET | `/urls` | Lista todas as URLs curtas |
| GET | `/urls/{code}` | Detalhes de uma URL curta |
| GET | `/{code}` | Redireciona (302) para a URL original e conta o clique |
| GET | `/openapi/v1.json` | Documento OpenAPI (apenas em Development) |

Use o arquivo `src/EncurtaUrl.Api/EncurtaUrl.Api.http` para testar.

## Estrutura

```
src/EncurtaUrl.Api
├── Contracts/ShortUrlContracts.cs   # request e response
├── Data/AppDbContext.cs             # EF Core + índice único em Code
├── Endpoints/ShortUrlEndpoints.cs   # Minimal APIs
├── Models/ShortUrl.cs               # entidade
├── Services/ShortCodeGenerator.cs   # código aleatório base62 de 7 caracteres
└── Program.cs
```
