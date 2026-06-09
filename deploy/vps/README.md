# Deploy em VPS Linux sem Docker

Dominio alvo: `folha.caruaru.tec.br`

Este modelo foi validado em VPS Ubuntu 24.04 LTS com Nginx, PostgreSQL nativo e API ASP.NET Core via systemd.

## Arquitetura

- Nginx publico em `80/443`.
- Frontend React publicado em `/var/www/rh-folha`.
- API ASP.NET Core rodando via systemd em `127.0.0.1:5086`.
- PostgreSQL instalado no sistema operacional.
- Uploads persistentes em `/opt/rh-folha/publish/api/data/uploads`.
- Logs via `journalctl`.
- Backups em `/opt/rh-folha/backups`.

## Diretorios no servidor

```bash
sudo mkdir -p /opt/rh-folha/publish/api
sudo mkdir -p /opt/rh-folha/publish/api/data/uploads/employees
sudo mkdir -p /opt/rh-folha/backups
sudo mkdir -p /var/www/rh-folha
sudo mkdir -p /etc/rh-folha
sudo chown -R www-data:www-data /opt/rh-folha/publish/api/data
sudo chmod -R 750 /opt/rh-folha/publish/api/data
```

## Pacotes esperados

```bash
sudo apt update
sudo apt install -y nginx postgresql postgresql-contrib certbot python3-certbot-nginx
sudo apt install -y dotnet-sdk-10.0
dotnet --info
```

O projeto foi homologado na VPS com .NET SDK 10.

## Banco de dados

```bash
sudo -u postgres psql
```

```sql
CREATE USER rh_folha_user WITH PASSWORD 'troque_esta_senha';
CREATE DATABASE rh_folha OWNER rh_folha_user;
GRANT ALL PRIVILEGES ON DATABASE rh_folha TO rh_folha_user;
\q
```

Teste:

```bash
PGPASSWORD='troque_esta_senha' psql -h localhost -p 5432 -U rh_folha_user -d rh_folha -c "SELECT current_database(), current_user;"
```

## Variaveis de ambiente

```bash
sudo cp deploy/vps/rh-folha.env.example /etc/rh-folha/rh-folha.env
sudo nano /etc/rh-folha/rh-folha.env
sudo chmod 600 /etc/rh-folha/rh-folha.env
```

Obrigatorio trocar:

- senha do PostgreSQL;
- `Authentication__Jwt__Secret`;
- `Authentication__SeedAdmin__Password`;
- e-mail do administrador inicial.

Importante: o nome correto da connection string e `ConnectionStrings__DefaultConnection`.

## Publicacao na VPS

### Deploy automatico recomendado

Depois da primeira configuracao da VPS, use o script de deploy para evitar copia manual de blocos grandes no terminal:

```bash
cd /opt/rh-folha
git pull
sudo chmod +x deploy/vps/deploy.sh
sudo deploy/vps/deploy.sh
```

O script executa:

- `git pull --ff-only`;
- `dotnet publish` da API;
- `npm install` e `npm run build` do frontend;
- copia segura do `dist/.` para `/var/www/rh-folha`;
- ajuste de permissoes;
- garantia da pasta persistente `data/uploads`;
- restart do `rh-folha`;
- teste do healthcheck local.

Se voce ja executou `git pull` antes e nao quer repetir:

```bash
sudo RUN_GIT_PULL=0 deploy/vps/deploy.sh
```

O script deve terminar com `Deploy concluido`. Se falhar em qualquer etapa, ele para e mostra o erro.

### Deploy manual de emergencia

Backend:

```bash
cd /opt/rh-folha
dotnet restore src/backend/RhFolha.slnx
dotnet publish src/backend/RhFolha.Api/RhFolha.Api.csproj -c Release -o /opt/rh-folha/publish/api
```

Frontend:

```bash
cd /opt/rh-folha/src/frontend
npm install
npm run build
sudo rm -rf /var/www/rh-folha/*
sudo cp -a /opt/rh-folha/src/frontend/dist/. /var/www/rh-folha/
sudo chown -R www-data:www-data /var/www/rh-folha
sudo find /var/www/rh-folha -type d -exec chmod 755 {} \;
sudo find /var/www/rh-folha -type f -exec chmod 644 {} \;
ls -la /var/www/rh-folha
```

A pasta publica deve conter `index.html` e `assets`.

## systemd

```bash
sudo cp deploy/vps/rh-folha.service /etc/systemd/system/rh-folha.service
sudo systemctl daemon-reload
sudo systemctl enable rh-folha
sudo systemctl restart rh-folha
sudo systemctl status rh-folha --no-pager
```

Validacoes:

```bash
sudo systemctl cat rh-folha
sudo journalctl -u rh-folha -n 80 --no-pager -l
curl http://127.0.0.1:5086/api/health
```

O `WorkingDirectory` e o `ExecStart` devem apontar para `/opt/rh-folha/publish/api`.

## Nginx e HTTPS

Copiar o arquivo inicial:

```bash
sudo cp deploy/vps/nginx-folha.conf /etc/nginx/sites-available/folha.caruaru.tec.br
sudo ln -s /etc/nginx/sites-available/folha.caruaru.tec.br /etc/nginx/sites-enabled/folha.caruaru.tec.br
sudo nginx -t
sudo systemctl reload nginx
```

Antes do certificado, conferir DNS:

```bash
curl -4 ifconfig.me
dig +short folha.caruaru.tec.br
```

Os dois devem apontar para o mesmo IP publico da VPS.

Para Let's Encrypt via webroot, garantir no server de porta 80:

```nginx
location ^~ /.well-known/acme-challenge/ {
    root /var/www/letsencrypt;
    default_type "text/plain";
    try_files $uri =404;
}
```

Teste:

```bash
sudo mkdir -p /var/www/letsencrypt/.well-known/acme-challenge
echo ok | sudo tee /var/www/letsencrypt/.well-known/acme-challenge/teste
curl http://folha.caruaru.tec.br/.well-known/acme-challenge/teste
```

Emitir certificado:

```bash
sudo certbot certonly --webroot -w /var/www/letsencrypt -d folha.caruaru.tec.br
```

Depois configurar o bloco `443 ssl` no Nginx usando:

```text
/etc/letsencrypt/live/folha.caruaru.tec.br/fullchain.pem
/etc/letsencrypt/live/folha.caruaru.tec.br/privkey.pem
```

Validar:

```bash
sudo nginx -t
sudo systemctl reload nginx
curl https://folha.caruaru.tec.br/api/health
curl -I https://folha.caruaru.tec.br/index.html
```

## Checklist rapido de deploy validado

1. DNS aponta para a VPS correta.
2. PostgreSQL ativo e usuario da aplicacao conecta.
3. `/etc/rh-folha/rh-folha.env` usa `ConnectionStrings__DefaultConnection`.
4. `dotnet publish` foi gerado em `/opt/rh-folha/publish/api`.
5. Pasta `/opt/rh-folha/publish/api/data` pertence a `www-data`.
6. `systemd` foi recarregado com `daemon-reload`.
7. API responde em `http://127.0.0.1:5086/api/health`.
8. Certbot valida `/.well-known/acme-challenge`.
9. Frontend foi copiado com `dist/.` para `/var/www/rh-folha`.
10. `https://folha.caruaru.tec.br` abre a tela de login.

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
- Manter PostgreSQL local sem expor porta publicamente.
- Manter apenas uma instancia da API.
- Usar `journalctl --vacuum-time=14d` periodicamente se logs crescerem.
- Criar swap se a VPS nao tiver.
