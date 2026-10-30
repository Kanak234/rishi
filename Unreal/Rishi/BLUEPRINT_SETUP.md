# RISHI Blueprint-Only Setup Instructions

This project is configured to run with the **installed Unreal Engine 5.8.4** (no C++ compilation needed).

## 📁 Project Structure Created
```
Unreal/Rishi/
├── Rishi.uproject          # Blueprint-only (no C++ module)
├── Config/
│   ├── DefaultGame.ini     # GameMode, maps, input settings
│   ├── DefaultEngine.ini   # Renderer, collision, packaging
│   └── DefaultInput.ini    # Enhanced Input action mappings
└── Content/
    ├── Blueprints/         # Create Blueprints here (see below)
    ├── Input/              # Input Actions & Mapping Context
    ├── Maps/               # MainMenu, Mission_1...Mission_8
    └── Python/             # Setup scripts
```

## 🎮 Required Blueprints (Create in Editor)

### 1. **Input Actions** (Content/Input/)
Right-click → Input → Input Action, create:
| Name | Value Type | Description |
|------|------------|-------------|
| IA_Move | Axis2D | WASD / Left Stick |
| IA_Jump | Digital (Bool) | Space / A (Cross) |
| IA_Dodge | Digital (Bool) | Left Shift / B (Circle) |
| IA_LightAttack | Digital (Bool) | LMB / X (Square) |
| IA_HeavyAttack | Digital (Bool) | RMB / Y (Triangle) |
| IA_Interact | Digital (Bool) | F / A (Cross) |
| IA_PrevWeapon | Digital (Bool) | Q / D-Pad Left |
| IA_NextWeapon | Digital (Bool) | E / D-Pad Right |
| IA_Aim | Digital (Bool) | Left Ctrl / LT |
| IA_Pause | Digital (Bool) | Esc / Start |

### 2. **Input Mapping Context** (Content/Input/IMC_Rishi)
Right-click → Input → Input Mapping Context → Add all actions above with keys:
- **IA_Move**: W, A, S, D + Gamepad_LeftStick_2D
- **IA_Jump**: SpaceBar, Gamepad_FaceButton_Bottom
- **IA_Dodge**: LeftShift, Gamepad_FaceButton_Right
- **IA_LightAttack**: LeftMouseButton, Gamepad_FaceButton_Left
- **IA_HeavyAttack**: RightMouseButton, Gamepad_FaceButton_Top
- **IA_Interact**: F, Gamepad_FaceButton_Bottom
- **IA_PrevWeapon**: Q, Gamepad_DPad_Left
- **IA_NextWeapon**: E, Gamepad_DPad_Right
- **IA_Aim**: LeftControl, Gamepad_LeftTrigger
- **IA_Pause**: Escape, Gamepad_Special_Right

### 3. **BP_RishiPlayerCharacter** (Blueprints/Characters/)
**Parent:** Character
**Components to Add:**
- SpringArm (CameraBoom): TargetArmLength=1200, RelativeRotation=(-45,45,0), bUsePawnControlRotation=False, bDoCollisionTest=False
- Camera: Attached to CameraBoom, FieldOfView=40, bUsePawnControlRotation=False

**CharacterMovement Settings:**
- MaxWalkSpeed: 600
- RotationRate: (0,900,0)
- JumpZVelocity: 600
- AirControl: 0.6
- GravityScale: 1.0
- bOrientRotationToMovement: False
- bUseControllerRotationYaw: False

**CapsuleComponent:** Radius=40, HalfHeight=100

**Event Graph - Setup Enhanced Input:**
1. Event BeginPlay → Get Player Controller → Cast to EnhancedInputComponent → AddMappingContext(IMC_Rishi, 0)

**Event Graph - Movement:**
- IA_Move (Triggered) → Get World Delta Seconds → AddMovementInput(ControllerForward * Y + ControllerRight * X)

**Event Graph - Jump:**
- IA_Jump (Started) → Jump
- IA_Jump (Completed) → StopJumping

**Event Graph - Dodge (Simple):**
- IA_Dodge (Started) → LaunchCharacter(ControllerForward * 1300, True, True) → SetTimer(0.4s) → SetInvincible(False)

### 4. **BP_RishiGameMode** (Blueprints/)
**Parent:** GameModeBase
**Defaults:**
- DefaultPawnClass: BP_RishiPlayerCharacter
- HUDClass: BP_RishiHUD
- PlayerControllerClass: PlayerController

### 5. **BP_RishiHUD** (Blueprints/UI/)
**Parent:** UserWidget
**Designer:** Create health bar (ProgressBar), Jyoti counter (Text), Weapon indicator (Text), Boss health bar (ProgressBar - hidden by default)

### 6. **BP_MainMenuWidget** (Blueprints/UI/)
**Parent:** UserWidget
**Designer:** VerticalBox with Buttons:
- New Game → OpenLevel("Mission_1")
- Continue → LoadGame → OpenLevel(saved mission)
- Missions → Show mission list
- Settings → Open BP_SettingsWidget
- Controls → Show controls reference
- Credits → Show credits
- Quit → QuitGame

### 7. **BP_PauseMenuWidget** (Blueprints/UI/)
**Parent:** UserWidget
**Buttons:** Resume, Settings, Controls, Main Menu

### 8. **BP_SettingsWidget** (Blueprints/UI/)
**Parent:** UserWidget
**Sliders:** MasterVolume, MusicVolume, SFXVolume (0-1)
**Dropdown:** Quality (Low/Medium/High/Epic)
**Checkboxes:** Fullscreen, Subtitles, CameraShake
**Dropdown:** TextSize (Small/Normal/Large)

### 9. **Maps** (Content/Maps/)
Create empty levels:
- MainMenu
- Mission_1 through Mission_8

**MainMenu Level Setup:**
- Place BP_MainMenuWidget in level → Add to Viewport on BeginPlay
- Set GameMode to BP_RishiGameMode in World Settings

**Mission Levels:** Build using LevelBuilder logic (greybox geometry, checkpoints, encounters, shrines, boss arena)

## 🚀 Launch Commands

```bash
# Editor
/home/kanak/Documents/Codex/Applications/UnrealEngine-5.8.4/Engine/Binaries/Linux/UnrealEditor /path/to/Rishi.uproject

# Play in Editor (PIE) - MainMenu will load
```

## 📋 Controls Reference
| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD | Left Stick |
| Jump | Space | A / Cross |
| Dodge (i-frames) | Left Shift | B / Circle |
| Light Attack | LMB | X / Square |
| Heavy Attack (hold=charge) | RMB | Y / Triangle |
| Interact / Climb | F / Space | RB / A |
| Switch Weapon | Q / E | D-Pad Left/Right |
| Aim (Dhanush) | Left Ctrl (hold) | LT (hold) |
| Pause | Esc | Start |

## ⚠️ C++ Version (Full Features)
For the complete C++ implementation (custom movement state machine, combat system, enemy AI, procedural Sohrai art, save system, all 8 missions scripted):
1. Build UE5.8 from source (requires Epic Games GitHub access)
2. Restore `Rishi` module in `Rishi.uproject` `"Modules": [{"Name": "Rishi", "Type": "Runtime"}]`
3. Compile with source-built engine

## 🎯 Current Status
- ✅ Project opens in installed UE5.8.4 editor
- ✅ Config files ready (GameMode, Input, Maps)
- ✅ Folder structure created
- ⏳ Blueprints need manual creation (standard UE workflow)
- ⏳ Level greyboxing needed
- ⏳ Assets (character, enemies, VFX) needed

The C++ implementation (69 files) is complete in `Source/Rishi/` for when you build UE from source.