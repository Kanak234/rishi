// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Enemies/RishiEnemyAI.h"
#include "Combat/RishiHitFilterInterface.h"
#include "RishiBossAndhvir.generated.h"

UENUM(BlueprintType)
enum class EBossMove : uint8
{
	Prahar,
	Chakkar,
	BhoomiBhed
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnBossPhaseChanged, int32, NewPhase);

UCLASS()
class RISHI_API ARishiBossAndhvir : public ARishiEnemyAI, public IRishiHitFilterInterface
{
	GENERATED_BODY()

public:
	ARishiBossAndhvir();

	// IRishiHitFilterInterface
	virtual bool FilterHit_Implementation(FHitInfo& HitInfo) override;

	UFUNCTION(BlueprintPure, Category = "Boss")
	int32 GetPhase() const { return Phase; }

	UFUNCTION(BlueprintPure, Category = "Boss")
	bool IsInOpening() const { return bInOpening; }

	UPROPERTY(BlueprintAssignable, Category = "Boss")
	FOnBossPhaseChanged OnPhaseChanged;

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;
	virtual void ChooseNextMove();
	virtual void PerformAttack() override;
	virtual float GetStunDuration(const FHitInfo& HitInfo) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	FString DisplayName = TEXT("Andhvir");

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	FString Subtitle = TEXT("Tamasur ka senapati");

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	float Phase2HealthThreshold = 0.66f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	float Phase3HealthThreshold = 0.30f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	float SpinRadius = 340.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	float SlamRadius = 550.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	float OpeningHeavyMultiplier = 2.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	float OpeningLightMultiplier = 1.25f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	TSubclassOf<class ARishiEnemyAI> MinionClass;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	TArray<TObjectPtr<AActor>> MinionSpawnPoints;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	int32 MaxMinions = 3;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
	TObjectPtr<UStaticMeshComponent> TelegraphDisc;

private:
	int32 Phase = 1;
	int32 AttackCount = 0;
	float BaseMoveSpeed = 0.0f;
	float BaseWindupScale = 1.0f;
	EBossMove CurrentMove = EBossMove::Prahar;
	bool bInOpening = false;
	TArray<TObjectPtr<AActor>> ActiveMinions;

	void UpdatePhase();
	void UpdateTelegraph(float DeltaTime);
	void SummonMinions();
	void DoAreaHit(const FVector& Center, float Radius, float Knockback);
};