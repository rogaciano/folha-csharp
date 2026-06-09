# RH Folha

Sistema gerencial de RH e folha de pagamento.

## Stack

- Backend: .NET / ASP.NET Core Web API
- Banco: PostgreSQL via Docker
- ORM: Entity Framework Core
- Frontend: React + TypeScript + Vite

Repositorio alvo:

- https://github.com/rogaciano/folha-csharp

## Desenvolvimento local

Subir o banco:

```powershell
docker compose up -d postgres
```

O PostgreSQL local do projeto usa a porta `5434` para evitar conflito com outras instalacoes locais.

Rodar a API:

```powershell
cd src/backend/RhFolha.Api
dotnet run
```

Rodar o frontend:

```powershell
cd src/frontend
npm run dev
```

Login inicial de desenvolvimento:

```text
E-mail: admin@rhfolha.local
Senha: Admin@123
```

Antes de homologacao/producao, altere `Authentication__SeedAdmin__Password` e `Authentication__Jwt__Secret` nas variaveis de ambiente.

Healthcheck da API:

```text
GET /api/health
```

## Padroes do projeto

- Regras de UI e operacao: [`docs/regras-ui-operacionais.md`](docs/regras-ui-operacionais.md)
- Especificacao funcional: [`docs/specs-rh-folha.md`](docs/specs-rh-folha.md)
- Plano de implementacao: [`docs/plano-implementacao.md`](docs/plano-implementacao.md)
- Deploy em VPS sem Docker: [`deploy/vps/README.md`](deploy/vps/README.md)
- Pos-mortem do primeiro deploy: [`docs/pos-mortem-deploy-vps.md`](docs/pos-mortem-deploy-vps.md)

Criar migration:

```powershell
dotnet ef migrations add NomeDaMigration --project src/backend/RhFolha.Infrastructure --startup-project src/backend/RhFolha.Api --context RhFolhaDbContext
```

Aplicar migrations:

```powershell
dotnet ef database update --project src/backend/RhFolha.Infrastructure --startup-project src/backend/RhFolha.Api --context RhFolhaDbContext
```
