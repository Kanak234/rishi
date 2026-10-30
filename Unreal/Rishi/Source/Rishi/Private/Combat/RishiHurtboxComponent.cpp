// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Combat/RishiHurtboxComponent.h"
#include "Combat/RishiHealthComponent.h"
#include "GameFramework/Actor.h"

URishiHurtboxComponent::URishiHurtboxComponent()
{
	PrimaryComponentTick.bCanEverTick = false;
}

void URishiHurtboxComponent::BeginPlay()
{
	Super::BeginPlay();
	HealthComponent = GetOwner()->FindComponentByClass<URishiHealthComponent>();
}

bool URishiHurtboxComponent::ReceiveHit(const FHitInfo& HitInfo)
{
	if (!HealthComponent)
	{
		return false;
	}

	// Check if attacker is on the same team
	if (HitInfo.AttackerTeam == Team)
	{
		return false;
	}

	bool bDamageApplied = HealthComponent->TakeDamage(HitInfo.Damage, HitInfo);
	
	if (bDamageApplied)
	{
		OnHit.Broadcast(this, HitInfo);
	}

	return bDamageApplied;
}