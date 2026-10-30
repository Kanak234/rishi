// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiKillZone.generated.h"

UCLASS()
class RISHI_API ARishiKillZone : public AActor
{
	GENERATED_BODY()

public:
	ARishiKillZone();

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "KillZone")
	TObjectPtr<AActor> RespawnPoint;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "KillZone")
	bool bUseCheckpointRespawn = true;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<UBoxComponent> TriggerVolume;

	void OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
};