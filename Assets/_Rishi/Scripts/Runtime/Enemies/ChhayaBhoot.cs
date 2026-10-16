// =====================================================================
// ChhayaBhoot.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD: "tez, kamzor, jhund me aate hain" — Rishi ko ghero aur
//       combo/dodge ka abhyas karao. Akela aasaan, paanch saath khatarnak.
// KYA:  EnemyAI ke default (seedha peecha, saamne melee) + apne numbers:
//       tez chaal, kam jaan, chhota windup, GDD hit = 8.
//       Jab attack token na mile to Rishi ke charon taraf ghoomta hai
//       (seedha line me khade na rahein — jhund jaisa dikhe).
// AAGE: EnemyAI base sab chalata hai. Spawner/level M4 me.
// =====================================================================
using UnityEngine;

namespace Rishi.Enemies
{
    public class ChhayaBhoot : EnemyAI
    {
        [SerializeField] float circleSpeed = 2.5f; // token ka intezaar karte waqt ghera

        // Editor me component lagate hi GDD wale numbers (Inspector me badal sakte ho).
        void Reset()
        {
            detectRange = 12f; moveSpeed = 5.5f;   // tez
            attackRange = 1.5f; windup = 0.35f; activeTime = 0.1f; recovery = 0.45f; attackCooldown = 0.9f;
            damage = 8f;                           // GDD: Chhaya-bhoot hit = 8
            hitRadius = 0.8f; hitReach = 0.9f; hurtStun = 0.4f;
        }

        protected override Vector3 ChaseVelocity(Vector3 toTarget, float dist)
        {
            // Paas pahunch gaye par attack nahi kar sakte (token / cooldown) → ghero, ruko mat.
            if (dist <= attackRange + 0.6f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, toTarget / Mathf.Max(dist, 0.001f));
                // Har bhoot apni taraf ghoome (instance id se) — sab ek hi disha me na bhaagein.
                float sign = (GetHashCode() & 1) == 0 ? 1f : -1f;
                return side * (circleSpeed * sign);
            }
            return base.ChaseVelocity(toTarget, dist);
        }
    }
}
