# Analise e decisao de stack - Sistema RH e Folha

Data: 2026-06-05

## 1. Decisao

Stack escolhida para o MVP:

- Backend: .NET / ASP.NET Core Web API
- Linguagem backend: C#
- Banco de dados: PostgreSQL
- Banco local: PostgreSQL via Docker
- ORM/migrations: Entity Framework Core
- Frontend: React + TypeScript
- Build frontend: Vite
- Testes backend: xUnit
- Validacoes backend: FluentValidation
- Deploy alvo: VPS Linux/Ubuntu
- Proxy/HTTPS em producao: Nginx + Let's Encrypt

## 2. Motivos da escolha

O sistema de RH e folha exige dominio forte, muitas regras de negocio, calculos financeiros, historico, auditoria, permissoes, relatorios e futuras integracoes.

.NET foi escolhido porque oferece:

- tipagem forte;
- bom suporte a `decimal` para dinheiro;
- ASP.NET Core maduro para APIs;
- Entity Framework Core com migrations;
- bom ecossistema de testes;
- boa performance;
- deploy estavel em Linux;
- manutencao adequada para sistemas empresariais;
- facilidade para separar dominio, aplicacao, infraestrutura e API.

PostgreSQL foi escolhido porque oferece:

- consistencia relacional;
- constraints e indices robustos;
- bom suporte a transacoes;
- tipos adequados para valores financeiros e datas;
- maturidade para relatorios e dados historicos;
- facilidade de rodar localmente via Docker e em VPS.

React + TypeScript foi escolhido porque:

- o prototipo atual ja usa uma abordagem de UI em React/JavaScript;
- TypeScript melhora manutencao;
- React tem ecossistema maduro;
- Vite simplifica desenvolvimento local;
- frontend fica separado do motor oficial de calculo.

## 3. Arquitetura recomendada

Estrutura logica:

- Frontend React/TypeScript
- API ASP.NET Core
- Camada de aplicacao
- Camada de dominio
- Camada de infraestrutura
- PostgreSQL

Separacao recomendada:

- UI exibe dados e chama API.
- API recebe requisicoes, autentica e valida.
- Aplicacao orquestra casos de uso.
- Dominio concentra regras de folha.
- Infraestrutura acessa banco, migrations e servicos externos.

Regra importante:

- A interface nao deve conter calculo oficial de folha.
- O calculo deve ficar no backend, testado automaticamente.

## 4. Estrutura inicial sugerida do repositorio

Estrutura proposta:

```text
rh-folha/
  docs/
  src/
    backend/
      RhFolha.Api/
      RhFolha.Application/
      RhFolha.Domain/
      RhFolha.Infrastructure/
      RhFolha.Tests/
    frontend/
  docker/
  docker-compose.yml
  README.md
```

Projetos backend:

- `RhFolha.Api`: controllers/endpoints, autenticacao, middlewares, configuracao HTTP.
- `RhFolha.Application`: casos de uso, DTOs, validacoes, services de aplicacao.
- `RhFolha.Domain`: entidades, value objects, enums, regras de dominio, motor de calculo.
- `RhFolha.Infrastructure`: Entity Framework, repositorios, migrations, integracoes externas.
- `RhFolha.Tests`: testes unitarios e de integracao.

Frontend:

- `src/frontend`: aplicacao React + TypeScript com Vite.

## 5. Ambiente local

Ambiente confirmado em 2026-06-05:

- .NET SDK instalado: `10.0.300`
- Node.js instalado: `v22.14.0`
- npm instalado: `11.2.0`
- Docker instalado: `29.3.1`
- Docker Compose instalado: `v5.1.1`
- `psql` local nao instalado, mas nao e obrigatorio se usarmos Docker.

PostgreSQL local:

- Rodar via Docker Compose.
- Expor porta local `5432`, salvo conflito.
- Criar banco `rh_folha`.
- Criar usuario de desenvolvimento.

## 6. Docker Compose local proposto

Servicos iniciais:

- `postgres`
- futuramente `pgadmin`, opcional

Configuracao esperada:

```text
postgres:
  image: postgres
  database: rh_folha
  user: rh_folha_user
  password: senha_local_dev
  port: 5432
  volume: postgres_data
```

Observacao:

- Senhas de desenvolvimento devem ficar em `.env` local e nao devem ser versionadas se forem sensiveis.

## 7. Deploy em VPS Ubuntu

Opcoes possiveis:

### Opcao A - Deploy com Docker Compose

Servicos:

- API .NET
- PostgreSQL
- frontend estatico
- Nginx

Vantagens:

- ambiente padronizado;
- facil replicar local/producao;
- facilita backups e atualizacoes;
- bom para VPS.

### Opcao B - Deploy direto no Ubuntu

Componentes:

- .NET Runtime instalado no Ubuntu;
- PostgreSQL instalado no Ubuntu;
- Nginx como proxy reverso;
- API rodando via `systemd`;
- frontend servido como estatico pelo Nginx.

Vantagens:

- menos camadas;
- simples para uma VPS pequena.

Recomendacao:

- Para inicio, usar Docker Compose local.
- Para producao, decidir entre Docker Compose e deploy direto quando a primeira API estiver pronta.

## 8. Pacotes backend previstos

Pacotes provaveis:

- ASP.NET Core Web API
- Entity Framework Core
- Npgsql Entity Framework Core Provider
- FluentValidation
- xUnit
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swagger/OpenAPI

Pacotes futuros:

- QuestPDF ou alternativa para PDF;
- Hangfire ou Quartz.NET para jobs;
- Serilog para logs estruturados;
- libs de importacao CSV/XLSX, se necessario.

## 9. Pacotes frontend previstos

Pacotes provaveis:

- React
- TypeScript
- Vite
- React Router
- TanStack Query
- React Hook Form
- Zod
- Tailwind CSS, se aprovado
- biblioteca de componentes ou design system proprio simples

Observacao:

- A escolha visual final deve respeitar o fluxo de telas do MVP.
- O frontend deve priorizar telas operacionais densas e claras.

## 10. Riscos e mitigacoes

Risco: regras de folha ficarem espalhadas.

- Mitigacao: manter motor de calculo no dominio/backend e cobrir com testes.

Risco: snapshots incompletos.

- Mitigacao: modelar `FolhaColaborador` e `FolhaItem` antes do primeiro fechamento.

Risco: alteracoes legais quebrarem historico.

- Mitigacao: tabelas legais e rubricas versionadas por vigencia.

Risco: sistema virar apenas CRUD.

- Mitigacao: tratar folha como fluxo de competencia com status, validacao, calculo, aprovacao e fechamento.

Risco: deploy ficar complexo.

- Mitigacao: usar Docker para PostgreSQL local e documentar deploy Ubuntu desde o inicio.

## 11. Proximos passos tecnicos

1. Criar estrutura do projeto novo.
2. Criar `docker-compose.yml` com PostgreSQL.
3. Criar solution .NET.
4. Criar projetos backend.
5. Criar projeto React/TypeScript com Vite.
6. Configurar API com healthcheck e Swagger.
7. Configurar EF Core e conexao PostgreSQL.
8. Criar primeiras migrations: empresa, setor, cargo, colaborador, rubrica, competencia.
9. Criar testes iniciais do dominio.
10. Implementar primeiro fluxo: cadastros base.

