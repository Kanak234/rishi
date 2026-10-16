// =====================================================================
// Hurtbox.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: "Hitbox" = jahan vaar lagta hai (CombatSystem ka sphere);
//       "Hurtbox" = jahan chot lag sakti hai. Dono alag rakhne se asur ka
//       sharir, boss ki gada, ya dummy — sab ek hi tarike se hit lete hain.
// KYA:  Collider wale object par lagta hai, apni Team aur Health jaanta hai.
//       ReceiveHit(): team check → Health.TakeDamage() → Hit event.
// AAGE: CombatSystem isko OverlapSphere se dhoondhta hai. EnemyAI (M3) "Hit"
//       event sun ke knockback / stagger / guard break karega.
// =====================================================================
using System;
using UnityEngine;

namespace Rishi.Combat
{
    [RequireComponent(typeof(Collider))]
    public class Hurtbox : MonoBehaviour
    {
        [SerializeField] Team team = Team.Asur;
        [Tooltip("Khaali ho to parent me dhoondhega (asur ka collider child par ho sakta hai).")]
        [SerializeField] Health health;

        public Team Team => team;
        public Health Health => health;

        /// <summary>Hit laga aur damage hua (i-frames / dead me nahi aata).</summary>
        public event Action<HitInfo> Hit;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
        }

        /// <summary>true = hit laga. false = apni team / mara hua / i-frames.</summary>
        public bool ReceiveHit(in HitInfo info)
        {
            // Edge case: apni team ka vaar ignore (Rishi ka combo uske apne collider ko na lage).
            if (info.AttackerTeam == team) return false;
            if (health == null || !health.TakeDamage(info.Damage)) return false;
            Hit?.Invoke(info);
            return true;
        }
    }
}
