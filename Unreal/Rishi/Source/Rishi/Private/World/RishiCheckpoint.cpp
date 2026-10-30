// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiCheckpoint.h"
#include "Components/BoxComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiWeaponManagerComponent.h"
#include "Core/RishiSaveSystem.h"
#include "Core/RishiGameFlow.h"
#include "Core/RishiGameAudio.h"
#include "World/RishiLevelDirector.h"

ARishiCheckpoint::ARishiCheckpoint()
{
	PrimaryActorTick.bCanEverTick = false;

	TriggerVolume = CreateDefaultSubobject<UBoxComponent>(TEXT("TriggerVolume"));
	RootComponent = TriggerVolume;
	TriggerVolume->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
	TriggerVolume->SetCollisionResponseToAllChannels(ECR_Ignore);
	TriggerVolume->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);
	TriggerVolume->SetGenerateOverlapEvents(true);
	TriggerVolume->SetBoxExtent(FVector(300.0f, 150.0f, 200.0f));
}

void ARishiCheckpoint::BeginPlay()
{
	Super::BeginPlay();

	TriggerVolume->OnComponentBeginOverlap.AddDynamic(this, &ARishiCheckpoint::OnOverlapBegin);

	if (LitVisual)
	{
		LitVisual->SetActorHiddenInGame(true);
	}
}

void ARishiCheckpoint::LightSilently()
{
	bIsLit = true;
	if (LitVisual)
	{
		LitVisual->SetActorHiddenInGame(false);
	}
}

void ARishiCheckpoint::OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	if (bIsLit) return;

	ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(OtherActor);
	if (!Player) return;

	LightSilently();

	URishiHealthComponent* Health = Player->FindComponentByClass<URishiHealthComponent>();
	if (Health) Health->ResetFull();

	// URishiGameAudio::Play(ESfx::Deepak, GetActorLocation());

	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	FSaveData& Save = SaveSystem->GetMutableSave();

	FString Message = TEXT("Deepak jala — yahan se dobara shuru hoga");

	// Try upgrade
	URishiWeaponManagerComponent* WeaponManager = Player->FindComponentByClass<URishiWeaponManagerComponent>();
	if (WeaponManager)
	{
		int32 CurrentLevel = WeaponManager->GetCurrentLevel();
		if (CurrentLevel < UpgradeCosts.Num() && Save.Jyoti >= UpgradeCosts[CurrentLevel])
		{
			if (WeaponManager->UpgradeWeapon(WeaponManager->GetCurrentWeapon()))
			{
				Save.Jyoti -= UpgradeCosts[CurrentLevel];
				Message = FString::Printf(TEXT("%s ka ashirvad badha — Lv %d (damage x%.2f)"), 
					*WeaponManager->GetCurrentWeapon().ToString(), CurrentLevel + 1, 
					CurrentLevel == 0 ? 1.25f : 1.5f);
				// URishiGameAudio::Play(ESfx::Pickup, GetActorLocation());
			}
		}
	}

	Save.Checkpoint = FMath::Max(Save.Checkpoint, CheckpointIndex);
	Save.bHasDhanush = WeaponManager ? WeaponManager->IsUnlocked(EWeaponId::Dhanush) : false;
	
	if (WeaponManager)
	{
		Save.TrishulLevel = WeaponManager->GetWeaponLevel(EWeaponId::Trishul);
		Save.DhanushLevel = WeaponManager->GetWeaponLevel(EWeaponId::Dhanush);
	}

	SaveSystem->SaveGame();
	OnMessage.Broadcast(Message);
}