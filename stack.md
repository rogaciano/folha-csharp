# Stack do Projeto RH Folha

Este documento registra a stack usada no desenvolvimento local e a stack validada no servidor de homologacao/producao inicial.

## Stack de Desenvolvimento

Ambiente principal:

- Sistema operacional local: Windows.
- Terminal: PowerShell.
- Backend: .NET 10 / ASP.NET Core Web API.
- Linguagem backend: C#.
- ORM: Entity Framework Core.
- Banco local: PostgreSQL via Docker Compose.
- Porta PostgreSQL local: `5434`, para evitar conflito com instalacoes locais.
- Frontend: React.
- Linguagem frontend: TypeScript.
- Build frontend: Vite.
- Controle de versao: Git.
- Repositorio remoto: `https://github.com/rogaciano/folha-csharp`.

Comandos principais de desenvolvimento:

```powershell
docker compose up -d postgres
dotnet build src\backend\RhFolha.slnx --no-restore /nr:false -v:minimal
dotnet test src\backend\RhFolha.slnx --no-restore /nr:false -v:minimal
```

Frontend:

```powershell
cd src\frontend
npm install
npm run dev
npm run build
```

Migrations:

```powershell
dotnet ef migrations add NomeDaMigration --project src/backend/RhFolha.Infrastructure --startup-project src/backend/RhFolha.Api --context RhFolhaDbContext
```

## Stack do Servidor

Ambiente validado:

- VPS Linux.
- Sistema operacional: Ubuntu 24.04 LTS x64.
- CPU: 2 vCPU.
- RAM: 4GB.
- Web server/proxy reverso: Nginx 1.24.
- HTTPS: Let's Encrypt / Certbot.
- Backend: .NET SDK/Runtime 10.
- API: ASP.NET Core via `systemd`.
- Banco: PostgreSQL 16 nativo no sistema operacional.
- Frontend: React compilado como arquivos estaticos.
- Deploy: Git pull + publish local na VPS.
- Docker em producao: nao utilizado.

Portas e exposicao:

- `80`: Nginx HTTP, usado para redirect e validacao Let's Encrypt.
- `443`: Nginx HTTPS publico.
- `5086`: API ASP.NET Core somente local em `127.0.0.1`.
- `5432`: PostgreSQL somente local, sem exposicao publica.

Dominio:

```text
https://folha.caruaru.tec.br
```

## Caminhos no Servidor

Repositorio:

```text
/opt/rh-folha
```

Backend publicado:

```text
/opt/rh-folha/publish/api
```

Uploads:

```text
/opt/rh-folha/publish/api/data/uploads
```

Frontend publicado:

```text
/var/www/rh-folha
```

Variaveis de ambiente:

```text
/etc/rh-folha/rh-folha.env
```

Servico systemd:

```text
/etc/systemd/system/rh-folha.service
```

Configuracao Nginx:

```text
/etc/nginx/sites-available/folha.caruaru.tec.br
/etc/nginx/sites-enabled/folha.caruaru.tec.br
```

Certificados:

```text
/etc/letsencrypt/live/folha.caruaru.tec.br/fullchain.pem
/etc/letsencrypt/live/folha.caruaru.tec.br/privkey.pem
```

## Fluxo de Deploy

No desenvolvimento local:

1. Alterar codigo.
2. Rodar build e testes.
3. Criar migration se houver mudanca de banco.
4. Commit e push para GitHub.

Na VPS:

```bash
cd /opt/rh-folha
git pull
dotnet restore src/backend/RhFolha.slnx
dotnet publish src/backend/RhFolha.Api/RhFolha.Api.csproj -c Release -o /opt/rh-folha/publish/api
sudo systemctl restart rh-folha
```

Frontend:

```bash
cd /opt/rh-folha/src/frontend
npm install
npm run build
sudo rm -rf /var/www/rh-folha/*
sudo cp -a /opt/rh-folha/src/frontend/dist/. /var/www/rh-folha/
sudo chown -R www-data:www-data /var/www/rh-folha
sudo systemctl reload nginx
```

Validacao:

```bash
curl https://folha.caruaru.tec.br/api/health
curl -I https://folha.caruaru.tec.br/index.html
```

## Decisoes Tecnicas

- Manter .NET 10 porque a VPS Ubuntu 24.04 disponibiliza `dotnet-sdk-10.0`.
- Nao usar Docker na VPS para economizar memoria e reduzir complexidade operacional.
- PostgreSQL roda nativo no servidor.
- API fica privada em `127.0.0.1:5086`, exposta somente pelo Nginx.
- Frontend React e servido como estatico pelo Nginx.
- Migrations podem ser aplicadas automaticamente na subida da API quando `Database__ApplyMigrations=true`.
- Segredos ficam fora do repositorio, em `/etc/rh-folha/rh-folha.env`.

## Documentos Relacionados

- `deploy/vps/README.md`
- `docs/pos-mortem-deploy-vps.md`
- `docs/plano-producao-mvp.md`
- `docs/regras-ui-operacionais.md`
