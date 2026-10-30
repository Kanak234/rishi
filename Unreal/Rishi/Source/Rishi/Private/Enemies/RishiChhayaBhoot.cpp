// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Enemies/RishiChhayaBhoot.h"

ARishiChhayaBhoot::ARishiChhayaBhoot()
{
	// GDD defaults for Chhaya-bhoot
	DetectRange = 1200.0f;
	MoveSpeed = 550.0f;
	TurnSpeed = 540.0f;
	AttackRange = 150.0f;
	WindupTime = 0.35f;
	ActiveTime = 0.1f;
	RecoveryTime = 0.45f;
	AttackCooldown = 0.9f;
	Damage = 8.0f;
	HitRadius = 80.0f;
	HitReach = 90.0f;
	HurtStunTime = 0.4f;
	JyotiReward = 1;
}

FVector ARishiChhayaBhoot::GetChaseVelocity(const FVector& ToTarget, float Distance)
{
	// Circle around when close but can't attack (waiting for token)
	if (Distance <= AttackRange + 60.0f)
	{
		FVector SideDir = FVector::CrossProduct(FVector::UpVector, ToTarget / FMath::Max(Distance, 1.0f));
		float Sign = (GetUniqueID() & 1) == 0 ? 1.0f : -1.0f;
		return SideDir * (CircleSpeed * Sign);
	}
	return Super::GetChaseVelocity(ToTarget, Distance);
}