// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiObjectiveShrine.generated.h"

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnShrineLit, class ARishiObjectiveShrine*, Shrine);

UCLASS()
class RISHI_API ARishiObjectiveShrine : public AActor
{
	GENERATED_BODY()

public:
	ARishiObjectiveShrine();

	UFUNCTION(BlueprintCallable, Category = "Shrine")
	void Interact(AActor* Interactor);

	UFUNCTION(BlueprintPure, Category = "Shrine")
	bool IsLit() const { return bIsLit; }

	UFUNCTION(BlueprintPure, Category = "Shrine")
	float GetProgress() const { return Progress; }

	UPROPERTY(BlueprintAssignable, Category = "Shrine")
	FOnShrineLit OnLit;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Shrine")
	TObjectPtr<AActor> LitVisual;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Shrine")
	float InteractHoldTime = 1.2f;

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;

private:
	UPROPERTY()
	TObjectPtr<UBoxComponent> TriggerVolume;

	bool bIsLit = false;
	float Progress = 0.0f;
	AActor* CurrentInteractor = nullptr;

	void OnOverlapBegin(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);
	void OnOverlapEnd(UPrimitiveComponent* OverlappedComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex);
};