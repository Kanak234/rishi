// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "RishiGameTypes.h"
#include "RishiPlayerCharacter.generated.h"

class UCameraComponent;
class USpringArmComponent;
class URishiInputComponent;
class URishiHealthComponent;
class URishiCombatSystemComponent;
class URishiBowCombatComponent;
class URishiWeaponManagerComponent;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnMoveStateChanged, ERishiMoveState, PrevState, ERishiMoveState, NewState);

UCLASS()
class RISHI_API ARishiPlayerCharacter : public ACharacter
{
	GENERATED_BODY()

public:
	ARishiPlayerCharacter();

	UFUNCTION(BlueprintCallable, Category = "Player")
	void SetMoveScale(float Scale) { MoveScale = FMath::Clamp(Scale, 0.0f, 1.0f); }

	UFUNCTION(BlueprintCallable, Category = "Player")
	void SetExtraVelocity(const FVector& Velocity) { ExtraVelocity = Velocity; }

	UFUNCTION(BlueprintCallable, Category = "Player")
	void SetJumpLocked(bool bLocked) { bJumpLocked = bLocked; }

	UFUNCTION(BlueprintCallable, Category = "Player")
	void SetLockFacing(bool bLocked) { bLockFacing = bLocked; }

	UFUNCTION(BlueprintPure, Category = "Player")
	ERishiMoveState GetMoveState() const { return MoveState; }

	UFUNCTION(BlueprintPure, Category = "Player")
	FVector GetVelocity() const { return GetCharacterMovement()->Velocity; }

	UFUNCTION(BlueprintPure, Category = "Player")
	float GetDodgeCooldownRemaining() const { return FMath::Max(0.0f, NextDodgeTime - GetWorld()->GetTimeSeconds()); }

	UFUNCTION(BlueprintPure, Category = "Player")
	float GetWallRunTimeRemaining() const { return MoveState == ERishiMoveState::WallRunning ? FMath::Max(0.0f, WallRunMaxTime - StateTimer) : 0.0f; }

	UFUNCTION(BlueprintCallable, Category = "Player")
	void TeleportTo(const FVector& Location, const FRotator& Rotation);

	UPROPERTY(BlueprintAssignable, Category = "Player")
	FOnMoveStateChanged OnMoveStateChanged;

	// Movement settings
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
	float MoveSpeed = 600.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
	float TurnSpeed = 900.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
	float JumpHeight = 160.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
	float Gravity = -2500.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
	float AirControl = 0.6f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
	float CoyoteTime = 0.12f;

	// Dodge settings
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Dodge")
	float DodgeSpeed = 1300.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Dodge")
	float DodgeDuration = 0.4f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Dodge")
	float DodgeCooldown = 1.0f;

	// Wall run settings
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallRunMaxTime = 2.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallRunSpeed = 800.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallRunGravity = -250.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallCheckDistance = 90.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallRunMinHeight = 100.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallJumpPush = 700.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "WallRun")
	float WallReattachDelay = 0.35f;

	// Ledge climb settings
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Ledge")
	float LedgeReach = 80.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Ledge")
	float HangDepth = 175.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Ledge")
	float ClimbDuration = 0.55f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Ledge")
	float LedgeRegrabDelay = 0.4f;

	// Hit reaction
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "HitReaction")
	float FallKnockback = 300.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Collision")
	TEnumAsByte<ECollisionChannel> EnvironmentChannel = ECC_WorldStatic;

protected:
	virtual void BeginPlay() override;
	virtual void Tick(float DeltaTime) override;
	virtual void SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) override;

private:
	// Components
	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<UCameraComponent> CameraComponent;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<USpringArmComponent> CameraBoom;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<URishiInputComponent> InputComponent;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<URishiHealthComponent> HealthComponent;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<URishiCombatSystemComponent> CombatSystem;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<URishiBowCombatComponent> BowCombat;

	UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components", meta = (AllowPrivateAccess = "true"))
	TObjectPtr<URishiWeaponManagerComponent> WeaponManager;

	// State
	ERishiMoveState MoveState = ERishiMoveState::Grounded;
	float StateTimer = 0.0f;
	float LastGroundedTime = 0.0f;
	float NextDodgeTime = 0.0f;
	float NextWallRunTime = 0.0f;
	float NextLedgeTime = 0.0f;
	FVector DodgeDirection;
	FVector WallNormal;
	FVector WallRunDirection;
	FVector HangPosition;
	FVector ClimbTarget;
	FVector ClimbStartPos;

	// Combat hooks
	float MoveScale = 1.0f;
	FVector ExtraVelocity = FVector::ZeroVector;
	bool bJumpLocked = false;
	bool bLockFacing = false;

	// Movement functions
	void UpdateGrounded(float DeltaTime);
	void UpdateAirborne(float DeltaTime);
	void UpdateDodging(float DeltaTime);
	void UpdateWallRun(float DeltaTime);
	void UpdateLedgeHanging();
	void UpdateClimbing(float DeltaTime);

	bool TryStartDodge(const FVector& WishDir);
	bool TryStartWallRun();
	void WallJump();
	bool CheckWall(const FVector& Origin, const FVector& Dir, FHitResult& OutHit) const;
	bool TryGrabLedge();
	void DropFromLedge(const FVector& Push);
	void DoJump();

	FVector GetCameraRelativeInput() const;
	void FaceDirection(const FVector& Dir, float DeltaTime);
	float GetHeightAboveGround() const;
	void SetMoveState(ERishiMoveState NewState);

	// Hit reaction
	void OnDamaged(float DamageAmount);
};