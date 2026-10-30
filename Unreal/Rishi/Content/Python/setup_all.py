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
        print('Created: ' + f)
    else:
        print('Exists: ' + f)

# Create Maps
maps = ['/Game/Maps/MainMenu', '/Game/Maps/Mission_1']
for m in maps:
    if not unreal.EditorAssetLibrary.does_asset_exist(m):
        unreal.EditorLevelLibrary.new_level(m)
        unreal.EditorLevelLibrary.save_current_level(m, True)
        print('Created map: ' + m)
    else:
        print('Exists: ' + m)

# Project Settings
unreal.GameMapsSettings.set_game_default_map('/Game/Maps/MainMenu')
unreal.GameMapsSettings.set_editor_startup_map('/Game/Maps/MainMenu')
unreal.GameMapsSettings.set_global_default_game_mode('/Game/Blueprints/BP_RishiGameMode')

unreal.InputSettings.set_default_player_input_class(unreal.EnhancedPlayerInput)
unreal.InputSettings.set_default_input_component_class(unreal.EnhancedInputComponent)

print('Setup complete')