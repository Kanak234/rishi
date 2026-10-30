// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Combat/RishiWeaponManagerComponent.h"
#include "Combat/RishiCombatSystemComponent.h"
#include "Combat/RishiBowCombatComponent.h"
#include "Core/RishiInputComponent.h"
#include "Components/SkeletalMeshComponent.h"

URishiWeaponManagerComponent::URishiWeaponManagerComponent()
{
	PrimaryComponentTick.bCanEverTick = true;

	WeaponSlots.Add(FWeaponSlot{EWeaponId::Trishul, true, 0, nullptr});
	WeaponSlots.Add(FWeaponSlot{EWeaponId::Dhanush, false, 0, nullptr});

	LevelMultipliers.Add(1.0f);
	LevelMultipliers.Add(1.25f);
	LevelMultipliers.Add(1.5f);
}

void URishiWeaponManagerComponent::BeginPlay()
{
	Super::BeginPlay();

	CombatSystem = GetOwner()->FindComponentByClass<URishiCombatSystemComponent>();
	BowCombat = GetOwner()->FindComponentByClass<URishiBowCombatComponent>();
	InputComponent = GetOwner()->FindComponentByClass<URishiInputComponent>();

	EquipWeapon(GetFirstUnlockedSlot(), false);
}

void URishiWeaponManagerComponent::UpdateWeapon(float DeltaTime)
{
	if (GetWorld()->GetTimeDilation() <= 0.0f) return;
	if (!InputComponent) return;

	bool bNextPressed = InputComponent->WasNextWeaponPressed();
	bool bPrevPressed = InputComponent->WasPrevWeaponPressed();

	if (!bNextPressed && !bPrevPressed) return;

	// Don't switch during attack/charge/aim
	if ((CombatSystem && CombatSystem->IsBusy()) || (BowCombat && BowCombat->IsBusy())) return;

	int32 Direction = bNextPressed ? 1 : -1;
	int32 NextSlot = GetNextUnlockedSlot(Direction);
	if (NextSlot != CurrentSlotIndex)
	{
		EquipWeapon(NextSlot, true);
	}
}

void URishiWeaponManagerComponent::EquipWeapon(int32 SlotIndex, bool bNotify)
{
	if (SlotIndex < 0 || SlotIndex >= WeaponSlots.Num()) return;

	CurrentSlotIndex = SlotIndex;
	CurrentWeapon = WeaponSlots[SlotIndex].WeaponId;
	CurrentLevel = WeaponSlots[SlotIndex].Level;

	// Enable/disable combat systems
	if (CombatSystem) CombatSystem->SetComponentEnabled(CurrentWeapon == EWeaponId::Trishul);
	if (BowCombat) BowCombat->SetComponentEnabled(CurrentWeapon == EWeaponId::Dhanush);

	// Show/hide visuals
	for (int32 i = 0; i < WeaponSlots.Num(); i++)
	{
		if (WeaponSlots[i].VisualComponent)
		{
			WeaponSlots[i].VisualComponent->SetVisibility(i == SlotIndex);
		}
	}

	ApplyMultiplier();

	if (bNotify)
	{
		OnWeaponChanged.Broadcast(CurrentWeapon);
	}
}

void URishiWeaponManagerComponent::ApplyMultiplier()
{
	float Multiplier = LevelMultipliers.IsValidIndex(CurrentLevel) ? LevelMultipliers[CurrentLevel] : 1.0f;
	if (CombatSystem) CombatSystem->DamageMultiplier = Multiplier;
	if (BowCombat) BowCombat->DamageMultiplier = Multiplier;
}

bool URishiWeaponManagerComponent::IsUnlocked(EWeaponId WeaponId) const
{
	int32 Index = FindSlotIndex(WeaponId);
	return Index >= 0 && WeaponSlots[Index].bUnlocked;
}

void URishiWeaponManagerComponent::UnlockWeapon(EWeaponId WeaponId)
{
	int32 Index = FindSlotIndex(WeaponId);
	if (Index < 0 || WeaponSlots[Index].bUnlocked) return;

	WeaponSlots[Index].bUnlocked = true;
	OnWeaponUnlocked.Broadcast(WeaponId);

	// Auto-equip if not busy
	if ((CombatSystem == nullptr || !CombatSystem->IsBusy()) && (BowCombat == nullptr || !BowCombat->IsBusy()))
	{
		EquipWeapon(Index, true);
	}
}

bool URishiWeaponManagerComponent::UpgradeWeapon(EWeaponId WeaponId)
{
	int32 Index = FindSlotIndex(WeaponId);
	if (Index < 0 || !WeaponSlots[Index].bUnlocked) return false;

	int32 MaxLevel = LevelMultipliers.Num() - 1;
	if (WeaponSlots[Index].Level >= MaxLevel) return false;

	WeaponSlots[Index].Level++;
	if (Index == CurrentSlotIndex)
	{
		CurrentLevel = WeaponSlots[Index].Level;
		ApplyMultiplier();
	}

	OnWeaponUpgraded.Broadcast(WeaponId, WeaponSlots[Index].Level);
	return true;
}

int32 URishiWeaponManagerComponent::GetWeaponLevel(EWeaponId WeaponId) const
{
	int32 Index = FindSlotIndex(WeaponId);
	return Index >= 0 ? WeaponSlots[Index].Level : 0;
}

void URishiWeaponManagerComponent::RestoreWeaponLevel(EWeaponId WeaponId, int32 Level)
{
	int32 Index = FindSlotIndex(WeaponId);
	if (Index < 0) return;

	int32 MaxLevel = LevelMultipliers.Num() - 1;
	WeaponSlots[Index].Level = FMath::Clamp(Level, 0, MaxLevel);
	if (Index == CurrentSlotIndex)
	{
		CurrentLevel = WeaponSlots[Index].Level;
		ApplyMultiplier();
	}
}

int32 URishiWeaponManagerComponent::FindSlotIndex(EWeaponId WeaponId) const
{
	for (int32 i = 0; i < WeaponSlots.Num(); i++)
	{
		if (WeaponSlots[i].WeaponId == WeaponId) return i;
	}
	return -1;
}

int32 URishiWeaponManagerComponent::GetNextUnlockedSlot(int32 Direction) const
{
	for (int32 Step = 1; Step <= WeaponSlots.Num(); Step++)
	{
		int32 Index = (CurrentSlotIndex + Direction * Step) % WeaponSlots.Num();
		if (Index < 0) Index += WeaponSlots.Num();
		if (WeaponSlots[Index].bUnlocked) return Index;
	}
	return CurrentSlotIndex;
}

int32 URishiWeaponManagerComponent::GetFirstUnlockedSlot() const
{
	for (int32 i = 0; i < WeaponSlots.Num(); i++)
	{
		if (WeaponSlots[i].bUnlocked) return i;
	}
	return 0;
}