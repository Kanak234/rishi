// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiGameTypes.h"
#include "RishiArrow.generated.h"

class UStaticMeshComponent;
class UProjectileMovementComponent;

UCLASS()
class RISHI_API ARishiArrow : public AActor
{
	GENERATED_BODY()

public:
	ARishiArrow();

	UFUNCTION(BlueprintCallable, Category = "Arrow")
	void Launch(const FVector& Direction, float Speed, float Lifetime, const FHitInfo& HitInfo, ECollisionChannel HitChannel);

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

	UFUNCTION()
	void OnHit(UPrimitiveComponent* HitComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit);

private:
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Arrow")
	TObjectPtr<UStaticMeshComponent> MeshComponent;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Arrow")
	TObjectPtr<UProjectileMovementComponent> MovementComponent;

	FHitInfo HitInfo;
	ECollisionChannel HitCollisionChannel;
	FTimerHandle LifetimeTimer;
};