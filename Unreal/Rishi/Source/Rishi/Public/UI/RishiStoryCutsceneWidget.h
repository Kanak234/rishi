// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "RishiGameTypes.h"
#include "RishiStoryCutsceneWidget.generated.h"

class UImage;
class UTextBlock;
class UWidgetAnimation;

UCLASS()
class RISHI_API URishiStoryCutsceneWidget : public UUserWidget
{
	GENERATED_BODY()

public:
	UFUNCTION(BlueprintCallable, Category = "Cutscene")
	void PlayCutscene(const TArray<FSlide>& Slides, TFunction<void()> OnComplete);

	UFUNCTION(BlueprintCallable, Category = "Cutscene")
	static void SkipCurrent();

protected:
	virtual void NativeTick(const FGeometry& MyGeometry, float InDeltaTime) override;

private:
	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UImage> PaintingImage;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UTextBlock> SubtitleText;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UTextBlock> ProgressText;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UWidgetAnimation> FadeInAnim;

	UPROPERTY(meta = (BindWidget))
	TObjectPtr<UWidgetAnimation> FadeOutAnim;

	TArray<FSlide> CurrentSlides;
	TFunction<void()> OnCompleteCallback;
	int32 CurrentSlideIndex = 0;
	float SlideTimer = 0.0f;
	bool bEnding = false;
	float FadeAlpha = 1.0f;

	static const float RevealTime;
	static const float AutoAdvanceTime;

	static TObjectPtr<URishiStoryCutsceneWidget> CurrentInstance;

	void AdvanceSlide();
	void FinishCutscene();
	void UpdatePaintingReveal();
};