// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "RishiSettingsWidget.generated.h"

class USlider;
class UButton;
class UComboBoxString;
class UCheckBox;
class UTextBlock;

UCLASS()
class RISHI_API URishiSettingsWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	virtual bool Initialize() override;

private:
	UPROPERTY(meta = (BindWidget))
	TObjectPtr<USlider> MasterVolumeSlider;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<USlider> MusicVolumeSlider;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<USlider> SfxVolumeSlider;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UComboBoxString> QualityCombo;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UCheckBox> FullscreenCheckBox;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UCheckBox> SubtitlesCheckBox;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UComboBoxString> TextSizeCombo;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UCheckBox> CameraShakeCheckBox;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UButton> BackButton;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UTextBlock> MasterVolumeText;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UTextBlock> MusicVolumeText;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UTextBlock> SfxVolumeText;

	void SetupSliders();
	void SetupComboBoxes();
	void LoadCurrentSettings();
	void ApplySettings();

	UFUNCTION()
	void OnMasterVolumeChanged(float Value);

	UFUNCTION()
	void OnMusicVolumeChanged(float Value);

	UFUNCTION()
	void OnSfxVolumeChanged(float Value);

	UFUNCTION()
	void OnQualityChanged(FString SelectedItem, ESelectInfo::Type SelectionType);

	UFUNCTION()
	void OnFullscreenChanged(bool bChecked);

	UFUNCTION()
	void OnSubtitlesChanged(bool bChecked);

	UFUNCTION()
	void OnTextSizeChanged(FString SelectedItem, ESelectInfo::Type SelectionType);

	UFUNCTION()
	void OnCameraShakeChanged(bool bChecked);

	UFUNCTION()
	void OnBackClicked();
};