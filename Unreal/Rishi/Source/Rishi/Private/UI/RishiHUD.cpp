// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "UI/RishiHUD.h"
#include "World/RishiLevelDirector.h"
#include "World/RishiEncounterZone.h"
#include "World/RishiShrineGate.h"
#include "World/RishiObjectiveShrine.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiWeaponManagerComponent.h"
#include "Core/RishiSaveSystem.h"

TObjectPtr<ARishiHUD> ARishiHUD::HUDInstance = nullptr;

ARishiHUD::ARishiHUD()
{
	HUDInstance = this;
}

void ARishiHUD::BeginPlay()
{
	Super::BeginPlay();
}

void ARishiHUD::DrawHUD()
{
	Super::DrawHUD();

	if (!PlayerOwner) return;
	ARishiLevelDirector* Director = ARishiLevelDirector::Get();
	if (!Director) return;

	ARishiPlayerCharacter* Player = Cast<ARishiPlayerCharacter>(PlayerOwner->GetPawn());
	if (!Player) return;

	URishiHealthComponent* Health = Player->FindComponentByClass<URishiHealthComponent>();
	URishiWeaponManagerComponent* Weapons = Player->FindComponentByClass<URishiWeaponManagerComponent>();
	if (!Health) return;

	// Low health vignette
	float HPPercent = Health->GetHealthPercent();
	if (HPPercent < 0.3f && !Health->IsDead())
	{
		// Draw red vignette
		DrawRect(FLinearColor(0.6f, 0.05f, 0.02f, 0.25f + 0.15f * FMath::Sin(GetWorld()->GetTimeSeconds() * 6.0f)), 0, 0, Canvas->SizeX, 60);
		DrawRect(FLinearColor(0.6f, 0.05f, 0.02f, 0.25f + 0.15f * FMath::Sin(GetWorld()->GetTimeSeconds() * 6.0f)), 0, Canvas->SizeY - 60, Canvas->SizeX, 60);
		DrawRect(FLinearColor(0.6f, 0.05f, 0.02f, 0.25f + 0.15f * FMath::Sin(GetWorld()->GetTimeSeconds() * 6.0f)), 0, 0, 60, Canvas->SizeY);
		DrawRect(FLinearColor(0.6f, 0.05f, 0.02f, 0.25f + 0.15f * FMath::Sin(GetWorld()->GetTimeSeconds() * 6.0f)), Canvas->SizeX - 60, 0, 60, Canvas->SizeY);
	}

	// Health bar (placeholder - would use UMG in production)
	// Draw health bar background
	DrawRect(FLinearColor(0, 0, 0, 0.45f), 40, 40, 800, 180);
	// Health fill
	DrawRect(FLinearColor(0.85f, 0.2f, 0.05f, 1.0f), 124, 62, 640 * HPPercent, 44);
	DrawRect(FLinearColor(1.0f, 0.75f, 0.2f, 1.0f), 124, 62, 640 * HPPercent, 44);
	// Diya base
	DrawRect(FLinearColor(0, 0, 0, 1.0f), 54, 114, 80, 20);
	DrawRect(FLinearColor(0.6f, 0.27f, 0.16f, 1.0f), 58, 114, 72, 8);

	// Jyoti count
	FString JyotiText = FString::Printf(TEXT("Jyoti  %d"), Director->GetJyoti());
	DrawText(JyotiText, FLinearColor::White, 354, 114, nullptr, 1.5f);

	// Weapon
	if (Weapons)
	{
		FString WeaponText = FString::Printf(TEXT("%s  Lv %d"), *Weapons->GetCurrentWeapon().ToString(), Weapons->GetCurrentLevel());
		DrawText(WeaponText, FLinearColor::White, Canvas->SizeX - 400, 40, nullptr, 1.5f);
		
		if (Weapons->IsUnlocked(EWeaponId::Dhanush))
		{
			DrawText(TEXT("Q / E = badlo"), FLinearColor::White, Canvas->SizeX - 400, 80, nullptr, 1.0f);
		}
	}

	// Encounter info
	ARishiEncounterZone* Zone = ARishiEncounterZone::GetActiveZone();
	if (Zone && !Zone->IsBossEncounter())
	{
		FString WaveText = Zone->GetTotalWaves() > 1 ? FString::Printf(TEXT("  (lehar %d/%d)"), Zone->GetCurrentWaveIndex() + 1, Zone->GetTotalWaves()) : TEXT("");
		FString EncounterText = FString::Printf(TEXT("%s — asur baaki: %d%s"), *Zone->GetTitle(), Zone->GetRemainingEnemies(), *WaveText);
		DrawText(EncounterText, FLinearColor::White, Canvas->SizeX / 2, 60, nullptr, 1.5f, true);
	}

	// Boss health bar
	if (Director->IsBossActive() && Director->GetBoss())
	{
		URishiHealthComponent* BossHealth = Director->GetBoss()->FindComponentByClass<URishiHealthComponent>();
		if (BossHealth)
		{
			float BossHP = BossHealth->GetHealthPercent();
			FString BossText = FString::Printf(TEXT("%s — %s · charan %d/3"), *Director->GetBoss()->GetDisplayName(), *Director->GetBoss()->GetSubtitle(), Director->GetBoss()->GetPhase());
			DrawText(BossText, FLinearColor::White, Canvas->SizeX / 2, Canvas->SizeY - 180, nullptr, 2.0f, true);
			
			DrawRect(FLinearColor(0, 0, 0, 1.0f), Canvas->SizeX / 2 - 900, Canvas->SizeY - 150, 1800, 45);
			DrawRect(FLinearColor(0.9f, 0.87f, 0.8f, 1.0f), Canvas->SizeX / 2 - 897, Canvas->SizeY - 147, 1794 * BossHP, 39);
			DrawRect(FLinearColor(0.2f, 0.12f, 0.45f, 1.0f), Canvas->SizeX / 2 - 897, Canvas->SizeY - 147, 1794 * BossHP, 39);
			
			// Phase markers
			DrawRect(FLinearColor(0.95f, 0.66f, 0.18f, 1.0f), Canvas->SizeX / 2 - 897 + 1794 * 0.66f, Canvas->SizeY - 154, 6, 49);
			DrawRect(FLinearColor(0.95f, 0.66f, 0.18f, 1.0f), Canvas->SizeX / 2 - 897 + 1794 * 0.30f, Canvas->SizeY - 154, 6, 49);

			if (Director->GetBoss()->IsInOpening())
			{
				DrawText(TEXT("<color=#FFC040>KHULA MAUKA — bhaari vaar (RMB / Y)!</>"), FLinearColor::White, Canvas->SizeX / 2, Canvas->SizeY - 80, nullptr, 2.0f, true);
			}
		}
	}

	// Shrine progress
	ARishiShrineGate* Gate = ARishiShrineGate::GetActiveGate();
	if (Gate && Gate->GetTotalShrines() > 0)
	{
		ARishiPlayerCharacter* PlayerChar = Cast<ARishiPlayerCharacter>(Player);
		if (PlayerChar && (Gate->GetActorLocation() - PlayerChar->GetActorLocation()).SizeSquared() < 4500 * 4500)
		{
			FString ShrineText = FString::Printf(TEXT("%s jale: %d/%d"), *Gate->GetLabel(), Gate->GetLitCount(), Gate->GetTotalShrines());
			DrawText(ShrineText, FLinearColor::White, Canvas->SizeX / 2, 144, nullptr, 1.5f, true);
		}
	}

	// Nearby shrine interaction
	ARishiObjectiveShrine* NearShrine = ARishiObjectiveShrine::GetNearbyShrine();
	if (NearShrine && !NearShrine->IsLit())
	{
		FString ShrineText = TEXT("Deep jal raha hai… ruko (F / RB = jaldi)");
		DrawText(ShrineText, FLinearColor::White, Canvas->SizeX / 2, Canvas->SizeY * 0.6f, nullptr, 2.0f, true);
		
		DrawRect(FLinearColor(0, 0, 0, 1.0f), Canvas->SizeX / 2 - 300, Canvas->SizeY * 0.6f + 50, 600, 33);
		DrawRect(FLinearColor(0.95f, 0.66f, 0.18f, 1.0f), Canvas->SizeX / 2 - 297, Canvas->SizeY * 0.6f + 53, 594 * NearShrine->GetProgress(), 27);
	}
}

void ARishiHUD::ShowHintStatic(const FString& Text, float Duration)
{
	if (HUDInstance)
	{
		HUDInstance->ShowHintInternal(Text, Duration);
	}
}

void ARishiHUD::PulseJyotiStatic()
{
	// Visual pulse for Jyoti counter
}

void ARishiHUD::ShowBannerStatic(const FString& Level, const FString& Mission, const FString& Goal)
{
	if (HUDInstance)
	{
		HUDInstance->ShowBannerInternal(Level, Mission, Goal);
	}
}

void ARishiHUD::ShowHintInternal(const FString& Text, float Duration)
{
	// Would show hint widget
}

void ARishiHUD::ShowBannerInternal(const FString& Level, const FString& Mission, const FString& Goal)
{
	// Would show mission banner widget
}