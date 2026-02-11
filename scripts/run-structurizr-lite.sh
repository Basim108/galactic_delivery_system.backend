#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

docker run --rm \
  -p 8080:8080 \
  -v "${ROOT_DIR}/docs/structurizr:/usr/local/structurizr" \
  structurizr/lite:2025.11.08
