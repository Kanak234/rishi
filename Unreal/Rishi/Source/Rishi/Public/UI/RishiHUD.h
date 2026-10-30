// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/HUD.h"
#include "RishiHUD.generated.h"

class URishiUserWidget;

UCLASS()
class RISHI_API ARishiHUD : public AHUD
{
	GENERATED_BODY()

public:
	ARishiHUD();

	static void ShowHintStatic(const FString& Text, float Duration);
	static void PulseJyotiStatic();
	static void ShowBannerStatic(const FString& Level, const FString& Mission, const FString& Goal);

protected:
	virtual void BeginPlay() override;
	virtual void DrawHUD() override;

private:
	UPROPERTY()
	TObjectPtr<URishiUserWidget> HUDWidget;

	static TObjectPtr<ARishiHUD> HUDInstance;
};