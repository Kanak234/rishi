// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RishiGameTypes.h"
#include "RishiHurtboxComponent.generated.h"

class URishiHealthComponent;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnHit, class URishiHurtboxComponent*, Hurtbox, const FHitInfo&, HitInfo);

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class RISHI_API URishiHurtboxComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	URishiHurtboxComponent();

	UFUNCTION(BlueprintCallable, Category = "Combat")
	bool ReceiveHit(const FHitInfo& HitInfo);

	UFUNCTION(BlueprintPure, Category = "Combat")
	ETeam GetTeam() const { return Team; }

	UFUNCTION(BlueprintPure, Category = "Combat")
	URishiHealthComponent* GetHealthComponent() const { return HealthComponent; }

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	ETeam Team = ETeam::Player;

	UPROPERTY(BlueprintAssignable, Category = "Combat")
	FOnHit OnHit;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<URishiHealthComponent> HealthComponent;
};