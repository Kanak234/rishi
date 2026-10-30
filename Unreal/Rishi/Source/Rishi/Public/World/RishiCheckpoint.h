// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiCheckpoint.generated.h"

class URishiHealthComponent;
class URishiWeaponManagerComponent;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnCheckpointMessage, const FString&, Message);

UCLASS()
class RISHI_API ARishiCheckpoint : public AActor
{
	GENERATED_BODY()

public:
	ARishiCheckpoint();

	UFUNCTION(BlueprintCallable, Category = "Checkpoint")
	void LightSilently();

	UFUNCTION(BlueprintPure, Category = "Checkpoint")
	int32 GetIndex() const { return CheckpointIndex; }

	UPROPERTY(BlueprintAssignable, Category = "Checkpoint")
	FOnCheckpointMessage OnMessage;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Checkpoint")
	int32 CheckpointIndex = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Checkpoint")
	TObjectPtr<class AActor> LitVisual;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Checkpoint")
	TArray<int32> UpgradeCosts;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<UBoxComponent> TriggerVolume;

	bool bIsLit = false;

	void OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
};