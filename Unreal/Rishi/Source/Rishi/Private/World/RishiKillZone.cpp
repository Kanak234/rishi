// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiKillZone.h"
#include "Components/BoxComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "World/RishiCheckpoint.h"
#include "Combat/RishiHealthComponent.h"
#include "Core/RishiGameAudio.h"

ARishiKillZone::ARishiKillZone()
{
	PrimaryActorTick.bCanEverTick = false;

	TriggerVolume = CreateDefaultSubobject<UBoxComponent>(TEXT("TriggerVolume"));
	RootComponent = TriggerVolume;
	TriggerVolume->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
	TriggerVolume->SetCollisionResponseToAllChannels(ECR_Ignore);
	TriggerVolume->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);
	TriggerVolume->SetGenerateOverlapEvents(true);
	TriggerVolume->SetBoxExtent(FVector(10000.0f, 10000.0f, 200.0f));
}

void ARishiKillZone::BeginPlay()
{
	Super::BeginPlay();

	TriggerVolume->OnComponentBeginOverlap.AddDynamic(this, &ARishiKillZone::OnOverlapBegin);
}

void ARishiKillZone::OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(OtherActor);
	if (!Player) return;

	URishiHealthComponent* Health = Player->FindComponentByClass<URishiHealthComponent>();
	if (Health)
	{
		// Apply damage but don't kill instantly - let checkpoint handle it
		Health->TakeDamage(10.0f, FHitInfo()); // Small damage to trigger hit reaction
	}

	FVector RespawnLocation;
	FRotator RespawnRotation = FRotator::ZeroRotator;

	if (bUseCheckpointRespawn)
	{
		// Find latest checkpoint
		ARishiCheckpoint* LatestCP = nullptr;
		for (TActorIterator<ARishiCheckpoint> It(GetWorld()); It; ++It)
		{
			if (It->GetIndex() > (LatestCP ? LatestCP->GetIndex() : -1))
			{
				LatestCP = *It;
			}
		}

		if (LatestCP)
		{
			RespawnLocation = LatestCP->GetActorLocation() + FVector(0, 0, 50.0f);
			RespawnRotation = LatestCP->GetActorRotation();
		}
		else if (RespawnPoint)
		{
			RespawnLocation = RespawnPoint->GetActorLocation();
			RespawnRotation = RespawnPoint->GetActorRotation();
		}
		else
		{
			RespawnLocation = GetActorLocation();
		}
	}
	else if (RespawnPoint)
	{
		RespawnLocation = RespawnPoint->GetActorLocation();
		RespawnRotation = RespawnPoint->GetActorRotation();
	}
	else
	{
		RespawnLocation = GetActorLocation();
	}

	Player->TeleportTo(RespawnLocation, RespawnRotation);

	if (Health) Health->ResetFull();

	// URishiGameAudio::Play(ESfx::Deepak, RespawnLocation);
}