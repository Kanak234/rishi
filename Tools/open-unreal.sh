#!/usr/bin/env bash
set -euo pipefail
repo="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
engine="${UNREAL_EDITOR:-/home/kanak/Documents/Codex/Applications/UnrealEngine-5.8.4/Engine/Binaries/Linux/UnrealEditor}"
python3 "$repo/Tools/UnrealMCP/check_canon.py"
# Fail once in restricted environments instead of opening repeated Unreal dialogs.
python3 - <<'CHECK_SETTINGS'
from pathlib import Path
import tempfile
settings = Path.home() / '.config/Epic'
try:
    settings.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryFile(dir=settings):
        pass
    with (settings / 'Unreal.uproject').open('a'):
        pass
except OSError as exc:
    raise SystemExit(f'Unreal settings are not writable: {settings}\n{exc}\nRun this launcher from your normal Ubuntu terminal, outside the Codex sandbox.')
CHECK_SETTINGS
exec "$engine" "$repo/Unreal/Rishi/Rishi.uproject" "$@"
