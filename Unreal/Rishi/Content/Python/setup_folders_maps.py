import unreal

# Create folder structure
folders = [
    '/Game/Input',
    '/Game/Blueprints/Characters',
    '/Game/Blueprints/UI',
    '/Game/Blueprints/World',
    '/Game/Maps',
    '/Game/Cinematics'
]

for f in folders:
    if not unreal.EditorAssetLibrary.does_directory_exist(f):
        unreal.EditorAssetLibrary.make_directory(f)
        print(f'Created: {f}')
    else:
        print(f'Exists: {f}')

# Create Maps
maps = ['/Game/Maps/MainMenu', '/Game/Maps/Mission_1']
for m in maps:
    if not unreal.EditorAssetLibrary.does_asset_exist(m):
        unreal.EditorLevelLibrary.new_level(m)
        unreal.EditorLevelLibrary.save_current_level(m, True)
        print(f'Created map: {m}')
    else:
        print(f'Exists: {m}')

print('Folders and maps setup complete')