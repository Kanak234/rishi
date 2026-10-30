// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiShrineGate.h"
#include "World/RishiObjectiveShrine.h"
#include "Core/RishiGameAudio.h"
#include "Player/RishiPlayerCharacter.h"
#include "UI/RishiHUD.h"

ARishiShrineGate::ARishiShrineGate()
{
	PrimaryActorTick.bCanEverTick = false;
}

void ARishiShrineGate::BeginPlay()
{
	Super::BeginPlay();

	for (AActor* Wall : Walls)
	{
		if (Wall) Wall->SetActorHiddenInGame(false);
	}

	if (!ActiveGate) ActiveGate = this;

	for (ARishiObjectiveShrine* Shrine : Shrines)
	{
		if (Shrine)
		{
			Shrine->OnLit.AddDynamic(this, &ARishiShrineGate::CheckAllLit);
		}
	}
}

void ARishiShrineGate::NotifyShrineLit()
{
	for (TActorIterator<ARishiShrineGate> It(GetWorld()); It; ++It)
	{
		It->CheckAllLit();
	}
}

void ARishiShrineGate::CheckAllLit()
{
	if (bIsOpen) return;

	int32 Lit = 0;
	for (ARishiObjectiveShrine* Shrine : Shrines)
	{
		if (Shrine && Shrine->IsLit()) Lit++;
	}

	if (Lit >= Shrines.Num())
	{
		bIsOpen = true;
		for (AActor* Wall : Walls)
		{
			if (Wall) Wall->SetActorHiddenInGame(true);
		}

		// URishiGameAudio::Play(ESfx::Boom, GetActorLocation(), 0.6f);
		
		FString Message = FString::Printf(TEXT("Saare %s jal gaye — raasta khul gaya!"), *Label);
		ARishiHUD::ShowHintStatic(Message, 4.0f);

		if (ActiveGate == this) ActiveGate = nullptr;

		// Find next gate
		for (TActorIterator<ARishiShrineGate> It(GetWorld()); It; ++It)
		{
			if (!It->bIsOpen)
			{
				ActiveGate = *It;
				break;
			}
		}

		OnOpened.Broadcast();
	}
}

int32 ARishiShrineGate::GetLitCount() const
{
	int32 Count = 0;
	for (ARishiObjectiveShrine* Shrine : Shrines)
	{
		if (Shrine && Shrine->IsLit()) Count++;
	}
	return Count;
}

ARishiShrineGate* ARishiShrineGate::ActiveGate = nullptr;