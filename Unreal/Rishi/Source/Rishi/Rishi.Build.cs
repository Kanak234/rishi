using UnrealBuildTool;

public class Rishi : ModuleRules
{
	public Rishi(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
	
		PublicDependencyModuleNames.AddRange(new string[] { 
			"Core", 
			"CoreUObject", 
			"Engine", 
			"InputCore", 
			"EnhancedInput", 
			"NavigationSystem", 
			"AIModule", 
			"GameplayTasks",
			"UMG",
			"Slate",
			"SlateCore"
		});

		PrivateDependencyModuleNames.AddRange(new string[] { 
			"GameplayTags"
		});

		PublicIncludePaths.AddRange(new string[] {
			ModuleDirectory + "/Public",
			ModuleDirectory + "/Public/Core",
			ModuleDirectory + "/Public/Player",
			ModuleDirectory + "/Public/Combat",
			ModuleDirectory + "/Public/Enemies",
			ModuleDirectory + "/Public/World",
			ModuleDirectory + "/Public/UI"
		});
	}
}