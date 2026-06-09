#!/usr/bin/env bash
set -Eeuo pipefail

APP_DIR="${APP_DIR:-/opt/rh-folha}"
API_PROJECT="${API_PROJECT:-$APP_DIR/src/backend/RhFolha.Api/RhFolha.Api.csproj}"
API_PUBLISH_DIR="${API_PUBLISH_DIR:-$APP_DIR/publish/api}"
FRONTEND_DIR="${FRONTEND_DIR:-$APP_DIR/src/frontend}"
FRONTEND_PUBLIC_DIR="${FRONTEND_PUBLIC_DIR:-/var/www/rh-folha}"
SERVICE_NAME="${SERVICE_NAME:-rh-folha}"
HEALTH_URL="${HEALTH_URL:-http://127.0.0.1:5086/api/health}"
RUN_GIT_PULL="${RUN_GIT_PULL:-1}"

log() {
  printf '\n[%s] %s\n' "$(date '+%Y-%m-%d %H:%M:%S')" "$*"
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Comando obrigatorio nao encontrado: $1" >&2
    exit 1
  fi
}

require_path() {
  if [ ! -e "$1" ]; then
    echo "Caminho obrigatorio nao encontrado: $1" >&2
    exit 1
  fi
}

if [ "$(id -u)" -ne 0 ]; then
  echo "Execute como root ou com sudo: sudo $0" >&2
  exit 1
fi

require_command git
require_command dotnet
require_command npm
require_command systemctl
require_command curl
require_path "$APP_DIR/.git"
require_path "$API_PROJECT"
require_path "$FRONTEND_DIR/package.json"

cd "$APP_DIR"

if [ "$RUN_GIT_PULL" = "1" ]; then
  log "Atualizando repositorio"
  git pull --ff-only
else
  log "Pulando git pull por RUN_GIT_PULL=$RUN_GIT_PULL"
fi

log "Publicando API"
dotnet publish "$API_PROJECT" -c Release -o "$API_PUBLISH_DIR" --nologo

log "Compilando frontend"
cd "$FRONTEND_DIR"
npm install
npm run build
require_path "$FRONTEND_DIR/dist/index.html"

log "Copiando frontend para $FRONTEND_PUBLIC_DIR"
rm -rf "$FRONTEND_PUBLIC_DIR"
mkdir -p "$FRONTEND_PUBLIC_DIR"
cp -a "$FRONTEND_DIR/dist/." "$FRONTEND_PUBLIC_DIR/"
chown -R www-data:www-data "$FRONTEND_PUBLIC_DIR"
find "$FRONTEND_PUBLIC_DIR" -type d -exec chmod 755 {} \;
find "$FRONTEND_PUBLIC_DIR" -type f -exec chmod 644 {} \;
require_path "$FRONTEND_PUBLIC_DIR/index.html"

log "Garantindo pasta persistente da API"
mkdir -p "$API_PUBLISH_DIR/data/uploads"
chown -R www-data:www-data "$API_PUBLISH_DIR/data"
find "$API_PUBLISH_DIR/data" -type d -exec chmod 755 {} \;
find "$API_PUBLISH_DIR/data" -type f -exec chmod 644 {} \;

log "Reiniciando servico $SERVICE_NAME"
systemctl daemon-reload
systemctl restart "$SERVICE_NAME"
sleep 3
systemctl --no-pager --full status "$SERVICE_NAME"

log "Validando healthcheck"
curl --fail --silent --show-error "$HEALTH_URL"
printf '\n'

log "Deploy concluido"
