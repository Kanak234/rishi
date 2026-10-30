# Create Blueprint assets for RISHI Blueprint-only build
import unreal

# Asset paths
BP_CHAR_PATH = "/Game/Blueprints/Characters/BP_RishiPlayerCharacter"
BP_GAMEMODE_PATH = "/Game/Blueprints/BP_RishiGameMode"
BP_HUD_PATH = "/Game/Blueprints/UI/BP_RishiHUD"
BP_MAINMENU_PATH = "/Game/Blueprints/UI/BP_MainMenuWidget"
BP_PAUSEMENU_PATH = "/Game/Blueprints/UI/BP_PauseMenuWidget"
BP_SETTINGS_PATH = "/Game/Blueprints/UI/BP_SettingsWidget"

def create_basic_blueprint(asset_path, parent_class, asset_name):
    """Create a basic Blueprint asset"""
    if unreal.EditorAssetLibrary.does_asset_exist(asset_path):
        print(f"Exists {asset_path}")
        return None
    
    bp = unreal.AssetToolsHelpers.get_asset_tools().create_asset(
        asset_name, 
        "/".join(asset_path.split("/")[:-1]), 
        unreal.Blueprint, 
        unreal.BlueprintFactory()
    )
    bp.parent_class = parent_class
    unreal.EditorAssetLibrary.save_asset(asset_path)
    print(f"Created {asset_path}")
    return bp

def create_player_character_bp():
    """Create BP_RishiPlayerCharacter Blueprint"""
    bp = create_basic_blueprint(BP_CHAR_PATH, unreal.Character, "BP_RishiPlayerCharacter")
    if not bp:
        return
    
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp)
    cdo = bp_class.get_default_object()
    
    # Configure CharacterMovement
    cdo.get_character_movement().max_walk_speed = 600.0
    cdo.get_character_movement().rotation_rate = unreal.Rotator(0, 900, 0)
    cdo.get_character_movement().jump_z_velocity = 600.0
    cdo.get_character_movement().air_control = 0.6
    cdo.get_character_movement().gravity_scale = 1.0
    cdo.get_character_movement().b_orient_rotation_to_movement = False
    cdo.get_character_movement().b_use_controller_rotation_yaw = False
    
    # Capsule
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
    
    unreal.EditorAssetLibrary.save_asset(BP_CHAR_PATH)

def create_gamemode_bp():
    """Create BP_RishiGameMode Blueprint"""
    bp = create_basic_blueprint(BP_GAMEMODE_PATH, unreal.GameModeBase, "BP_RishiGameMode")
    if not bp:
        return
    
    bp_class = unreal.Blueprint.get_blueprint_generated_class(bp)
    cdo = bp_class.get_default_object()
    
    # Set default classes
    pawn_class = unreal.EditorAssetLibrary.load_asset(BP_CHAR_PATH)
    hud_class = unreal.EditorAssetLibrary.load_asset(BP_HUD_PATH)
    
    if pawn_class:
        cdo.default_pawn_class = pawn_class
    if hud_class:
        cdo.hud_class = hud_class
    cdo.player_controller_class = unreal.PlayerController
    
    unreal.EditorAssetLibrary.save_asset(BP_GAMEMODE_PATH)

def create_hud_widget():
    """Create BP_RishiHUD UserWidget"""
    create_basic_blueprint(BP_HUD_PATH, unreal.UserWidget, "BP_RishiHUD")

def create_mainmenu_widget():
    """Create BP_MainMenuWidget UserWidget"""
    create_basic_blueprint(BP_MAINMENU_PATH, unreal.UserWidget, "BP_MainMenuWidget")

def create_pausemenu_widget():
    """Create BP_PauseMenuWidget UserWidget"""
    create_basic_blueprint(BP_PAUSEMENU_PATH, unreal.UserWidget, "BP_PauseMenuWidget")

def create_settings_widget():
    """Create BP_SettingsWidget UserWidget"""
    create_basic_blueprint(BP_SETTINGS_PATH, unreal.UserWidget, "BP_SettingsWidget")

def create_mainmenu_map():
    """Create MainMenu map"""
    map_path = "/Game/Maps/MainMenu"
    if unreal.EditorAssetLibrary.does_asset_exist(map_path):
        print(f"Exists {map_path}")
        return
    
    unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.Empty)
    unreal.EditorLevelLibrary.save_current_level(map_path, True)
    print(f"Created {map_path}")

def create_mission1_map():
    """Create Mission_1 map"""
    map_path = "/Game/Maps/Mission_1"
    if unreal.EditorAssetLibrary.does_asset_exist(map_path):
        print(f"Exists {map_path}")
        return
    
    unreal.EditorLevelLibrary.new_level(unreal.NewLevelEditorLevelType.Empty)
    unreal.EditorLevelLibrary.save_current_level(map_path, True)
    print(f"Created {map_path}")

def setup_project_settings():
    """Configure project settings"""
    unreal.GameMapsSettings.set_game_default_map("/Game/Maps/MainMenu")
    unreal.GameMapsSettings.set_editor_startup_map("/Game/Maps/MainMenu")
    unreal.GameMapsSettings.set_global_default_game_mode("/Game/Blueprints/BP_RishiGameMode")
    
    unreal.InputSettings.set_default_player_input_class(unreal.EnhancedPlayerInput)
    unreal.InputSettings.set_default_input_component_class(unreal.EnhancedInputComponent)
    
    print("Project settings configured")

# Run all creation functions
print("=== Creating RISHI Blueprint Assets ===")
create_player_character_bp()
create_hud_widget()
create_mainmenu_widget()
create_pausemenu_widget()
create_settings_widget()
create_gamemode_bp()
create_mainmenu_map()
create_mission1_map()
setup_project_settings()
print("=== Blueprint asset creation complete ===")