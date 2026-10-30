// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Enemies/RishiPashanRakshas.h"
#include "Combat/RishiHurtboxComponent.h"
#include "Core/RishiGameSettings.h"
#include "Core/RishiGameAudio.h"

ARishiPashanRakshas::ARishiPashanRakshas()
{
	// GDD defaults for Pashan-rakshas
	DetectRange = 1000.0f;
	MoveSpeed = 220.0f;
	TurnSpeed = 200.0f;
	AttackRange = 200.0f;
	WindupTime = 0.8f;
	ActiveTime = 0.15f;
	RecoveryTime = 0.9f;
	AttackCooldown = 1.6f;
	Damage = 20.0f;
	HitRadius = 130.0f;
	HitReach = 140.0f;
	HurtStunTime = 0.3f;
	bSuperArmor = true;
	KnockbackDecay = 14.0f;
	JyotiReward = 3;
}

bool ARishiPashanRakshas::FilterHit_Implementation(FHitInfo& HitInfo)
{
	// Heavy attacks always break guard
	if (HitInfo.bGuardBreak) return true;

	// No guard when hurt or dead
	if (GetState() == EEnemyState::Hurt || GetState() == EEnemyState::Dead) return true;

	// Check if attack comes from front
	FVector FromAttacker = -HitInfo.Direction;
	FromAttacker.Z = 0;
	
	if (FromAttacker.SizeSquared() > 0.001f)
	{
		float Angle = FMath::Acos(FVector::DotProduct(GetActorForwardVector(), FromAttacker.GetSafeNormal())) * 180.0f / 3.14159f;
		if (Angle <= GuardAngle)
		{
			// Blocked!
			FlashTimer = GetWorld()->GetTimeSeconds() + 0.08f;
			OnGuardBlocked.Broadcast();
			
			// Play clang sound
			if (UGameplayStatics::GetPlayerController(GetWorld(), 0))
			{
				// Sound would be played here via GameAudio
			}
			
			UE_LOG(LogTemp, Log, TEXT("[RISHI] Pashan blocked attack - use heavy (RMB) or attack from behind"));
			return false; // Damage blocked
		}
	}

	return true; // Not from front, damage applies
}

float ARishiPashanRakshas::GetStunDuration(const FHitInfo& HitInfo)
{
	return HitInfo.bGuardBreak ? GuardBreakStun : HurtStunTime;
}