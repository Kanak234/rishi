"""Project-local file queue consumed on Unreal's editor main thread. No listening ports."""
import json
import os
from pathlib import Path
import time
import unreal

PROJECT = Path(__file__).resolve().parents[2]
QUEUE = PROJECT / 'Saved/RishiMCP'
_HANDLE = None
_LAST = 0.0
_TAG = 'RishiMCP'

def atomic(path, data):
    tmp = path.with_suffix('.tmp')
    tmp.write_text(json.dumps(data), encoding='utf-8')
    tmp.replace(path)

def actors():
    return unreal.get_editor_subsystem(unreal.EditorActorSubsystem)

def describe(actor):
    p = actor.get_actor_location()
    return {'path': actor.get_path_name(), 'label': actor.get_actor_label(),
            'class': actor.get_class().get_name(), 'location': [p.x,p.y,p.z],
            'bridge_owned': unreal.Name(_TAG) in actor.tags}

def execute(operation, args):
    if operation == 'list_actors':
        all_actors = actors().get_all_level_actors()
        return {'actors': [describe(a) for a in all_actors[:500]], 'count': len(all_actors), 'truncated': len(all_actors) > 500}
    if operation == 'spawn_actor':
        kind = args['kind']
        if kind not in {'cube','sphere','cylinder','cone','empty','point_light'}:
            raise ValueError('Unsupported actor kind')
        cls = unreal.PointLight if kind == 'point_light' else (unreal.Actor if kind == 'empty' else unreal.StaticMeshActor)
        a = actors().spawn_actor_from_class(cls, unreal.Vector(*args['location']))
        if not a: raise RuntimeError('Actor spawn failed')
        a.set_actor_label('Rishi_' + args['label'])
        a.tags = list(a.tags) + [unreal.Name(_TAG)]
        a.set_folder_path('RishiMCP')
        if kind in {'cube','sphere','cylinder','cone'}:
            mesh = unreal.load_asset('/Engine/BasicShapes/' + kind.capitalize())
            if not mesh: raise RuntimeError('Engine basic shape unavailable')
            a.static_mesh_component.set_static_mesh(mesh)
        return describe(a)
    if operation == 'set_location':
        a = next((a for a in actors().get_all_level_actors() if a.get_path_name() == args['actor_path']), None)
        if not a or unreal.Name(_TAG) not in a.tags:
            raise ValueError('Only bridge-created actors may be modified')
        if not a.set_actor_location(unreal.Vector(*args['location']), False, True):
            raise RuntimeError('Actor movement failed')
        return describe(a)
    if operation == 'save_level':
        subsystem = unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
        if not subsystem.save_current_level():
            raise RuntimeError('Save failed; use Save As in Unreal for an unsaved level first')
        return {'saved': True}
    raise ValueError('Unsupported operation')

def tick(delta):
    global _LAST
    now = time.time()
    if now - _LAST < 0.2: return
    _LAST = now
    try:
        atomic(QUEUE/'heartbeat.json', {'time': now, 'pid': os.getpid(), 'project': str(PROJECT / 'Rishi.uproject'), 'engine': unreal.SystemLibrary.get_engine_version()})
        for path in sorted((QUEUE/'requests').glob('*.json'))[:4]:
            request = json.loads(path.read_text())
            if request.get('id') != path.stem or len(path.stem) != 32:
                path.unlink(); continue
            try:
                if request.get('expires',0) < time.time(): raise ValueError('Request expired; no operation performed')
                result = {'ok':True,'result':execute(request['operation'], request.get('args',{}))}
            except Exception as e:
                result = {'ok':False,'error':str(e)}
            atomic(QUEUE/'responses'/path.name, result)
            path.unlink(missing_ok=True)
        for reply in (QUEUE/'responses').glob('*.json'):
            if now - reply.stat().st_mtime > 120: reply.unlink(missing_ok=True)
    except Exception as e:
        unreal.log_warning('Rishi MCP bridge: ' + str(e))

def start():
    global _HANDLE
    if _HANDLE is not None: return
    QUEUE.mkdir(parents=True,exist_ok=True)
    for folder in ['requests','responses']:
        (QUEUE/folder).mkdir(exist_ok=True)
    _HANDLE = unreal.register_slate_post_tick_callback(tick)
    unreal.log('Rishi MCP bridge ready: ' + str(QUEUE))
