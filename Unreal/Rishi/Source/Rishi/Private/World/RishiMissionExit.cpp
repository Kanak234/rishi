// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiMissionExit.h"
#include "Components/BoxComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "World/RishiLevelDirector.h"

ARishiMissionExit::ARishiMissionExit()
{
	PrimaryActorTick.bCanEverTick = false;

	TriggerVolume = CreateDefaultSubobject<UBoxComponent>(TEXT("TriggerVolume"));
	RootComponent = TriggerVolume;
	TriggerVolume->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
	TriggerVolume->SetCollisionResponseToAllChannels(ECR_Ignore);
	TriggerVolume->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);
	TriggerVolume->SetGenerateOverlapEvents(true);
	TriggerVolume->SetBoxExtent(FVector(250.0f, 200.0f, 75.0f));
}

void ARishiMissionExit::BeginPlay()
{
	Super::BeginPlay();

	TriggerVolume->OnComponentBeginOverlap.AddDynamic(this, &ARishiMissionExit::OnOverlapBegin);
}

void ARishiMissionExit::OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(OtherActor);
	if (!Player) return;

	ARishiLevelDirector* Director = ARishiLevelDirector::Get();
	if (Director)
	{
		Director->CompleteMission();
	}
}