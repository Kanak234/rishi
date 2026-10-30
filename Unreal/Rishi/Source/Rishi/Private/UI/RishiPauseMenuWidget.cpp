// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "UI/RishiPauseMenuWidget.h"
#include "Components/Button.h"
#include "Core/RishiGamePause.h"
#include "Core/RishiGameFlow.h"

bool URishiPauseMenuWidget::Initialize()
{
	bool bSuccess = Super::Initialize();
	if (!bSuccess) return false;

	if (ResumeButton) ResumeButton->OnClicked.AddDynamic(this, &URishiPauseMenuWidget::OnResumeClicked);
	if (SettingsButton) SettingsButton->OnClicked.AddDynamic(this, &URishiPauseMenuWidget::OnSettingsClicked);
	if (ControlsButton) ControlsButton->OnClicked.AddDynamic(this, &URishiPauseMenuWidget::OnControlsClicked);
	if (MainMenuButton) MainMenuButton->OnClicked.AddDynamic(this, &URishiPauseMenuWidget::OnMainMenuClicked);

	return true;
}

void URishiPauseMenuWidget::OnResumeClicked()
{
	ARishiGamePause::Get()->SetPaused(false);
	RemoveFromParent();
}

void URishiPauseMenuWidget::OnSettingsClicked()
{
	// Open settings widget
}

void URishiPauseMenuWidget::OnControlsClicked()
{
	// Open controls widget
}

void URishiPauseMenuWidget::OnMainMenuClicked()
{
	ARishiGamePause::Get()->SetPaused(false);
	URishiGameFlow::Get()->ReturnToMenu();
}