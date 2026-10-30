// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "UI/RishiSohraiPainter.h"
#include "Engine/Texture2D.h"
#include "Misc/AssertionMacros.h"

URishiSohraiPainter* URishiSohraiPainter::SohraiPainterInstance = nullptr;

URishiSohraiPainter::URishiSohraiPainter()
{
	PaintingCache.SetNum((int32)ESohraiPainting::Dawn + 1);
}

URishiSohraiPainter* URishiSohraiPainter::Get()
{
	if (!SohraiPainterInstance)
	{
		SohraiPainterInstance = NewObject<URishiSohraiPainter>(GetTransientPackage(), URishiSohraiPainter::StaticClass(), NAME_None, RF_Transient | RF_StrongRefOnFrame);
	}
	return SohraiPainterInstance;
}

UTexture2D* URishiSohraiPainter::GetPainting(ESohraiPainting Painting)
{
	int32 Index = (int32)Painting;
	if (PaintingCache.IsValidIndex(Index) && PaintingCache[Index])
	{
		return PaintingCache[Index];
	}

	TArray<FColor> Pixels;
	Pixels.SetNum(PaintingWidth * PaintingHeight);
	GeneratePainting(Painting, Pixels);
	DrawFrame(Pixels);

	// Flip vertically for texture
	TArray<FColor> FlippedPixels;
	FlippedPixels.SetNum(PaintingWidth * PaintingHeight);
	for (int32 Y = 0; Y < PaintingHeight; Y++)
	{
		for (int32 X = 0; X < PaintingWidth; X++)
		{
			FlippedPixels[(PaintingHeight - 1 - Y) * PaintingWidth + X] = Pixels[Y * PaintingWidth + X];
		}
	}

	UTexture2D* Texture = UTexture2D::CreateTransient(PaintingWidth, PaintingHeight, PF_B8G8R8A8);
	if (Texture)
	{
		void* TextureData = Texture->GetPlatformData()->Mips[0].BulkData.Lock(LOCK_READ_WRITE);
		FMemory::Memcpy(TextureData, FlippedPixels.GetData(), FlippedPixels.Num() * sizeof(FColor));
		Texture->GetPlatformData()->Mips[0].BulkData.Unlock();
		Texture->UpdateResource();
	}

	if (PaintingCache.IsValidIndex(Index))
	{
		PaintingCache[Index] = Texture;
	}

	return Texture;
}

void URishiSohraiPainter::GenerateAllPaintings()
{
	for (int32 i = 0; i <= (int32)ESohraiPainting::Dawn; i++)
	{
		GetPainting((ESohraiPainting)i);
	}
}

void URishiSohraiPainter::GeneratePainting(ESohraiPainting Painting, TArray<FColor>& Pixels)
{
	DrawWall(Pixels);

	switch (Painting)
	{
	case ESohraiPainting::Village:
		DrawMoon(Pixels, 520, 70, 34);
		DrawRiver(Pixels, 270);
		for (int32 i = 0; i < 3; i++) DrawHut(Pixels, 90 + i * 120, 230, 1.0f);
		for (int32 i = 0; i < 5; i++) DrawPerson(Pixels, 380 + i * 34, 245, 0.8f, Chuna, i % 2 == 0);
		for (int32 i = 0; i < 6; i++) DrawDeepak(Pixels, 60 + i * 100, 262);
		DrawTree(Pixels, 600, 210);
		break;

	case ESohraiPainting::TamasRises:
		DrawRiver(Pixels, 250);
		FillCircle(Pixels, 320, 210, 120, Tamas);
		FillCircle(Pixels, 250, 170, 70, Tamas);
		FillCircle(Pixels, 400, 160, 80, Tamas);
		FillCircle(Pixels, 320, 110, 70, Tamas);
		DrawRing(Pixels, 320, 180, 150, 3, Kaala);
		FillCircle(Pixels, 290, 120, 12, Sona); FillCircle(Pixels, 350, 120, 12, Sona);
		FillCircle(Pixels, 290, 120, 5, Kaala); FillCircle(Pixels, 350, 120, 5, Kaala);
		DrawLine(Pixels, 270, 98, 305, 108, 4, Sona); DrawLine(Pixels, 370, 98, 335, 108, 4, Sona);
		for (int32 i = 0; i < 4; i++) { int32 x = 70 + i * 160; DrawLamp(Pixels, x, 300); DrawWavy(Pixels, x, 290, 40, Chuna, true); }
		break;

	case ESohraiPainting::GuruTaken:
		DrawMoon(Pixels, 110, 70, 28);
		DrawRiver(Pixels, 280);
		FillTriangle(Pixels, 380, 40, 620, 60, 520, 300, Tamas);
		FillCircle(Pixels, 500, 120, 60, Tamas);
		FillCircle(Pixels, 480, 100, 8, Sona); FillCircle(Pixels, 520, 100, 8, Sona);
		DrawPerson(Pixels, 430, 190, 1.1f, Chuna, false);
		DrawLine(Pixels, 410, 160, 460, 150, 3, Kaala);
		DrawPerson(Pixels, 180, 250, 1.2f, Sona, true);
		break;

	case ESohraiPainting::Journey:
		DrawSun(Pixels, 520, 80, 40);
		FillTriangle(Pixels, 0, 300, 160, 120, 320, 300, GeruD);
		FillTriangle(Pixels, 240, 300, 420, 100, 600, 300, GeruD);
		DrawOutline(Pixels, 240, 300, 420, 100, 600, 300);
		DrawOutline(Pixels, 0, 300, 160, 120, 320, 300);
		DrawShikhara(Pixels, 470, 180, 0.6f, false);
		DrawDots(Pixels, 40, 320, 600, 320, 18, Chuna);
		DrawPerson(Pixels, 140, 290, 1.2f, Sona, true);
		DrawTrishul(Pixels, 165, 290, 1.2f);
		break;

	case ESohraiPainting::Mandir:
		DrawMoon(Pixels, 540, 60, 26);
		DrawRiver(Pixels, 300);
		DrawShikhara(Pixels, 320, 270, 1.5f, false);
		for (int32 i = 0; i < 6; i++)
		{
			int32 x = 60 + i * 104;
			if (FMath::Abs(x - 320) < 90) continue;
			DrawLamp(Pixels, x, 296);
		}
		break;

	case ESohraiPainting::Andhvir:
		DrawMoon(Pixels, 90, 60, 24);
		DrawShikhara(Pixels, 110, 260, 0.9f, false);
		DrawPerson(Pixels, 420, 300, 2.4f, Tamas, false);
		DrawLine(Pixels, 470, 180, 560, 90, 8, Kaala);
		FillCircle(Pixels, 565, 82, 26, Kaala); DrawRing(Pixels, 565, 82, 26, 3, Sona);
		FillCircle(Pixels, 410, 150, 6, Sona); FillCircle(Pixels, 432, 150, 6, Sona);
		DrawPerson(Pixels, 230, 300, 1.1f, Sona, true);
		DrawTrishul(Pixels, 255, 300, 1.1f);
		DrawDots(Pixels, 20, 320, 620, 320, 24, Chuna);
		break;

	case ESohraiPainting::JyotiReturns:
		DrawSun(Pixels, 320, 70, 46);
		DrawRiver(Pixels, 300);
		DrawShikhara(Pixels, 320, 270, 1.5f, true);
		for (int32 i = 0; i < 6; i++)
		{
			int32 x = 60 + i * 104;
			if (FMath::Abs(x - 320) < 90) continue;
			DrawDeepak(Pixels, x, 290);
		}
		DrawPerson(Pixels, 130, 285, 1.0f, Sona, true);
		DrawPerson(Pixels, 510, 285, 1.0f, Chuna, false);
		break;

	case ESohraiPainting::Kashi:
		DrawMoon(Pixels, 530, 60, 26);
		DrawRiver(Pixels, 240);
		for (int32 s = 0; s < 5; s++) { DrawRect(Pixels, 0, 240 - s * 18, 640, 18, s % 2 == 0 ? GeruD : Geru); DrawLine(Pixels, 0, 240 - s * 18, 640, 240 - s * 18, 2, Chuna); }
		for (int32 i = 0; i < 8; i++) DrawDeepak(Pixels, 40 + i * 80, 236 - (i % 3) * 18);
		DrawShikhara(Pixels, 160, 150, 0.6f, false); DrawShikhara(Pixels, 460, 150, 0.7f, false);
		for (int32 i = 0; i < 9; i++) DrawDeepak(Pixels, 30 + i * 70, 300 + (i % 2) * 12);
		break;

	case ESohraiPainting::Aarti:
		DrawMoon(Pixels, 560, 50, 22);
		DrawRiver(Pixels, 250);
		for (int32 s = 0; s < 4; s++) { DrawRect(Pixels, 0, 250 - s * 20, 640, 20, s % 2 == 0 ? GeruD : Geru); DrawLine(Pixels, 0, 250 - s * 20, 640, 250 - s * 20, 2, Chuna); }
		for (int32 i = 0; i < 3; i++)
		{
			int32 x = 160 + i * 160;
			DrawRect(Pixels, x - 6, 120, 12, 70, Chuna);
			DrawLamp(Pixels, x, 118);
			FillTriangle(Pixels, x - 12, 110, x + 12, 110, x, 70, Sona);
			FillCircle(Pixels, x, 100, 5, Chuna);
			DrawRing(Pixels, x, 92, 34, 2, Sona);
		}
		DrawPerson(Pixels, 320, 245, 1.0f, Chuna, true);
		for (int32 i = 0; i < 10; i++) DrawDeepak(Pixels, 20 + i * 64, 300 + (i % 2) * 14);
		break;

	case ESohraiPainting::Hampi:
		DrawSun(Pixels, 540, 70, 36);
		FillCircle(Pixels, 90, 250, 70, GeruD); DrawRing(Pixels, 90, 250, 70, 2, Kaala);
		FillCircle(Pixels, 170, 270, 45, GeruD); DrawRing(Pixels, 170, 270, 45, 2, Kaala);
		FillCircle(Pixels, 580, 260, 60, GeruD); DrawRing(Pixels, 580, 260, 60, 2, Kaala);
		for (int32 i = 0; i < 4; i++) DrawRect(Pixels, 250 + i * 40, 170, 14, 110, Chuna);
		DrawRect(Pixels, 240, 160, 170, 14, Chuna);
		DrawRect(Pixels, 420, 210, 110, 50, Chuna);
		DrawRing(Pixels, 445, 268, 18, 3, Kaala); DrawRing(Pixels, 505, 268, 18, 3, Kaala);
		FillTriangle(Pixels, 425, 210, 525, 210, 475, 140, Chuna); DrawOutline(Pixels, 425, 210, 525, 210, 475, 140);
		DrawDots(Pixels, 30, 320, 610, 320, 26, Chuna);
		break;

	case ESohraiPainting::Shilaasur:
		DrawSun(Pixels, 80, 60, 26);
		FColor StoneColor(90, 80, 100, 255);
		FillCircle(Pixels, 420, 250, 80, StoneColor);
		FillCircle(Pixels, 420, 140, 50, StoneColor);
		FillCircle(Pixels, 340, 200, 34, StoneColor); FillCircle(Pixels, 500, 200, 34, StoneColor);
		DrawRing(Pixels, 420, 250, 80, 3, Kaala); DrawRing(Pixels, 420, 140, 50, 3, Kaala);
		FillCircle(Pixels, 402, 132, 9, Sona); FillCircle(Pixels, 438, 132, 9, Sona);
		for (int32 i = 0; i < 5; i++) DrawLine(Pixels, 380 + i * 18, 230, 372 + i * 20, 300, 2, Kaala);
		DrawPerson(Pixels, 170, 300, 1.1f, Sona, true);
		DrawTrishul(Pixels, 195, 300, 1.1f);
		break;

	case ESohraiPainting::Ellora:
		DrawRect(Pixels, 0, 60, 640, 260, GeruD);
		FillTriangle(Pixels, 0, 60, 640, 60, 320, 0, GeruD);
		DrawRect(Pixels, 150, 120, 340, 200, Kaala);
		FillTriangle(Pixels, 150, 120, 490, 120, 320, 70, Kaala);
		DrawShikhara(Pixels, 320, 300, 1.1f, false);
		for (int32 i = 0; i < 5; i++) DrawRect(Pixels, 170 + i * 72, 130, 10, 190, Chuna);
		DrawPerson(Pixels, 560, 305, 1.0f, Sona, true);
		DrawDots(Pixels, 20, 330, 620, 330, 26, Chuna);
		break;

	case ESohraiPainting::GuruFreed:
		DrawRect(Pixels, 0, 40, 640, 290, Kaala);
		FillTriangle(Pixels, 0, 40, 640, 40, 320, 12, Kaala);
		for (int32 i = 0; i < 3; i++) DrawDeepak(Pixels, 160 + i * 160, 200);
		DrawPerson(Pixels, 260, 300, 1.3f, Sona, true);
		DrawPerson(Pixels, 380, 300, 1.3f, Chuna, true);
		for (int32 i = 0; i < 6; i++) DrawRing(Pixels, 420 + i * 16, 250 + (i % 2) * 6, 6, 2, Chuna);
		DrawSun(Pixels, 320, 110, 28);
		break;

	case ESohraiPainting::TamasLok:
		DrawRect(Pixels, 0, 0, 640, 360, FColor(30, 18, 56, 255));
		for (int32 i = 0; i < 30; i++) FillCircle(Pixels, (i * 97) % 640, (i * 53) % 300 + 20, 2 + i % 3, Chuna);
		FillCircle(Pixels, 320, 330, 210, Tamas);
		DrawRing(Pixels, 320, 330, 210, 3, Kaala);
		for (int32 i = 0; i < 5; i++) { int32 x = 200 + i * 60; FillCircle(Pixels, x, 210 - (i % 2) * 20, 8, Sona); FillCircle(Pixels, x, 210 - (i % 2) * 20, 3, Kaala); }
		DrawPerson(Pixels, 320, 340, 0.9f, Sona, true);
		break;

	case ESohraiPainting::Dawn:
		DrawSun(Pixels, 320, 230, 70);
		DrawRiver(Pixels, 260);
		DrawShikhara(Pixels, 110, 260, 0.8f, true); DrawShikhara(Pixels, 530, 260, 0.8f, true);
		for (int32 i = 0; i < 9; i++) DrawDeepak(Pixels, 40 + i * 70, 300 + (i % 2) * 14);
		DrawPerson(Pixels, 300, 258, 1.1f, Sona, true); DrawPerson(Pixels, 345, 258, 1.1f, Chuna, true);
		for (int32 i = 0; i < 5; i++) DrawPerson(Pixels, 80 + i * 26, 258, 0.6f, Chuna, i % 2 == 0);
		break;
	}
}

void URishiSohraiPainter::DrawWall(TArray<FColor>& Pixels)
{
	for (int32 Y = 0; Y < PaintingHeight; Y++)
	{
		for (int32 X = 0; X < PaintingWidth; X++)
		{
			float N = FMath::PerlinNoise2D(FVector2D(X * 0.02f, Y * 0.02f)) * 0.6f + FMath::PerlinNoise2D(FVector2D(X * 0.11f, Y * 0.11f)) * 0.4f;
			FColor Color = FMath::Lerp(GeruD, Geru, 0.55f + N * 0.45f);
			PutPixel(Pixels, X, Y, Color);
		}
	}
}

void URishiSohraiPainter::DrawFrame(TArray<FColor>& Pixels)
{
	DrawRect(Pixels, 0, 0, PaintingWidth, 10, Chuna);
	DrawRect(Pixels, 0, PaintingHeight - 10, PaintingWidth, 10, Chuna);
	DrawRect(Pixels, 0, 0, 10, PaintingHeight, Chuna);
	DrawRect(Pixels, PaintingWidth - 10, 0, 10, PaintingHeight, Chuna);
	DrawLine(Pixels, 14, 14, PaintingWidth - 14, 14, 2, Kaala);
	DrawLine(Pixels, 14, PaintingHeight - 15, PaintingWidth - 14, PaintingHeight - 15, 2, Kaala);
	DrawLine(Pixels, 14, 14, 14, PaintingHeight - 15, 2, Kaala);
	DrawLine(Pixels, PaintingWidth - 15, 14, PaintingWidth - 15, PaintingHeight - 15, 2, Kaala);
	DrawDots(Pixels, 24, 22, PaintingWidth - 24, 22, 40, Sona);
	DrawDots(Pixels, 24, PaintingHeight - 23, PaintingWidth - 24, PaintingHeight - 23, 40, Sona);
}

void URishiSohraiPainter::DrawRiver(TArray<FColor>& Pixels, int32 Y)
{
	FColor RiverColor(80, 34, 22, 255);
	DrawRect(Pixels, 0, Y, PaintingWidth, PaintingHeight - Y, RiverColor);
	for (int32 r = 0; r < 4; r++) DrawWavy(Pixels, 0, Y + 14 + r * 18, PaintingWidth, Chuna, false);
}

void URishiSohraiPainter::DrawMoon(TArray<FColor>& Pixels, int32 X, int32 Y, int32 R)
{
	FillCircle(Pixels, X, Y, R, Chuna);
	DrawRing(Pixels, X, Y, R + 8, 2, Chuna);
	DrawDots(Pixels, X - R - 30, Y, X + R + 30, Y, 0, Chuna);
}

void URishiSohraiPainter::DrawSun(TArray<FColor>& Pixels, int32 X, int32 Y, int32 R)
{
	FillCircle(Pixels, X, Y, R, Sona);
	DrawRing(Pixels, X, Y, R, 3, Kaala);
	for (int32 i = 0; i < 16; i++)
	{
		float A = i * 3.14159f / 8.0f;
		DrawLine(Pixels, X + (int32)(FMath::Cos(A) * (R + 8)), Y + (int32)(FMath::Sin(A) * (R + 8)),
			X + (int32)(FMath::Cos(A) * (R + 26)), Y + (int32)(FMath::Sin(A) * (R + 26)), 3, Sona);
	}
}

void URishiSohraiPainter::DrawHut(TArray<FColor>& Pixels, int32 X, int32 BaseY, float Scale)
{
	int32 W = (int32)(80 * Scale);
	int32 H = (int32)(50 * Scale);
	DrawRect(Pixels, X - W / 2, BaseY - H, W, H, Chuna);
	FillTriangle(Pixels, X - W / 2 - 12, BaseY - H, X + W / 2 + 12, BaseY - H, X, BaseY - H - (int32)(44 * Scale), Kaala);
	DrawRect(Pixels, X - 10, BaseY - 30, 20, 30, GeruD);
	DrawDots(Pixels, X - W / 2 + 6, BaseY - H + 12, X + W / 2 - 6, BaseY - H + 12, 7, Geru);
}

void URishiSohraiPainter::DrawPerson(TArray<FColor>& Pixels, int32 X, int32 FootY, float Scale, FColor Color, bool bArmsUp)
{
	int32 H = (int32)(60 * Scale);
	int32 W = (int32)(18 * Scale);
	int32 Waist = FootY - (int32)(H * 0.45f);
	int32 Shoulder = FootY - (int32)(H * 0.85f);
	FillTriangle(Pixels, X - W, Shoulder, X + W, Shoulder, X, Waist, Color);
	FillTriangle(Pixels, X, Waist, X - W, FootY - (int32)(H * 0.12f), X + W, FootY - (int32)(H * 0.12f), Color);
	FillCircle(Pixels, X, Shoulder - (int32)(9 * Scale), (int32)(8 * Scale), Color);
	int32 Th = FMath::Max(2, (int32)(3 * Scale));
	DrawLine(Pixels, X - W / 2, FootY - (int32)(H * 0.12f), X - W / 2, FootY, Th, Color);
	DrawLine(Pixels, X + W / 2, FootY - (int32)(H * 0.12f), X + W / 2, FootY, Th, Color);
	if (bArmsUp)
	{
		DrawLine(Pixels, X - W, Shoulder, X - W - (int32)(12 * Scale), Shoulder - (int32)(18 * Scale), Th, Color);
		DrawLine(Pixels, X + W, Shoulder, X + W + (int32)(12 * Scale), Shoulder - (int32)(18 * Scale), Th, Color);
	}
	else
	{
		DrawLine(Pixels, X - W, Shoulder, X - W - (int32)(8 * Scale), Waist, Th, Color);
		DrawLine(Pixels, X + W, Shoulder, X + W + (int32)(8 * Scale), Waist, Th, Color);
	}
}

void URishiSohraiPainter::DrawTrishul(TArray<FColor>& Pixels, int32 X, int32 FootY, float Scale)
{
	int32 Top = FootY - (int32)(90 * Scale);
	DrawLine(Pixels, X, FootY, X, Top, 3, Sona);
	DrawLine(Pixels, X - (int32)(10 * Scale), Top + (int32)(12 * Scale), X + (int32)(10 * Scale), Top + (int32)(12 * Scale), 3, Sona);
	DrawLine(Pixels, X - (int32)(10 * Scale), Top + (int32)(12 * Scale), X - (int32)(10 * Scale), Top, 3, Sona);
	DrawLine(Pixels, X + (int32)(10 * Scale), Top + (int32)(12 * Scale), X + (int32)(10 * Scale), Top, 3, Sona);
	DrawLine(Pixels, X, Top, X, Top - (int32)(8 * Scale), 3, Sona);
}

void URishiSohraiPainter::DrawShikhara(TArray<FColor>& Pixels, int32 X, int32 BaseY, float Scale, bool bLit)
{
	int32 W = (int32)(90 * Scale);
	int32 H = (int32)(150 * Scale);
	DrawRect(Pixels, X - W / 2, BaseY - (int32)(50 * Scale), W, (int32)(50 * Scale), Chuna);
	FillTriangle(Pixels, X - W / 2 + 6, BaseY - (int32)(50 * Scale), X + W / 2 - 6, BaseY - (int32)(50 * Scale), X, BaseY - H, Chuna);
	DrawOutline(Pixels, X - W / 2 + 6, BaseY - (int32)(50 * Scale), X + W / 2 - 6, BaseY - (int32)(50 * Scale), X, BaseY - H);
	for (int32 i = 1; i < 5; i++)
	{
		int32 YY = BaseY - (int32)(50 * Scale) - i * (int32)(20 * Scale);
		int32 Half = (int32)((W / 2 - 6) * (1.0f - i * 20.0f * Scale / (H - 50.0f * Scale)));
		DrawLine(Pixels, X - Half, YY, X + Half, YY, 2, Kaala);
	}
	DrawRect(Pixels, X - (int32)(12 * Scale), BaseY - (int32)(34 * Scale), (int32)(24 * Scale), (int32)(34 * Scale), Kaala);
	FillCircle(Pixels, X, BaseY - H - (int32)(8 * Scale), (int32)(7 * Scale), bLit ? Sona : Kaala);
	if (bLit) DrawDeepak(Pixels, X, BaseY - (int32)(36 * Scale));
}

void URishiSohraiPainter::DrawDeepak(TArray<FColor>& Pixels, int32 X, int32 Y)
{
	DrawLamp(Pixels, X, Y);
	FillTriangle(Pixels, X - 5, Y - 6, X + 5, Y - 6, X, Y - 24, Sona);
	FillCircle(Pixels, X, Y - 9, 3, Chuna);
}

void URishiSohraiPainter::DrawLamp(TArray<FColor>& Pixels, int32 X, int32 Y)
{
	FillTriangle(Pixels, X - 12, Y - 6, X + 12, Y - 6, X, Y + 4, Kaala);
	DrawLine(Pixels, X - 12, Y - 6, X + 12, Y - 6, 2, Chuna);
}

void URishiSohraiPainter::DrawTree(TArray<FColor>& Pixels, int32 X, int32 BaseY)
{
	DrawLine(Pixels, X, BaseY, X, BaseY - 70, 5, Kaala);
	for (int32 i = 0; i < 18; i++)
	{
		float A = i * 0.7f;
		int32 RR = 16 + (i % 4) * 8;
		FillCircle(Pixels, X + (int32)(FMath::Cos(A) * RR), BaseY - 80 + (int32)(FMath::Sin(A) * RR * 0.7f), 4, Chuna);
	}
}

void URishiSohraiPainter::FillCircle(TArray<FColor>& Pixels, int32 Cx, int32 Cy, int32 R, FColor Color)
{
	for (int32 Y = -R; Y <= R; Y++)
	{
		for (int32 X = -R; X <= R; X++)
		{
			if (X * X + Y * Y <= R * R) PutPixel(Pixels, Cx + X, Cy + Y, Color);
		}
	}
}

void URishiSohraiPainter::DrawRing(TArray<FColor>& Pixels, int32 Cx, int32 Cy, int32 R, int32 Thickness, FColor Color)
{
	int32 RO = R + Thickness;
	int32 RI = FMath::Max(0, R - Thickness);
	for (int32 Y = -RO; Y <= RO; Y++)
	{
		for (int32 X = -RO; X <= RO; X++)
		{
			int32 D = X * X + Y * Y;
			if (D <= RO * RO && D >= RI * RI) PutPixel(Pixels, Cx + X, Cy + Y, Color);
		}
	}
}

void URishiSohraiPainter::DrawLine(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 Thickness, FColor Color)
{
	int32 Steps = FMath::Max(FMath::Abs(X1 - X0), FMath::Abs(Y1 - Y0), 1);
	int32 R = FMath::Max(0, Thickness / 2);
	for (int32 i = 0; i <= Steps; i++)
	{
		int32 X = X0 + (X1 - X0) * i / Steps;
		int32 Y = Y0 + (Y1 - Y0) * i / Steps;
		for (int32 DY = -R; DY <= R; DY++)
			for (int32 DX = -R; DX <= R; DX++) PutPixel(Pixels, X + DX, Y + DY, Color);
	}
}

void URishiSohraiPainter::FillTriangle(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 X2, int32 Y2, FColor Color)
{
	int32 MinX = FMath::Max(0, FMath::Min(X0, FMath::Min(X1, X2)));
	int32 MaxX = FMath::Min(PaintingWidth - 1, FMath::Max(X0, FMath::Max(X1, X2)));
	int32 MinY = FMath::Max(0, FMath::Min(Y0, FMath::Min(Y1, Y2)));
	int32 MaxY = FMath::Min(PaintingHeight - 1, FMath::Max(Y0, FMath::Max(Y1, Y2)));

	for (int32 Y = MinY; Y <= MaxY; Y++)
	{
		for (int32 X = MinX; X <= MaxX; X++)
		{
			float D1 = (X - X2) * (Y0 - Y2) - (X0 - X2) * (Y - Y2);
			float D2 = (X - X0) * (Y1 - Y0) - (X1 - X0) * (Y - Y0);
			float D3 = (X - X1) * (Y2 - Y1) - (X2 - X1) * (Y - Y1);
			bool bNeg = D1 < 0 || D2 < 0 || D3 < 0;
			bool bPos = D1 > 0 || D2 > 0 || D3 > 0;
			if (!(bNeg && bPos)) PutPixel(Pixels, X, Y, Color);
		}
	}
}

void URishiSohraiPainter::DrawOutline(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 X2, int32 Y2)
{
	DrawLine(Pixels, X0, Y0, X1, Y1, 3, Kaala);
	DrawLine(Pixels, X1, Y1, X2, Y2, 3, Kaala);
	DrawLine(Pixels, X2, Y2, X0, Y0, 3, Kaala);
}

void URishiSohraiPainter::DrawDots(TArray<FColor>& Pixels, int32 X0, int32 Y0, int32 X1, int32 Y1, int32 Count, FColor Color)
{
	for (int32 i = 0; i < Count; i++)
	{
		float T = Count == 1 ? 0.5f : (float)i / (Count - 1);
		FillCircle(Pixels, FMath::RoundToInt(FMath::Lerp(X0, X1, T)), FMath::RoundToInt(FMath::Lerp(Y0, Y1, T)), 3, Color);
	}
}

void URishiSohraiPainter::DrawWavy(TArray<FColor>& Pixels, int32 X, int32 Y, int32 Len, FColor Color, bool bVertical)
{
	for (int32 i = 0; i < Len; i++)
	{
		int32 O = FMath::RoundToInt(FMath::Sin(i * 0.18f) * 4.0f);
		if (bVertical) { PutPixel(Pixels, X + O, Y - i, Color); PutPixel(Pixels, X + O + 1, Y - i, Color); }
		else { PutPixel(Pixels, X + i, Y + O, Color); PutPixel(Pixels, X + i, Y + O + 1, Color); }
	}
}

void URishiSohraiPainter::DrawRect(TArray<FColor>& Pixels, int32 X, int32 Y, int32 W, int32 H, FColor Color)
{
	for (int32 YY = FMath::Max(0, Y); YY < FMath::Min(PaintingHeight, Y + H); YY++)
	{
		for (int32 XX = FMath::Max(0, X); XX < FMath::Min(PaintingWidth, X + W); XX++)
		{
			PutPixel(Pixels, XX, YY, Color);
		}
	}
}

void URishiSohraiPainter::PutPixel(TArray<FColor>& Pixels, int32 X, int32 Y, FColor Color)
{
	if ((uint32)X < (uint32)PaintingWidth && (uint32)Y < (uint32)PaintingHeight)
	{
		Pixels[Y * PaintingWidth + X] = Color;
	}
}