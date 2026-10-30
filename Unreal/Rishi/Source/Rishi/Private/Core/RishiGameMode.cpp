// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiGameMode.h"
#include "Player/RishiPlayerCharacter.h"
#include "UI/RishiHUD.h"
#include "World/RishiLevelDirector.h"
#include "Core/RishiGamePause.h"
#include "Core/RishiGameSettings.h"
#include "Core/RishiSaveSystem.h"
#include "Core/RishiGameFlow.h"
#include "UI/RishiMainMenuWidget.h"
#include "UI/RishiPauseMenuWidget.h"
#include "UI/RishiSettingsWidget.h"
#include "UI/RishiStoryCutsceneWidget.h"
#include "Core/RishiGameAudio.h"

ARishiGameMode::ARishiGameMode()
{
	DefaultPawnClass = ARishiPlayerCharacter::StaticClass();
	HUDClass = ARishiHUD::StaticClass();
	PlayerControllerClass = APlayerController::StaticClass();
}

void ARishiGameMode::BeginPlay()
{
	Super::BeginPlay();

	// Apply settings on game start
	URishiGameSettings::Get()->ApplySettings();

	// Spawn global systems
	GetWorld()->SpawnActor<ARishiGamePause>();
	URishiGameAudio::Get(); // Initialize audio singleton

	// Check if this is the main menu
	FString CurrentLevel = UGameplayStatics::GetCurrentLevelName(GetWorld(), true);
	if (CurrentLevel == TEXT("MainMenu"))
	{
		// Show main menu
		if (URishiMainMenuWidget::StaticClass())
		{
			UUserWidget* MenuWidget = CreateWidget<UUserWidget>(GetWorld(), URishiMainMenuWidget::StaticClass());
			if (MenuWidget) MenuWidget->AddToViewport();
		}
	}
}