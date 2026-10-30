// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

using UnrealBuildTool;

public class RishiEditorTarget : TargetRules
{
	public RishiEditorTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V5;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("Rishi");
	}
}