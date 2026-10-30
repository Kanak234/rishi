// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "RishiGameTypes.h"
#include "RishiWeaponManagerComponent.generated.h"

class URishiCombatSystemComponent;
class URishiBowCombatComponent;

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnWeaponChanged, EWeaponId, WeaponId);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnWeaponUnlocked, EWeaponId, WeaponId);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnWeaponUpgraded, EWeaponId, WeaponId, int32, NewLevel);

USTRUCT(BlueprintType)
struct FWeaponSlot
{
	GENERATED_BODY()

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
	EWeaponId WeaponId = EWeaponId::Trishul;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
	bool bUnlocked = false;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
	int32 Level = 0;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
	TObjectPtr<class USkeletalMeshComponent> VisualComponent;
};

UCLASS(ClassGroup = (Custom), meta = (BlueprintSpawnableComponent))
class RISHI_API URishiWeaponManagerComponent : public UActorComponent
{
	GENERATED_BODY()

public:
	URishiWeaponManagerComponent();

	UFUNCTION(BlueprintCallable, Category = "Weapon")
	void UpdateWeapon(float DeltaTime);

	UFUNCTION(BlueprintPure, Category = "Weapon")
	EWeaponId GetCurrentWeapon() const { return CurrentWeapon; }

	UFUNCTION(BlueprintPure, Category = "Weapon")
	int32 GetCurrentLevel() const { return CurrentLevel; }

	UFUNCTION(BlueprintPure, Category = "Weapon")
	bool IsUnlocked(EWeaponId WeaponId) const;

	UFUNCTION(BlueprintCallable, Category = "Weapon")
	void UnlockWeapon(EWeaponId WeaponId);

	UFUNCTION(BlueprintCallable, Category = "Weapon")
	bool UpgradeWeapon(EWeaponId WeaponId);

	UFUNCTION(BlueprintPure, Category = "Weapon")
	int32 GetWeaponLevel(EWeaponId WeaponId) const;

	UFUNCTION(BlueprintCallable, Category = "Weapon")
	void RestoreWeaponLevel(EWeaponId WeaponId, int32 Level);

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
	TArray<FWeaponSlot> WeaponSlots;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
	TArray<float> LevelMultipliers;

	UPROPERTY(BlueprintAssignable, Category = "Weapon")
	FOnWeaponChanged OnWeaponChanged;

	UPROPERTY(BlueprintAssignable, Category = "Weapon")
	FOnWeaponUnlocked OnWeaponUnlocked;

	UPROPERTY(BlueprintAssignable, Category = "Weapon")
	FOnWeaponUpgraded OnWeaponUpgraded;

protected:
	virtual void BeginPlay() override;

private:
	UPROPERTY()
	TObjectPtr<URishiCombatSystemComponent> CombatSystem;

	UPROPERTY()
	TObjectPtr<URishiBowCombatComponent> BowCombat;

	UPROPERTY()
	TObjectPtr<class URishiInputComponent> InputComponent;

	EWeaponId CurrentWeapon = EWeaponId::Trishul;
	int32 CurrentLevel = 0;
	int32 CurrentSlotIndex = 0;

	void EquipWeapon(int32 SlotIndex, bool bNotify);
	void ApplyMultiplier();
	int32 FindSlotIndex(EWeaponId WeaponId) const;
	int32 GetNextUnlockedSlot(int32 Direction) const;
	int32 GetFirstUnlockedSlot() const;
	void OnNextWeaponPressed();
	void OnPrevWeaponPressed();
};