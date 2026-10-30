// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "World/RishiLevelDirector.h"
#include "Player/RishiPlayerCharacter.h"
#include "World/RishiCheckpoint.h"
#include "Enemies/RishiEnemyAI.h"
#include "Enemies/RishiBossAndhvir.h"
#include "World/RishiEncounterZone.h"
#include "Combat/RishiHealthComponent.h"
#include "Combat/RishiWeaponManagerComponent.h"
#include "Combat/RishiCombatSystemComponent.h"
#include "Combat/RishiBowCombatComponent.h"
#include "Core/RishiInputComponent.h"
#include "Core/RishiSaveSystem.h"
#include "Core/RishiGameFlow.h"
#include "Core/RishiGameAudio.h"
#include "UI/RishiHUD.h"
#include "Kismet/GameplayStatics.h"

ARishiLevelDirector* ARishiLevelDirector::LevelDirectorInstance = nullptr;

TArray<FMissionDef> ARishiLevelDirector::MissionDefs = []()
{
	TArray<FMissionDef> Defs;
	
	// Mission 1: Navami ki Raat
	{
		FMissionDef M;
		M.Level = TEXT("Damodar Gaon"); M.LevelNumber = 1; M.Title = TEXT("Navami ki Raat");
		M.Goal = TEXT("Gaon ko Chhaya-bhooton se bachao, ghat tak pahuncho");
		M.Intro = {
			{FName("Village"), TEXT("Damodar ke kinare ek chhota gaon. Navami ki raat — har ghar me deepak, har aangan me naach.")},
			{FName("TamasRises"), TEXT("Usi raat nadi ke neeche ke andhkar se Tamasur jaaga. Uski chhaayaayein gaon ki taraf badhne lagin.")},
			{FName("Journey"), TEXT("Acharya Vasudev ka shishya Rishi, guru ka diya Trishul uthata hai. \"Pehle gaon, phir kuch aur.\"")}
		};
		M.Outro = {
			{FName("GuruTaken"), TEXT("Ghat par pahunch kar Rishi ne dekha — Tamas ki chhaaya Acharya Vasudev ko utha le ja rahi thi.")},
			{FName("Journey"), TEXT("Guru ki aakhri awaaz: \"Rajrappa… jyoti bachao…\" Rishi sangam ki or chal pada.")}
		};
		Defs.Add(M);
	}
	
	// Mission 2: Sangam ki Chattanein
	{
		FMissionDef M;
		M.Level = TEXT("Rajrappa"); M.LevelNumber = 2; M.Title = TEXT("Sangam ki Chattanein");
		M.Goal = TEXT("Damodar-Bhairavi sangam ki chattanein paar karo");
		M.Intro = {{FName("Journey"), TEXT("Rajrappa — jahan Bhairavi, Damodar se milti hai. Par raasta nadi ki fisalti chattanon se hokar jaata hai.")}};
		M.Outro = {{FName("Mandir"), TEXT("Chattanon ke paar mandir dikha. Uski akhand jyoti ab bas ek kaanpti lau thi.")}};
		Defs.Add(M);
	}
	
	// Mission 3: Chhinnamastika ka Aangan
	{
		FMissionDef M;
		M.Level = TEXT("Rajrappa"); M.LevelNumber = 2; M.Title = TEXT("Chhinnamastika ka Aangan");
		M.Goal = TEXT("Mandir ke aangan me Dhanush dhoondho, jyoti bachao");
		M.Intro = {{FName("Mandir"), TEXT("Mandir ke aangan me sona chamak raha tha — kisi purane yoddha ka Dhanush.")}};
		M.BossIntro = {{FName("Andhvir"), TEXT("Wahan Tamasur ka senapati Andhvir khada tha — bhari gada, aur aankhon me sirf andhkar.")}};
		M.Outro = {
			{FName("JyotiReturns"), TEXT("Andhvir gira, aur Rajrappa ki jyoti phir jal uthi.")},
			{FName("Kashi"), TEXT("Girte hue usne kaha: \"Agla deep Kashi ka bujhega… aur tera guru Ellora ki gufaon me hai.\"")}
		};
		Defs.Add(M);
	}
	
	// Mission 4: Deepon ki Seedhiyan
	{
		FMissionDef M;
		M.Level = TEXT("Kashi"); M.LevelNumber = 3; M.Title = TEXT("Deepon ki Seedhiyan");
		M.Goal = TEXT("Ghaton ki seedhiyan chadho, aasmaan ke Vayu-pishach giraao");
		M.Intro = {{FName("Kashi"), TEXT("Kashi ke ghat — hazaaron deepon wali seedhiyan. Par aaj aasmaan Vayu-pishachon se kaala tha.")}};
		M.Outro = {{FName("Aarti"), TEXT("Seedhiyon ke upar maha-aarti ke teen bade deep bujhe pade the. Aarti ruki, to Kashi andhere me doob jaayegi.")}};
		Defs.Add(M);
	}
	
	// Mission 5: Ganga Aarti ki Raksha
	{
		FMissionDef M;
		M.Level = TEXT("Kashi"); M.LevelNumber = 3; M.Title = TEXT("Ganga Aarti ki Raksha");
		M.Goal = TEXT("Aarti ke teeno maha-deep jalao — asur rokne aayenge");
		M.Intro = {{FName("Aarti"), TEXT("Rishi ko teeno maha-deep jalane the — aur Tamas ki sena use rokne ko taiyaar thi.")}};
		M.Outro = {
			{FName("Aarti"), TEXT("Teeno deep jale, aur Ganga par aarti ki roshni phail gayi.")},
			{FName("Hampi"), TEXT("Ek naavik ne bataya: dakshin me Hampi ke khandaharon me patthar ke asur jama ho rahe hain — Shilaasur ki sena.")}
		};
		Defs.Add(M);
	}
	
	// Mission 6: Vijayanagara ke Khandahar
	{
		FMissionDef M;
		M.Level = TEXT("Hampi"); M.LevelNumber = 4; M.Title = TEXT("Vijayanagara ke Khandahar");
		M.Goal = TEXT("Khandaharon se raasta banao, Shilaasur ko roko");
		M.Intro = {{FName("Hampi"), TEXT("Vijayanagara ke khandahar — pathar ka rath, toote mandap. Har chattan ke peeche Pashan-rakshas.")}};
		M.BossIntro = {{FName("Shilaasur"), TEXT("Rath ke saamne Shilaasur utha — Tamasur ka doosra senapati, jiska sharir hi patthar hai.")}};
		M.Outro = {
			{FName("Shilaasur"), TEXT("Shilaasur toota. Uske patthar par Ellora ki gufaon ka naksha khuda tha.")},
			{FName("Ellora"), TEXT("Rishi samajh gaya — guru wahin hain, Kailasa ki gufaon me.")}
		};
		Defs.Add(M);
	}
	
	// Mission 7: Kailasa ki Gufayein
	{
		FMissionDef M;
		M.Level = TEXT("Ellora"); M.LevelNumber = 5; M.Title = TEXT("Kailasa ki Gufayein");
		M.Goal = TEXT("Gufaon ke teen deep jalao, guru tak pahuncho");
		M.Intro = {{FName("Ellora"), TEXT("Ellora — ek hi pahad se kaata gaya Kailasa mandir. Andar ki gufaayein Tamas se bhari thin.")}};
		M.Outro = {
			{FName("GuruFreed"), TEXT("Aakhri gufa me Acharya Vasudev the. Teeno deep jalte hi Tamas ki zanjeerein toot gayin.")},
			{FName("TamasLok"), TEXT("Guru bole: \"Tamasur ka hriday wahin hai jahan se wo jaaga — Damodar ke neeche. Wahan jaana hoga.\"")}
		};
		Defs.Add(M);
	}
	
	// Mission 8: Damodar ke Neeche
	{
		FMissionDef M;
		M.Level = TEXT("Tamas-lok"); M.LevelNumber = 6; M.Title = TEXT("Damodar ke Neeche");
		M.Goal = TEXT("Andhkar ke lok me utro, Tamasur ka saamna karo");
		M.Intro = {{FName("TamasLok"), TEXT("Nadi ke neeche ek doosra lok — na din, na raat, sirf Tamas. Rishi guru ka ashirvad le kar utra.")}};
		M.BossIntro = {{FName("TamasRises"), TEXT("Aur wahan, apne andhkar ke singhasan par, Tamasur ne aankhein kholin.")}};
		M.Outro = {
			{FName("Dawn"), TEXT("Tamasur ka andhkar bikhar gaya. Ek-ek karke Bharat ke saare teerthon ki jyoti laut aayi.")},
			{FName("Village"), TEXT("Navami ki subah gaon laut kar Rishi ne dekha — budhe chitrakar deewar par uski gaatha ka aakhri chitra bana rahe the.")}
		};
		Defs.Add(M);
	}
	
	return Defs;
}();

ARishiLevelDirector::ARishiLevelDirector()
{
	PrimaryActorTick.bCanEverTick = true;
}

ARishiLevelDirector* ARishiLevelDirector::Get()
{
	return LevelDirectorInstance;
}

void ARishiLevelDirector::BeginPlay()
{
	Super::BeginPlay();
	LevelDirectorInstance = this;

	if (!Player)
	{
		TArray<AActor*> Players;
		UGameplayStatics::GetAllActorsOfClass(GetWorld(), ARishiPlayerCharacter::StaticClass(), Players);
		if (Players.Num() > 0) Player = Cast<ARishiPlayerCharacter>(Players[0]);
	}

	if (Player)
	{
		PlayerHealth = Player->FindComponentByClass<URishiHealthComponent>();
		WeaponManager = Player->FindComponentByClass<URishiWeaponManagerComponent>();
		MeleeCombat = Player->FindComponentByClass<URishiCombatSystemComponent>();
		BowCombat = Player->FindComponentByClass<URishiBowCombatComponent>();
		InputComponent = Player->FindComponentByClass<URishiInputComponent>();
	}

	// Bind events
	ARishiEnemyAI::OnDied.AddDynamic(this, &ARishiLevelDirector::OnEnemyDied);
	if (PlayerHealth) { PlayerHealth->OnDied.AddDynamic(this, &ARishiLevelDirector::OnPlayerDied); PlayerHealth->OnDamaged.AddDynamic(this, &ARishiLevelDirector::OnPlayerDamaged); }
	if (Boss) Boss->OnDied.AddDynamic(this, &ARishiLevelDirector::OnBossDied);
	if (MeleeCombat) { MeleeCombat->OnAttackStarted.AddDynamic(this, &ARishiLevelDirector::OnAttackStarted); MeleeCombat->OnHitLanded.AddDynamic(this, &ARishiLevelDirector::OnHitLanded); }
	if (BowCombat) BowCombat->OnArrowFired.AddDynamic(this, &ARishiLevelDirector::OnBowFired);

	// Load save and restore
	URishiSaveSystem* SaveSystem = URishiSaveSystem::Get();
	SaveSystem->LoadGame();
	FSaveData& Save = SaveSystem->GetMutableSave();

	// Restore from checkpoint
	if (Save.Checkpoint >= 0 && Checkpoints.Num() > 0)
	{
		ARishiCheckpoint* BestCP = nullptr;
		for (ARishiCheckpoint* CP : Checkpoints)
		{
			if (CP && CP->GetIndex() <= Save.Checkpoint && (!BestCP || CP->GetIndex() > BestCP->GetIndex()))
			{
				BestCP = CP;
			}
		}
		if (BestCP)
		{
			BestCP->LightSilently();
			Player->TeleportTo(BestCP->GetActorLocation() + FVector(0, 0, 50), BestCP->GetActorRotation());
		}
	}

	// Mission 3+ always has Dhanush
	if (MissionIndex >= 3) Save.bHasDhanush = true;
	if (WeaponManager)
	{
		if (Save.bHasDhanush) WeaponManager->UnlockWeapon(EWeaponId::Dhanush);
		WeaponManager->RestoreWeaponLevel(EWeaponId::Trishul, Save.TrishulLevel);
		WeaponManager->RestoreWeaponLevel(EWeaponId::Dhanush, Save.DhanushLevel);
		
		if (Save.bHasDhanush)
		{
			// Hide Dhanush pickups
			for (TActorIterator<AActor> It(GetWorld()); It; ++It)
			{
				if (It->GetName().Contains(TEXT("Pickup_Dhanush"))) It->SetActorHiddenInGame(true);
			}
		}
	}

	// URishiGameAudio::SetMusic(EMusicTrack::Explore);

	const FMissionDef& Mission = GetMission();
	OnMissionBanner.Broadcast(FString::Printf(TEXT("Level %d · %s"), Mission.LevelNumber, *Mission.Level), 
		FString::Printf(TEXT("Mission %d: %s"), MissionIndex + 1, *Mission.Title), Mission.Goal);

	// Play intro if first time
	int32 IntroBit = 1 << MissionIndex;
	if ((Save.IntroMask & IntroBit) == 0 && Save.Checkpoint < 0)
	{
		Save.IntroMask |= IntroBit;
		SaveSystem->SaveGame();
		PlayIntroCutscene();
	}
}

const FMissionDef& ARishiLevelDirector::GetMission() const
{
	int32 ClampedIndex = FMath::Clamp(MissionIndex, 0, MissionDefs.Num() - 1);
	return MissionDefs[ClampedIndex];
}

int32 ARishiLevelDirector::GetJyoti() const
{
	return URishiSaveSystem::Get()->GetCurrentSave().Jyoti;
}

void ARishiLevelDirector::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (!bGameOver && !bVictory) RunTime += DeltaTime;

	if (bGameOver && GetWorld()->GetTimeSeconds() - DeadTime > 1.5f)
	{
		ShowGameOverMenu();
		HandleGameOverInput();
	}

	if (bEndScreen)
	{
		// Wait for input to return to menu
		if (InputComponent && (InputComponent->WasJumpPressed() || InputComponent->WasInteractPressed()))
		{
			URishiGameFlow::Get()->ReturnToMenu();
		}
	}
}

void ARishiLevelDirector::OnEnemyDied(ARishiEnemyAI* Enemy)
{
	if (Enemy->GetJyotiReward() <= 0) return;

	FSaveData& Save = URishiSaveSystem::Get()->GetMutableSave();
	Save.Jyoti += Enemy->GetJyotiReward();
	URishiSaveSystem::Get()->SaveGame();
	ARishiHUD::PulseJyotiStatic();
}

void ARishiLevelDirector::OnBossDied(ARishiEnemyAI* Enemy)
{
	bBossActive = false;
	CompleteMission(2.5f);
}

void ARishiLevelDirector::OnPlayerDied()
{
	if (bVictory) return;
	bGameOver = true;
	DeadTime = GetWorld()->GetTimeSeconds();
	// URishiGameAudio::SetMusic(EMusicTrack::Menu);
	// URishiGameAudio::Play(ESfx::Death, Player->GetActorLocation());
	if (InputComponent) InputComponent->SetComponentEnabled(false);
}

void ARishiLevelDirector::OnPlayerDamaged(float Damage)
{
	// URishiGameAudio::Play(ESfx::Hurt, Player->GetActorLocation());
	// Camera shake
}

void ARishiLevelDirector::OnAttackStarted(EAttackKind Kind, int32 Step)
{
	if (Kind == EAttackKind::Heavy && Step == 0) return; // Charge start
	// URishiGameAudio::Play(Kind == EAttackKind::Heavy ? ESfx::Swoosh : ESfx::Swoosh, Player->GetActorLocation(), Kind == EAttackKind::Heavy ? 1.0f : 0.6f);
}

void ARishiLevelDirector::OnHitLanded(URishiHurtboxComponent* Hurtbox, const FHitInfo& HitInfo)
{
	if (HitInfo.bGuardBreak || HitInfo.bIsFinisher)
	{
		// Camera shake
	}
}

void ARishiLevelDirector::OnBowFired(bool bCharged)
{
	// URishiGameAudio::Play(ESfx::Arrow, Player->GetActorLocation(), bCharged ? 1.0f : 0.7f);
}

void ARishiLevelDirector::BeginBoss(ARishiEncounterZone* Zone, TFunction<void()> OnBossBegin)
{
	auto Go = [this, OnBossBegin]()
	{
		bBossActive = true;
		// URishiGameAudio::SetMusic(EMusicTrack::Boss);
		
		// Set camera distance for boss arena
		// Camera would be adjusted here
		
		ARishiHUD::ShowHintStatic(TEXT("Senapati — laal ghere se bacho (dodge / kood).\nBada vaar chookne par wo thakta hai: tab bhaari vaar!"), 7.0f);
		
		OnBossBegin();
	};

	int32 BossBit = 1 << MissionIndex;
	FSaveData& Save = URishiSaveSystem::Get()->GetMutableSave();
	
	if ((Save.BossMask & BossBit) == 0 && GetMission().BossIntro.Num() > 0)
	{
		Save.BossMask |= BossBit;
		URishiSaveSystem::Get()->SaveGame();
		PlayBossIntro(Go);
	}
	else
	{
		Go();
	}
}

void ARishiLevelDirector::CompleteMission(float Delay)
{
	if (bVictory || bGameOver) return;
	bVictory = true;
	if (InputComponent) InputComponent->SetComponentEnabled(false);
	// URishiGameAudio::SetMusic(EMusicTrack::Explore);

	FSaveData& Save = URishiSaveSystem::Get()->GetMutableSave();
	Save.bHasDhanush = WeaponManager ? WeaponManager->IsUnlocked(EWeaponId::Dhanush) : false;
	if (WeaponManager)
	{
		Save.TrishulLevel = WeaponManager->GetWeaponLevel(EWeaponId::Trishul);
		Save.DhanushLevel = WeaponManager->GetWeaponLevel(EWeaponId::Dhanush);
	}
	URishiSaveSystem::Get()->SaveGame();

	GetWorldTimerManager().SetTimerForNextTick([this, Delay]() 
	{
		PlayEndingCutscene();
	});
}

void ARishiLevelDirector::PlayIntroCutscene()
{
	// Would play Sohrai painting cutscene
	// For now just show banner
}

void ARishiLevelDirector::PlayBossIntro(TFunction<void()> OnDone)
{
	// Would play boss intro cutscene
	OnDone();
}

void ARishiLevelDirector::PlayEndingCutscene()
{
	bool bLast = MissionIndex >= MissionDefs.Num() - 1;
	
	// Would play outro cutscene
	// For now just transition
	
	if (bLast)
	{
		bEndScreen = true;
		// URishiGameAudio::SetMusic(EMusicTrack::Menu);
		FSaveData& Save = URishiSaveSystem::Get()->GetMutableSave();
		Save.bFinished = true;
		Save.Unlocked = MissionDefs.Num() - 1;
		URishiSaveSystem::Get()->SaveGame();
	}
	else
	{
		URishiGameFlow::Get()->CompleteMission(MissionIndex);
	}
}

void ARishiLevelDirector::ShowGameOverMenu()
{
	// Would show game over UI
}

void ARishiLevelDirector::HandleGameOverInput()
{
	if (!InputComponent) return;
	
	// Simplified - just retry on any input
	if (InputComponent->WasJumpPressed() || InputComponent->WasInteractPressed())
	{
		ChooseGameOverOption(0);
	}
}

void ARishiLevelDirector::ChooseGameOverOption(int32 Index)
{
	if (Index == 0) URishiGameFlow::Get()->Retry();
	else URishiGameFlow::Get()->ReturnToMenu();
}