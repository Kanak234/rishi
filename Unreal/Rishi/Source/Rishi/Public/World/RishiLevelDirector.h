// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiGameTypes.h"
#include "RishiLevelDirector.generated.h"

class ARishiPlayerCharacter;
class ARishiCheckpoint;
class ARishiBossAndhvir;
class ARishiEncounterZone;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(FOnMissionBanner, const FString&, Level, const FString&, Mission, const FString&, Goal);

UCLASS()
class RISHI_API ARishiLevelDirector : public AActor
{
	GENERATED_BODY()

public:
	ARishiLevelDirector();

	static ARishiLevelDirector* Get();

	UFUNCTION(BlueprintCallable, Category = "LevelDirector")
	void BeginBoss(ARishiEncounterZone* Zone, TFunction<void()> OnBossBegin);

	UFUNCTION(BlueprintCallable, Category = "LevelDirector")
	void CompleteMission(float Delay = 0.3f);

	UFUNCTION(BlueprintPure, Category = "LevelDirector")
	int32 GetMissionIndex() const { return MissionIndex; }

	UFUNCTION(BlueprintPure, Category = "LevelDirector")
	const FMissionDef& GetMission() const;

	UFUNCTION(BlueprintPure, Category = "LevelDirector")
	int32 GetJyoti() const;

	UFUNCTION(BlueprintPure, Category = "LevelDirector")
	bool IsGameOver() const { return bGameOver; }

	UFUNCTION(BlueprintPure, Category = "LevelDirector")
	bool IsVictory() const { return bVictory; }

	UFUNCTION(BlueprintPure, Category = "LevelDirector")
	bool IsBossActive() const { return bBossActive; }

	UPROPERTY(BlueprintAssignable, Category = "LevelDirector")
	FOnMissionBanner OnMissionBanner;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "LevelDirector")
	int32 MissionIndex = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "LevelDirector")
	TObjectPtr<ARishiPlayerCharacter> Player;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "LevelDirector")
	TArray<TObjectPtr<ARishiCheckpoint>> Checkpoints;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "LevelDirector")
	TObjectPtr<ARishiBossAndhvir> Boss;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "LevelDirector")
	float BossCameraDistance = 1900.0f;

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

private:
	UPROPERTY()
	TObjectPtr<URishiHealthComponent> PlayerHealth;

	UPROPERTY()
	TObjectPtr<URishiWeaponManagerComponent> WeaponManager;

	UPROPERTY()
	TObjectPtr<URishiCombatSystemComponent> MeleeCombat;

	UPROPERTY()
	TObjectPtr<URishiBowCombatComponent> BowCombat;

	UPROPERTY()
	TObjectPtr<URishiInputComponent> InputComponent;

	bool bBossActive = false;
	bool bGameOver = false;
	bool bVictory = false;
	bool bEndScreen = false;
	float DeadTime = -1.0f;
	float RunTime = 0.0f;

	// Game over menu
	TArray<FString> OverMenuItems = { TEXT("Deepak se dobara"), TEXT("Main menu") };
	int32 OverMenuSelection = 0;

	void OnEnemyDied(ARishiEnemyAI* Enemy);
	void OnBossDied(ARishiEnemyAI* Enemy);
	void OnPlayerDied();
	void OnPlayerDamaged(float Damage);
	void OnAttackStarted(EAttackKind Kind, int32 Step);
	void OnHitLanded(URishiHurtboxComponent* Hurtbox, const FHitInfo& HitInfo);
	void OnBowFired(bool bCharged);

	void PlayIntroCutscene();
	void PlayBossIntro(TFunction<void()> OnDone);
	void PlayEndingCutscene();

	void ShowGameOverMenu();
	void HandleGameOverInput();
	void ChooseGameOverOption(int32 Index);

	static TArray<FMissionDef> MissionDefs;
};