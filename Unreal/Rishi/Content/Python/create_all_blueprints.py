# Create ALL Blueprint assets for RISHI Linux playable build
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
# INPUT ACTIONS
# ============================================================
print("Creating Input Actions...")
input_actions = [
    (IA_MOVE, "Axis2D", "IA_Move"),
    (IA_JUMP, "Digital", "IA_Jump"),
    (IA_DODGE, "Digital", "IA_Dodge"),
    (IA_LIGHT, "Digital", "IA_LightAttack"),
    (IA_HEAVY, "Digital", "IA_HeavyAttack"),
    (IA_INTERACT, "Digital", "IA_Interact"),
    (IA_PREV, "Digital", "IA_PrevWeapon"),
    (IA_NEXT, "Digital", "IA_NextWeapon"),
    (IA_AIM, "Digital", "IA_Aim"),
    (IA_PAUSE, "Digital", "IA_Pause"),
]

for path, value_type, name in input_actions:
    if not asset_exists(path):
        factory = unreal.InputActionFactory()
        asset = unreal.AssetToolsHelpers.get_asset_tools().create_asset(name, INPUT_DIR, unreal.InputAction, factory)
        # Set value type
        if value_type == "Axis2D":
            asset.value_type = unreal.InputActionValueType.AXIS2D
        else:
            asset.value_type = unreal.InputActionValueType.DIGITAL_BOOL
        save_asset(path)
        print(f"  Created {path}")
    else:
        print(f"  Exists {path}")

# ============================================================
# INPUT MAPPING CONTEXT
# ============================================================
print("Creating Input Mapping Context...")
if not asset_exists(IMC_RISHI):
    factory = unreal.InputMappingContextFactory()
    imc = unreal.AssetToolsHelpers.get_asset_tools().create_asset("IMC_Rishi", INPUT_DIR, unreal.InputMappingContext, factory)
    
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
    
    # Keyboard mappings
    mappings = [
        (ia_move, unreal.Key("W"), 1.0, unreal.InputTriggers.pressed()),
        (ia_move, unreal.Key("S"), -1.0, unreal.InputTriggers.pressed()),
        (ia_move, unreal.Key("A"), -1.0, unreal.InputTriggers.pressed()),
        (ia_move, unreal.Key("D"), 1.0, unreal.InputTriggers.pressed()),
        (ia_jump, unreal.Key("SpaceBar"), 1.0, unreal.InputTriggers.pressed()),
        (ia_dodge, unreal.Key("LeftShift"), 1.0, unreal.InputTriggers.pressed()),
        (ia_light, unreal.Key("LeftMouseButton"), 1.0, unreal.InputTriggers.pressed()),
        (ia_heavy, unreal.Key("RightMouseButton"), 1.0, unreal.InputTriggers.pressed()),
        (ia_interact, unreal.Key("F"), 1.0, unreal.InputTriggers.pressed()),
        (ia_prev, unreal.Key("Q"), 1.0, unreal.InputTriggers.pressed()),
        (ia_next, unreal.Key("E"), 1.0, unreal.InputTriggers.pressed()),
        (ia_aim, unreal.Key("LeftControl"), 1.0, unreal.InputTriggers.pressed()),
        (ia_pause, unreal.Key("Escape"), 1.0, unreal.InputTriggers.pressed()),
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
    
    for ia, key, *args in mappings:
        mapping = unreal.InputMapping()
        mapping.input_action = ia
        mapping.key = key
        if args:
            mapping.value = args[0]
        if len(args) > 1:
            mapping.triggers = args[1]
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
    print(f"  Exists {IMC_RISHI}")

# ============================================================
# CREATE BASIC BLUEPRINT FUNCTION
# ============================================================
def create_bp(asset_path, parent_class, name):
    if asset_exists(asset_path):
        print(f"  Exists {asset_path}")
        return load_asset(asset_path)
    
    factory = unreal.BlueprintFactory()
    bp = unreal.AssetToolsHelpers.get_asset_tools().create_asset(name, os.path.dirname(asset_path), unreal.Blueprint, factory)
    bp.parent_class = parent_class
    save_asset(asset_path)
    print(f"  Created {asset_path}")
    return bp

# ============================================================
# CREATE ALL BLUEPRINTS
# ============================================================
print("\n=== Creating Blueprints ===")

# 1. Player Character
bp_char = create_bp(BP_CHAR, unreal.Character, "BP_RishiPlayerCharacter")
if bp_char:
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp_char)
    cdo = bp_class.get_default_object()
    
    # Movement
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
    cdo.hud_class = unreal.UserWidget  # Will set after HUD created
    cdo.player_controller_class = unreal.PlayerController
    save_asset(BP_GAMEMODE)

# 3. HUD Widget
bp_hud = create_bp(BP_HUD, unreal.UserWidget, "BP_RishiHUD")

# 4. MainMenu Widget
bp_menu = create_bp(BP_MAINMENU, unreal.UserWidget, "BP_MainMenuWidget")

# 5. Pause Menu Widget
bp_pause = create_bp(BP_PAUSE, unreal.UserWidget, "BP_PauseMenuWidget")

# 6. Settings Widget
bp_settings = create_bp(BP_SETTINGS, unreal.UserWidget, "BP_SettingsWidget")

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

# 8. Checkpoint
bp_cp = create_bp(BP_CHECKPOINT, unreal.Actor, "BP_Checkpoint")

# 9. Shrine
bp_shrine = create_bp(BP_SHRINE, unreal.Actor, "BP_ObjectiveShrine")

# 10. KillZone
bp_kz = create_bp(BP_KILLZONE, unreal.Actor, "BP_KillZone")

# 11. Encounter Zone
bp_enc = create_bp(BP_ENCOUNTER, unreal.Actor, "BP_EncounterZone")

# Update GameMode with HUD
if bp_gm and bp_hud:
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp_gm)
    cdo = bp_class.get_default_object()
    cdo.hud_class = unreal.Blueprint.get_blueprint_generated_class(bp_hud)
    save_asset(BP_GAMEMODE)

print("\n=== All Blueprints Created ===")

# ============================================================
# CREATE MAPS
# ============================================================
print("\n=== Creating Maps ===")

# MainMenu
if not asset_exists(MAP_MENU):
    unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.Empty)
    unreal.EditorLevelLibrary.save_current_level(MAP_MENU, True)
    print(f"  Created {MAP_MENU}")
else:
    print(f"  Exists {MAP_MENU}")

# Mission 1
if not asset_exists(MAP_M1):
    unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.Empty)
    unreal.EditorLevelLibrary.save_current_level(MAP_M1, True)
    print(f"  Created {MAP_M1}")
else:
    print(f"  Exists {MAP_M1}")

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
print(f"Maps: {MAP_MENU}, {MAP_M1}")
print(f"Blueprints: {BP_CHAR}, {BP_GAMEMODE}, {BP_HUD}, {BP_MAINMENU}, {BP_PAUSE}, {BP_SETTINGS}")
print(f"Enemy: {BP_ENEMY_CHAYABHOOT}")
print(f"World: {BP_CHECKPOINT}, {BP_SHRINE}, {BP_KILLZONE}, {BP_ENCOUNTER}")
print(f"Input: {IMC_RISHI} + 10 IAs")