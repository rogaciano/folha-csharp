#!/usr/bin/env bash
set -euo pipefail

BACKUP_DIR="${BACKUP_DIR:-/opt/rh-folha/backups}"
UPLOADS_DIR="${UPLOADS_DIR:-/opt/rh-folha/data/uploads}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"

mkdir -p "$BACKUP_DIR"
tar -czf "$BACKUP_DIR/rh-folha-uploads-$TIMESTAMP.tar.gz" -C "$UPLOADS_DIR" .

find "$BACKUP_DIR" -name "rh-folha-uploads-*.tar.gz" -mtime +14 -delete
