// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiGamePause.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnPauseChanged, bool, bPaused);

UCLASS(BlueprintType)
class RISHI_API ARishiGamePause : public AActor
{
	GENERATED_BODY()

public:
	ARishiGamePause();

	static ARishiGamePause* Get();

	UFUNCTION(BlueprintCallable, Category = "GamePause")
	void SetPaused(bool bPaused);

	UFUNCTION(BlueprintPure, Category = "GamePause")
	bool IsPaused() const { return bIsPaused; }

	UFUNCTION(BlueprintCallable, Category = "GamePause")
	void SetLocked(bool bLocked) { bIsLocked = bLocked; }

	UFUNCTION(BlueprintPure, Category = "GamePause")
	bool IsLocked() const { return bIsLocked; }

	UPROPERTY(BlueprintAssignable, Category = "GamePause")
	FOnPauseChanged OnPauseChanged;

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

private:
	UPROPERTY()
	bool bIsPaused = false;

	UPROPERTY()
	bool bIsLocked = false;

	FInputActionBinding PauseActionBinding;

	void OnPauseActionPressed();
	void OnApplicationFocusChanged(bool bHasFocus);
};