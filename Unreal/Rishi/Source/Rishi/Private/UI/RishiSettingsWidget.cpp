// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "UI/RishiSettingsWidget.h"
#include "Components/Slider.h"
#include "Components/Button.h"
#include "Components/ComboBoxString.h"
#include "Components/CheckBox.h"
#include "Components/TextBlock.h"
#include "Core/RishiGameSettings.h"
#include "Engine/Engine.h"

bool URishiSettingsWidget::Initialize()
{
	bool bSuccess = Super::Initialize();
	if (!bSuccess) return false;

	SetupSliders();
	SetupComboBoxes();
	LoadCurrentSettings();

	if (MasterVolumeSlider) MasterVolumeSlider->OnValueChanged.AddDynamic(this, &URishiSettingsWidget::OnMasterVolumeChanged);
	if (MusicVolumeSlider) MusicVolumeSlider->OnValueChanged.AddDynamic(this, &URishiSettingsWidget::OnMusicVolumeChanged);
	if (SfxVolumeSlider) SfxVolumeSlider->OnValueChanged.AddDynamic(this, &URishiSettingsWidget::OnSfxVolumeChanged);
	if (QualityCombo) QualityCombo->OnSelectionChanged.AddDynamic(this, &URishiSettingsWidget::OnQualityChanged);
	if (FullscreenCheckBox) FullscreenCheckBox->OnCheckStateChanged.AddDynamic(this, &URishiSettingsWidget::OnFullscreenChanged);
	if (SubtitlesCheckBox) SubtitlesCheckBox->OnCheckStateChanged.AddDynamic(this, &URishiSettingsWidget::OnSubtitlesChanged);
	if (TextSizeCombo) TextSizeCombo->OnSelectionChanged.AddDynamic(this, &URishiSettingsWidget::OnTextSizeChanged);
	if (CameraShakeCheckBox) CameraShakeCheckBox->OnCheckStateChanged.AddDynamic(this, &URishiSettingsWidget::OnCameraShakeChanged);
	if (BackButton) BackButton->OnClicked.AddDynamic(this, &URishiSettingsWidget::OnBackClicked);

	return true;
}

void URishiSettingsWidget::SetupSliders()
{
	if (MasterVolumeSlider) { MasterVolumeSlider->SetMinValue(0.0f); MasterVolumeSlider->SetMaxValue(1.0f); MasterVolumeSlider->SetStepSize(0.05f); }
	if (MusicVolumeSlider) { MusicVolumeSlider->SetMinValue(0.0f); MusicVolumeSlider->SetMaxValue(1.0f); MusicVolumeSlider->SetStepSize(0.05f); }
	if (SfxVolumeSlider) { SfxVolumeSlider->SetMinValue(0.0f); SfxVolumeSlider->SetMaxValue(1.0f); SfxVolumeSlider->SetStepSize(0.05f); }
}

void URishiSettingsWidget::SetupComboBoxes()
{
	if (QualityCombo)
	{
		QualityCombo->ClearOptions();
		if (GEngine)
		{
			for (int32 i = 0; i < GEngine->GetNumQualityLevels(); i++)
			{
				QualityCombo->AddOption(FString::Printf(TEXT("Quality %d"), i));
			}
		}
	}

	if (TextSizeCombo)
	{
		TextSizeCombo->ClearOptions();
		TextSizeCombo->AddOption(TEXT("Chhota"));
		TextSizeCombo->AddOption(TEXT("Normal"));
		TextSizeCombo->AddOption(TEXT("Bada"));
	}
}

void URishiSettingsWidget::LoadCurrentSettings()
{
	URishiGameSettings* Settings = URishiGameSettings::Get();

	if (MasterVolumeSlider) { MasterVolumeSlider->SetValue(Settings->MasterVolume); if (MasterVolumeText) MasterVolumeText->SetText(FText::FromString(FString::Printf(TEXT("%.0f%%"), Settings->MasterVolume * 100))); }
	if (MusicVolumeSlider) { MusicVolumeSlider->SetValue(Settings->MusicVolume); if (MusicVolumeText) MusicVolumeText->SetText(FText::FromString(FString::Printf(TEXT("%.0f%%"), Settings->MusicVolume * 100))); }
	if (SfxVolumeSlider) { SfxVolumeSlider->SetValue(Settings->SfxVolume); if (SfxVolumeText) SfxVolumeText->SetText(FText::FromString(FString::Printf(TEXT("%.0f%%"), Settings->SfxVolume * 100))); }
	if (QualityCombo) QualityCombo->SetSelectedOption(FString::Printf(TEXT("Quality %d"), Settings->QualityLevel >= 0 ? Settings->QualityLevel : 2));
	if (FullscreenCheckBox) FullscreenCheckBox->SetCheckedState(Settings->bFullscreen ? ECheckBoxState::Checked : ECheckBoxState::Unchecked);
	if (SubtitlesCheckBox) SubtitlesCheckBox->SetCheckedState(Settings->bSubtitles ? ECheckBoxState::Checked : ECheckBoxState::Unchecked);
	if (TextSizeCombo) TextSizeCombo->SetSelectedOption(Settings->TextSize == 0 ? TEXT("Chhota") : (Settings->TextSize == 2 ? TEXT("Bada") : TEXT("Normal")));
	if (CameraShakeCheckBox) CameraShakeCheckBox->SetCheckedState(Settings->bCameraShake ? ECheckBoxState::Checked : ECheckBoxState::Unchecked);
}

void URishiSettingsWidget::ApplySettings()
{
	URishiGameSettings* Settings = URishiGameSettings::Get();
	Settings->ApplySettings();
}

void URishiSettingsWidget::OnMasterVolumeChanged(float Value)
{
	URishiGameSettings::Get()->MasterVolume = Value;
	if (MasterVolumeText) MasterVolumeText->SetText(FText::FromString(FString::Printf(TEXT("%.0f%%"), Value * 100)));
	ApplySettings();
}

void URishiSettingsWidget::OnMusicVolumeChanged(float Value)
{
	URishiGameSettings::Get()->MusicVolume = Value;
	if (MusicVolumeText) MusicVolumeText->SetText(FText::FromString(FString::Printf(TEXT("%.0f%%"), Value * 100)));
	ApplySettings();
}

void URishiSettingsWidget::OnSfxVolumeChanged(float Value)
{
	URishiGameSettings::Get()->SfxVolume = Value;
	if (SfxVolumeText) SfxVolumeText->SetText(FText::FromString(FString::Printf(TEXT("%.0f%%"), Value * 100)));
	ApplySettings();
}

void URishiSettingsWidget::OnQualityChanged(FString SelectedItem, ESelectInfo::Type SelectionType)
{
	int32 Quality = 2;
	if (SelectedItem.StartsWith(TEXT("Quality ")))
	{
		Quality = FCString::Atoi(*SelectedItem.RightChop(8));
	}
	URishiGameSettings::Get()->QualityLevel = Quality;
	ApplySettings();
}

void URishiSettingsWidget::OnFullscreenChanged(bool bChecked)
{
	URishiGameSettings::Get()->bFullscreen = bChecked;
	ApplySettings();
}

void URishiSettingsWidget::OnSubtitlesChanged(bool bChecked)
{
	URishiGameSettings::Get()->bSubtitles = bChecked;
	ApplySettings();
}

void URishiSettingsWidget::OnTextSizeChanged(FString SelectedItem, ESelectInfo::Type SelectionType)
{
	int32 Size = 1;
	if (SelectedItem == TEXT("Chhota")) Size = 0;
	else if (SelectedItem == TEXT("Bada")) Size = 2;
	URishiGameSettings::Get()->TextSize = Size;
	ApplySettings();
}

void URishiSettingsWidget::OnCameraShakeChanged(bool bChecked)
{
	URishiGameSettings::Get()->bCameraShake = bChecked;
	ApplySettings();
}

void URishiSettingsWidget::OnBackClicked()
{
	ApplySettings();
	RemoveFromParent();
}