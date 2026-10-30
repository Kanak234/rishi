// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Core/RishiInputComponent.h"
#include "EnhancedInputComponent.h"
#include "GameFramework/Pawn.h"

URishiInputComponent::URishiInputComponent()
{
	PrimaryComponentTick.bCanEverTick = false;
}

void URishiInputComponent::BeginPlay()
{
	Super::BeginPlay();

	if (APawn* Pawn = Cast<APawn>(GetOwner()))
	{
		if (APlayerController* PC = Cast<APlayerController>(Pawn->GetController()))
		{
			if (UEnhancedInputComponent* EnhancedInput = Cast<UEnhancedInputComponent>(PC->InputComponent))
			{
				if (MoveAction) EnhancedInput->BindAction(MoveAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnMoveAction);
				if (JumpAction) EnhancedInput->BindAction(JumpAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnJumpAction);
				if (DodgeAction) EnhancedInput->BindAction(DodgeAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnDodgeAction);
				if (LightAttackAction) EnhancedInput->BindAction(LightAttackAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnLightAttackAction);
				if (HeavyAttackAction) 
				{
					EnhancedInput->BindAction(HeavyAttackAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnHeavyAttackAction);
					EnhancedInput->BindAction(HeavyAttackAction, ETriggerEvent::Completed, this, [this](const FInputActionValue&) { bHeavyAttackHeld = false; });
				}
				if (InteractAction) EnhancedInput->BindAction(InteractAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnInteractAction);
				if (PrevWeaponAction) EnhancedInput->BindAction(PrevWeaponAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnPrevWeaponAction);
				if (NextWeaponAction) EnhancedInput->BindAction(NextWeaponAction, ETriggerEvent::Triggered, this, &URishiInputComponent::OnNextWeaponAction);
				if (AimAction) 
				{
					EnhancedInput->BindAction(AimAction, ETriggerEvent::Triggered, this, [this](const FInputActionValue&) { bAimHeld = true; });
					EnhancedInput->BindAction(AimAction, ETriggerEvent::Completed, this, [this](const FInputActionValue&) { bAimHeld = false; });
				}
			}
		}
	}
}

void URishiInputComponent::OnMoveAction(const FInputActionValue& Value)
{
	MoveValue = Value.Get<FVector2D>();
}

void URishiInputComponent::OnJumpAction(const FInputActionValue& Value)
{
	bJumpPressed = true;
}

void URishiInputComponent::OnDodgeAction(const FInputActionValue& Value)
{
	bDodgePressed = true;
}

void URishiInputComponent::OnLightAttackAction(const FInputActionValue& Value)
{
	bLightAttackPressed = true;
}

void URishiInputComponent::OnHeavyAttackAction(const FInputActionValue& Value)
{
	bHeavyAttackPressed = true;
	bHeavyAttackHeld = true;
}

void URishiInputComponent::OnInteractAction(const FInputActionValue& Value)
{
	bInteractPressed = true;
}

void URishiInputComponent::OnPrevWeaponAction(const FInputActionValue& Value)
{
	bPrevWeaponPressed = true;
}

void URishiInputComponent::OnNextWeaponAction(const FInputActionValue& Value)
{
	bNextWeaponPressed = true;
}

FVector2D URishiInputComponent::GetMoveValue() const
{
	return MoveValue;
}

bool URishiInputComponent::WasJumpPressed() const
{
	return bJumpPressed;
}

bool URishiInputComponent::WasDodgePressed() const
{
	return bDodgePressed;
}

bool URishiInputComponent::WasLightAttackPressed() const
{
	return bLightAttackPressed;
}

bool URishiInputComponent::WasHeavyAttackPressed() const
{
	return bHeavyAttackPressed;
}

bool URishiInputComponent::IsHeavyAttackHeld() const
{
	return bHeavyAttackHeld;
}

bool URishiInputComponent::WasInteractPressed() const
{
	return bInteractPressed;
}

bool URishiInputComponent::WasPrevWeaponPressed() const
{
	return bPrevWeaponPressed;
}

bool URishiInputComponent::WasNextWeaponPressed() const
{
	return bNextWeaponPressed;
}

bool URishiInputComponent::IsAimHeld() const
{
	return bAimHeld;
}