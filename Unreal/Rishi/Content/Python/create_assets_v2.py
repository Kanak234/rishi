# Create ALL Blueprint assets for RISHI Linux playable build (simplified API)
import unreal
import os

# ============================================================
# PATHS
# ============================================================
CONTENT_DIR = "/Game"
BLUEPRINTS_DIR = f"{CONTENT_DIR}/Blueprints"
CHAR_DIR = f"{BLUEPRINTS_DIR}/Characters"
UI_DIR = f"{BLUEPRINTS_DIR}/UI"
WORLD_DIR = f"{BLUEPRINTS_DIR}/World"
INPUT_DIR = f"{CONTENT_DIR}/Input"
MAPS_DIR = f"{CONTENT_DIR}/Maps"

# Asset paths
BP_CHAR = f"{CHAR_DIR}/BP_RishiPlayerCharacter"
BP_GAMEMODE = f"{BLUEPRINTS_DIR}/BP_RishiGameMode"
BP_HUD = f"{UI_DIR}/BP_RishiHUD"
BP_MAINMENU = f"{UI_DIR}/BP_MainMenuWidget"
BP_PAUSE = f"{UI_DIR}/BP_PauseMenuWidget"
BP_SETTINGS = f"{UI_DIR}/BP_SettingsWidget"
BP_ENEMY_CHAYABHOOT = f"{CHAR_DIR}/BP_ChhayaBhoot"
BP_CHECKPOINT = f"{WORLD_DIR}/BP_Checkpoint"
BP_SHRINE = f"{WORLD_DIR}/BP_ObjectiveShrine"
BP_KILLZONE = f"{WORLD_DIR}/BP_KillZone"
BP_ENCOUNTER = f"{WORLD_DIR}/BP_EncounterZone"

IA_MOVE = f"{INPUT_DIR}/IA_Move"
IA_JUMP = f"{INPUT_DIR}/IA_Jump"
IA_DODGE = f"{INPUT_DIR}/IA_Dodge"
IA_LIGHT = f"{INPUT_DIR}/IA_LightAttack"
IA_HEAVY = f"{INPUT_DIR}/IA_HeavyAttack"
IA_INTERACT = f"{INPUT_DIR}/IA_Interact"
IA_PREV = f"{INPUT_DIR}/IA_PrevWeapon"
IA_NEXT = f"{INPUT_DIR}/IA_NextWeapon"
IA_AIM = f"{INPUT_DIR}/IA_Aim"
IA_PAUSE = f"{INPUT_DIR}/IA_Pause"
IMC_RISHI = f"{INPUT_DIR}/IMC_Rishi"

MAP_MENU = f"{MAPS_DIR}/MainMenu"
MAP_M1 = f"{MAPS_DIR}/Mission_1"

# ============================================================
# HELPERS
# ============================================================
def asset_exists(path):
    return unreal.EditorAssetLibrary.does_asset_exist(path)

def load_asset(path):
    return unreal.EditorAssetLibrary.load_asset(path)

def save_asset(path):
    unreal.EditorAssetLibrary.save_asset(path)

def make_dir(path):
    if not unreal.EditorAssetLibrary.does_directory_exist(path):
        unreal.EditorAssetLibrary.make_directory(path)

# ============================================================
# CREATE FOLDERS
# ============================================================
for d in [CHAR_DIR, UI_DIR, WORLD_DIR, INPUT_DIR, MAPS_DIR]:
    make_dir(d)
print("Folders created")

# ============================================================
# INPUT ACTIONS - Create via AssetTools directly
# ============================================================
print("Creating Input Actions...")
input_actions = [
    (IA_MOVE, "IA_Move"),
    (IA_JUMP, "IA_Jump"),
    (IA_DODGE, "IA_Dodge"),
    (IA_LIGHT, "IA_LightAttack"),
    (IA_HEAVY, "IA_HeavyAttack"),
    (IA_INTERACT, "IA_Interact"),
    (IA_PREV, "IA_PrevWeapon"),
    (IA_NEXT, "IA_NextWeapon"),
    (IA_AIM, "IA_Aim"),
    (IA_PAUSE, "IA_Pause"),
]

for path, name in input_actions:
    if not asset_exists(path):
        # Create InputAction directly
        asset = unreal.AssetToolsHelpers.get_asset_tools().create_asset(name, INPUT_DIR, unreal.InputAction, None)
        if asset:
            # Set value type after creation
            if name == "IA_Move":
                asset.value_type = unreal.InputActionValueType.AXIS2D
            else:
                asset.value_type = unreal.InputActionValueType.DIGITAL_BOOL
            save_asset(path)
            print(f"  Created {path}")
        else:
            print(f"  FAILED {path}")
    else:
        print(f"  Exists {path}")

# ============================================================
# INPUT MAPPING CONTEXT
# ============================================================
print("Creating Input Mapping Context...")
if not asset_exists(IMC_RISHI):
    imc = unreal.AssetToolsHelpers.get_asset_tools().create_asset("IMC_Rishi", INPUT_DIR, unreal.InputMappingContext, None)
    
    if imc:
        # Load all IAs
        ia_move = load_asset(IA_MOVE)
        ia_jump = load_asset(IA_JUMP)
        ia_dodge = load_asset(IA_DODGE)
        ia_light = load_asset(IA_LIGHT)
        ia_heavy = load_asset(IA_HEAVY)
        ia_interact = load_asset(IA_INTERACT)
        ia_prev = load_asset(IA_PREV)
        ia_next = load_asset(IA_NEXT)
        ia_aim = load_asset(IA_AIM)
        ia_pause = load_asset(IA_PAUSE)
        
        if all([ia_move, ia_jump, ia_dodge, ia_light, ia_heavy, ia_interact, ia_prev, ia_next, ia_aim, ia_pause]):
            # Keyboard mappings
            kb_mappings = [
                (ia_move, unreal.Key("W"), 1.0),
                (ia_move, unreal.Key("S"), -1.0),
                (ia_move, unreal.Key("A"), -1.0),
                (ia_move, unreal.Key("D"), 1.0),
                (ia_jump, unreal.Key("SpaceBar"), 1.0),
                (ia_dodge, unreal.Key("LeftShift"), 1.0),
                (ia_light, unreal.Key("LeftMouseButton"), 1.0),
                (ia_heavy, unreal.Key("RightMouseButton"), 1.0),
                (ia_interact, unreal.Key("F"), 1.0),
                (ia_prev, unreal.Key("Q"), 1.0),
                (ia_next, unreal.Key("E"), 1.0),
                (ia_aim, unreal.Key("LeftControl"), 1.0),
                (ia_pause, unreal.Key("Escape"), 1.0),
            ]
            
            # Gamepad mappings
            gp_mappings = [
                (ia_move, unreal.Key("Gamepad_LeftStick_2D"), 1.0),
                (ia_jump, unreal.Key("Gamepad_FaceButton_Bottom"), 1.0),
                (ia_dodge, unreal.Key("Gamepad_FaceButton_Right"), 1.0),
                (ia_light, unreal.Key("Gamepad_FaceButton_Left"), 1.0),
                (ia_heavy, unreal.Key("Gamepad_FaceButton_Top"), 1.0),
                (ia_interact, unreal.Key("Gamepad_FaceButton_Bottom"), 1.0),
                (ia_prev, unreal.Key("Gamepad_DPad_Left"), 1.0),
                (ia_next, unreal.Key("Gamepad_DPad_Right"), 1.0),
                (ia_aim, unreal.Key("Gamepad_LeftTrigger"), 1.0),
                (ia_pause, unreal.Key("Gamepad_Special_Right"), 1.0),
            ]
            
            for ia, key, val in kb_mappings:
                mapping = unreal.InputMapping()
                mapping.input_action = ia
                mapping.key = key
                mapping.value = val
                unreal.InputMappingContext.add_mapping(imc, mapping)
            
            for ia, key, val in gp_mappings:
                mapping = unreal.InputMapping()
                mapping.input_action = ia
                mapping.key = key
                mapping.value = val
                unreal.InputMappingContext.add_mapping(imc, mapping)
            
            save_asset(IMC_RISHI)
            print(f"  Created {IMC_RISHI}")
        else:
            print("  FAILED - Some IAs not loaded")
    else:
        print(f"  FAILED to create IMC")
else:
    print(f"  Exists {IMC_RISHI}")

# ============================================================
# CREATE BASIC BLUEPRINT FUNCTION
# ============================================================
def create_bp(asset_path, parent_class, name):
    if asset_exists(asset_path):
        print(f"  Exists {asset_path}")
        return load_asset(asset_path)
    
    bp = unreal.AssetToolsHelpers.get_asset_tools().create_asset(name, os.path.dirname(asset_path), unreal.Blueprint, None)
    if bp:
        bp.parent_class = parent_class
        save_asset(asset_path)
        print(f"  Created {asset_path}")
        return bp
    else:
        print(f"  FAILED {asset_path}")
        return None

# ============================================================
# CREATE ALL BLUEPRINTS
# ============================================================
print("\n=== Creating Blueprints ===")

# 1. Player Character
bp_char = create_bp(BP_CHAR, unreal.Character, "BP_RishiPlayerCharacter")
if bp_char:
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp_char)
    cdo = bp_class.get_default_object()
    cdo.get_character_movement().max_walk_speed = 600.0
    cdo.get_character_movement().rotation_rate = unreal.Rotator(0, 900, 0)
    cdo.get_character_movement().jump_z_velocity = 600.0
    cdo.get_character_movement().air_control = 0.6
    cdo.get_character_movement().gravity_scale = 1.0
    cdo.get_character_movement().b_orient_rotation_to_movement = False
    cdo.get_character_movement().b_use_controller_rotation_yaw = False
    cdo.get_capsule_component().set_capsule_size(40.0, 100.0)
    
    # Camera Boom
    boom = unreal.SpringArmComponent("CameraBoom")
    boom.setup_attachment(cdo.get_root_component())
    boom.target_arm_length = 1200.0
    boom.set_relative_rotation(unreal.Rotator(-45, 45, 0))
    boom.b_use_pawn_control_rotation = False
    boom.b_inherit_pitch = False
    boom.b_inherit_yaw = False
    boom.b_inherit_roll = False
    boom.b_do_collision_test = False
    boom.register_component()
    
    # Camera
    cam = unreal.CameraComponent("Camera")
    cam.setup_attachment(boom, unreal.SpringArmComponent.socket_name)
    cam.b_use_pawn_control_rotation = False
    cam.field_of_view = 40.0
    cam.register_component()
    
    save_asset(BP_CHAR)

# 2. GameMode
bp_gm = create_bp(BP_GAMEMODE, unreal.GameModeBase, "BP_RishiGameMode")
if bp_gm:
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp_gm)
    cdo = bp_class.get_default_object()
    cdo.default_pawn_class = load_asset(BP_CHAR)
    cdo.player_controller_class = unreal.PlayerController
    save_asset(BP_GAMEMODE)

# 3-6. UI Widgets
for path, name in [(BP_HUD, "BP_RishiHUD"), (BP_MAINMENU, "BP_MainMenuWidget"), 
                    (BP_PAUSE, "BP_PauseMenuWidget"), (BP_SETTINGS, "BP_SettingsWidget")]:
    create_bp(path, unreal.UserWidget, name)

# 7. Enemy - ChhayaBhoot
bp_enemy = create_bp(BP_ENEMY_CHAYABHOOT, unreal.Character, "BP_ChhayaBhoot")
if bp_enemy:
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp_enemy)
    cdo = bp_class.get_default_object()
    cdo.get_character_movement().max_walk_speed = 550.0
    cdo.get_character_movement().rotation_rate = unreal.Rotator(0, 540, 0)
    cdo.get_character_movement().b_orient_rotation_to_movement = False
    cdo.get_capsule_component().set_capsule_size(40.0, 100.0)
    save_asset(BP_ENEMY_CHAYABHOOT)

# 8-11. World Actors
for path, name in [(BP_CHECKPOINT, "BP_Checkpoint"), (BP_SHRINE, "BP_ObjectiveShrine"),
                    (BP_KILLZONE, "BP_KillZone"), (BP_ENCOUNTER, "BP_EncounterZone")]:
    create_bp(path, unreal.Actor, name)

# Update GameMode with HUD
if asset_exists(BP_GAMEMODE) and asset_exists(BP_HUD):
    bp_gm = load_asset(BP_GAMEMODE)
    bp_hud_class = unreal.Blueprint.get_blueprint_generated_class(load_asset(BP_HUD))
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp_gm)
    cdo = bp_class.get_default_object()
    cdo.hud_class = bp_hud_class
    save_asset(BP_GAMEMODE)

print("\n=== All Blueprints Created ===")

# ============================================================
# CREATE MAPS
# ============================================================
print("\n=== Creating Maps ===")
for map_path, name in [(MAP_MENU, "MainMenu"), (MAP_M1, "Mission_1")]:
    if not asset_exists(map_path):
        unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.Empty)
        unreal.EditorLevelLibrary.save_current_level(map_path, True)
        print(f"  Created {map_path}")
    else:
        print(f"  Exists {map_path}")

# ============================================================
# PROJECT SETTINGS
# ============================================================
print("\n=== Configuring Project Settings ===")
unreal.GameMapsSettings.set_game_default_map(MAP_MENU)
unreal.GameMapsSettings.set_editor_startup_map(MAP_MENU)
unreal.GameMapsSettings.set_global_default_game_mode(BP_GAMEMODE)
unreal.InputSettings.set_default_player_input_class(unreal.EnhancedPlayerInput)
unreal.InputSettings.set_default_input_component_class(unreal.EnhancedInputComponent)

print("\n=== ASSET CREATION COMPLETE ===")