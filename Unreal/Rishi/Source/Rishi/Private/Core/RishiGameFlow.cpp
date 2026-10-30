// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiGameFlow.h"
#include "Core/RishiSaveSystem.h"
#include "Kismet/GameplayStatics.h"

URishiGameFlow* URishiGameFlow::GameFlowInstance = nullptr;

URishiGameFlow::URishiGameFlow()
{
	GameFlowInstance = this;
}

URishiGameFlow* URishiGameFlow::Get()
{
	if (!GameFlowInstance)
	{
		GameFlowInstance = NewObject<URishiGameFlow>(GetTransientPackage(), URishiGameFlow::StaticClass(), NAME_None, RF_Transient | RF_StrongRefOnFrame);
	}
	return GameFlowInstance;
}

void URishiGameFlow::NewGame()
{
	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	SaveSystem->NewGame();
	
	FString SceneName = GetMissionSceneName(0);
	UGameplayStatics::OpenLevel(GetWorld(), FName(*SceneName));
}

void URishiGameFlow::ContinueGame()
{
	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	SaveSystem->LoadGame();
	
	int32 MissionIndex = FMath::Clamp(SaveSystem->GetCurrentSave().Mission, 0, 7);
	FString SceneName = GetMissionSceneName(MissionIndex);
	UGameplayStatics::OpenLevel(GetWorld(), FName(*SceneName));
}

void URishiGameFlow::StartMission(int32 MissionIndex)
{
	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	SaveSystem->LoadGame();
	
	FSaveData& Save = SaveSystem->GetMutableSave();
	Save.Mission = MissionIndex;
	Save.Checkpoint = -1;
	SaveSystem->SaveGame();
	
	FString SceneName = GetMissionSceneName(MissionIndex);
	UGameplayStatics::OpenLevel(GetWorld(), FName(*SceneName));
}

void URishiGameFlow::CompleteMission(int32 MissionIndex)
{
	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	FSaveData& Save = SaveSystem->GetMutableSave();
	
	int32 NextMission = MissionIndex + 1;
	Save.Unlocked = FMath::Max(Save.Unlocked, FMath::Min(NextMission, 7));
	Save.Checkpoint = -1;
	
	if (NextMission >= 8)
	{
		Save.bFinished = true;
		Save.Mission = 7;
		SaveSystem->SaveGame();
		UGameplayStatics::OpenLevel(GetWorld(), FName(*GetMenuSceneName()));
		return;
	}
	
	Save.Mission = NextMission;
	SaveSystem->SaveGame();
	
	FString SceneName = GetMissionSceneName(NextMission);
	UGameplayStatics::OpenLevel(GetWorld(), FName(*SceneName));
}

void URishiGameFlow::Retry()
{
	ContinueGame();
}

void URishiGameFlow::ReturnToMenu()
{
	UGameplayStatics::OpenLevel(GetWorld(), FName(*GetMenuSceneName()));
}

FString URishiGameFlow::GetMissionSceneName(int32 MissionIndex) const
{
	return FString::Printf(TEXT("Mission_%d"), MissionIndex + 1);
}