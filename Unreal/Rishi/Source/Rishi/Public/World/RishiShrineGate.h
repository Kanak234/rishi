// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RishiShrineGate.generated.h"

class ARishiObjectiveShrine;

DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnGateOpened);

UCLASS()
class RISHI_API ARishiShrineGate : public AActor
{
	GENERATED_BODY()

public:
	ARishiShrineGate();

	UFUNCTION(BlueprintPure, Category = "ShrineGate")
	int32 GetLitCount() const;

	UFUNCTION(BlueprintPure, Category = "ShrineGate")
	int32 GetTotalShrines() const { return Shrines.Num(); }

	UFUNCTION(BlueprintPure, Category = "ShrineGate")
	const FString& GetLabel() const { return Label; }

	static void NotifyShrineLit();

	UPROPERTY(BlueprintAssignable, Category = "ShrineGate")
	FOnGateOpened OnOpened;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "ShrineGate")
	TArray<TObjectPtr<ARishiObjectiveShrine>> Shrines;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "ShrineGate")
	TArray<TObjectPtr<AActor>> Walls;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "ShrineGate")
	FString Label = TEXT("Maha-deep");

protected:
	virtual void BeginPlay() override;

private:
	bool bIsOpen = false;

	void CheckAllLit();
};