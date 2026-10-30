// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RishiGameTypes.h"
#include "RishiBowCombatComponent.generated.h"

class ARishiPlayerCharacter;
class URishiInputComponent;
class ARishiArrow;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnArrowFired, bool, bCharged);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnAimChanged, bool, bAiming);

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class RISHI_API URishiBowCombatComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	URishiBowCombatComponent();

	UFUNCTION(BlueprintCallable, Category = "Combat")
	void UpdateBow(float DeltaTime);

	UFUNCTION(BlueprintCallable, Category = "Combat")
	void Shoot(float Damage, float Knockback, bool bCharged);

	UFUNCTION(BlueprintPure, Category = "Combat")
	bool IsAiming() const { return bIsAiming; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	bool IsCharging() const { return bCharging; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	float GetChargeProgress() const { return bCharging ? FMath::Clamp(ChargeTimer / ChargeTime, 0.0f, 1.0f) : 0.0f; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	bool IsBusy() const { return bIsAiming || bCharging; }

	// Arrow settings
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float QuickDamage = 12.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float ChargedMinDamage = 16.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float ChargedMaxDamage = 32.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float ChargeTime = 0.8f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float FireCooldown = 0.35f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float ArrowSpeed = 3000.0f; // cm/s

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float ArrowLifetime = 1.2f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float ChargedKnockback = 400.0f;

	// Aim settings
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float AimMoveScale = 0.4f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float AutoAimRange = 1600.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float AutoAimAngle = 35.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float VerticalAssistAngle = 12.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float MuzzleHeight = 140.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	float MuzzleForward = 60.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	TSubclassOf<ARishiArrow> ArrowClass;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bow")
	TEnumAsByte<ECollisionChannel> HitCollisionChannel = ECC_GameTraceChannel1;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	float DamageMultiplier = 1.0f;

	UPROPERTY(BlueprintAssignable, Category = "Combat")
	FOnArrowFired OnArrowFired;

	UPROPERTY(BlueprintAssignable, Category = "Combat")
	FOnAimChanged OnAimChanged;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<ARishiPlayerCharacter> PlayerCharacter;

	UPROPERTY()
	TObjectPtr<URishiInputComponent> InputComponent;

	UPROPERTY()
	TObjectPtr<URishiHealthComponent> HealthComponent;

	bool bIsAiming = false;
	bool bCharging = false;
	float ChargeTimer = 0.0f;
	float NextFireTime = 0.0f;
	FVector AimDirection = FVector::ZeroVector;

	void SetAiming(bool bAim);
	void UpdateAimDirection();
	FVector GetAutoAimDirection();
	FVector ApplyVerticalAssist(const FVector& FlatDir, const FVector& MuzzlePos);
	void OnMoveStateChanged(ERishiMoveState PrevState, ERishiMoveState NewState);
};