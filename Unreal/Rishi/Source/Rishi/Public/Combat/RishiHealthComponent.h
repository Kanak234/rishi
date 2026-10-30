// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RishiGameTypes.h"
#include "RishiHealthComponent.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnDamaged, float, DamageAmount);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnDied);

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class RISHI_API URishiHealthComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	URishiHealthComponent();

	UFUNCTION(BlueprintCallable, Category = "Health")
	bool TakeDamage(float Amount, const FHitInfo& HitInfo);

	UFUNCTION(BlueprintCallable, Category = "Health")
	void Heal(float Amount);

	UFUNCTION(BlueprintCallable, Category = "Health")
	void ResetFull();

	UFUNCTION(BlueprintPure, Category = "Health")
	float GetHealthPercent() const;

	UFUNCTION(BlueprintPure, Category = "Health")
	bool IsDead() const { return bIsDead; }

	UFUNCTION(BlueprintCallable, Category = "Health")
	void SetInvincible(bool bInvincible) { bIsInvincible = bInvincible; }

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Health")
	float MaxHealth = 100.0f;

	UPROPERTY(BlueprintReadOnly, Category = "Health")
	float CurrentHealth = 100.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Health")
	bool bIsInvincible = false;

	UPROPERTY(BlueprintAssignable, Category = "Health")
	FOnDamaged OnDamaged;

	UPROPERTY(BlueprintAssignable, Category = "Health")
	FOnDied OnDied;

protected:
	virtual void BeginPlay() override;

private:
	bool bIsDead = false;
};