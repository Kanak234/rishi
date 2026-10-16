// =====================================================================
// HitInfo.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: Ek vaar ki saari jaankari (kitna damage, kis taraf dhakka, heavy
//       tha ya nahi) ek packet me jaaye — taaki hit lene wala (asur, boss,
//       dummy) khud tay kare ki kaise react kare.
// KYA:  Team enum (apne hi saathi ko na maaro) + HitInfo struct.
//       Struct hai, class nahi — har hit par heap allocation nahi.
// AAGE: CombatSystem banata hai → Hurtbox.ReceiveHit() → Health.TakeDamage()
//       aur Hurtbox.Hit event (EnemyAI knockback / guard break sunega).
// =====================================================================
using UnityEngine;

namespace Rishi.Combat
{
    /// <summary>Kaun kis paksh me — same team ke hurtbox par hit nahi lagta.</summary>
    public enum Team { Player, Asur, Neutral }

    public struct HitInfo
    {
        public float Damage;
        public Vector3 Direction;   // hit ki disha (attacker se target ki taraf, flat)
        public float Knockback;     // dhakke ki speed (m/s) — target khud lagaye
        public bool GuardBreak;     // GDD: heavy vaar Pashan-rakshas ka guard todta hai
        public bool IsFinisher;     // combo ka teesra hit — GDD: dushman ko peeche dhakelta hai
        public Team AttackerTeam;
        public Transform Attacker;
    }
}
