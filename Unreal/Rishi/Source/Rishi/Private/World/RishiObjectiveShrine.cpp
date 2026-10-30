// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiObjectiveShrine.h"
#include "Components/BoxComponent.h"
#include "Player/RishiPlayerCharacter.h"
#include "Core/RishiInputComponent.h"
#include "Core/RishiGameAudio.h"
#include "World/RishiShrineGate.h"

ARishiObjectiveShrine::ARishiObjectiveShrine()
{
	PrimaryActorTick.bCanEverTick = true;

	TriggerVolume = CreateDefaultSubobject<UBoxComponent>(TEXT("TriggerVolume"));
	RootComponent = TriggerVolume;
	TriggerVolume->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
	TriggerVolume->SetCollisionResponseToAllChannels(ECR_Ignore);
	TriggerVolume->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);
	TriggerVolume->SetGenerateOverlapEvents(true);
	TriggerVolume->SetBoxExtent(FVector(200.0f, 150.0f, 200.0f));
}

void ARishiObjectiveShrine::BeginPlay()
{
	Super::BeginPlay();

	TriggerVolume->OnComponentBeginOverlap.AddDynamic(this, &ARishiObjectiveShrine::OnOverlapBegin);
	TriggerVolume->OnComponentEndOverlap.AddDynamic(this, &ARishiObjectiveShrine::OnOverlapEnd);

	if (LitVisual)
	{
		LitVisual->SetActorHiddenInGame(true);
	}
}

void ARishiObjectiveShrine::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (CurrentInteractor && !bIsLit)
	{
		ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(CurrentInteractor);
		URishiInputComponent* Input = Player ? Player->FindComponentByClass<URishiInputComponent>() : nullptr;

		// Check if still holding interact or pressing F/Space
		bool bHolding = Input && (Input->WasInteractPressed() || Input->GetMoveValue().Y > 0); // Simplified

		if (bHolding)
		{
			Progress = FMath::Clamp(Progress + DeltaTime / InteractHoldTime, 0.0f, 1.0f);
			if (Progress >= 1.0f)
			{
				Interact(CurrentInteractor);
			}
		}
		else
		{
			Progress = FMath::Max(0.0f, Progress - DeltaTime * 2.0f);
		}
	}
}

void ARishiObjectiveShrine::Interact(AActor* Interactor)
{
	if (bIsLit) return;

	bIsLit = true;
	Progress = 0.0f;
	CurrentInteractor = nullptr;

	if (LitVisual)
	{
		LitVisual->SetActorHiddenInGame(false);
	}

	// URishiGameAudio::Play(ESfx::Deepak, GetActorLocation());
	OnLit.Broadcast(this);

	// Notify shrine gate
	ARishiShrineGate::NotifyShrineLit();
}

void ARishiObjectiveShrine::OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
	ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(OtherActor);
	if (Player && !bIsLit)
	{
		CurrentInteractor = OtherActor;
		Progress = 0.0f;
	}
}

void ARishiObjectiveShrine::OnOverlapEnd(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex)
{
	if (CurrentInteractor == OtherActor)
	{
		CurrentInteractor = nullptr;
		Progress = 0.0f;
	}
}