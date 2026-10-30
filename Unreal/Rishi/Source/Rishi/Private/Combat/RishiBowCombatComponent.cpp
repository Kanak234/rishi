// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Combat/RishiBowCombatComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "Core/RishiInputComponent.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiArrow.h"
#include "Components/CapsuleComponent.h"
#include "Camera/PlayerCameraManager.h"
#include "Kismet/KismetMathLibrary.h"

URishiBowCombatComponent::URishiBowCombatComponent()
{
	PrimaryComponentTick.bCanEverTick = true;
}

void URishiBowCombatComponent::BeginPlay()
{
	Super::BeginPlay();

	PlayerCharacter = Cast<ARishiPlayerCharacter>(GetOwner());
	InputComponent = GetOwner()->FindComponentByClass<URishiInputComponent>();
	HealthComponent = GetOwner()->FindComponentByClass<URishiHealthComponent>();

	if (PlayerCharacter)
	{
		PlayerCharacter->OnMoveStateChanged.AddDynamic(this, &URishiBowCombatComponent::OnMoveStateChanged);
	}
	if (HealthComponent)
	{
		HealthComponent->OnDied.AddDynamic(this, [this]() { 
			bool bWasAiming = bIsAiming;
			bCharging = false;
			bIsAiming = false;
			if (PlayerCharacter)
			{
				PlayerCharacter->SetMoveScale(1.0f);
				PlayerCharacter->SetLockFacing(false);
				PlayerCharacter->SetJumpLocked(false);
			}
			if (bWasAiming) OnAimChanged.Broadcast(false);
		});
	}
}

void URishiBowCombatComponent::UpdateBow(float DeltaTime)
{
	if (GetWorld()->GetTimeDilation() <= 0.0f || (HealthComponent && HealthComponent->IsDead()))
	{
		return;
	}

	bool bGrounded = PlayerCharacter && PlayerCharacter->GetMoveState() == ERishiMoveState::Grounded;

	// Aim
	bool bWantAim = bGrounded && InputComponent && InputComponent->IsAimHeld();
	if (bWantAim != bIsAiming)
	{
		SetAiming(bWantAim);
	}

	if (bIsAiming)
	{
		UpdateAimDirection();
	}

	// Charge (Heavy Attack)
	if (bCharging)
	{
		ChargeTimer += DeltaTime;
		if (InputComponent && !InputComponent->IsHeavyAttackHeld())
		{
			float t = FMath::Clamp(ChargeTimer / ChargeTime, 0.0f, 1.0f);
			float Damage = FMath::Lerp(ChargedMinDamage, ChargedMaxDamage, t);
			float Knockback = t >= 1.0f ? ChargedKnockback : 0.0f;
			Shoot(Damage, Knockback, true);
			bCharging = false;
			ChargeTimer = 0.0f;
			if (!bIsAiming)
			{
				PlayerCharacter->SetMoveScale(1.0f);
				PlayerCharacter->SetLockFacing(false);
				PlayerCharacter->SetJumpLocked(false);
			}
		}
		return;
	}

	if (!bGrounded) return;

	if (InputComponent && InputComponent->WasHeavyAttackPressed() && GetWorld()->GetTimeSeconds() >= NextFireTime)
	{
		bCharging = true;
		ChargeTimer = 0.0f;
		PlayerCharacter->SetMoveScale(AimMoveScale);
		PlayerCharacter->SetLockFacing(true);
		PlayerCharacter->SetJumpLocked(true);
		if (!bIsAiming)
		{
			AimDirection = GetAutoAimDirection();
		}
		return;
	}

	if (InputComponent && InputComponent->WasLightAttackPressed() && GetWorld()->GetTimeSeconds() >= NextFireTime)
	{
		if (!bIsAiming)
		{
			AimDirection = GetAutoAimDirection();
		}
		Shoot(QuickDamage, 0.0f, false);
	}
}

void URishiBowCombatComponent::SetAiming(bool bAim)
{
	bIsAiming = bAim;
	if (bAim)
	{
		PlayerCharacter->SetMoveScale(AimMoveScale);
		PlayerCharacter->SetLockFacing(true);
		PlayerCharacter->SetJumpLocked(true);
	}
	else if (!bCharging)
	{
		PlayerCharacter->SetMoveScale(1.0f);
		PlayerCharacter->SetLockFacing(false);
		PlayerCharacter->SetJumpLocked(false);
	}
	OnAimChanged.Broadcast(bAim);
}

void URishiBowCombatComponent::UpdateAimDirection()
{
	if (!PlayerCharacter) return;

	APlayerController* PC = Cast<APlayerController>(PlayerCharacter->GetController());
	if (!PC) return;

	// Mouse aim
	if (APlayerCameraManager* CameraManager = PC->PlayerCameraManager)
	{
		FVector2D MousePos;
		if (PC->GetMousePosition(MousePos.X, MousePos.Y))
		{
			FVector WorldPos, WorldDir;
			if (PC->DeprojectScreenPositionToWorld(MousePos.X, MousePos.Y, WorldPos, WorldDir))
			{
				FPlane Plane(FVector::UpVector, PlayerCharacter->GetActorLocation() + FVector(0, 0, MuzzleHeight));
				float IntersectionDist;
				if (FMath::RayPlaneIntersection(WorldPos, WorldDir, Plane, IntersectionDist))
				{
					FVector HitPos = WorldPos + WorldDir * IntersectionDist;
					FVector Dir = HitPos - PlayerCharacter->GetActorLocation();
					Dir.Z = 0;
					if (!Dir.IsNearlyZero())
					{
						AimDirection = Dir.GetSafeNormal();
						PlayerCharacter->SetActorRotation(AimDirection.Rotation());
					}
				}
			}
		}
	}
	else
	{
		// Gamepad: use move input
		FVector2D MoveInput = InputComponent ? InputComponent->GetMoveValue() : FVector2D::ZeroVector;
		if (MoveInput.SizeSquared() > 0.04f)
		{
			FVector Forward = PlayerCharacter->GetActorForwardVector();
			FVector Right = PlayerCharacter->GetActorRightVector();
			AimDirection = (Forward * MoveInput.Y + Right * MoveInput.X).GetSafeNormal2D();
			PlayerCharacter->SetActorRotation(AimDirection.Rotation());
		}
	}
}

FVector URishiBowCombatComponent::GetAutoAimDirection()
{
	if (!PlayerCharacter) return PlayerCharacter ? PlayerCharacter->GetActorForwardVector() : FVector::ForwardVector;

	TArray<FOverlapResult> Overlaps;
	FCollisionQueryParams Params;
	Params.AddIgnoredActor(GetOwner());
	
	bool bHit = GetWorld()->OverlapMultiByChannel(Overlaps, PlayerCharacter->GetActorLocation(), FQuat::Identity, HitCollisionChannel, FCollisionShape::MakeSphere(AutoAimRange), Params);

	float BestDist = FLT_MAX;
	FVector BestDir = PlayerCharacter->GetActorForwardVector();
	float CosLimit = FMath::Cos(FMath::DegreesToRadians(AutoAimAngle));

	for (const FOverlapResult& Overlap : Overlaps)
	{
		AActor* OtherActor = Overlap.GetActor();
		if (!OtherActor) continue;

		URishiHurtboxComponent* Hurtbox = OtherActor->FindComponentByClass<URishiHurtboxComponent>();
		if (!Hurtbox || Hurtbox->GetTeam() == ETeam::Player) continue;

		URishiHealthComponent* Health = OtherActor->FindComponentByClass<URishiHealthComponent>();
		if (Health && Health->IsDead()) continue;

		FVector ToTarget = OtherActor->GetActorLocation() - PlayerCharacter->GetActorLocation();
		ToTarget.Z = 0;
		float Dist = ToTarget.Size();
		if (Dist < 0.01f) continue;

		FVector Dir = ToTarget / Dist;
		if (FVector::DotProduct(Dir, PlayerCharacter->GetActorForwardVector()) < CosLimit) continue;

		if (Dist < BestDist)
		{
			BestDist = Dist;
			BestDir = Dir;
		}
	}

	if (BestDist < FLT_MAX)
	{
		PlayerCharacter->SetActorRotation(BestDir.Rotation());
	}
	return BestDir;
}

FVector URishiBowCombatComponent::ApplyVerticalAssist(const FVector& FlatDir, const FVector& MuzzlePos)
{
	if (!PlayerCharacter) return FlatDir;

	TArray<FOverlapResult> Overlaps;
	FCollisionQueryParams Params;
	Params.AddIgnoredActor(GetOwner());

	bool bHit = GetWorld()->OverlapMultiByChannel(Overlaps, MuzzlePos, FQuat::Identity, HitCollisionChannel, FCollisionShape::MakeSphere(AutoAimRange * 1.5f), Params);

	float BestDist = FLT_MAX;
	FVector ResultDir = FlatDir;
	float CosLimit = FMath::Cos(FMath::DegreesToRadians(VerticalAssistAngle));

	for (const FOverlapResult& Overlap : Overlaps)
	{
		AActor* OtherActor = Overlap.GetActor();
		if (!OtherActor) continue;

		URishiHurtboxComponent* Hurtbox = OtherActor->FindComponentByClass<URishiHurtboxComponent>();
		if (!Hurtbox || Hurtbox->GetTeam() == ETeam::Player) continue;

		URishiHealthComponent* Health = OtherActor->FindComponentByClass<URishiHealthComponent>();
		if (Health && Health->IsDead()) continue;

		FVector ToTarget = OtherActor->GetActorLocation() - MuzzlePos;
		FVector FlatTo = FVector(ToTarget.X, ToTarget.Y, 0);
		float Dist = FlatTo.Size();
		if (Dist < 50.0f) continue;

		if (FVector::DotProduct(FlatTo / Dist, FlatDir) < CosLimit) continue;

		if (Dist < BestDist)
		{
			BestDist = Dist;
			ResultDir = ToTarget.GetSafeNormal();
		}
	}

	return ResultDir;
}

void URishiBowCombatComponent::Shoot(float Damage, float Knockback, bool bCharged)
{
	if (!PlayerCharacter || !ArrowClass) return;

	FVector Dir = AimDirection.IsNearlyZero() ? PlayerCharacter->GetActorForwardVector() : AimDirection;
	FVector MuzzlePos = PlayerCharacter->GetActorLocation() + FVector(0, 0, MuzzleHeight) + Dir * MuzzleForward;
	Dir = ApplyVerticalAssist(Dir, MuzzlePos);

	FActorSpawnParameters SpawnParams;
	SpawnParams.Owner = GetOwner();
	SpawnParams.Instigator = PlayerCharacter;

	ARishiArrow* Arrow = GetWorld()->SpawnActor<ARishiArrow>(ArrowClass, MuzzlePos, Dir.Rotation(), SpawnParams);
	if (Arrow)
	{
		FHitInfo HitInfo;
		HitInfo.Damage = Damage * DamageMultiplier;
		HitInfo.Direction = Dir;
		HitInfo.Knockback = Knockback;
		HitInfo.bGuardBreak = false;
		HitInfo.bIsFinisher = false;
		HitInfo.AttackerTeam = ETeam::Player;
		HitInfo.Attacker = GetOwner();

		Arrow->Launch(Dir, ArrowSpeed, ArrowLifetime, HitInfo, HitCollisionChannel);
		NextFireTime = GetWorld()->GetTimeSeconds() + FireCooldown;
		OnArrowFired.Broadcast(bCharged);
	}
}

void URishiBowCombatComponent::OnMoveStateChanged(ERishiMoveState PrevState, ERishiMoveState NewState)
{
	if (NewState != ERishiMoveState::Grounded)
	{
		bool bWasAiming = bIsAiming;
		bCharging = false;
		bIsAiming = false;
		if (PlayerCharacter)
		{
			PlayerCharacter->SetMoveScale(1.0f);
			PlayerCharacter->SetLockFacing(false);
			PlayerCharacter->SetJumpLocked(false);
		}
		if (bWasAiming) OnAimChanged.Broadcast(false);
	}
}