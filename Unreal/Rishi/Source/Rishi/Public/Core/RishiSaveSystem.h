// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "RishiGameTypes.h"
#include "RishiSaveSystem.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnSaveCompleted);

UCLASS(BlueprintType)
class RISHI_API URishiSaveSystem : public UObject
{
	GENERATED_BODY()

public:
	static URishiSaveSystem* Get();

	UFUNCTION(BlueprintCallable, Category = "SaveSystem")
	void NewGame();

	UFUNCTION(BlueprintCallable, Category = "SaveSystem")
	bool LoadGame();

	UFUNCTION(BlueprintCallable, Category = "SaveSystem")
	void SaveGame();

	UFUNCTION(BlueprintCallable, Category = "SaveSystem")
	void DeleteSave();

	UFUNCTION(BlueprintPure, Category = "SaveSystem")
	bool HasSave() const;

	UFUNCTION(BlueprintPure, Category = "SaveSystem")
	const FSaveData& GetCurrentSave() const { return CurrentSave; }

	UFUNCTION(BlueprintPure, Category = "SaveSystem")
	FSaveData& GetMutableSave() { return CurrentSave; }

	UPROPERTY(BlueprintAssignable, Category = "SaveSystem")
	FOnSaveCompleted OnSaveCompleted;

private:
	FSaveData CurrentSave;
	FString SaveFilePath;

	URishiSaveSystem();
};