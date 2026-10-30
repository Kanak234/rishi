// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Enemies/RishiEnemyAI.h"
#include "RishiVayuPishach.generated.h"

UCLASS()
class RISHI_API ARishiVayuPishach : public ARishiEnemyAI
{
	GENERATED_BODY()

public:
	ARishiVayuPishach();

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;
	virtual FVector GetChaseVelocity(const FVector& ToTarget, float Distance) override;
	virtual void PerformAttack() override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	float HoverHeight = 300.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	float BobAmount = 25.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	float PreferredDistance = 900.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	float TooCloseDistance = 600.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	float StrafeSpeed = 150.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	float ArrowSpeed = 1800.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "VayuPishach")
	TSubclassOf<class ARishiArrow> ArrowClass;

private:
	float StrafeSign = 1.0f;
	float NextStrafeFlip = 0.0f;
};