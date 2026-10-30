"""RISHI editor tools over MCP stdio. No network listener or arbitrary code execution."""
import json
import math
import os
from pathlib import Path
import time
import uuid
from mcp.server.fastmcp import FastMCP
from check_canon import check

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / 'Unreal/Rishi'
QUEUE = PROJECT / 'Saved/RishiMCP'
MCP = FastMCP('rishi_unreal', instructions='Preserve RISHI canon. Inspect status and canon before scene work. Saving is explicit. This is an Unreal rebuild workspace, not a completed game.')

def heartbeat():
    try:
        data = json.loads((QUEUE / 'heartbeat.json').read_text())
        data['connected'] = 0 <= time.time() - data['time'] < 6
        return data
    except (OSError, ValueError, KeyError):
        return {'connected': False, 'reason': 'Open Unreal with Tools/open-unreal.sh; Python bridge heartbeat is absent.'}

def dispatch(operation, args=None, timeout=20):
    check()
    if not heartbeat()['connected']:
        raise ValueError('Unreal bridge is offline. Start Tools/open-unreal.sh and wait for the project to load.')
    request_id = uuid.uuid4().hex
    requests = QUEUE / 'requests'; responses = QUEUE / 'responses'
    requests.mkdir(parents=True, exist_ok=True); responses.mkdir(parents=True, exist_ok=True)
    path = requests / (request_id + '.json'); reply = responses / (request_id + '.json')
    payload = {'id': request_id, 'operation': operation, 'args': args or {}, 'expires': time.time() + timeout}
    tmp = path.with_suffix('.tmp'); tmp.write_text(json.dumps(payload)); tmp.replace(path)
    deadline = time.monotonic() + timeout
    try:
        while time.monotonic() < deadline:
            if reply.exists():
                result = json.loads(reply.read_text())
                if not result.get('ok'):
                    raise ValueError(result.get('error', 'Editor operation failed'))
                return result['result']
            time.sleep(0.1)
        raise TimeoutError('Editor request timed out. Check the scene before retrying a mutation; it may have run.')
    finally:
        path.unlink(missing_ok=True); reply.unlink(missing_ok=True)

@MCP.tool()
def rishi_status() -> dict:
    """Show editor heartbeat, project and original canon integrity. Never implies the full game is ported."""
    return {'project': str(PROJECT / 'Rishi.uproject'), 'repository': str(ROOT), 'canon': check(), 'editor': heartbeat(), 'game_rebuild': 'project shell; gameplay migration pending'}

@MCP.tool()
def rishi_canon(mission_id: int = 0) -> dict:
    """Read original story and goals. 0 returns all eight missions; 1..8 selects one. Do not rewrite canon."""
    check()
    data = json.loads((PROJECT / 'Content/Story/missions.json').read_text())
    if not 0 <= mission_id <= 8:
        raise ValueError('mission_id must be 0..8')
    return data if mission_id == 0 else data['missions'][mission_id - 1]

@MCP.tool()
def rishi_list_actors() -> dict:
    """Inspect loaded editor world actors; includes whether each was created by this bridge."""
    return dispatch('list_actors')

@MCP.tool()
def rishi_spawn_actor(kind: str, label: str, x: float = 0, y: float = 0, z: float = 100) -> dict:
    """Create a prototype actor. kind: cube, sphere, cylinder, cone, point_light or empty. Units: centimeters."""
    if kind not in {'cube', 'sphere', 'cylinder', 'cone', 'point_light', 'empty'}:
        raise ValueError('Unsupported kind')
    if not label or len(label) > 80 or not all(c.isalnum() or c in '_ -' for c in label):
        raise ValueError('Use a label up to 80 letters, digits, spaces, underscores or hyphens')
    if not all(math.isfinite(v) and abs(v) <= 1000000 for v in [x,y,z]):
        raise ValueError('Location must be finite and within +/-1,000,000 cm')
    return dispatch('spawn_actor', {'kind': kind, 'label': label, 'location': [x,y,z]})

@MCP.tool()
def rishi_set_actor_location(actor_path: str, x: float, y: float, z: float) -> dict:
    """Move an actor created by this bridge. Original actors are rejected. Units: centimeters."""
    if not all(math.isfinite(v) and abs(v) <= 1000000 for v in [x,y,z]):
        raise ValueError('Invalid location')
    return dispatch('set_location', {'actor_path': actor_path, 'location': [x,y,z]})

@MCP.tool()
def rishi_save_level() -> dict:
    """Explicitly save the current level. For new unsaved levels, first use Unreal Save As under /Game/."""
    return dispatch('save_level')

if __name__ == '__main__':
    MCP.run(transport='stdio')
