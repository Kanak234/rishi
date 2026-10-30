// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Enemies/RishiVayuPishach.h"
#include "Combat/RishiArrow.h"
#include "Combat/RishiHurtboxComponent.h"
#include "Core/RishiGameAudio.h"
#include "Kismet/KismetMathLibrary.h"

ARishiVayuPishach::ARishiVayuPishach()
{
	// GDD defaults for Vayu-pishach
	DetectRange = 1600.0f;
	LoseRange = 2400.0f;
	MoveSpeed = 350.0f;
	AttackRange = 1300.0f;
	WindupTime = 0.7f;
	ActiveTime = 0.05f;
	RecoveryTime = 0.6f;
	AttackCooldown = 2.2f;
	Damage = 10.0f;
	HurtStunTime = 0.5f;
	KnockbackDecay = 6.0f;
	JyotiReward = 2;

	// Disable character movement component - we fly
	GetCharacterMovement()->SetMovementMode(MOVE_Flying);
	GetCharacterMovement()->GravityScale = 0.0f;
	GetCharacterMovement()->MaxFlySpeed = MoveSpeed;
}

void ARishiVayuPishach::BeginPlay()
{
	Super::BeginPlay();
	
	// Initialize strafe flip timer
	NextStrafeFlip = GetWorld()->GetTimeSeconds() + FMath::FRandRange(2.0f, 4.0f);
}

void ARishiVayuPishach::Tick(float DeltaTime)
{
	// Custom movement for flying - call parent but override movement
	if (GetWorld()->GetTimeDilation() <= 0.0f) return;
	
	StateTimer += DeltaTime;
	DesiredVelocity = FVector::ZeroVector;

	if (State == EEnemyState::Dead)
	{
		UpdateDead(DeltaTime);
		UpdateVisuals(DeltaTime);
		return;
	}

	if (!TargetActor || (TargetHealth && TargetHealth->IsDead()))
	{
		if (State != EEnemyState::Idle && State != EEnemyState::Hurt)
			SetState(EEnemyState::Idle);
	}

	switch (State)
	{
	case EEnemyState::Idle:
		UpdateIdle();
		break;
	case EEnemyState::Chase:
		UpdateChase(DeltaTime);
		break;
	case EEnemyState::Attack:
		UpdateAttack(DeltaTime);
		break;
	case EEnemyState::Hurt:
		UpdateHurt(DeltaTime);
		break;
	}

	// Apply movement
	FVector FinalVelocity = DesiredVelocity + KnockbackVelocity;
	SetActorLocation(GetActorLocation() + FinalVelocity * DeltaTime);
	
	// Hover at height above ground
	FHitResult Hit;
	FVector Start = GetActorLocation() + FVector(0, 0, 50.0f);
	if (GetWorld()->LineTraceSingleByChannel(Hit, Start, Start + FVector(0, 0, -3000.0f), ECC_WorldStatic))
	{
		float GroundY = Hit.Location.Z;
		float TargetY = GroundY + HoverHeight + FMath::Sin(GetWorld()->GetTimeSeconds() * 2.0f + GetUniqueID()) * BobAmount;
		FVector NewLoc = GetActorLocation();
		NewLoc.Z = FMath::FInterpTo(NewLoc.Z, TargetY, DeltaTime, 4.0f);
		SetActorLocation(NewLoc);
	}

	KnockbackVelocity = FMath::VInterpTo(KnockbackVelocity, FVector::ZeroVector, DeltaTime, KnockbackDecay);
	UpdateVisuals(DeltaTime);

	if (GetActorLocation().Z < -2000.0f)
	{
		if (HealthComponent) HealthComponent->TakeDamage(HealthComponent->MaxHealth * 10.0f, FHitInfo());
	}
}

FVector ARishiVayuPishach::GetChaseVelocity(const FVector& ToTarget, float Distance)
{
	FVector Dir = ToTarget / FMath::Max(Distance, 1.0f);
	FVector SideDir = FVector::CrossProduct(FVector::UpVector, Dir);

	if (GetWorld()->GetTimeSeconds() >= NextStrafeFlip)
	{
		StrafeSign = -StrafeSign;
		NextStrafeFlip = GetWorld()->GetTimeSeconds() + FMath::FRandRange(2.0f, 4.0f);
	}

	if (Distance < TooCloseDistance) return -Dir * MoveSpeed;
	if (Distance > PreferredDistance + 200.0f) return Dir * MoveSpeed;
	return SideDir * (StrafeSpeed * StrafeSign);
}

void ARishiVayuPishach::PerformAttack()
{
	if (!TargetActor || !ArrowClass) return;

	FActorSpawnParameters SpawnParams;
	SpawnParams.Owner = this;
	SpawnParams.Instigator = this;

	FVector MuzzlePos = GetActorLocation() + GetActorForwardVector() * 70.0f;
	FVector ChestPos = TargetActor->GetActorLocation() + FVector(0, 0, 120.0f);
	FVector Dir = (ChestPos - MuzzlePos).GetSafeNormal();

	ARishiArrow* Arrow = GetWorld()->SpawnActor<ARishiArrow>(ArrowClass, MuzzlePos, Dir.Rotation(), SpawnParams);
	if (Arrow)
	{
		FHitInfo HitInfo;
		HitInfo.Damage = Damage;
		HitInfo.Direction = FVector(Dir.X, Dir.Y, 0).GetSafeNormal();
		HitInfo.Knockback = 0.0f;
		HitInfo.bGuardBreak = false;
		HitInfo.bIsFinisher = false;
		HitInfo.AttackerTeam = ETeam::Asur;
		HitInfo.Attacker = this;

		Arrow->Launch(Dir, ArrowSpeed, 1.5f, HitInfo, ECC_GameTraceChannel1);
	}

	// Play arrow sound via GameAudio
	// URishiGameAudio::Play(ESfx::Arrow, MuzzlePos, 0.7f);
}