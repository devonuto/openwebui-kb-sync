#!/usr/bin/env bash
# owui-sync.sh — Pull all git repos in HOST_DROP, then trigger Open WebUI import.
#
# Required environment variables (or edit defaults below):
#   HOST_DROP       — path on the NAS/host to the drop folder (for git pulls)
#   CONTAINER_DROP  — same folder as seen from inside the container
#   OWUI_CONTAINER  — docker container name/id running Open WebUI
#   SCRIPTS_DIR     — path inside the container where run_import.py is mounted

set -euo pipefail

HOST_DROP="${HOST_DROP:-/host/path/to/drop}"
CONTAINER_DROP="${CONTAINER_DROP:-/app/backend/data/drop}"
OWUI_CONTAINER="${OWUI_CONTAINER:-open-webui}"
# Directory inside the container where run_import.py is mounted.
# Add to your docker-compose volumes: - /volume2/docker/markdown-sync:/scripts:ro
SCRIPTS_DIR="${SCRIPTS_DIR:-/scripts}"

# ── 1. Git pull every immediate subfolder (runs on the host) ─────────────────
for dir in "$HOST_DROP"/*/; do
  [ -d "$dir/.git" ] || continue
  echo "[sync] Pulling $dir"
  git -C "$dir" pull --ff-only
done

# ── 2. Run the import directly inside the container via docker exec ───────────
#    This bypasses the LLM entirely — Open WebUI's /api/chat/completions endpoint
#    only proxies to the model provider and does NOT execute tool Python code
#    server-side when using external providers like OpenAI.
echo "[sync] Triggering import inside container $OWUI_CONTAINER (drop=$CONTAINER_DROP)"

docker exec "$OWUI_CONTAINER" \
  python3 "$SCRIPTS_DIR/run_import.py" "$CONTAINER_DROP"

echo "[sync] Done."
