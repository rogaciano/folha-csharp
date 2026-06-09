# Pos-mortem do primeiro deploy em VPS

Data: 2026-06-08

Ambiente validado:

- VPS Ubuntu 24.04 LTS x64.
- 2 vCPU.
- 4GB RAM apos upgrade.
- PostgreSQL 16.
- .NET SDK 10.
- Nginx 1.24.
- Dominio: `folha.caruaru.tec.br`.

## Resultado

Deploy concluido com:

- API respondendo em `http://127.0.0.1:5086/api/health`.
- API respondendo publicamente em `https://folha.caruaru.tec.br/api/health`.
- PostgreSQL acessivel pela aplicacao.
- Certificado Let's Encrypt emitido.
- Frontend publicado em `/var/www/rh-folha`.

## Problemas encontrados

### DNS apontando para VPS antiga

Sintoma:

- Certbot falhava com `unauthorized`.
- O teste HTTP retornava pagina com `Apache/2.4.41`, mas a VPS nova usava Nginx.

Diagnostico:

```bash
curl -4 ifconfig.me
dig +short folha.caruaru.tec.br
curl -I http://IP_DO_DNS -H "Host: folha.caruaru.tec.br"
```

Causa:

- O registro `A` de `folha.caruaru.tec.br` ainda apontava para a VPS anterior.

Prevencao:

- Antes do Certbot, confirmar que `dig +short folha.caruaru.tec.br` retorna o mesmo IP de `curl -4 ifconfig.me`.

### Certbot falhando por desafio HTTP

Sintoma:

- `Invalid response from http://folha.caruaru.tec.br/.well-known/acme-challenge/...: 404`.

Diagnostico:

```bash
echo ok | sudo tee /var/www/letsencrypt/.well-known/acme-challenge/teste
curl -H "Host: folha.caruaru.tec.br" http://127.0.0.1/.well-known/acme-challenge/teste
curl http://folha.caruaru.tec.br/.well-known/acme-challenge/teste
```

Causa:

- O dominio ainda chegava em outro servidor. A configuracao local do Nginx estava correta.

Prevencao:

- Validar o desafio local e publico antes de executar `certbot`.

### systemd usando caminho antigo da API

Sintoma:

- `status=200/CHDIR`.
- Log: `Changing to the requested working directory failed`.
- `ExecStart=/usr/bin/dotnet /opt/rh-folha/api/RhFolha.Api.dll`.

Causa:

- O arquivo de servico apontava para `/opt/rh-folha/api`, mas o publish foi gerado em `/opt/rh-folha/publish/api`.

Correcao:

```ini
WorkingDirectory=/opt/rh-folha/publish/api
ExecStart=/usr/bin/dotnet /opt/rh-folha/publish/api/RhFolha.Api.dll
```

Validacao:

```bash
sudo systemctl cat rh-folha
```

### systemd nao recarregado apos alterar unit file

Sintoma:

- `systemctl cat` mostrava arquivo novo, mas `status` ainda executava o caminho antigo.
- Aviso: `Run 'systemctl daemon-reload' to reload units`.

Causa:

- `daemon-reload` nao foi executado ou foi colado junto com o prompt do terminal.

Correcao:

```bash
sudo systemctl stop rh-folha
sudo systemctl daemon-reload
sudo systemctl reset-failed rh-folha
sudo systemctl start rh-folha
```

### Nome errado da connection string

Sintoma:

- A aplicacao tentava conectar em `tcp://localhost:5434`, mesmo com PostgreSQL rodando em `5432`.

Causa:

- O arquivo de ambiente usava `ConnectionStrings__Default`, mas o codigo le `DefaultConnection`.

Correto:

```bash
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=rh_folha;Username=rh_folha_user;Password=senha
```

Validacao:

```bash
sudo grep -n "ConnectionStrings" /etc/rh-folha/rh-folha.env
```

### Permissao negada na pasta `data`

Sintoma:

- `UnauthorizedAccessException: Access to the path '/opt/rh-folha/publish/api/data' is denied`.

Causa:

- A API roda como `www-data`, mas a pasta estava sob propriedade de `root`.

Correcao:

```bash
sudo mkdir -p /opt/rh-folha/publish/api/data/uploads/employees
sudo chown -R www-data:www-data /opt/rh-folha/publish/api/data
sudo chmod -R 750 /opt/rh-folha/publish/api/data
```

### Frontend nao publicado

Sintoma:

- `403 Forbidden`.
- Log do Nginx: `directory index of "/var/www/rh-folha/" is forbidden`.

Causa:

- `/var/www/rh-folha` estava vazio.

Correcao:

```bash
cd /opt/rh-folha/src/frontend
npm install
npm run build
sudo rm -rf /var/www/rh-folha/*
sudo cp -a /opt/rh-folha/src/frontend/dist/. /var/www/rh-folha/
ls -la /var/www/rh-folha
```

A validacao precisa mostrar:

- `index.html`;
- pasta `assets`.

### Comandos colados junto com o prompt

Sintoma:

- Comandos como `sudo systemctl start rh-folharoot@django:/opt/rh-folha#`.
- Copias para caminhos inexistentes como `/var/www/rh-folha/root@django:/...`.

Causa:

- O prompt do terminal foi copiado junto com o comando.

Prevencao:

- Executar comandos uma linha por vez.
- Nunca copiar `root@servidor:/caminho#`.
- Conferir com `ls` apos copias.

### `apt install nodejs npm` apos Node ja estar instalado

Sintoma:

- Erro de dependencias/conflito entre `nodejs` e `npm`.

Causa:

- O ambiente ja tinha Node/npm suficientes para `npm install` e `npm run build`.

Prevencao:

```bash
node -v
npm -v
```

Se o build funcionar, nao instalar novamente.

### Senha exposta no terminal/chat

Sintoma:

- A senha do PostgreSQL apareceu em comandos e logs compartilhados.

Risco:

- Para homologacao e aceitavel corrigir depois; para producao, trocar obrigatoriamente.

Correcao recomendada:

```bash
sudo -u postgres psql
```

```sql
ALTER USER rh_folha_user WITH PASSWORD 'nova_senha_forte';
\q
```

Depois atualizar `/etc/rh-folha/rh-folha.env` e reiniciar:

```bash
sudo systemctl restart rh-folha
```

## Checklist enxuto para o proximo deploy

1. Confirmar IP/DNS.
2. Instalar .NET SDK compativel.
3. Instalar PostgreSQL e criar banco/usuario.
4. Clonar repositorio em `/opt/rh-folha`.
5. Criar `/etc/rh-folha/rh-folha.env` com `ConnectionStrings__DefaultConnection`.
6. Rodar `dotnet publish` para `/opt/rh-folha/publish/api`.
7. Criar pasta `data/uploads/employees` e dar permissao para `www-data`.
8. Copiar unit file, executar `daemon-reload`, iniciar servico.
9. Validar `curl http://127.0.0.1:5086/api/health`.
10. Configurar Nginx porta 80 com desafio Let's Encrypt.
11. Emitir certificado.
12. Configurar Nginx 443.
13. Rodar build do frontend e copiar `dist/.` para `/var/www/rh-folha`.
14. Validar:

```bash
curl https://folha.caruaru.tec.br/api/health
curl -I https://folha.caruaru.tec.br/index.html
```

15. Abrir `https://folha.caruaru.tec.br` no navegador.

## Comandos de diagnostico mais uteis

```bash
sudo systemctl status rh-folha --no-pager
sudo journalctl -u rh-folha -n 80 --no-pager -l
sudo systemctl cat rh-folha
sudo nginx -t
sudo tail -n 80 /var/log/nginx/error.log
curl -4 ifconfig.me
dig +short folha.caruaru.tec.br
sudo ss -lntp | grep -E ':80|:443|:5086|:5432'
```
