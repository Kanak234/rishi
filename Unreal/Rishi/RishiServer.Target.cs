// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

using UnrealBuildTool;

public class RishiServerTarget : TargetRules
{
	public RishiServerTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Server;
		DefaultBuildSettings = BuildSettingsVersion.V5;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("Rishi");
	}
}