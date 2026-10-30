// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Enemies/RishiEnemyAI.h"
#include "Components/CapsuleComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiHurtboxComponent.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "Kismet/KismetMathLibrary.h"
#include "Kismet/GameplayStatics.h"

int32 ARishiEnemyAI::MaxSimultaneousAttackers = 3;
int32 ARishiEnemyAI::ActiveEnemyCount = 0;
int32 ARishiEnemyAI::CurrentAttackers = 0;

ARishiEnemyAI::ARishiEnemyAI()
{
	PrimaryActorTick.bCanEverTick = true;

	GetCapsuleComponent()->InitCapsuleSize(40.0f, 100.0f);
	GetCharacterMovement()->bOrientRotationToMovement = false;
	GetCharacterMovement()->RotationRate = FRotator(0.0f, TurnSpeed, 0.0f);
	GetCharacterMovement()->GravityScale = 1.0f;
	GetCharacterMovement()->MaxWalkSpeed = MoveSpeed;
	GetCharacterMovement()->JumpZVelocity = 0.0f;
	GetCharacterMovement()->AirControl = 0.0f;

	HealthComponent = CreateDefaultSubobject<URishiHealthComponent>(TEXT("Health"));
	HurtboxComponent = CreateDefaultSubobject<URishiHurtboxComponent>(TEXT("Hurtbox"));
	HurtboxComponent->Team = ETeam::Asur;
}

void ARishiEnemyAI::BeginPlay()
{
	Super::BeginPlay();

	HealthComponent->OnDied.AddDynamic(this, [this]() { 
		SetState(EEnemyState::Dead);
		OnDied.Broadcast(this);
	});

	HurtboxComponent->OnHit.AddDynamic(this, &ARishiEnemyAI::TakeHit);

	ActiveEnemyCount++;
	if (ActiveEnemyCount > 12)
	{
		UE_LOG(LogTemp, Warning, TEXT("[RISHI] %d enemies alive - exceeding GDD budget of 12"), ActiveEnemyCount);
	}

	// Find player
	TArray<AActor*> Players;
	UGameplayStatics::GetAllActorsOfClass(GetWorld(), ARishiPlayerCharacter::StaticClass(), Players);
	if (Players.Num() > 0)
	{
		TargetActor = Players[0];
		TargetHealth = TargetActor->FindComponentByClass<URishiHealthComponent>();
	}

	// Setup dynamic material
	if (BodyMesh && BodyMesh->GetMaterial(0))
	{
		DynamicMaterial = BodyMesh->CreateAndSetMaterialInstanceDynamic(0);
		if (DynamicMaterial)
		{
			DynamicMaterial->GetVectorParameterValue(FName("BaseColor"), BaseColor);
		}
	}
}

void ARishiEnemyAI::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	ActiveEnemyCount--;
	ReleaseAttackToken();
	Super::EndPlay(EndPlayReason);
}

void ARishiEnemyAI::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (GetWorld()->GetTimeDilation() <= 0.0f) return;

	StateTimer += DeltaTime;
	DesiredVelocity = FVector::ZeroVector;

	if (State == EEnemyState::Dead)
	{
		UpdateDead(DeltaTime);
		return;
	}

	// No target or target dead
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

	// Apply knockback decay
	KnockbackVelocity = FMath::VInterpTo(KnockbackVelocity, FVector::ZeroVector, DeltaTime, KnockbackDecay);
	
	FVector FinalVelocity = DesiredVelocity + KnockbackVelocity;
	GetCharacterMovement()->Velocity = FVector(FinalVelocity.X, FinalVelocity.Y, GetCharacterMovement()->Velocity.Z);

	UpdateVisuals(DeltaTime);

	// Fell off world
	if (GetActorLocation().Z < -2000.0f)
	{
		if (HealthComponent) HealthComponent->TakeDamage(HealthComponent->MaxHealth * 10.0f, FHitInfo());
	}
}

void ARishiEnemyAI::UpdateIdle()
{
	if (!TargetActor || (TargetHealth && TargetHealth->IsDead())) return;
	
	if (GetFlatDistance(TargetActor->GetActorLocation()) <= DetectRange)
	{
		SetState(EEnemyState::Chase);
	}
}

void ARishiEnemyAI::UpdateChase(float DeltaTime)
{
	if (!TargetActor) { SetState(EEnemyState::Idle); return; }

	FVector ToTarget = TargetActor->GetActorLocation() - GetActorLocation();
	ToTarget.Z = 0;
	float Dist = ToTarget.Size();

	if (Dist > LoseRange)
	{
		SetState(EEnemyState::Idle);
		return;
	}

	FaceTowards(ToTarget, DeltaTime);

	if (Dist <= AttackRange && GetWorld()->GetTimeSeconds() >= NextAttackTime && TryTakeAttackToken())
	{
		SetState(EEnemyState::Attack);
		return;
	}

	DesiredVelocity = GetChaseVelocity(ToTarget, Dist);
}

void ARishiEnemyAI::UpdateAttack(float DeltaTime)
{
	// Face target during first half of windup
	if (StateTimer < WindupTime * 0.5f && TargetActor)
	{
		FVector ToTarget = TargetActor->GetActorLocation() - GetActorLocation();
		ToTarget.Z = 0;
		FaceTowards(ToTarget, DeltaTime);
	}

	// Perform attack at windup end
	if (!bHitDone && StateTimer >= WindupTime)
	{
		bHitDone = true;
		PerformAttack();
	}

	// Attack recovery
	if (StateTimer >= WindupTime + ActiveTime + RecoveryTime)
	{
		ReleaseAttackToken();
		NextAttackTime = GetWorld()->GetTimeSeconds() + AttackCooldown;
		SetState(EEnemyState::Chase);
	}
}

void ARishiEnemyAI::UpdateHurt(float DeltaTime)
{
	if (StateTimer >= StunTimer)
	{
		SetState(EEnemyState::Chase);
	}
}

void ARishiEnemyAI::UpdateDead(float DeltaTime)
{
	if (StateTimer > 0.6f)
	{
		AddActorWorldOffset(FVector(0, 0, -120.0f * DeltaTime));
	}
	if (StateTimer > 2.0f)
	{
		SetActorHiddenInGame(true);
		SetActorEnableCollision(false);
	}
}

FVector ARishiEnemyAI::GetChaseVelocity(const FVector& ToTarget, float Distance)
{
	FVector Dir = ToTarget / FMath::Max(Distance, 1.0f);
	return Dir * MoveSpeed;
}

void ARishiEnemyAI::PerformAttack()
{
	if (!TargetActor) return;

	FVector Center = GetActorLocation() + FVector(0, 0, 100.0f) + GetActorForwardVector() * HitReach;
	TArray<FOverlapResult> Overlaps;
	FCollisionQueryParams Params;
	Params.AddIgnoredActor(this);

	GetWorld()->OverlapMultiByChannel(Overlaps, Center, FQuat::Identity, ECC_GameTraceChannel1, FCollisionShape::MakeSphere(HitRadius), Params);

	for (const FOverlapResult& Overlap : Overlaps)
	{
		AActor* OtherActor = Overlap.GetActor();
		if (!OtherActor) continue;

		URishiHurtboxComponent* Hurtbox = OtherActor->FindComponentByClass<URishiHurtboxComponent>();
		if (!Hurtbox || Hurtbox->GetTeam() != ETeam::Player) continue;

		FVector Dir = OtherActor->GetActorLocation() - GetActorLocation();
		Dir.Z = 0;
		if (Dir.IsNearlyZero()) Dir = GetActorForwardVector();
		Dir.Normalize();

		FHitInfo HitInfo;
		HitInfo.Damage = Damage;
		HitInfo.Direction = Dir;
		HitInfo.Knockback = 0.0f;
		HitInfo.bGuardBreak = false;
		HitInfo.bIsFinisher = false;
		HitInfo.AttackerTeam = ETeam::Asur;
		HitInfo.Attacker = this;

		Hurtbox->ReceiveHit(HitInfo);
		return; // One hit per attack
	}
}

void ARishiEnemyAI::TakeHit(const FHitInfo& HitInfo)
{
	if (State == EEnemyState::Dead) return;

	ApplyKnockback(HitInfo.Direction, HitInfo.Knockback);
	FlashTimer = GetWorld()->GetTimeSeconds() + 0.12f;

	// Agro if idle
	if (State == EEnemyState::Idle) SetState(EEnemyState::Chase);

	// Super armor check
	if (bSuperArmor && !HitInfo.bGuardBreak) return;

	ReleaseAttackToken();
	StunTimer = GetStunDuration(HitInfo);
	SetState(EEnemyState::Hurt);
}

void ARishiEnemyAI::SetState(EEnemyState NewState)
{
	if (NewState == State) return;

	EEnemyState PrevState = State;
	State = NewState;
	StateTimer = 0.0f;

	if (NewState == EEnemyState::Attack) bHitDone = false;

	OnStateChanged.Broadcast(PrevState, NewState);
}

bool ARishiEnemyAI::TryTakeAttackToken()
{
	if (bHasAttackToken) return true;
	if (CurrentAttackers >= MaxSimultaneousAttackers) return false;
	
	CurrentAttackers++;
	bHasAttackToken = true;
	return true;
}

void ARishiEnemyAI::ReleaseAttackToken()
{
	if (!bHasAttackToken) return;
	bHasAttackToken = false;
	CurrentAttackers = FMath::Max(0, CurrentAttackers - 1);
}

void ARishiEnemyAI::FaceTowards(const FVector& Dir, float DeltaTime)
{
	FVector FlatDir = FVector(Dir.X, Dir.Y, 0);
	if (FlatDir.SizeSquared() < 0.001f) return;
	FRotator TargetRot = FlatDir.Rotation();
	SetActorRotation(FMath::RInterpTo(GetActorRotation(), TargetRot, DeltaTime, TurnSpeed / 180.0f * 3.14159f));
}

float ARishiEnemyAI::GetFlatDistance(const FVector& TargetPos) const
{
	FVector Diff = TargetPos - GetActorLocation();
	Diff.Z = 0;
	return Diff.Size();
}

bool ARishiEnemyAI::HasGroundAhead(const FVector& Dir) const
{
	FVector ProbePos = GetActorLocation() + Dir * 80.0f + FVector(0, 0, 50.0f);
	FHitResult Hit;
	return GetWorld()->LineTraceSingleByChannel(Hit, ProbePos, ProbePos + FVector(0, 0, -200.0f), ECC_WorldStatic);
}

void ARishiEnemyAI::ApplyKnockback(const FVector& Direction, float Strength)
{
	KnockbackVelocity = Direction * Strength;
}

void ARishiEnemyAI::UpdateVisuals(float DeltaTime)
{
	if (!DynamicMaterial) return;

	FLinearColor Color = BaseColor;
	float CurrentTime = GetWorld()->GetTimeSeconds();

	if (CurrentTime < FlashTimer)
	{
		Color = HurtColor;
	}
	else if (State == EEnemyState::Attack && StateTimer < WindupTime)
	{
		float Alpha = StateTimer / FMath::Max(WindupTime, 0.01f);
		Color = FMath::Lerp(BaseColor, TelegraphColor, Alpha);
	}
	else if (State == EEnemyState::Dead)
	{
		Color = BaseColor * 0.3f;
	}

	DynamicMaterial->SetVectorParameterValue(FName("BaseColor"), Color);
}