// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "UI/RishiMainMenuWidget.h"
#include "Components/Button.h"
#include "Components/VerticalBox.h"
#include "Components/TextBlock.h"
#include "Kismet/GameplayStatics.h"
#include "Core/RishiGameFlow.h"
#include "Core/RishiSaveSystem.h"
#include "World/RishiLevelDirector.h"

bool URishiMainMenuWidget::Initialize()
{
	bool bSuccess = Super::Initialize();
	if (!bSuccess) return false;

	if (NewGameButton) NewGameButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnNewGameClicked);
	if (ContinueButton) ContinueButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnContinueClicked);
	if (MissionsButton) MissionsButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnMissionsClicked);
	if (SettingsButton) SettingsButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnSettingsClicked);
	if (ControlsButton) ControlsButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnControlsClicked);
	if (CreditsButton) CreditsButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnCreditsClicked);
	if (QuitButton) QuitButton->OnClicked.AddDynamic(this, &URishiMainMenuWidget::OnQuitClicked);

	// Check for save file
	if (ContinueButton)
	{
		ContinueButton->SetVisibility(URishiSaveSystem::Get()->HasSave() ? ESlateVisibility::Visible : ESlateVisibility::Collapsed);
	}

	if (FadeInAnim) PlayAnimation(FadeInAnim);

	return true;
}

void URishiMainMenuWidget::OnNewGameClicked()
{
	URishiGameFlow::Get()->NewGame();
}

void URishiMainMenuWidget::OnContinueClicked()
{
	URishiGameFlow::Get()->ContinueGame();
}

void URishiMainMenuWidget::OnMissionsClicked()
{
	PopulateMissionsList();
	if (MissionsList) MissionsList->SetVisibility(ESlateVisibility::Visible);
}

void URishiMainMenuWidget::OnSettingsClicked()
{
	// Open settings widget
}

void URishiMainMenuWidget::OnControlsClicked()
{
	// Open controls widget
}

void URishiMainMenuWidget::OnCreditsClicked()
{
	// Open credits widget
}

void URishiMainMenuWidget::OnQuitClicked()
{
	UKismetSystemLibrary::QuitGame(GetWorld(), nullptr, EQuitPreference::Quit, false);
}

void URishiMainMenuWidget::PopulateMissionsList()
{
	if (!MissionsList) return;
	MissionsList->ClearChildren();

	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	SaveSystem->LoadGame();
	int32 Unlocked = FMath::Clamp(SaveSystem->GetCurrentSave().Unlocked, 0, 7);

	for (int32 i = 0; i <= Unlocked; i++)
	{
		const FMissionDef& Mission = ARishiLevelDirector::MissionDefs[i];
		
		UTextBlock* MissionText = NewObject<UTextBlock>(MissionsList);
		MissionText->SetText(FText::FromString(FString::Printf(TEXT("%d. %s  ·  %s"), i + 1, *Mission.Title, *Mission.Level)));
		MissionText->SetFont(FSlateFontInfo(FCoreStyle::GetDefaultFontStyle("Regular", 24)));
		MissionText->SetColorAndOpacity(FLinearColor::White);
		
		UButton* MissionButton = NewObject<UButton>(MissionsList);
		MissionButton->AddChild(MissionText);
		int32 CapturedIndex = i;
		MissionButton->OnClicked.AddLambda([this, CapturedIndex]() { URishiGameFlow::Get()->StartMission(CapturedIndex); });
		
		MissionsList->AddChild(MissionButton);
	}

	// Back button
	UTextBlock* BackText = NewObject<UTextBlock>(MissionsList);
	BackText->SetText(FText::FromString(TEXT("Wapas")));
	BackText->SetFont(FSlateFontInfo(FCoreStyle::GetDefaultFontStyle("Regular", 24)));
	BackText->SetColorAndOpacity(FLinearColor::White);
	
	UButton* BackButton = NewObject<UButton>(MissionsList);
	BackButton->AddChild(BackText);
	BackButton->OnClicked.AddLambda([this]() { if (MissionsList) MissionsList->SetVisibility(ESlateVisibility::Collapsed); });
	
	MissionsList->AddChild(BackButton);
}