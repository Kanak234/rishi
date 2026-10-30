// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "RishiSohraiPainter.generated.h"

UENUM(BlueprintType)
enum class ESohraiPainting : uint8
{
	Village,
	TamasRises,
	GuruTaken,
	Journey,
	Mandir,
	Andhvir,
	JyotiReturns,
	Kashi,
	Aarti,
	Hampi,
	Shilaasur,
	Ellora,
	GuruFreed,
	TamasLok,
	Dawn
};

UCLASS()
class RISHI_API URishiSohraiPainter : public UObject
{
	GENERATED_BODY()

public:
	static URishiSohraiPainter* Get();

	UFUNCTION(BlueprintCallable, Category = "Sohrai")
	UTexture2D* GetPainting(ESohraiPainting Painting);

	UFUNCTION(BlueprintCallable, Category = "Sohrai")
	void GenerateAllPaintings();

private:
	URishiSohraiPainter();

	static const int32 PaintingWidth = 640;
	static const int32 PaintingHeight = 360;

	TArray<TObjectPtr<UTexture2D>> PaintingCache;

	// Colors (converted from Unity)
	const FColor Geru = FColor(150, 62, 34, 255);
	const FColor GeruD = FColor(112, 42, 22, 255);
	const FColor Chuna = FColor(242, 234, 214, 255);
	const FColor Kaala = FColor(22, 16, 14, 255);
	const FColor Sona = FColor(250, 184, 56, 255);
	const FColor Tamas = FColor(52, 30, 100, 255);

	void GeneratePainting(ESohraiPainting Painting, TArray<FColor>& Pixels);
	void DrawWall(TArray<FColor>& Pixels);
	void DrawFrame(TArray<FColor>& Pixels);

	// Motif functions
	void DrawRiver(TArray<FColor>& Pixels, int32 Y);
	void DrawMoon(TArray<FColor>& Pixels, int32 X, int32 Y, int32 R);
	void DrawSun(TArray<FColor>& Pixels, int32 X, int32 Y, int32 R);
	void DrawHut(TArray<FColor>& Pixels, int32 X, int32 BaseY, float Scale);
	void DrawPerson(TArray<FColor>& Pixels, int32 X, int32 FootY, float Scale, FColor Color, bool bArmsUp);
	void DrawTrishul(TArray<FColor>& Pixels, int32 X, int32 FootY, float Scale);
	void DrawShikhara(TArray<FColor>& Pixels, int32 X, int32 BaseY, float Scale, bool bLit);
	void DrawDeepak(TArray<FColor>& Pixels, int32 X, int32 Y);
	void DrawLamp(TArray<FColor>& Pixels, int32 X, int32 Y);
	void DrawTree(TArray<FColor>& Pixels, int32 X, int32 BaseY);
	void FillCircle(TArray<FColor>& Pixels, int32 Cx, int32 Cy, int32 R, FColor Color);
	void DrawRing(TArray<FColor>& Pixels, int32 Cx, int32 Cy, int32 R, int32 Thickness, FColor Color);
	void DrawLine(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 Thickness, FColor Color);
	void FillTriangle(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 X2, int32 Y2, FColor Color);
	void DrawOutline(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 X2, int32 Y2);
	void DrawDots(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 Count, FColor Color);
	void DrawWavy(TArray<FColor>& Pixels, int32 X, int32 Y, int32 Len, FColor Color, bool bVertical);
	void DrawRect(TArray<FColor>& Pixels, int32 X, int32 Y, int32 W, int32 H, FColor Color);
	void PutPixel(TArray<FColor>& Pixels, int32 X, int32 Y, FColor Color);
};