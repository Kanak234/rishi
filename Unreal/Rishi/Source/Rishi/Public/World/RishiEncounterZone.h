// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiEncounterZone.generated.h"

class ARishiEnemyAI;

USTRUCT(BlueprintType)
struct FEnemyWave
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Encounter")
	TArray<TObjectPtr<ARishiEnemyAI>> Enemies;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEncounterStarted, class ARishiEncounterZone*, Zone);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEncounterCleared, class ARishiEncounterZone*, Zone);

UCLASS()
class RISHI_API ARishiEncounterZone : public AActor
{
	GENERATED_BODY()

public:
	ARishiEncounterZone();

	UFUNCTION(BlueprintPure, Category = "Encounter")
	int32 GetRemainingEnemies() const;

	UFUNCTION(BlueprintPure, Category = "Encounter")
	int32 GetCurrentWaveIndex() const { return CurrentWaveIndex; }

	UFUNCTION(BlueprintPure, Category = "Encounter")
	int32 GetTotalWaves() const { return Waves.Num(); }

	UPROPERTY(BlueprintAssignable, Category = "Encounter")
	FOnEncounterStarted OnStarted;

	UPROPERTY(BlueprintAssignable, Category = "Encounter")
	FOnEncounterCleared OnCleared;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Encounter")
	FString Title = TEXT("Asur jhund");

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Encounter")
	TArray<FEnemyWave> Waves;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Encounter")
	TArray<TObjectPtr<AActor>> Gates;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Encounter")
	bool bIsBoss = false;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<UBoxComponent> TriggerVolume;

	bool bStarted = false;
	bool bCleared = false;
	int32 CurrentWaveIndex = -1;
	TArray<TObjectPtr<URishiHealthComponent>> CurrentWaveEnemies;

	void OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
	void BeginEncounter();
	void StartNextWave();
	void CheckWaveComplete();
	void SetGatesActive(bool bActive);
};