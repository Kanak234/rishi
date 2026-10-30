// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

using UnrealBuildTool;

public class RishiTarget : TargetRules
{
	public RishiTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;
		DefaultBuildSettings = BuildSettingsVersion.V5;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("Rishi");
	}
}