// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RishiGameTypes.h"
#include "RishiCombatSystemComponent.generated.h"

class ARishiPlayerCharacter;
class URishiInputComponent;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnAttackStarted, EAttackKind, Kind, int32, ComboStep);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnHitLanded, URishiHurtboxComponent*, Hurtbox, const FHitInfo&, HitInfo);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnAttackEnded);

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class RISHI_API URishiCombatSystemComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	URishiCombatSystemComponent();

	UFUNCTION(BlueprintCallable, Category = "Combat")
	void UpdateCombat(float DeltaTime);

	UFUNCTION(BlueprintPure, Category = "Combat")
	EAttackKind GetCurrentKind() const { return CurrentKind; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	int32 GetComboStep() const { return ComboIndex + 1; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	bool IsHitboxActive() const { return bHitboxActive; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	bool IsBusy() const { return CurrentKind != EAttackKind::None || bCharging; }

	UFUNCTION(BlueprintCallable, Category = "Combat")
	void CancelAttack();

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	TArray<FAttackData> LightCombo;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float ComboBufferWindow = 0.25f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float ComboResetTime = 0.35f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	FAttackData HeavyAttack;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float HeavyMaxDamage = 45.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float HeavyMinCharge = 0.25f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float HeavyMaxCharge = 1.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float ChargeMoveScale = 0.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	TEnumAsByte<ECollisionChannel> HurtboxCollisionChannel = ECC_GameTraceChannel1;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	float DamageMultiplier = 1.0f;

	UPROPERTY(BlueprintAssignable, Category = "Combat")
	FOnAttackStarted OnAttackStarted;

	UPROPERTY(BlueprintAssignable, Category = "Combat")
	FOnHitLanded OnHitLanded;

	UPROPERTY(BlueprintAssignable, Category = "Combat")
	FOnAttackEnded OnAttackEnded;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<ARishiPlayerCharacter> PlayerCharacter;

	UPROPERTY()
	TObjectPtr<URishiInputComponent> InputComponent;

	UPROPERTY()
	TObjectPtr<URishiHealthComponent> HealthComponent;

	EAttackKind CurrentKind = EAttackKind::None;
	FAttackData CurrentAttack;
	int32 ComboIndex = -1;
	float Timer = 0.0f;
	float SinceAttackEnd = 999.0f;
	bool bBufferedLight = false;
	bool bCharging = false;
	float ChargeTimer = 0.0f;
	float HeavyDamage = 0.0f;
	bool bHitboxActive = false;
	TArray<TObjectPtr<URishiHurtboxComponent>> AlreadyHit;
	FAttackData LastAttack;

	void StartLight(int32 Index);
	void StartCharge();
	void UpdateCharge(float DeltaTime);
	void BeginAttack(EAttackKind Kind);
	void FinishAttack();
	void DoHitboxCheck();
	FVector GetHitboxCenter() const;
	bool WasAlreadyHit(URishiHurtboxComponent* Hurtbox) const;
	void OnMoveStateChanged(ERishiMoveState PrevState, ERishiMoveState NewState);
};