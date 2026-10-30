// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "RishiGameTypes.h"
#include "RishiHitFilterInterface.generated.h"

UINTERFACE(MinimalAPI)
class URishiHitFilterInterface : public UInterface
{
	GENERATED_BODY()
};

class RISHI_API IRishiHitFilterInterface
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintNativeEvent, Category = "Combat")
	bool FilterHit(FHitInfo& HitInfo);
};