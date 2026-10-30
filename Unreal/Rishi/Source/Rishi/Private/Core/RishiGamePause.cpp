// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiGamePause.h"
#include "GameFramework/PlayerController.h"
#include "Engine/World.h"
#include "Engine/Engine.h"
#include "InputAction.h"
#include "EnhancedInputComponent.h"

ARishiGamePause* ARishiGamePause::GamePauseInstance = nullptr;

ARishiGamePause::ARishiGamePause()
{
	PrimaryActorTick.bCanEverTick = true;
	bReplicates = false;
}

ARishiGamePause* ARishiGamePause::Get()
{
	if (!GamePauseInstance)
	{
		UWorld* World = GEngine ? GEngine->GetWorldContexts()[0].World() : nullptr;
		if (World)
		{
			GamePauseInstance = World->SpawnActor<ARishiGamePause>();
		}
	}
	return GamePauseInstance;
}

void ARishiGamePause::BeginPlay()
{
	Super::BeginPlay();

	GamePauseInstance = this;

	// Bind pause action
	if (APlayerController* PC = UGameplayStatics::GetPlayerController(this, 0))
	{
		if (UEnhancedInputComponent* EnhancedInput = Cast<UEnhancedInputComponent>(PC->InputComponent))
		{
			UInputAction* PauseAction = NewObject<UInputAction>(this, TEXT("PauseAction"));
			PauseAction->AddBinding(FInputActionBinding(FName("Escape"), IE_Pressed, false, false, false, false));
			PauseAction->AddBinding(FInputActionBinding(FName("Gamepad_FaceButton_Start"), IE_Pressed, false, false, false, false));
			EnhancedInput->BindAction(PauseAction, ETriggerEvent::Triggered, this, &ARishiGamePause::OnPauseActionPressed);
		}
	}

	// Listen for focus changes
	FCoreDelegates::ApplicationHasFocusDelegate.AddUObject(this, &ARishiGamePause::OnApplicationFocusChanged);
}

void ARishiGamePause::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	FCoreDelegates::ApplicationHasFocusDelegate.RemoveAll(this);
	GamePauseInstance = nullptr;
	Super::EndPlay(EndPlayReason);
}

void ARishiGamePause::SetPaused(bool bPaused)
{
	if (bIsPaused == bPaused) return;
	if (bIsLocked && bPaused) return; // Can't pause when locked

	bIsPaused = bPaused;

	UWorld* World = GetWorld();
	if (World)
	{
		World->GetWorldSettings()->SetTimeDilation(bPaused ? 0.0f : 1.0f);
		World->GetWorldSettings()->SetPause(bPaused);
	}

	// Pause audio
	if (GEngine && GEngine->GetAudioDeviceManager())
	{
		FAudioDeviceManager* AudioManager = GEngine->GetAudioDeviceManager();
		if (AudioManager)
		{
			AudioManager->SetPause(bPaused);
		}
	}

	OnPauseChanged.Broadcast(bPaused);
}

void ARishiGamePause::OnPauseActionPressed()
{
	if (!bIsLocked && !bIsPaused)
	{
		SetPaused(true);
	}
}

void ARishiGamePause::OnApplicationFocusChanged(bool bHasFocus)
{
	if (!bHasFocus && !GIsEditor && !bIsLocked)
	{
		SetPaused(true);
	}
}