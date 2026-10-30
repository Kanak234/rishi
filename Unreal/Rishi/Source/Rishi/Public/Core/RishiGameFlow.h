// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "RishiGameTypes.h"
#include "RishiGameFlow.generated.h"

UCLASS(BlueprintType)
class RISHI_API URishiGameFlow : public UObject
{
	GENERATED_BODY()

public:
	static URishiGameFlow* Get();

	UFUNCTION(BlueprintCallable, Category = "GameFlow")
	void NewGame();

	UFUNCTION(BlueprintCallable, Category = "GameFlow")
	void ContinueGame();

	UFUNCTION(BlueprintCallable, Category = "GameFlow")
	void StartMission(int32 MissionIndex);

	UFUNCTION(BlueprintCallable, Category = "GameFlow")
	void CompleteMission(int32 MissionIndex);

	UFUNCTION(BlueprintCallable, Category = "GameFlow")
	void Retry();

	UFUNCTION(BlueprintCallable, Category = "GameFlow")
	void ReturnToMenu();

	UFUNCTION(BlueprintPure, Category = "GameFlow")
	FString GetMissionSceneName(int32 MissionIndex) const;

	UFUNCTION(BlueprintPure, Category = "GameFlow")
	FString GetMenuSceneName() const { return TEXT("MainMenu"); }

private:
	URishiGameFlow();
};