#!/usr/bin/env bash
set -euo pipefail
repo="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo"
python3 Tools/UnrealMCP/check_canon.py
exec "$HOME/.opencode/bin/opencode" "$@"
