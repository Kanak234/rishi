// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Enemies/RishiEnemyAI.h"
#include "RishiChhayaBhoot.generated.h"

UCLASS()
class RISHI_API ARishiChhayaBhoot : public ARishiEnemyAI
{
	GENERATED_BODY()

public:
	ARishiChhayaBhoot();

protected:
	virtual FVector GetChaseVelocity(const FVector& ToTarget, float Distance) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "ChhayaBhoot")
	float CircleSpeed = 250.0f;
};