// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiMissionExit.generated.h"

UCLASS()
class RISHI_API ARishiMissionExit : public AActor
{
	GENERATED_BODY()

public:
	ARishiMissionExit();

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<UBoxComponent> TriggerVolume;

	void OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
};