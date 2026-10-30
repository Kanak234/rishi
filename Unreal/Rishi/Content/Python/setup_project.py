# Create Maps and Configure Project for RISHI Linux Build
import unreal

# ============================================================
# PATHS
# ============================================================
MAPS_DIR = "/Game/Maps"
MAP_MENU = f"{MAPS_DIR}/MainMenu"
MAP_M1 = f"{MAPS_DIR}/Mission_1"

# ============================================================
# HELPERS
# ============================================================
def asset_exists(path):
    return unreal.EditorAssetLibrary.does_asset_exist(path)

def make_dir(path):
    if not unreal.EditorAssetLibrary.does_directory_exist(path):
        unreal.EditorAssetLibrary.make_directory(path)

# ============================================================
# CREATE MAPS DIRECTORY
# ============================================================
make_dir("/Game/Maps")
print("Maps directory created")

# ============================================================
# CREATE MAPS
# ============================================================
print("Creating maps...")

# MainMenu
if not asset_exists("/Game/Maps/MainMenu"):
    unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.EmptyLevel)
    unreal.EditorLevelLibrary.save_current_level("/Game/Maps/MainMenu", True)
    print("  Created /Game/Maps/MainMenu")
else:
    print("  Exists /Game/Maps/MainMenu")

# Mission 1
if not asset_exists("/Game/Maps/Mission_1"):
    unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.EmptyLevel)
    unreal.EditorLevelLibrary.save_current_level("/Game/Maps/Mission_1", True)
    print("  Created /Game/Maps/Mission_1")
else:
    print("  Exists /Game/Maps/Mission_1")

# ============================================================
# PROJECT SETTINGS
# ============================================================
print("\nConfiguring project settings...")

unreal.GameMapsSettings.set_game_default_map("/Game/Maps/MainMenu")
unreal.GameMapsSettings.set_editor_startup_map("/Game/Maps/MainMenu")
unreal.GameMapsSettings.set_global_default_game_mode("/Game/Blueprints/BP_RishiGameMode")

unreal.InputSettings.set_default_player_input_class(unreal.EnhancedPlayerInput)
unreal.InputSettings.set_default_input_component_class(unreal.EnhancedInputComponent)

print("\n=== PROJECT CONFIGURATION COMPLETE ===")