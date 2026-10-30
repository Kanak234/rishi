// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "RishiGameTypes.h"
#include "RishiEnemyAI.generated.h"

class URishiHealthComponent;
class URishiHurtboxComponent;
class UMaterialInstanceDynamic;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnEnemyDied, class ARishiEnemyAI*, Enemy);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnEnemyStateChanged, EEnemyState, PrevState, EEnemyState, NewState);

UCLASS()
class RISHI_API ARishiEnemyAI : public ACharacter
{
	GENERATED_BODY()

public:
	ARishiEnemyAI();

	UFUNCTION(BlueprintCallable, Category = "Enemy")
	virtual void TakeHit(const FHitInfo& HitInfo);

	UFUNCTION(BlueprintPure, Category = "Enemy")
	EEnemyState GetState() const { return State; }

	UFUNCTION(BlueprintPure, Category = "Enemy")
	int32 GetJyotiReward() const { return JyotiReward; }

	UPROPERTY(BlueprintAssignable, Category = "Enemy")
	FOnEnemyDied OnDied;

	UPROPERTY(BlueprintAssignable, Category = "Enemy")
	FOnEnemyStateChanged OnStateChanged;

	// Detection
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Detection")
	float DetectRange = 1200.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Detection")
	float LoseRange = 2000.0f;

	// Movement
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Movement")
	float MoveSpeed = 400.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Movement")
	float TurnSpeed = 540.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Movement")
	float Gravity = -2500.0f;

	// Attack
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float AttackRange = 160.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float WindupTime = 0.4f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float ActiveTime = 0.15f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float RecoveryTime = 0.5f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float AttackCooldown = 1.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float Damage = 8.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float HitRadius = 90.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Attack")
	float HitReach = 100.0f;

	// Hurt
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Hurt")
	float HurtStunTime = 0.35f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Hurt")
	float KnockbackDecay = 8.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Hurt")
	bool bSuperArmor = false;

	// Visual
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Visual")
	TObjectPtr<USkeletalMeshComponent> BodyMesh;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Visual")
	FLinearColor TelegraphColor = FLinearColor(1.0f, 0.55f, 0.1f);

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Visual")
	FLinearColor HurtColor = FLinearColor::White;

	// Reward
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Enemy|Reward")
	int32 JyotiReward = 1;

	// Static limits
	static int32 MaxSimultaneousAttackers;
	static int32 ActiveEnemyCount;

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;
	virtual void Tick(float DeltaTime) override;

	virtual void UpdateIdle();
	virtual void UpdateChase(float DeltaTime);
	virtual void UpdateAttack(float DeltaTime);
	virtual void UpdateHurt(float DeltaTime);
	virtual void UpdateDead(float DeltaTime);

	virtual FVector GetChaseVelocity(const FVector& ToTarget, float Distance);
	virtual void PerformAttack();
	virtual float GetStunDuration(const FHitInfo& HitInfo) { return HurtStunTime; }

	void SetState(EEnemyState NewState);
	bool TryTakeAttackToken();
	void ReleaseAttackToken();
	void FaceTowards(const FVector& Dir, float DeltaTime);
	float GetFlatDistance(const FVector& TargetPos) const;
	bool HasGroundAhead(const FVector& Dir) const;
	void UpdateVisuals(float DeltaTime);
	void ApplyKnockback(const FVector& Direction, float Strength);

	// Attack token system
	bool bHasAttackToken = false;
	static int32 CurrentAttackers;

	// State
	UPROPERTY()
	EEnemyState State = EEnemyState::Idle;
	float StateTimer = 0.0f;
	FVector DesiredVelocity = FVector::ZeroVector;
	FVector KnockbackVelocity = FVector::ZeroVector;

	// Target
	UPROPERTY()
	TObjectPtr<AActor> TargetActor;

	UPROPERTY()
	TObjectPtr<URishiHealthComponent> TargetHealth;

	// Attack timing
	float NextAttackTime = 0.0f;
	bool bHitDone = false;
	float StunTimer = 0.0f;
	float FlashTimer = 0.0f;

	// Components
	UPROPERTY()
	TObjectPtr<URishiHealthComponent> HealthComponent;

	UPROPERTY()
	TObjectPtr<URishiHurtboxComponent> HurtboxComponent;

	UPROPERTY()
	TObjectPtr<UMaterialInstanceDynamic> DynamicMaterial;

	// Visual params
	FLinearColor BaseColor = FLinearColor::Gray;
};