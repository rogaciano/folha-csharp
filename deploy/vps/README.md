# Deploy em VPS Linux sem Docker

Dominio alvo: `folha.caruaru.tec.br`

Este modelo foi preparado para VPS pequena, com 1GB de RAM, 2 vCPU e Nginx ja usado por outros projetos.

## Arquitetura

- Nginx publico em `80/443`.
- Frontend React publicado em `/var/www/rh-folha`.
- API ASP.NET Core rodando via systemd em `127.0.0.1:5086`.
- PostgreSQL instalado no sistema operacional.
- Uploads persistentes em `/opt/rh-folha/data/uploads`.
- Logs via `journalctl`.
- Backups em `/opt/rh-folha/backups`.

## Diretorios no servidor

```bash
sudo mkdir -p /opt/rh-folha/api
sudo mkdir -p /opt/rh-folha/data/uploads
sudo mkdir -p /opt/rh-folha/backups
sudo mkdir -p /var/www/rh-folha
sudo mkdir -p /etc/rh-folha
```

## Pacotes esperados

Instalar no Ubuntu:

```bash
sudo apt update
sudo apt install -y nginx postgresql postgresql-contrib certbot python3-certbot-nginx
```

Instalar o runtime ASP.NET Core compatível com o projeto:

```bash
dotnet --info
```

Se o runtime nao existir, instalar o ASP.NET Core Runtime da Microsoft para a versao usada pelo projeto.

## Banco de dados

Exemplo:

```bash
sudo -u postgres psql
```

```sql
CREATE DATABASE rh_folha;
CREATE USER rh_folha_user WITH PASSWORD 'troque_esta_senha';
GRANT ALL PRIVILEGES ON DATABASE rh_folha TO rh_folha_user;
\c rh_folha
GRANT ALL ON SCHEMA public TO rh_folha_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO rh_folha_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO rh_folha_user;
```

## Variaveis de ambiente

Copiar:

```bash
sudo cp deploy/vps/rh-folha.env.example /etc/rh-folha/rh-folha.env
sudo nano /etc/rh-folha/rh-folha.env
```

Obrigatorio trocar:

- senha do PostgreSQL;
- `Authentication__Jwt__Secret`;
- `Authentication__SeedAdmin__Password`;
- e-mail do administrador inicial.

## Publicacao local antes de enviar

Backend:

```powershell
dotnet publish src/backend/RhFolha.Api/RhFolha.Api.csproj -c Release -o publish/api
```

Frontend:

```powershell
cd src/frontend
npm run build
```

Enviar para a VPS:

```bash
rsync -av --delete publish/api/ usuario@servidor:/opt/rh-folha/api/
rsync -av --delete src/frontend/dist/ usuario@servidor:/var/www/rh-folha/
```

## systemd

No servidor:

```bash
sudo cp deploy/vps/rh-folha.service /etc/systemd/system/rh-folha.service
sudo systemctl daemon-reload
sudo systemctl enable rh-folha
sudo systemctl start rh-folha
sudo systemctl status rh-folha
```

Logs:

```bash
sudo journalctl -u rh-folha -f
```

## Nginx

Copiar o arquivo:

```bash
sudo cp deploy/vps/nginx-folha.conf /etc/nginx/sites-available/folha.caruaru.tec.br
sudo ln -s /etc/nginx/sites-available/folha.caruaru.tec.br /etc/nginx/sites-enabled/folha.caruaru.tec.br
sudo nginx -t
sudo systemctl reload nginx
```

Certificado:

```bash
sudo certbot --nginx -d folha.caruaru.tec.br
```

## Checklist de homologacao

- [ ] `https://folha.caruaru.tec.br` abre a tela de login.
- [ ] Login administrador funciona.
- [ ] `/api/health` responde.
- [ ] Upload de foto funciona.
- [ ] Cadastro e consulta de colaboradores funcionam.
- [ ] Lancamento avulso funciona.
- [ ] Lancamento em massa funciona.
- [ ] Lancamento fixo funciona.
- [ ] Calculo da folha funciona.
- [ ] Auditoria registra acoes sensiveis.
- [ ] Backup manual foi testado.

## Observacoes para VPS pequena

- Evitar Docker nesta VPS.
- Manter PostgreSQL local com poucos parametros e sem expor porta publicamente.
- Manter apenas uma instancia da API.
- Usar `journalctl --vacuum-time=14d` periodicamente se logs crescerem.
- Criar swap se a VPS nao tiver:

```bash
sudo fallocate -l 1G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```
