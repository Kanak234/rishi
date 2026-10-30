// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Enemies/RishiEnemyAI.h"
#include "Combat/RishiHitFilterInterface.h"
#include "RishiPashanRakshas.generated.h"

UCLASS()
class RISHI_API ARishiPashanRakshas : public ARishiEnemyAI, public IRishiHitFilterInterface
{
	GENERATED_BODY()

public:
	ARishiPashanRakshas();

	// IRishiHitFilterInterface
	virtual bool FilterHit_Implementation(FHitInfo& HitInfo) override;

	UPROPERTY(BlueprintAssignable, Category = "PashanRakshas")
	FOnGuardBlocked OnGuardBlocked;

protected:
	virtual float GetStunDuration(const FHitInfo& HitInfo) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "PashanRakshas")
	float GuardAngle = 70.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "PashanRakshas")
	float GuardBreakStun = 1.6f;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnGuardBlocked);