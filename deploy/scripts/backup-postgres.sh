#!/usr/bin/env bash
set -euo pipefail

BACKUP_DIR="${BACKUP_DIR:-/opt/rh-folha/backups}"
DB_NAME="${POSTGRES_DB:-rh_folha}"
DB_USER="${POSTGRES_USER:-rh_folha_user}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"

mkdir -p "$BACKUP_DIR"
pg_dump -U "$DB_USER" -h localhost "$DB_NAME" | gzip > "$BACKUP_DIR/rh-folha-db-$TIMESTAMP.sql.gz"

find "$BACKUP_DIR" -name "rh-folha-db-*.sql.gz" -mtime +14 -delete
