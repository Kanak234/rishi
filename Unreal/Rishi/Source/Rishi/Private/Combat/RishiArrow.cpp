// Copyright 2026 Kanak Prabhakar. All Rights Reserved.

#include "Combat/RishiArrow.h"
#include "Components/StaticMeshComponent.h"
#include "GameFramework/ProjectileMovementComponent.h"
#include "Combat/RishiHurtboxComponent.h"
#include "Combat/RishiHitFilterInterface.h"
#include "Engine/StaticMesh.h"
#include "Materials/MaterialInterface.h"

ARishiArrow::ARishiArrow()
{
	PrimaryActorTick.bCanEverTick = false;

	MeshComponent = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("Mesh"));
	RootComponent = MeshComponent;
	MeshComponent->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
	MeshComponent->SetCollisionResponseToAllChannels(ECR_Ignore);
	MeshComponent->SetCollisionResponseToChannel(ECC_WorldStatic, ECR_Block);
	MeshComponent->SetCollisionResponseToChannel(ECC_WorldDynamic, ECR_Block);
	MeshComponent->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);
	MeshComponent->SetGenerateOverlapEvents(true);

	MovementComponent = CreateDefaultSubobject<UProjectileMovementComponent>(TEXT("Movement"));
	MovementComponent->InitialSpeed = 3000.0f;
	MovementComponent->MaxSpeed = 3000.0f;
	MovementComponent->ProjectileGravityScale = 0.0f;
	MovementComponent->bRotationFollowsVelocity = true;
	MovementComponent->bShouldBounce = false;
}

void ARishiArrow::Launch(const FVector& Direction, float Speed, float Lifetime, const FHitInfo& InHitInfo, ECollisionChannel HitChannel)
{
	HitInfo = InHitInfo;
	HitCollisionChannel = HitChannel;

	MovementComponent->Velocity = Direction * Speed;
	SetActorRotation(Direction.Rotation());

	GetWorldTimerManager().SetTimer(LifetimeTimer, [this]() { Destroy(); }, Lifetime, false);

	MeshComponent->OnComponentHit.AddDynamic(this, &ARishiArrow::OnHit);
}

void ARishiArrow::BeginPlay()
{
	Super::BeginPlay();
}

void ARishiArrow::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	MeshComponent->OnComponentHit.RemoveAll(this);
	GetWorldTimerManager().ClearTimer(LifetimeTimer);
	Super::EndPlay(EndPlayReason);
}

void ARishiArrow::OnHit(UPrimitiveComponent* HitComponent, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit)
{
	if (!OtherActor || OtherActor == GetOwner()) return;

	// Check for hurtbox
	URishiHurtboxComponent* Hurtbox = OtherActor->FindComponentByClass<URishiHurtboxComponent>();
	if (Hurtbox)
	{
		// Check team
		if (Hurtbox->GetTeam() == HitInfo.AttackerTeam) return;

		// Apply hit filter if implemented
		if (OtherActor->GetClass()->ImplementsInterface(URishiHitFilterInterface::StaticClass()))
		{
			IRishiHitFilterInterface::Execute_FilterHit(OtherActor, HitInfo);
		}

		if (Hurtbox->ReceiveHit(HitInfo))
		{
			Destroy();
			return;
		}
	}

	// Hit world - destroy
	Destroy();
}