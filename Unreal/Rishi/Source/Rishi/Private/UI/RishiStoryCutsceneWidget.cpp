// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "UI/RishiStoryCutsceneWidget.h"
#include "Components/Image.h"
#include "Components/TextBlock.h"
#include "Engine/Texture2D.h"
#include "Kismet/GameplayStatics.h"
#include "Core/RishiGamePause.h"

const float URishiStoryCutsceneWidget::RevealTime = 1.6f;
const float URishiStoryCutsceneWidget::AutoAdvanceTime = 7.5f;
TObjectPtr<URishiStoryCutsceneWidget> URishiStoryCutsceneWidget::CurrentInstance = nullptr;

void URishiStoryCutsceneWidget::NativeTick(const FGeometry& MyGeometry, float InDeltaTime)
{
	Super::NativeTick(MyGeometry, InDeltaTime);

	if (CurrentSlides.Num() == 0) return;

	float dt = InDeltaTime;
	SlideTimer += dt;

	if (bEnding)
	{
		FadeAlpha = FMath::Clamp(FadeAlpha + dt * 2.5f, 0.0f, 1.0f);
		if (FadeAlpha >= 1.0f)
		{
			FinishCutscene();
		}
		return;
	}

	FadeAlpha = FMath::Clamp(FadeAlpha - dt * 2.0f, 0.0f, 1.0f);

	// Input handling
	bool bNext = false;
	bool bSkip = false;

	if (APlayerController* PC = GetOwningPlayer())
	{
		if (PC->WasInputKeyJustPressed(EKeys::SpaceBar) || PC->WasInputKeyJustPressed(EKeys::Enter) || PC->WasInputKeyJustPressed(EKeys::Gamepad_FaceButton_Bottom))
		{
			bNext = true;
		}
		if (PC->WasInputKeyJustPressed(EKeys::Escape) || PC->WasInputKeyJustPressed(EKeys::Gamepad_FaceButton_Right) || PC->WasInputKeyJustPressed(EKeys::Gamepad_Special_Right))
		{
			bSkip = true;
		}
	}

	if (bSkip)
	{
		bEnding = true;
		return;
	}

	if (bNext && SlideTimer < RevealTime)
	{
		SlideTimer = RevealTime;
		return;
	}

	if (bNext || SlideTimer > AutoAdvanceTime)
	{
		AdvanceSlide();
	}

	UpdatePaintingReveal();
}

void URishiStoryCutsceneWidget::PlayCutscene(const TArray<FSlide>& Slides, TFunction<void()> OnComplete)
{
	CurrentSlides = Slides;
	OnCompleteCallback = OnComplete;
	CurrentSlideIndex = 0;
	SlideTimer = 0.0f;
	bEnding = false;
	FadeAlpha = 1.0f;
	CurrentInstance = this;

	SetVisibility(ESlateVisibility::Visible);

	// Pause game
	ARishiGamePause::Get()->SetLocked(true);
	ARishiGamePause::Get()->SetPaused(true);

	if (FadeInAnim) PlayAnimation(FadeInAnim);
	
	if (ProgressText)
	{
		ProgressText->SetText(FText::FromString(FString::Printf(TEXT("%d/%d     Space / A = aage     Esc / B = skip"), 1, CurrentSlides.Num())));
	}

	// Show first slide
	if (CurrentSlides.Num() > 0)
	{
		const FSlide& Slide = CurrentSlides[0];
		if (SubtitleText) SubtitleText->SetText(FText::FromString(Slide.Line));
		
		// Load painting texture (would be loaded from assets)
		// PaintingImage->SetBrushFromTexture(/* texture for Slide.PaintingName */);
	}
}

void URishiStoryCutsceneWidget::SkipCurrent()
{
	if (CurrentInstance)
	{
		CurrentInstance->bEnding = true;
	}
}

void URishiStoryCutsceneWidget::AdvanceSlide()
{
	CurrentSlideIndex++;
	SlideTimer = 0.0f;

	if (CurrentSlideIndex >= CurrentSlides.Num())
	{
		CurrentSlideIndex = CurrentSlides.Num() - 1;
		bEnding = true;
	}
	else
	{
		// URishiGameAudio::Play(ESfx::UiMove, FVector::ZeroVector);
		
		const FSlide& Slide = CurrentSlides[CurrentSlideIndex];
		if (SubtitleText) SubtitleText->SetText(FText::FromString(Slide.Line));
		// PaintingImage->SetBrushFromTexture(/* texture for Slide.PaintingName */);
		
		if (ProgressText)
		{
			ProgressText->SetText(FText::FromString(FString::Printf(TEXT("%d/%d     Space / A = aage     Esc / B = skip"), CurrentSlideIndex + 1, CurrentSlides.Num())));
		}
	}
}

void URishiStoryCutsceneWidget::FinishCutscene()
{
	SetVisibility(ESlateVisibility::Collapsed);
	ARishiGamePause::Get()->SetLocked(false);
	ARishiGamePause::Get()->SetPaused(false);
	CurrentInstance = nullptr;

	if (OnCompleteCallback)
	{
		OnCompleteCallback();
		OnCompleteCallback = nullptr;
	}
}

void URishiStoryCutsceneWidget::UpdatePaintingReveal()
{
	// This would update the painting reveal animation
	// The painting is drawn left-to-right over RevealTime
	// In UMG this would be handled by a material or widget animation
}