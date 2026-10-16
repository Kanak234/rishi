// =====================================================================
// PashanRakshas.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD: "patthar ka bhari asur, dheere par zor se maarta hai" aur
//       "Heavy ... Pashan-rakshas ka guard todta hai". Player ko sikhata hai:
//       saamne se light combo bekaar — heavy maaro ya peeche jao.
// KYA:  IHitFilter = guard: saamne se aaye non-heavy vaar (light, teer) ruk
//       jaate hain ("clang"). Heavy (GuardBreak) guard todta hai → lamba stun
//       (khula mauka). Super armor: normal hit se attack nahi rukta.
//       Lamba windup (0.8 s) = dodge ka saaf ishara; GDD hit = 20.
// AAGE: Hurtbox.ReceiveHit() isko FilterHit() se poochhta hai. EnemyAI base baaki.
// =====================================================================
using UnityEngine;
using Rishi.Combat;

namespace Rishi.Enemies
{
    public class PashanRakshas : EnemyAI, IHitFilter
    {
        [SerializeField] float guardAngle = 70f;        // saamne ke ±itne degree = guard
        [SerializeField] float guardBreakStun = 1.6f;   // heavy ke baad itni der khula (GDD: khula mauka)

        public event System.Action GuardBlocked;        // SFX "clang" (M5)

        void Reset()
        {
            detectRange = 10f; moveSpeed = 2.2f; turnSpeed = 200f;  // dheema, bhaari
            attackRange = 2.0f; windup = 0.8f; activeTime = 0.15f; recovery = 0.9f; attackCooldown = 1.6f;
            damage = 20f;                                            // GDD: Pashan-rakshas hit = 20
            hitRadius = 1.3f; hitReach = 1.4f;
            hurtStun = 0.3f; superArmor = true; knockbackDecay = 14f; // patthar — kam khisakta hai
            jyotiReward = 3;
        }

        public bool FilterHit(ref HitInfo info)
        {
            if (info.GuardBreak) return true;                // heavy hamesha lagta hai
            if (State == EnemyState.Hurt || State == EnemyState.Dead) return true; // guard toota hua hai

            // Vaar kis taraf se aaya: info.Direction = attacker → mujh. Ulta = mujhse attacker ki taraf.
            Vector3 fromAttacker = -info.Direction; fromAttacker.y = 0f;
            bool front = fromAttacker.sqrMagnitude > 0.001f &&
                         Vector3.Angle(transform.forward, fromAttacker) <= guardAngle;
            if (!front) return true;                         // peeche/side se — lagta hai

            Flash(0.08f);
            GuardBlocked?.Invoke();
            Rishi.Core.GameAudio.Play(Rishi.Core.Sfx.Clang, transform.position);
            Debug.Log($"[ASUR] {name} ne guard se roka — heavy (RMB) maaro ya peeche jao.");
            return false;
        }

        protected override float StunDuration(in HitInfo info) => info.GuardBreak ? guardBreakStun : hurtStun;
    }
}
