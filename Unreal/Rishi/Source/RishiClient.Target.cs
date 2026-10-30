// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

using UnrealBuildTool;

public class RishiClientTarget : TargetRules
{
	public RishiClientTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Client;
		DefaultBuildSettings = BuildSettingsVersion.V5;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("Rishi");
	}
}