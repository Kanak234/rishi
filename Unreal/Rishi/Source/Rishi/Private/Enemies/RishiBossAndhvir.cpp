// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Enemies/RishiBossAndhvir.h"
#include "Combat/RishiHurtboxComponent.h"
#include "Core/RishiGameAudio.h"
#include "Core/RishiGamePause.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "Kismet/KismetMathLibrary.h"

ARishiBossAndhvir::ARishiBossAndhvir()
{
	// Boss defaults
	DetectRange = 3000.0f;
	LoseRange = 6000.0f;
	MoveSpeed = 320.0f;
	TurnSpeed = 240.0f;
	AttackRange = 260.0f;
	AttackCooldown = 0.6f;
	Damage = 15.0f;
	HitRadius = 160.0f;
	HitReach = 180.0f;
	HurtStunTime = 0.6f;
	bSuperArmor = true;
	KnockbackDecay = 16.0f;
	JyotiReward = 0;
}

void ARishiBossAndhvir::BeginPlay()
{
	Super::BeginPlay();
	BaseMoveSpeed = MoveSpeed;
	
	// Hide telegraph disc initially
	if (TelegraphDisc)
	{
		TelegraphDisc->SetVisibility(false);
	}
	
	// Listen for state changes
	OnStateChanged.AddDynamic(this, &ARishiBossAndhvir::OnStateChangedHandler);
}

void ARishiBossAndhvir::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
	
	UpdatePhase();
	UpdateTelegraph(DeltaTime);
}

void ARishiBossAndhvir::OnStateChangedHandler(EEnemyState PrevState, EEnemyState NewState)
{
	if (NewState == EEnemyState::Attack)
	{
		ChooseNextMove();
	}
	
	if (PrevState == EEnemyState::Attack)
	{
		bInOpening = false;
	}
	
	if (NewState == EEnemyState::Dead)
	{
		if (TelegraphDisc) TelegraphDisc->SetVisibility(false);
		
		// Kill minions
		for (AActor* Minion : ActiveMinions)
		{
			if (Minion && Minion->IsActorInitialized())
			{
				URishiHealthComponent* Health = Minion->FindComponentByClass<URishiHealthComponent>();
				if (Health) Health->TakeDamage(Health->MaxHealth * 10.0f, FHitInfo());
			}
		}
	}
}

void ARishiBossAndhvir::UpdatePhase()
{
	if (GetState() == EEnemyState::Dead || !HealthComponent) return;

	float HealthFraction = HealthComponent->GetHealthPercent();
	int32 DesiredPhase = (HealthFraction <= Phase3HealthThreshold) ? 3 : (HealthFraction <= Phase2HealthThreshold) ? 2 : 1;
	
	if (DesiredPhase <= Phase) return;

	Phase = DesiredPhase;
	
	if (Phase == 3)
	{
		// Rage mode
		MoveSpeed = BaseMoveSpeed * 1.45f;
		BaseWindupScale = 0.7f;
		AttackCooldown = 0.25f;
	}

	// Camera shake and effects
	// URishiGameAudio::Play(ESfx::Boom, GetActorLocation());
	// ARishiGamePause::Get()->SetPaused(false); // shake handled by camera
	
	UE_LOG(LogTemp, Log, TEXT("[BOSS] %s entered Phase %d"), *DisplayName, Phase);
	OnPhaseChanged.Broadcast(Phase);
}

void ARishiBossAndhvir::ChooseNextMove()
{
	AttackCount++;

	if (Phase == 1)
	{
		CurrentMove = (AttackCount % 3 == 0) ? EBossMove::Chakkar : EBossMove::Prahar;
	}
	else if (Phase == 2)
	{
		switch (AttackCount % 3)
		{
		case 0: CurrentMove = EBossMove::BhoomiBhed; break;
		case 1: CurrentMove = EBossMove::Prahar; break;
		default: CurrentMove = EBossMove::Chakkar; break;
		}
	}
	else // Phase 3
	{
		CurrentMove = (AttackCount % 2 == 0) ? EBossMove::BhoomiBhed : EBossMove::Chakkar;
	}

	float WindupScale = BaseWindupScale;

	switch (CurrentMove)
	{
	case EBossMove::Prahar:
		WindupTime = 0.65f * WindupScale;
		ActiveTime = 0.12f;
		RecoveryTime = 0.55f;
		Damage = 15.0f;
		break;
	case EBossMove::Chakkar:
		WindupTime = 0.95f * WindupScale;
		ActiveTime = 0.2f;
		RecoveryTime = 1.3f;
		Damage = 18.0f;
		break;
	case EBossMove::BhoomiBhed:
		WindupTime = 1.15f * WindupScale;
		ActiveTime = 0.1f;
		RecoveryTime = 2.0f;
		Damage = 20.0f;
		break;
	}
}

void ARishiBossAndhvir::PerformAttack()
{
	switch (CurrentMove)
	{
	case EBossMove::Prahar:
		Super::PerformAttack(); // Single target melee
		// URishiGameAudio::Play(ESfx::Swoosh, GetActorLocation());
		break;

	case EBossMove::Chakkar:
		DoAreaHit(GetActorLocation(), SpinRadius, 600.0f);
		// URishiGameAudio::Play(ESfx::Swoosh, GetActorLocation(), 1.0f);
		bInOpening = true;
		break;

	case EBossMove::BhoomiBhed:
		DoAreaHit(GetActorLocation(), SlamRadius, 900.0f);
		// URishiGameAudio::Play(ESfx::Boom, GetActorLocation());
		bInOpening = true;
		if (Phase >= 2) SummonMinions();
		break;
	}
}

void ARishiBossAndhvir::DoAreaHit(const FVector& Center, float Radius, float Knockback)
{
	TArray<FOverlapResult> Overlaps;
	FCollisionQueryParams Params;
	Params.AddIgnoredActor(this);

	GetWorld()->OverlapMultiByChannel(Overlaps, Center + FVector(0, 0, 100.0f), FQuat::Identity, ECC_GameTraceChannel1, FCollisionShape::MakeSphere(Radius), Params);

	for (const FOverlapResult& Overlap : Overlaps)
	{
		AActor* OtherActor = Overlap.GetActor();
		if (!OtherActor) continue;

		URishiHurtboxComponent* Hurtbox = OtherActor->FindComponentByClass<URishiHurtboxComponent>();
		if (!Hurtbox || Hurtbox->GetTeam() != ETeam::Player) continue;

		// For BhoomiBhed, jumping players avoid shockwave
		if (CurrentMove == EBossMove::BhoomiBhed)
		{
			if (OtherActor->GetActorLocation().Z > GetActorLocation().Z + 90.0f) continue;
		}

		FVector Dir = OtherActor->GetActorLocation() - GetActorLocation();
		Dir.Z = 0;
		Dir.Normalize();

		FHitInfo HitInfo;
		HitInfo.Damage = Damage;
		HitInfo.Direction = Dir;
		HitInfo.Knockback = Knockback;
		HitInfo.bGuardBreak = false;
		HitInfo.bIsFinisher = false;
		HitInfo.AttackerTeam = ETeam::Asur;
		HitInfo.Attacker = this;

		Hurtbox->ReceiveHit(HitInfo);
		return; // One hit per attack
	}
}

void ARishiBossAndhvir::SummonMinions()
{
	if (!MinionClass || MinionSpawnPoints.Num() == 0) return;

	int32 AliveCount = 0;
	for (AActor* Minion : ActiveMinions)
	{
		if (Minion && Minion->IsActorInitialized() && !Minion->IsPendingKill()) AliveCount++;
	}

	int32 ToSpawn = FMath::Min(2, MaxMinions - AliveCount);
	for (int32 i = 0; i < ToSpawn; i++)
	{
		int32 Slot = ActiveMinions.Find(nullptr);
		if (Slot == INDEX_NONE)
		{
			ActiveMinions.Add(nullptr);
			Slot = ActiveMinions.Num() - 1;
		}

		if (ActiveMinions[Slot]) ActiveMinions[Slot]->Destroy();

		AActor* SpawnPoint = MinionSpawnPoints[(AttackCount + i) % MinionSpawnPoints.Num()];
		if (!SpawnPoint) continue;

		FActorSpawnParameters SpawnParams;
		SpawnParams.Owner = this;
		AActor* Minion = GetWorld()->SpawnActor<AActor>(MinionClass, SpawnPoint->GetActorLocation(), SpawnPoint->GetActorRotation(), SpawnParams);
		
		if (Minion)
		{
			Minion->SetActorHiddenInGame(false);
			Minion->SetActorEnableCollision(true);
			ActiveMinions[Slot] = Minion;
		}
	}
}

bool ARishiBossAndhvir::FilterHit_Implementation(FHitInfo& HitInfo)
{
	if (bInOpening)
	{
		HitInfo.Damage *= HitInfo.bGuardBreak ? OpeningHeavyMultiplier : OpeningLightMultiplier;
	}
	return true;
}

float ARishiBossAndhvir::GetStunDuration(const FHitInfo& HitInfo)
{
	return (bInOpening && HitInfo.bGuardBreak) ? 0.9f : HurtStunTime;
}

void ARishiBossAndhvir::UpdateTelegraph(float DeltaTime)
{
	if (!TelegraphDisc) return;

	bool bShow = (GetState() == EEnemyState::Attack && StateTimer < WindupTime && CurrentMove != EBossMove::Prahar);
	TelegraphDisc->SetVisibility(bShow);

	if (!bShow) return;

	float Radius = (CurrentMove == EBossMove::Chakkar) ? SpinRadius : SlamRadius;
	float Grow = FMath::Lerp(0.35f, 1.0f, StateTimer / FMath::Max(WindupTime, 0.01f));
	
	FVector Scale = FVector(Radius * 2.0f * Grow / GetActorScale3D().X, 0.01f, Radius * 2.0f * Grow / GetActorScale3D().Z);
	TelegraphDisc->SetWorldScale3D(Scale);
}