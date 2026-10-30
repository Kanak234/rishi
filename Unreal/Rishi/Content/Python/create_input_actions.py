import unreal

# Create Input Actions
print("=== Creating Input Actions ===")

input_actions = [
    ('/Game/Input/IA_Move', 'IA_Move', unreal.InputActionValueType.AXIS2D),
    ('/Game/Input/IA_Jump', 'IA_Jump', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_Dodge', 'IA_Dodge', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_LightAttack', 'IA_LightAttack', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_HeavyAttack', 'IA_HeavyAttack', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_Interact', 'IA_Interact', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_PrevWeapon', 'IA_PrevWeapon', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_NextWeapon', 'IA_NextWeapon', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_Aim', 'IA_Aim', unreal.InputActionValueType.DIGITAL_BOOL),
    ('/Game/Input/IA_Pause', 'IA_Pause', unreal.InputActionValueType.DIGITAL_BOOL),
]

for path, name, value_type in input_actions:
    if not unreal.EditorAssetLibrary.does_asset_exist(path):
        try:
            asset = unreal.AssetToolsHelpers.get_asset_tools().create_asset(name, '/Game/Input', unreal.InputAction, None)
            if asset:
                asset.value_type = value_type
                unreal.EditorAssetLibrary.save_asset(path)
                print(f'Created: {path}')
            else:
                print(f'FAILED: {path}')
        except Exception as e:
            print(f'ERROR {path}: {e}')
    else:
        print(f'Exists: {path}')

print('Input Actions done')