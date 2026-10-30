// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "RishiGameMode.generated.h"

UCLASS()
class RISHI_API ARishiGameMode : public AGameModeBase
{
	GENERATED_BODY()

public:
	ARishiGameMode();

protected:
	virtual void BeginPlay() override;
};