// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiSaveSystem.h"
#include "Misc/Paths.h"
#include "Serialization/JsonSerializer.h"
#include "Dom/JsonObject.h"
#include "HAL/PlatformFilemanager.h"

URishiSaveSystem* URishiSaveSystem::SaveSystemInstance = nullptr;

URishiSaveSystem::URishiSaveSystem()
{
	SaveFilePath = FPaths::Combine(FPaths::ProjectSavedDir(), TEXT("rishi_save.json"));
}

URishiSaveSystem* URishiSaveSystem::Get()
{
	if (!SaveSystemInstance)
	{
		SaveSystemInstance = NewObject<URishiSaveSystem>(GetTransientPackage(), URishiSaveSystem::StaticClass(), NAME_None, RF_Transient | RF_StrongRefOnFrame);
	}
	return SaveSystemInstance;
}

void URishiSaveSystem::NewGame()
{
	CurrentSave = FSaveData();
	SaveGame();
}

bool URishiSaveSystem::LoadGame()
{
	if (!FPlatformFileManager::Get().GetPlatformFile().FileExists(*SaveFilePath))
	{
		CurrentSave = FSaveData();
		return false;
	}

	FString JsonString;
	if (!FFileHelper::LoadFileToString(JsonString, *SaveFilePath))
	{
		CurrentSave = FSaveData();
		return false;
	}

	TSharedRef<TJsonReader<>> Reader = TJsonReaderFactory<>::Create(JsonString);
	TSharedPtr<FJsonObject> JsonObject;
	if (!FJsonSerializer::Deserialize(Reader, JsonObject) || !JsonObject.IsValid())
	{
		CurrentSave = FSaveData();
		return false;
	}

	CurrentSave.Mission = JsonObject->GetIntegerField(TEXT("Mission"));
	CurrentSave.Unlocked = JsonObject->GetIntegerField(TEXT("Unlocked"));
	CurrentSave.Checkpoint = JsonObject->GetIntegerField(TEXT("Checkpoint"));
	CurrentSave.bHasDhanush = JsonObject->GetBoolField(TEXT("HasDhanush"));
	CurrentSave.Jyoti = JsonObject->GetIntegerField(TEXT("Jyoti"));
	CurrentSave.TrishulLevel = JsonObject->GetIntegerField(TEXT("TrishulLevel"));
	CurrentSave.DhanushLevel = JsonObject->GetIntegerField(TEXT("DhanushLevel"));
	CurrentSave.IntroMask = JsonObject->GetIntegerField(TEXT("IntroMask"));
	CurrentSave.BossMask = JsonObject->GetIntegerField(TEXT("BossMask"));
	CurrentSave.bFinished = JsonObject->GetBoolField(TEXT("Finished"));

	return true;
}

void URishiSaveSystem::SaveGame()
{
	TSharedPtr<FJsonObject> JsonObject = MakeShareable(new FJsonObject);
	JsonObject->SetNumberField(TEXT("Mission"), CurrentSave.Mission);
	JsonObject->SetNumberField(TEXT("Unlocked"), CurrentSave.Unlocked);
	JsonObject->SetNumberField(TEXT("Checkpoint"), CurrentSave.Checkpoint);
	JsonObject->SetBoolField(TEXT("HasDhanush"), CurrentSave.bHasDhanush);
	JsonObject->SetNumberField(TEXT("Jyoti"), CurrentSave.Jyoti);
	JsonObject->SetNumberField(TEXT("TrishulLevel"), CurrentSave.TrishulLevel);
	JsonObject->SetNumberField(TEXT("DhanushLevel"), CurrentSave.DhanushLevel);
	JsonObject->SetNumberField(TEXT("IntroMask"), CurrentSave.IntroMask);
	JsonObject->SetNumberField(TEXT("BossMask"), CurrentSave.BossMask);
	JsonObject->SetBoolField(TEXT("Finished"), CurrentSave.bFinished);

	FString JsonString;
	TSharedRef<TJsonWriter<>> Writer = TJsonWriterFactory<>::Create(&JsonString);
	FJsonSerializer::Serialize(JsonObject.ToSharedRef(), Writer);

	// Write to temp file first, then rename for atomic save
	FString TempPath = SaveFilePath + TEXT(".tmp");
	FFileHelper::SaveStringToFile(JsonString, *TempPath);
	
	if (FPlatformFileManager::Get().GetPlatformFile().FileExists(*SaveFilePath))
	{
		FPlatformFileManager::Get().GetPlatformFile().DeleteFile(*SaveFilePath);
	}
	FPlatformFileManager::Get().GetPlatformFile().MoveFile(*SaveFilePath, *TempPath);

	OnSaveCompleted.Broadcast();
}

void URishiSaveSystem::DeleteSave()
{
	if (FPlatformFileManager::Get().GetPlatformFile().FileExists(*SaveFilePath))
	{
		FPlatformFileManager::Get().GetPlatformFile().DeleteFile(*SaveFilePath);
	}
}

bool URishiSaveSystem::HasSave() const
{
	return FPlatformFileManager::Get().GetPlatformFile().FileExists(*SaveFilePath);
}