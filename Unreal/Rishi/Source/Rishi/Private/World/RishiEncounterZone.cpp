// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiEncounterZone.h"
#include "Components/BoxComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "Combat/RishiHealthComponent.h"
#include "Enemies/RishiEnemyAI.h"
#include "Core/RishiGameAudio.h"
#include "World/RishiLevelDirector.h"

ARishiEncounterZone::ARishiEncounterZone()
{
	PrimaryActorTick.bCanEverTick = true;

	TriggerVolume = CreateDefaultSubobject<UBoxComponent>(TEXT("TriggerVolume"));
	RootComponent = TriggerVolume;
	TriggerVolume->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
	TriggerVolume->SetCollisionResponseToAllChannels(ECR_Ignore);
	TriggerVolume->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);
	TriggerVolume->SetGenerateOverlapEvents(true);
	TriggerVolume->SetBoxExtent(FVector(500.0f, 500.0f, 200.0f));
}

void ARishiEncounterZone::BeginPlay()
{
	Super::BeginPlay();

	TriggerVolume->OnComponentBeginOverlap.AddDynamic(this, &ARishiEncounterZone::OnOverlapBegin);

	// Hide enemies initially
	for (const FEnemyWave& Wave : Waves)
	{
		for (ARishiEnemyAI* Enemy : Wave.Enemies)
		{
			if (Enemy)
			{
				Enemy->SetActorHiddenInGame(true);
				Enemy->SetActorEnableCollision(false);
			}
		}
	}

	SetGatesActive(false);
}

void ARishiEncounterZone::OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (bStarted) return;

	ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(OtherActor);
	if (!Player) return;

	bStarted = true;

	if (bIsBoss)
	{
		ARishiLevelDirector* Director = ARishiLevelDirector::Get();
		if (Director)
		{
			Director->BeginBoss(this, [this]() { BeginEncounter(); });
		}
	}
	else
	{
		BeginEncounter();
	}
}

void ARishiEncounterZone::BeginEncounter()
{
	SetGatesActive(true);
	OnStarted.Broadcast(this);
	StartNextWave();
}

void ARishiEncounterZone::StartNextWave()
{
	CurrentWaveIndex++;
	
	if (CurrentWaveIndex >= Waves.Num())
	{
		FinishEncounter();
		return;
	}

	CurrentWaveEnemies.Empty();
	
	for (ARishiEnemyAI* Enemy : Waves[CurrentWaveIndex].Enemies)
	{
		if (Enemy)
		{
			Enemy->SetActorHiddenInGame(false);
			Enemy->SetActorEnableCollision(true);
			
			URishiHealthComponent* Health = Enemy->FindComponentByClass<URishiHealthComponent>();
			if (Health) CurrentWaveEnemies.Add(Health);
		}
	}

	if (CurrentWaveIndex > 0)
	{
		// URishiGameAudio::Play(ESfx::Boom, GetActorLocation(), 0.5f);
	}
}

void ARishiEncounterZone::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (!bStarted || bCleared) return;

	CheckWaveComplete();
}

void ARishiEncounterZone::CheckWaveComplete()
{
	for (URishiHealthComponent* Health : CurrentWaveEnemies)
	{
		if (Health && !Health->IsDead()) return;
	}

	StartNextWave();
}

void ARishiEncounterZone::FinishEncounter()
{
	bCleared = true;
	SetGatesActive(false);
	
	// URishiGameAudio::Play(ESfx::Deepak, GetActorLocation(), 0.6f);
	OnCleared.Broadcast(this);
}

void ARishiEncounterZone::SetGatesActive(bool bActive)
{
	for (AActor* Gate : Gates)
	{
		if (Gate)
		{
			Gate->SetActorHiddenInGame(!bActive);
			Gate->SetActorEnableCollision(bActive);
		}
	}
}

int32 ARishiEncounterZone::GetRemainingEnemies() const
{
	int32 Count = 0;
	for (URishiHealthComponent* Health : CurrentWaveEnemies)
	{
		if (Health && !Health->IsDead()) Count++;
	}
	return Count;
}