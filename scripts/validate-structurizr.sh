#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

docker run --rm \
  -v "${ROOT_DIR}/docs/structurizr:/workspace" \
  structurizr/cli:2025.11.09 validate -w /workspace/workspace.dsl
