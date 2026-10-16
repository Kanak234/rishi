// =====================================================================
// VayuPishach.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD: "udte hue, door se teer chalate hain" — Trishul wahan tak
//       nahi pahunchta, isliye Dhanush ka kaam aata hai (shastra switch sikhao).
// KYA:  Zameen se hoverHeight upar tairta hai (CharacterController nahi).
//       Rishi se preferred doori rakhta hai (paas aaye to peeche hate),
//       side me dheere ghoomta hai, aur windup ke baad Rishi ke seene par teer.
//       GDD teer = 10.
// AAGE: EnemyAI base (states, token, hurt). ArrowPool se teer → Rishi ka Hurtbox.
// =====================================================================
using UnityEngine;
using Rishi.Combat;

namespace Rishi.Enemies
{
    public class VayuPishach : EnemyAI
    {
        [Header("Udna")]
        [SerializeField] float hoverHeight = 3.0f;      // Trishul ki pahunch (≈2 m) se upar
        [SerializeField] float bobAmount = 0.25f;
        [SerializeField] float preferredDistance = 9f;
        [SerializeField] float tooClose = 6f;
        [SerializeField] float strafeSpeed = 1.5f;

        [Header("Teer")]
        [SerializeField] float arrowSpeed = 18f;        // Rishi ke teer se dheema — dekh ke dodge kar sako
        [SerializeField] Material arrowMaterial;

        ArrowPool _pool;
        float _strafeSign = 1f;
        float _nextStrafeFlip;

        void Reset()
        {
            detectRange = 16f; loseRange = 24f; moveSpeed = 3.5f;
            attackRange = 13f; windup = 0.7f; activeTime = 0.05f; recovery = 0.6f; attackCooldown = 2.2f;
            damage = 10f;                                  // GDD: Vayu-pishach teer = 10
            hurtStun = 0.5f; knockbackDecay = 6f;
            jyotiReward = 2;
        }

        protected override void Awake()
        {
            base.Awake();
            _pool = new ArrowPool($"ArrowPool ({name})", 3, arrowMaterial, new Vector3(0.05f, 0.05f, 0.6f));
        }

        void OnDestroy() => _pool?.Dispose();

        protected override Vector3 ChaseVelocity(Vector3 toTarget, float dist)
        {
            Vector3 dir = toTarget / Mathf.Max(dist, 0.001f);
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            if (Time.time >= _nextStrafeFlip) { _strafeSign = -_strafeSign; _nextStrafeFlip = Time.time + Random.Range(2f, 4f); }

            if (dist < tooClose) return -dir * moveSpeed;                       // Rishi paas aaya → peeche hato
            if (dist > preferredDistance + 2f) return dir * moveSpeed;           // door hai → paas aao
            return side * (strafeSpeed * _strafeSign);                            // sahi doori → ghoomte raho
        }

        protected override void PerformAttack()
        {
            if (Target == null) return;
            Arrow a = _pool.Get();
            if (a == null) return;

            Vector3 muzzle = transform.position + transform.forward * 0.7f;  // apne collider se bahar
            Vector3 chest = Target.position + Vector3.up * 1.2f;
            Vector3 dir = (chest - muzzle).normalized;
            var info = new HitInfo
            {
                Damage = damage,
                Direction = new Vector3(dir.x, 0f, dir.z).normalized,
                AttackerTeam = Team.Asur,
                Attacker = transform,
            };
            a.Launch(muzzle, dir, arrowSpeed, 1.5f, info, ~0);
            Rishi.Core.GameAudio.Play(Rishi.Core.Sfx.Arrow, muzzle, 0.7f);
        }

        protected override void MoveBody(Vector3 horizontal, float dt)
        {
            // Zameen se unchai naap ke hover (seedhi/chabootre par bhi upar rahe).
            float groundY = Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit h, 30f, ~0, QueryTriggerInteraction.Ignore)
                ? h.point.y : transform.position.y - hoverHeight;
            float targetY = groundY + hoverHeight + Mathf.Sin(Time.time * 2f + GetHashCode()) * bobAmount;
            Vector3 p = transform.position + horizontal * dt;
            p.y = Mathf.Lerp(transform.position.y, targetY, 1f - Mathf.Exp(-4f * dt));
            transform.position = p;
        }
    }
}
