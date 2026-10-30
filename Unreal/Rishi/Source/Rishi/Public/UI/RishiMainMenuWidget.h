// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "RishiMainMenuWidget.generated.h"

class UButton;
class UVerticalBox;
class UTextBlock;
class UWidgetAnimation;

UCLASS()
class RISHI_API URishiMainMenuWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual bool Initialize() override;

protected:
	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> NewGameButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> ContinueButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> MissionsButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> SettingsButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> ControlsButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> CreditsButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> QuitButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UVerticalBox> MissionsList;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UWidgetAnimation> FadeInAnim;

private:
	UFUNCTION()
	void OnNewGameClicked();

	UFUNCTION()
	void OnContinueClicked();

	UFUNCTION()
	void OnMissionsClicked();

	UFUNCTION()
	void OnSettingsClicked();

	UFUNCTION()
	void OnControlsClicked();

	UFUNCTION()
	void OnCreditsClicked();

	UFUNCTION()
	void OnQuitClicked();

	void PopulateMissionsList();
};