// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "RishiGameSettings.generated.h"

UCLASS(BlueprintType, Blueprintable)
class RISHI_API URishiGameSettings : public UObject
{
	GENERATED_BODY()

public:
	static URishiGameSettings* Get();

	UFUNCTION(BlueprintCallable, Category = "Settings")
	void ApplySettings();

	UFUNCTION(BlueprintCallable, Category = "Settings")
	void LoadSettings();

	UFUNCTION(BlueprintCallable, Category = "Settings")
	void SaveSettings();

	// Audio
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Audio", meta = (ClampMin = "0.0", ClampMax = "1.0"))
	float MasterVolume = 0.9f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Audio", meta = (ClampMin = "0.0", ClampMax = "1.0"))
	float MusicVolume = 0.6f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Audio", meta = (ClampMin = "0.0", ClampMax = "1.0"))
	float SfxVolume = 0.9f;

	// Graphics
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Graphics")
	int32 QualityLevel = -1; // -1 = auto (middle preset)

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Graphics")
	bool bFullscreen = true;

	// Accessibility
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Accessibility")
	bool bSubtitles = true;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Accessibility")
	bool bCameraShake = true;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Accessibility")
	int32 TextSize = 1; // 0 = small, 1 = normal, 2 = large

	UFUNCTION(BlueprintPure, Category = "Settings")
	float GetTextScale() const;

private:
	static TObjectPtr<URishiGameSettings> SettingsInstance;
};