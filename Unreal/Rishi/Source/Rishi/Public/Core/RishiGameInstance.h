// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "RishiGameInstance.generated.h"

UCLASS()
class RISHI_API URishiGameInstance : public UGameInstance
{
	GENERATED_BODY()

public:
	URishiGameInstance();

	virtual void Init() override;
	virtual void Shutdown() override;
};