// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RishiInputComponent.generated.h"

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class RISHI_API URishiInputComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	URishiInputComponent();

	// Input Actions
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> MoveAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> JumpAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> DodgeAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> LightAttackAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> HeavyAttackAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> InteractAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> PrevWeaponAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> NextWeaponAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> AimAction;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Input", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<class UInputAction> PauseAction;

	// Input Values
	UFUNCTION(BlueprintPure, Category = "Input")
	FVector2D GetMoveValue() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasJumpPressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasDodgePressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasLightAttackPressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasHeavyAttackPressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool IsHeavyAttackHeld() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasInteractPressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasPrevWeaponPressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool WasNextWeaponPressed() const;

	UFUNCTION(BlueprintPure, Category = "Input")
	bool IsAimHeld() const;

protected:
	virtual void BeginPlay() override;

private:
	FVector2D MoveValue;
	bool bJumpPressed = false;
	bool bDodgePressed = false;
	bool bLightAttackPressed = false;
	bool bHeavyAttackPressed = false;
	bool bHeavyAttackHeld = false;
	bool bInteractPressed = false;
	bool bPrevWeaponPressed = false;
	bool bNextWeaponPressed = false;
	bool bAimHeld = false;

	void OnMoveAction(const FInputActionValue& Value);
	void OnJumpAction(const FInputActionValue& Value);
	void OnDodgeAction(const FInputActionValue& Value);
	void OnLightAttackAction(const FInputActionValue& Value);
	void OnHeavyAttackAction(const FInputActionValue& Value);
	void OnInteractAction(const FInputActionValue& Value);
	void OnPrevWeaponAction(const FInputActionValue& Value);
	void OnNextWeaponAction(const FInputActionValue& Value);
	void OnAimAction(const FInputActionValue& Value);
};