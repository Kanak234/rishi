"""Verify original source files and frozen story/design snapshots without changing them."""
import hashlib
import json
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
def check():
    manifest = json.loads((ROOT / 'Docs/Canon/manifest.json').read_text())
    errors = []
    for source, item in manifest['files'].items():
        for name in [source, item['snapshot']]:
            p = ROOT / name
            if not p.is_file() or hashlib.sha256(p.read_bytes()).hexdigest() != item['sha256']:
                errors.append(name)
    if errors:
        raise ValueError('Canon drift: ' + ', '.join(errors))
    return {'ok': True, 'source_commit': manifest['source_commit'], 'verified_files': len(manifest['files']) * 2}
if __name__ == '__main__':
    print(json.dumps(check(), indent=2))
