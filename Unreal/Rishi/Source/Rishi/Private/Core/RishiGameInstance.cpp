// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiGameInstance.h"
#include "Core/RishiGameSettings.h"
#include "Core/RishiSaveSystem.h"
#include "Core/RishiGameAudio.h"

URishiGameInstance::URishiGameInstance()
{
}

void URishiGameInstance::Init()
{
	Super::Init();

	// Initialize global systems
	URishiGameSettings::Get()->ApplySettings();
	URishiSaveSystem::Get()->LoadGame();
	URishiGameAudio::Get(); // Initialize audio
}

void URishiGameInstance::Shutdown()
{
	Super::Shutdown();
}