// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Combat/RishiHealthComponent.h"

URishiHealthComponent::URishiHealthComponent()
{
	PrimaryComponentTick.bCanEverTick = false;
}

void URishiHealthComponent::BeginPlay()
{
	Super::BeginPlay();
	CurrentHealth = MaxHealth;
	bIsDead = false;
}

bool URishiHealthComponent::TakeDamage(float Amount, const FHitInfo& HitInfo)
{
	if (bIsDead || bIsInvincible || Amount <= 0.0f)
	{
		return false;
	}

	CurrentHealth = FMath::Max(0.0f, CurrentHealth - Amount);
	OnDamaged.Broadcast(Amount);

	if (CurrentHealth <= 0.0f)
	{
		bIsDead = true;
		OnDied.Broadcast();
	}

	return true;
}

void URishiHealthComponent::Heal(float Amount)
{
	if (bIsDead || Amount <= 0.0f) return;

	CurrentHealth = FMath::Min(MaxHealth, CurrentHealth + Amount);
}

void URishiHealthComponent::ResetFull()
{
	CurrentHealth = MaxHealth;
	bIsDead = false;
	bIsInvincible = false;
}

float URishiHealthComponent::GetHealthPercent() const
{
	return MaxHealth > 0.0f ? CurrentHealth / MaxHealth : 0.0f;
}