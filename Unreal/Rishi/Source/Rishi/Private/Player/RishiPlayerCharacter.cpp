// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Player/RishiPlayerCharacter.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Core/RishiInputComponent.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiCombatSystemComponent.h"
#include "Combat/RishiBowCombatComponent.h"
#include "Combat/RishiWeaponManagerComponent.h"
#include "Kismet/KismetMathLibrary.h"

ARishiPlayerCharacter::ARishiPlayerCharacter()
{
	PrimaryActorTick.bCanEverTick = true;

	// Character setup
	GetCapsuleComponent()->InitCapsuleSize(40.0f, 100.0f);
	GetCharacterMovement()->bOrientRotationToMovement = false;
	GetCharacterMovement()->RotationRate = FRotator(0.0f, TurnSpeed, 0.0f);
	GetCharacterMovement()->JumpZVelocity = FMath::Sqrt(2.0f * -Gravity * JumpHeight);
	GetCharacterMovement()->GravityScale = 1.0f;
	GetCharacterMovement()->AirControl = AirControl;
	GetCharacterMovement()->MaxWalkSpeed = MoveSpeed;

	// Camera boom
	CameraBoom = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
	CameraBoom->SetupAttachment(RootComponent);
	CameraBoom->TargetArmLength = 1200.0f;
	CameraBoom->SetRelativeRotation(FRotator(-45.0f, 45.0f, 0.0f));
	CameraBoom->bUsePawnControlRotation = false;
	CameraBoom->bInheritPitch = false;
	CameraBoom->bInheritYaw = false;
	CameraBoom->bInheritRoll = false;
	CameraBoom->bDoCollisionTest = false;

	// Camera
	CameraComponent = CreateDefaultSubobject<UCameraComponent>(TEXT("Camera"));
	CameraComponent->SetupAttachment(CameraBoom, USpringArmComponent::SocketName);
	CameraComponent->bUsePawnControlRotation = false;
	CameraComponent->FieldOfView = 40.0f;

	// Input component
	InputComponent = CreateDefaultSubobject<URishiInputComponent>(TEXT("InputComponent"));

	// Health
	HealthComponent = CreateDefaultSubobject<URishiHealthComponent>(TEXT("Health"));
	HealthComponent->MaxHealth = 100.0f;

	// Combat
	CombatSystem = CreateDefaultSubobject<URishiCombatSystemComponent>(TEXT("CombatSystem"));
	BowCombat = CreateDefaultSubobject<URishiBowCombatComponent>(TEXT("BowCombat"));
	WeaponManager = CreateDefaultSubobject<URishiWeaponManagerComponent>(TEXT("WeaponManager"));
}

void ARishiPlayerCharacter::BeginPlay()
{
	Super::BeginPlay();

	if (HealthComponent)
	{
		HealthComponent->OnDamaged.AddDynamic(this, &ARishiPlayerCharacter::OnDamaged);
	}

	// Set initial state
	SetMoveState(ERishiMoveState::Airborne);
}

void ARishiPlayerCharacter::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (GetWorld()->GetTimeDilation() <= 0.0f) return;
	if (HealthComponent && HealthComponent->IsDead()) return;

	StateTimer += DeltaTime;

	switch (MoveState)
	{
	case ERishiMoveState::Grounded:
		UpdateGrounded(DeltaTime);
		break;
	case ERishiMoveState::Airborne:
		UpdateAirborne(DeltaTime);
		break;
	case ERishiMoveState::Dodging:
		UpdateDodging(DeltaTime);
		break;
	case ERishiMoveState::WallRunning:
		UpdateWallRun(DeltaTime);
		break;
	case ERishiMoveState::LedgeHanging:
		UpdateLedgeHanging();
		break;
	case ERishiMoveState::Climbing:
		UpdateClimbing(DeltaTime);
		break;
	}

	// Update combat systems
	if (CombatSystem) CombatSystem->UpdateCombat(DeltaTime);
	if (BowCombat) BowCombat->UpdateBow(DeltaTime);
	if (WeaponManager) WeaponManager->UpdateWeapon(DeltaTime);
}

void ARishiPlayerCharacter::UpdateGrounded(float DeltaTime)
{
	FVector WishDir = GetCameraRelativeInput();

	// Ground movement with combat modifiers
	FVector Velocity = WishDir * MoveSpeed * MoveScale + ExtraVelocity;
	Velocity.Z = -20.0f; // Small downward force to stay grounded
	GetCharacterMovement()->Velocity = Velocity;

	if (MoveScale > 0.0f && !bLockFacing)
	{
		FaceDirection(WishDir, DeltaTime);
	}

	if (InputComponent && InputComponent->WasDodgePressed() && TryStartDodge(WishDir)) return;
	if (InputComponent && InputComponent->WasJumpPressed() && !bJumpLocked) { DoJump(); return; }

	if (GetCharacterMovement()->IsMovingOnGround())
	{
		LastGroundedTime = GetWorld()->GetTimeSeconds();
	}
	else
	{
		SetMoveState(ERishiMoveState::Airborne);
	}
}

void ARishiPlayerCharacter::UpdateAirborne(float DeltaTime)
{
	FVector WishDir = GetCameraRelativeInput();

	// Air control
	FVector HorizVel = FVector(GetCharacterMovement()->Velocity.X, GetCharacterMovement()->Velocity.Y, 0.0f);
	FVector TargetVel = WishDir * MoveSpeed;
	HorizVel = FMath::VInterpTo(HorizVel, TargetVel, DeltaTime, MoveSpeed * AirControl * 6.0f);
	GetCharacterMovement()->Velocity = FVector(HorizVel.X, HorizVel.Y, GetCharacterMovement()->Velocity.Z);

	// Gravity
	GetCharacterMovement()->Velocity.Z += Gravity * DeltaTime;

	FaceDirection(WishDir, DeltaTime);

	// Coyote jump
	if (InputComponent && InputComponent->WasJumpPressed() && 
		GetWorld()->GetTimeSeconds() - LastGroundedTime <= CoyoteTime && 
		GetCharacterMovement()->Velocity.Z <= 0.0f)
	{
		DoJump();
		return;
	}

	// Ledge grab takes priority over wall run
	if (TryGrabLedge()) return;
	if (WishDir.SizeSquared() > 0.01f && TryStartWallRun()) return;

	if (GetCharacterMovement()->IsMovingOnGround() && GetCharacterMovement()->Velocity.Z <= 0.0f)
	{
		LastGroundedTime = GetWorld()->GetTimeSeconds();
		SetMoveState(ERishiMoveState::Grounded);
	}
}

void ARishiPlayerCharacter::UpdateDodging(float DeltaTime)
{
	GetCharacterMovement()->Velocity = FVector(
		DodgeDirection.X * DodgeSpeed,
		DodgeDirection.Y * DodgeSpeed,
		GetCharacterMovement()->IsMovingOnGround() ? -20.0f : GetCharacterMovement()->Velocity.Z + Gravity * DeltaTime
	);

	if (StateTimer >= DodgeDuration)
	{
		if (HealthComponent) HealthComponent->SetInvincible(false);
		SetMoveState(GetCharacterMovement()->IsMovingOnGround() ? ERishiMoveState::Grounded : ERishiMoveState::Airborne);
	}
}

bool ARishiPlayerCharacter::TryStartDodge(const FVector& WishDir)
{
	if (GetWorld()->GetTimeSeconds() < NextDodgeTime) return false;

	DodgeDirection = WishDir.SizeSquared() > 0.01f ? WishDir.GetSafeNormal() : GetActorForwardVector();
	SetActorRotation(DodgeDirection.Rotation());
	NextDodgeTime = GetWorld()->GetTimeSeconds() + DodgeCooldown;

	if (HealthComponent) HealthComponent->SetInvincible(true); // i-frames
	SetMoveState(ERishiMoveState::Dodging);
	return true;
}

bool ARishiPlayerCharacter::TryStartWallRun()
{
	if (GetWorld()->GetTimeSeconds() < NextWallRunTime) return false;
	if (GetCharacterMovement()->Velocity.Z < -600.0f) return false; // Falling too fast
	if (GetHeightAboveGround() < WallRunMinHeight) return false;

	FVector Origin = GetActorLocation() + FVector(0, 0, GetCapsuleComponent()->GetScaledCapsuleHalfHeight());
	FHitResult Hit;

	if (!CheckWall(Origin, GetActorRightVector(), Hit) && !CheckWall(Origin, -GetActorRightVector(), Hit))
		return false;

	WallNormal = Hit.Normal;
	WallRunDirection = FVector::CrossProduct(WallNormal, FVector::UpVector);
	if (FVector::DotProduct(WallRunDirection, GetActorForwardVector()) < 0.0f)
		WallRunDirection = -WallRunDirection;

	GetCharacterMovement()->Velocity = WallRunDirection * WallRunSpeed;
	GetCharacterMovement()->Velocity.Z = FMath::Max(GetCharacterMovement()->Velocity.Z, 150.0f);
	SetActorRotation(WallRunDirection.Rotation());

	SetMoveState(ERishiMoveState::WallRunning);
	return true;
}

void ARishiPlayerCharacter::UpdateWallRun(float DeltaTime)
{
	// Jump or Dodge = wall jump off
	if (InputComponent && (InputComponent->WasJumpPressed() || InputComponent->WasDodgePressed()))
	{
		WallJump();
		return;
	}

	FVector Origin = GetActorLocation() + FVector(0, 0, GetCapsuleComponent()->GetScaledCapsuleHalfHeight());
	FHitResult Hit;
	bool bStillOnWall = CheckWall(Origin, -WallNormal, Hit);
	bool bNoInput = GetCameraRelativeInput().SizeSquared() < 0.01f;

	if (StateTimer >= WallRunMaxTime || !bStillOnWall || GetCharacterMovement()->IsMovingOnGround() || bNoInput)
	{
		NextWallRunTime = GetWorld()->GetTimeSeconds() + WallReattachDelay;
		SetMoveState(GetCharacterMovement()->IsMovingOnGround() ? ERishiMoveState::Grounded : ERishiMoveState::Airborne);
		return;
	}

	WallNormal = Hit.Normal;
	FVector Vel = WallRunDirection * WallRunSpeed - WallNormal * 100.0f;
	GetCharacterMovement()->Velocity = FVector(Vel.X, Vel.Y, FMath::Max(GetCharacterMovement()->Velocity.Z + WallRunGravity * DeltaTime, -300.0f));
}

void ARishiPlayerCharacter::WallJump()
{
	float UpVel = FMath::Sqrt(2.0f * -Gravity * JumpHeight * 0.85f);
	FVector Vel = WallNormal * WallJumpPush + WallRunDirection * (WallRunSpeed * 0.6f) + FVector(0, 0, UpVel);
	GetCharacterMovement()->Velocity = Vel;

	NextWallRunTime = GetWorld()->GetTimeSeconds() + WallReattachDelay;

	FVector FaceDir = FVector(Vel.X, Vel.Y, 0);
	if (FaceDir.SizeSquared() > 0.01f)
		SetActorRotation(FaceDir.Rotation());

	SetMoveState(ERishiMoveState::Airborne);
}

bool ARishiPlayerCharacter::CheckWall(const FVector& Origin, const FVector& Dir, FHitResult& OutHit) const
{
	FCollisionQueryParams Params;
	Params.AddIgnoredActor(this);
	return GetWorld()->LineTraceSingleByChannel(OutHit, Origin, Origin + Dir * WallCheckDistance, EnvironmentChannel, Params) &&
		FMath::Abs(OutHit.Normal.Z) < 0.2f;
}

bool ARishiPlayerCharacter::TryGrabLedge()
{
	if (GetWorld()->GetTimeSeconds() < NextLedgeTime) return false;
	if (GetCharacterMovement()->Velocity.Z > 200.0f) return false; // Moving up too fast

	FVector Forward = GetActorForwardVector();
	FVector Pos = GetActorLocation();

	// 1) Chest height ray forward for wall
	FVector ChestPos = Pos + FVector(0, 0, 140.0f);
	FHitResult WallHit;
	if (!GetWorld()->LineTraceSingleByChannel(WallHit, ChestPos, ChestPos + Forward * LedgeReach, EnvironmentChannel)) return false;
	if (FMath::Abs(WallHit.Normal.Z) > 0.3f) return false;

	// 2) Above head must be clear
	FVector AboveHead = Pos + FVector(0, 0, GetCapsuleComponent()->GetScaledCapsuleHalfHeight() * 2.0f + 40.0f);
	if (GetWorld()->LineTraceTestByChannel(AboveHead, AboveHead + Forward * (LedgeReach + 30.0f), EnvironmentChannel)) return false;

	// 3) Down from above for ledge top
	FVector DownFrom = AboveHead + Forward * (WallHit.Distance + 25.0f);
	FHitResult TopHit;
	if (!GetWorld()->LineTraceSingleByChannel(TopHit, DownFrom, DownFrom + FVector(0, 0, -160.0f), EnvironmentChannel)) return false;
	if (TopHit.Normal.Z < 0.7f) return false;

	// 4) Check stand position fits
	FVector StandPos = TopHit.Location + Forward * 45.0f;
	FVector P1 = StandPos + FVector(0, 0, GetCapsuleComponent()->GetScaledCapsuleRadius() + 5.0f);
	FVector P2 = StandPos + FVector(0, 0, GetCapsuleComponent()->GetScaledCapsuleHalfHeight() * 2.0f - GetCapsuleComponent()->GetScaledCapsuleRadius());
	if (GetWorld()->OverlapAnyTestByChannel(P1, FQuat::Identity, EnvironmentChannel, FCollisionShape::MakeCapsule(GetCapsuleComponent()->GetScaledCapsuleRadius() * 0.9f, (P2 - P1).Size() * 0.5f))) return false;

	// All good - hang
	FVector WallFace = -WallHit.Normal;
	HangPosition = FVector(WallHit.Location.X, TopHit.Location.Z - HangDepth, WallHit.Location.Z) + WallHit.Normal * (GetCapsuleComponent()->GetScaledCapsuleRadius() + 5.0f);
	ClimbTarget = StandPos;

	GetCharacterMovement()->Velocity = FVector::ZeroVector;
	GetCharacterMovement()->SetMovementMode(MOVE_None);
	SetActorLocationAndRotation(HangPosition, FRotator(0, WallFace.Rotation().Yaw, 0));
	SetMoveState(ERishiMoveState::LedgeHanging);
	return true;
}

void ARishiPlayerCharacter::UpdateLedgeHanging()
{
	if (InputComponent && (InputComponent->WasJumpPressed() || InputComponent->WasInteractPressed()))
	{
		ClimbStartPos = GetActorLocation();
		SetMoveState(ERishiMoveState::Climbing);
		return;
	}

	if (InputComponent && (InputComponent->WasDodgePressed() || InputComponent->GetMoveValue().Y < -0.6f))
	{
		DropFromLedge(FVector::ZeroVector);
	}
}

void ARishiPlayerCharacter::UpdateClimbing(float DeltaTime)
{
	float t = FMath::Clamp(StateTimer / ClimbDuration, 0.0f, 1.0f);
	FVector UpPoint = FVector(ClimbStartPos.X, ClimbTarget.Z + 10.0f, ClimbStartPos.Z);
	FVector NewPos = (t < 0.6f)
		? FMath::Lerp(ClimbStartPos, UpPoint, FMath::SmoothStep(0.0f, 1.0f, t / 0.6f))
		: FMath::Lerp(UpPoint, ClimbTarget, FMath::SmoothStep(0.0f, 1.0f, (t - 0.6f) / 0.4f));

	SetActorLocation(NewPos);

	if (t >= 1.0f)
	{
		GetCharacterMovement()->SetMovementMode(MOVE_Walking);
		GetCharacterMovement()->Velocity = FVector::ZeroVector;
		LastGroundedTime = GetWorld()->GetTimeSeconds();
		SetMoveState(ERishiMoveState::Grounded);
	}
}

void ARishiPlayerCharacter::DropFromLedge(const FVector& Push)
{
	GetCharacterMovement()->SetMovementMode(MOVE_Falling);
	GetCharacterMovement()->Velocity = Push;
	NextLedgeTime = GetWorld()->GetTimeSeconds() + LedgeRegrabDelay;
	SetMoveState(ERishiMoveState::Airborne);
}

void ARishiPlayerCharacter::DoJump()
{
	GetCharacterMovement()->Velocity.Z = FMath::Sqrt(2.0f * -Gravity * JumpHeight);
	LastGroundedTime = -999.0f;
	SetMoveState(ERishiMoveState::Airborne);
}

FVector ARishiPlayerCharacter::GetCameraRelativeInput() const
{
	if (!InputComponent) return FVector::ZeroVector;

	FVector2D MoveInput = InputComponent->GetMoveValue();
	if (MoveInput.SizeSquared() < 0.01f) return FVector::ZeroVector;

	FVector Forward = CameraComponent ? CameraComponent->GetForwardVector() : GetActorForwardVector();
	FVector Right = CameraComponent ? CameraComponent->GetRightVector() : GetActorRightVector();
	Forward.Z = 0; Right.Z = 0;
	Forward.Normalize(); Right.Normalize();

	FVector Result = Forward * MoveInput.Y + Right * MoveInput.X;
	return Result.SizeSquared() > 1.0f ? Result.GetSafeNormal() : Result;
}

void ARishiPlayerCharacter::FaceDirection(const FVector& Dir, float DeltaTime)
{
	if (Dir.SizeSquared() < 0.01f) return;
	FRotator TargetRot = Dir.Rotation();
	SetActorRotation(FMath::RInterpTo(GetActorRotation(), TargetRot, DeltaTime, TurnSpeed / 180.0f * 3.14159f));
}

float ARishiPlayerCharacter::GetHeightAboveGround() const
{
	FHitResult Hit;
	FVector Start = GetActorLocation() + FVector(0, 0, 10.0f);
	if (GetWorld()->LineTraceSingleByChannel(Hit, Start, Start + FVector(0, 0, -5000.0f), EnvironmentChannel))
	{
		return Hit.Distance;
	}
	return 5000.0f;
}

void ARishiPlayerCharacter::SetMoveState(ERishiMoveState NewState)
{
	if (NewState == MoveState) return;

	ERishiMoveState PrevState = MoveState;

	// Always clear invincibility when leaving dodge
	if (PrevState == ERishiMoveState::Dodging && HealthComponent)
	{
		HealthComponent->SetInvincible(false);
	}

	MoveState = NewState;
	StateTimer = 0.0f;
	OnMoveStateChanged.Broadcast(PrevState, NewState);
}

void ARishiPlayerCharacter::OnDamaged(float DamageAmount)
{
	if (MoveState == ERishiMoveState::LedgeHanging || MoveState == ERishiMoveState::Climbing)
	{
		DropFromLedge(-GetActorForwardVector() * FallKnockback);
	}
}

void ARishiPlayerCharacter::TeleportTo(const FVector& Location, const FRotator& Rotation)
{
	GetCharacterMovement()->SetMovementMode(MOVE_None);
	SetActorLocationAndRotation(Location, Rotation);
	GetCharacterMovement()->SetMovementMode(MOVE_Walking);
	GetCharacterMovement()->Velocity = FVector::ZeroVector;
	if (HealthComponent) HealthComponent->SetInvincible(false);
	SetMoveState(ERishiMoveState::Airborne); // Will detect ground next frame
}