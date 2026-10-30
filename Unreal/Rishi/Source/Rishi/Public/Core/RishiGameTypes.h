// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "RishiGameTypes.generated.h"

UENUM(BlueprintType)
enum class ERishiMoveState : uint8
{
	Grounded,
	Airborne,
	Dodging,
	WallRunning,
	LedgeHanging,
	Climbing
};

UENUM(BlueprintType)
enum class EEnemyState : uint8
{
	Idle,
	Chase,
	Attack,
	Hurt,
	Dead
};

UENUM(BlueprintType)
enum class EAttackKind : uint8
{
	None,
	Light,
	Heavy
};

UENUM(BlueprintType)
enum class EWeaponId : uint8
{
	Trishul,
	Dhanush
};

UENUM(BlueprintType)
enum class ETeam : uint8
{
	Player,
	Asur
};

UENUM(BlueprintType)
enum class EMusicTrack : uint8
{
	None,
	Menu,
	Explore,
	Boss
};

UENUM(BlueprintType)
enum class ESfx : uint8
{
	Hit,
	Clang,
	Swoosh,
	Hurt,
	Death,
	Pickup,
	Deepak,
	Boom,
	Arrow,
	UiMove,
	UiSelect
};

USTRUCT(BlueprintType)
struct FAttackData
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float Duration = 0.42f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float ActiveStart = 0.12f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float ActiveEnd = 0.24f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float Damage = 10.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float Knockback = 1.5f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float Radius = 0.9f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float Reach = 1.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
	float Lunge = 3.0f;

	FAttackData() = default;
	FAttackData(float InDuration, float InActiveStart, float InActiveEnd, float InDamage, float InKnockback, float InRadius, float InReach, float InLunge)
		: Duration(InDuration), ActiveStart(InActiveStart), ActiveEnd(InActiveEnd), Damage(InDamage), Knockback(InKnockback), Radius(InRadius), Reach(InReach), Lunge(InLunge) {}
};

USTRUCT(BlueprintType)
struct FHitInfo
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	float Damage = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	FVector Direction = FVector::ZeroVector;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	float Knockback = 0.0f;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	bool bGuardBreak = false;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	bool bIsFinisher = false;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	ETeam AttackerTeam = ETeam::Player;

	UPROPERTY(BlueprintReadWrite, Category = "Combat")
	TWeakObjectPtr<AActor> Attacker = nullptr;
};

USTRUCT(BlueprintType)
struct FSlide
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Story")
	FName PaintingName;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Story")
	FString Line;

	FSlide() = default;
	FSlide(FName InPainting, const FString& InLine) : PaintingName(InPainting), Line(InLine) {}
};

USTRUCT(BlueprintType)
struct FMissionDef
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	FString Level;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	int32 LevelNumber = 1;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	FString Title;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	FString Goal;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	TArray<FSlide> Intro;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	TArray<FSlide> BossIntro;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Mission")
	TArray<FSlide> Outro;
};

USTRUCT(BlueprintType)
struct FSaveData
{
	GENERATED_BODY()

	UPROPERTY()
	int32 Mission = 0;

	UPROPERTY()
	int32 Unlocked = 0;

	UPROPERTY()
	int32 Checkpoint = -1;

	UPROPERTY()
	bool bHasDhanush = false;

	UPROPERTY()
	int32 Jyoti = 0;

	UPROPERTY()
	int32 TrishulLevel = 0;

	UPROPERTY()
	int32 DhanushLevel = 0;

	UPROPERTY()
	int32 IntroMask = 0;

	UPROPERTY()
	int32 BossMask = 0;

	UPROPERTY()
	bool bFinished = false;
};