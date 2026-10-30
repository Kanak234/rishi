// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiGameSettings.h"
#include "Engine/Engine.h"
#include "GameUserSettings.h"
#include "Kismet/GameplayStatics.h"
#include "AudioDevice.h"

TObjectPtr<URishiGameSettings> URishiGameSettings::SettingsInstance = nullptr;

URishiGameSettings* URishiGameSettings::Get()
{
	if (!SettingsInstance)
	{
		SettingsInstance = NewObject<URishiGameSettings>(GetTransientPackage(), URishiGameSettings::StaticClass(), NAME_None, RF_Transient | RF_StrongRefOnFrame);
		SettingsInstance->LoadSettings();
	}
	return SettingsInstance;
}

void URishiGameSettings::LoadSettings()
{
	if (GConfig)
	{
		GConfig->GetFloat(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("MasterVolume"), MasterVolume, GGameIni);
		GConfig->GetFloat(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("MusicVolume"), MusicVolume, GGameIni);
		GConfig->GetFloat(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("SfxVolume"), SfxVolume, GGameIni);
		GConfig->GetInt(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("QualityLevel"), QualityLevel, GGameIni);
		GConfig->GetBool(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("Fullscreen"), bFullscreen, GGameIni);
		GConfig->GetBool(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("Subtitles"), bSubtitles, GGameIni);
		GConfig->GetBool(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("CameraShake"), bCameraShake, GGameIni);
		GConfig->GetInt(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("TextSize"), TextSize, GGameIni);
	}
}

void URishiGameSettings::SaveSettings()
{
	if (GConfig)
	{
		GConfig->SetFloat(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("MasterVolume"), MasterVolume, GGameIni);
		GConfig->SetFloat(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("MusicVolume"), MusicVolume, GGameIni);
		GConfig->SetFloat(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("SfxVolume"), SfxVolume, GGameIni);
		GConfig->SetInt(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("QualityLevel"), QualityLevel, GGameIni);
		GConfig->SetBool(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("Fullscreen"), bFullscreen, GGameIni);
		GConfig->SetBool(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("Subtitles"), bSubtitles, GGameIni);
		GConfig->SetBool(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("CameraShake"), bCameraShake, GGameIni);
		GConfig->SetInt(TEXT("/Script/Rishi.RishiGameSettings"), TEXT("TextSize"), TextSize, GGameIni);
		GConfig->Flush(false, GGameIni);
	}
}

void URishiGameSettings::ApplySettings()
{
	// Audio
	if (GEngine && GEngine->GetAudioDeviceManager())
	{
		FAudioDeviceManager* AudioDeviceManager = GEngine->GetAudioDeviceManager();
		if (AudioDeviceManager)
		{
			AudioDeviceManager->SetMasterVolume(MasterVolume);
		}
	}

	// Graphics Quality
	if (QualityLevel >= 0)
	{
		UGameUserSettings* UserSettings = UGameUserSettings::GetGameUserSettings();
		if (UserSettings)
		{
			int32 NumQualityLevels = GEngine ? GEngine->GetNumQualityLevels() : 6;
			int32 ClampedQuality = FMath::Clamp(QualityLevel, 0, NumQualityLevels - 1);
			UserSettings->SetOverallScalabilityLevel(ClampedQuality);
			UserSettings->SetFullscreenMode(bFullscreen ? EWindowMode::Fullscreen : EWindowMode::Windowed);
			UserSettings->ApplySettings(false);
		}
	}

	// Save to config
	SaveSettings();
}

float URishiGameSettings::GetTextScale() const
{
	switch (TextSize)
	{
	case 0: return 0.85f;
	case 2: return 1.25f;
	default: return 1.0f;
	}
}