// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Combat/RishiCombatSystemComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "Core/RishiInputComponent.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiHurtboxComponent.h"
#include "Combat/RishiHitFilterInterface.h"
#include "Components/CapsuleComponent.h"
#include "Kismet/KismetMathLibrary.h"

URishiCombatSystemComponent::URishiCombatSystemComponent()
{
	PrimaryComponentTick.bCanEverTick = true;

	// Default light combo (GDD values)
	LightCombo.Add(FAttackData(0.42f, 0.12f, 0.24f, 10.0f, 1.5f, 0.9f, 1.0f, 3.0f));
	LightCombo.Add(FAttackData(0.42f, 0.12f, 0.24f, 10.0f, 1.5f, 0.9f, 1.0f, 3.0f));
	LightCombo.Add(FAttackData(0.60f, 0.20f, 0.34f, 18.0f, 7.0f, 1.1f, 1.1f, 4.5f)); // Finisher

	// Default heavy attack
	HeavyAttack = FAttackData(0.65f, 0.18f, 0.34f, 25.0f, 6.0f, 1.2f, 1.2f, 5.0f);

	AlreadyHit.SetNum(16);
}

void URishiCombatSystemComponent::BeginPlay()
{
	Super::BeginPlay();

	PlayerCharacter = Cast<ARishiPlayerCharacter>(GetOwner());
	InputComponent = GetOwner()->FindComponentByClass<URishiInputComponent>();
	HealthComponent = GetOwner()->FindComponentByClass<URishiHealthComponent>();

	if (PlayerCharacter)
	{
		PlayerCharacter->OnMoveStateChanged.AddDynamic(this, &URishiCombatSystemComponent::OnMoveStateChanged);
	}
	if (HealthComponent)
	{
		HealthComponent->OnDied.AddDynamic(this, &URishiCombatSystemComponent::CancelAttack);
	}
}

void URishiCombatSystemComponent::UpdateCombat(float DeltaTime)
{
	if (GetWorld()->GetTimeDilation() <= 0.0f || (HealthComponent && HealthComponent->IsDead()))
	{
		return;
	}

	bool bLightPressed = InputComponent && InputComponent->WasLightAttackPressed();
	bool bHeavyPressed = InputComponent && InputComponent->WasHeavyAttackPressed();
	bool bCanStart = PlayerCharacter && PlayerCharacter->GetMoveState() == ERishiMoveState::Grounded;

	if (bCharging)
	{
		UpdateCharge(DeltaTime);
		return;
	}

	if (CurrentKind == EAttackKind::None)
	{
		SinceAttackEnd += DeltaTime;
		if (!bCanStart) return;

		if (bHeavyPressed)
		{
			StartCharge();
			return;
		}

		if (bLightPressed)
		{
			int32 NextIndex = (SinceAttackEnd <= ComboResetTime && ComboIndex >= 0 && ComboIndex < LightCombo.Num() - 1)
				? ComboIndex + 1 : 0;
			StartLight(NextIndex);
		}
		return;
	}

	// Attack in progress
	float PrevTimer = Timer;
	Timer += DeltaTime;

	// Input buffer for combo
	if (bLightPressed && CurrentKind == EAttackKind::Light && Timer >= CurrentAttack.Duration - ComboBufferWindow)
	{
		bBufferedLight = true;
	}

	// Lunge velocity during active window
	if (PlayerCharacter)
	{
		PlayerCharacter->SetExtraVelocity(Timer < CurrentAttack.ActiveEnd ? PlayerCharacter->GetActorForwardVector() * CurrentAttack.Lunge : FVector::ZeroVector);
	}

	// Hitbox window - check if window was entered this frame
	bHitboxActive = Timer >= CurrentAttack.ActiveStart && PrevTimer <= CurrentAttack.ActiveEnd;
	if (bHitboxActive)
	{
		DoHitboxCheck();
	}
	if (Timer > CurrentAttack.ActiveEnd)
	{
		bHitboxActive = false;
	}

	if (Timer >= CurrentAttack.Duration)
	{
		bool bChain = bBufferedLight && CurrentKind == EAttackKind::Light && ComboIndex < LightCombo.Num() - 1 && bCanStart;
		int32 NextIndex = ComboIndex + 1;
		FinishAttack();
		if (bChain)
		{
			StartLight(NextIndex);
		}
		else if (CurrentKind == EAttackKind::None && ComboIndex >= LightCombo.Num() - 1)
		{
			ComboIndex = -1; // Reset after full combo
		}
	}
}

void URishiCombatSystemComponent::StartLight(int32 Index)
{
	ComboIndex = FMath::Clamp(Index, 0, LightCombo.Num() - 1);
	CurrentAttack = LightCombo[ComboIndex];
	BeginAttack(EAttackKind::Light);
	OnAttackStarted.Broadcast(EAttackKind::Light, ComboIndex + 1);
}

void URishiCombatSystemComponent::StartCharge()
{
	bCharging = true;
	ChargeTimer = 0.0f;
	ComboIndex = -1; // Heavy breaks combo
	
	if (PlayerCharacter)
	{
		PlayerCharacter->SetMoveScale(ChargeMoveScale);
		PlayerCharacter->SetJumpLocked(true);
		// Face input direction
		FVector2D MoveInput = InputComponent ? InputComponent->GetMoveValue() : FVector2D::ZeroVector;
		if (MoveInput.SizeSquared() > 0.04f)
		{
			FVector Forward = PlayerCharacter->GetActorForwardVector();
			FVector Right = PlayerCharacter->GetActorRightVector();
			FVector Dir = (Forward * MoveInput.Y + Right * MoveInput.X).GetSafeNormal2D();
			PlayerCharacter->SetActorRotation(Dir.Rotation());
		}
	}
	OnAttackStarted.Broadcast(EAttackKind::Heavy, 0); // 0 = charge start
}

void URishiCombatSystemComponent::UpdateCharge(float DeltaTime)
{
	ChargeTimer += DeltaTime;
	bool bReleased = InputComponent && !InputComponent->IsHeavyAttackHeld();

	if (!bReleased && ChargeTimer < HeavyMaxCharge)
	{
		return;
	}

	// Calculate damage based on charge time
	float t = FMath::Clamp((ChargeTimer - HeavyMinCharge) / (HeavyMaxCharge - HeavyMinCharge), 0.0f, 1.0f);
	HeavyDamage = FMath::Lerp(HeavyAttack.Damage, HeavyMaxDamage, t);

	bCharging = false;
	CurrentAttack = HeavyAttack;
	BeginAttack(EAttackKind::Heavy);
	OnAttackStarted.Broadcast(EAttackKind::Heavy, 1); // 1 = strike
}

void URishiCombatSystemComponent::BeginAttack(EAttackKind Kind)
{
	CurrentKind = Kind;
	Timer = 0.0f;
	bBufferedLight = false;
	AlreadyHit.Empty();

	if (PlayerCharacter)
	{
		// Face input direction
		FVector2D MoveInput = InputComponent ? InputComponent->GetMoveValue() : FVector2D::ZeroVector;
		if (MoveInput.SizeSquared() > 0.04f)
		{
			FVector Forward = PlayerCharacter->GetActorForwardVector();
			FVector Right = PlayerCharacter->GetActorRightVector();
			FVector Dir = (Forward * MoveInput.Y + Right * MoveInput.X).GetSafeNormal2D();
			PlayerCharacter->SetActorRotation(Dir.Rotation());
		}

		PlayerCharacter->SetMoveScale(0.0f);
		PlayerCharacter->SetJumpLocked(true);
	}
}

void URishiCombatSystemComponent::FinishAttack()
{
	CurrentKind = EAttackKind::None;
	bHitboxActive = false;
	SinceAttackEnd = 0.0f;

	if (PlayerCharacter)
	{
		PlayerCharacter->SetMoveScale(1.0f);
		PlayerCharacter->SetExtraVelocity(FVector::ZeroVector);
		PlayerCharacter->SetJumpLocked(false);
	}

	OnAttackEnded.Broadcast();
}

void URishiCombatSystemComponent::CancelAttack()
{
	bool bWasBusy = CurrentKind != EAttackKind::None || bCharging;
	bCharging = false;
	ComboIndex = -1;

	if (PlayerCharacter)
	{
		PlayerCharacter->SetMoveScale(1.0f);
		PlayerCharacter->SetExtraVelocity(FVector::ZeroVector);
		PlayerCharacter->SetJumpLocked(false);
	}

	CurrentKind = EAttackKind::None;
	bHitboxActive = false;
	SinceAttackEnd = 999.0f;

	if (bWasBusy)
	{
		OnAttackEnded.Broadcast();
	}
}

void URishiCombatSystemComponent::DoHitboxCheck()
{
	if (!PlayerCharacter) return;

	FVector Center = GetHitboxCenter();
	TArray<FOverlapResult> Overlaps;
	FCollisionQueryParams Params;
	Params.AddIgnoredActor(GetOwner());
	Params.bReturnPhysicalMaterial = false;

	bool bHit = GetWorld()->OverlapMultiByChannel(Overlaps, Center, FQuat::Identity, HurtboxCollisionChannel, FCollisionShape::MakeSphere(CurrentAttack.Radius), Params);

	for (const FOverlapResult& Overlap : Overlaps)
	{
		AActor* OtherActor = Overlap.GetActor();
		if (!OtherActor) continue;

		URishiHurtboxComponent* Hurtbox = OtherActor->FindComponentByClass<URishiHurtboxComponent>();
		if (!Hurtbox) continue;

		if (WasAlreadyHit(Hurtbox)) continue;

		AlreadyHit.Add(Hurtbox);

		FVector Dir = Hurtbox->GetOwner()->GetActorLocation() - GetOwner()->GetActorLocation();
		Dir.Z = 0.0f;
		if (Dir.IsNearlyZero()) Dir = GetOwner()->GetActorForwardVector();
		Dir.Normalize();

		bool bIsFinisher = CurrentKind == EAttackKind::Light && ComboIndex == LightCombo.Num() - 1;

		FHitInfo HitInfo;
		HitInfo.Damage = (CurrentKind == EAttackKind::Heavy ? HeavyDamage : CurrentAttack.Damage) * DamageMultiplier;
		HitInfo.Direction = Dir;
		HitInfo.Knockback = CurrentAttack.Knockback;
		HitInfo.bGuardBreak = CurrentKind == EAttackKind::Heavy;
		HitInfo.bIsFinisher = bIsFinisher;
		HitInfo.AttackerTeam = ETeam::Player;
		HitInfo.Attacker = GetOwner();

		// Check for hit filter (guard, etc.)
		if (Hurtbox->GetOwner()->GetClass()->ImplementsInterface(URishiHitFilterInterface::StaticClass()))
		{
			IRishiHitFilterInterface::Execute_FilterHit(Hurtbox->GetOwner(), HitInfo);
		}

		if (Hurtbox->ReceiveHit(HitInfo))
		{
			OnHitLanded.Broadcast(Hurtbox, HitInfo);
		}
	}
}

FVector URishiCombatSystemComponent::GetHitboxCenter() const
{
	if (!PlayerCharacter) return FVector::ZeroVector;
	return PlayerCharacter->GetActorLocation() + FVector(0.0f, 0.0f, 100.0f) + PlayerCharacter->GetActorForwardVector() * CurrentAttack.Reach * 100.0f;
}

bool URishiCombatSystemComponent::WasAlreadyHit(URishiHurtboxComponent* Hurtbox) const
{
	for (URishiHurtboxComponent* HitHurtbox : AlreadyHit)
	{
		if (HitHurtbox == Hurtbox) return true;
	}
	return false;
}

void URishiCombatSystemComponent::OnMoveStateChanged(ERishiMoveState PrevState, ERishiMoveState NewState)
{
	// Cancel attack when leaving grounded state (dodge, fall, etc.)
	if (NewState != ERishiMoveState::Grounded)
	{
		CancelAttack();
	}
}