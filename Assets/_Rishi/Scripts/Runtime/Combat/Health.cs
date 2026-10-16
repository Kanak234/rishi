// =====================================================================
// Health.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: Rishi, har asur aur boss — sabki jaan (HP) ek hi tarah chalni
//       chahiye. Ek common component = ek jagah bug fix.
// KYA:  HP rakhta hai, damage leta hai, invincibility (dodge ke i-frames)
//       samajhta hai, aur events bhejta hai: Damaged, Died.
// AAGE: PlayerController "Damaged" sunta hai (ledge par hit lage to gir jao).
//       KillZone / DamageZone isko TakeDamage() bulate hain.
//       M3 me CombatSystem aur EnemyAI bhi isi ko use karenge; M6 me HUD
//       ka deepak-shaped health bar "Current / Max" padhega.
// =====================================================================
using System;
using UnityEngine;

namespace Rishi.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField, Tooltip("GDD: Rishi = 100")] float maxHealth = 100f;

        public float Max => maxHealth;
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;

        // Dodge ke 0.4 s me true hota hai — tab koi damage nahi lagta.
        public bool IsInvincible { get; set; }

        // Events: doosri scripts subscribe karti hain, Health ko unka pata nahi hona chahiye.
        public event Action<float> Damaged; // kitna damage laga
        public event Action Died;

        void Awake() => Current = maxHealth;

        /// <summary>Damage do. true = damage laga, false = ignore hua (dead / i-frames / galat value).</summary>
        public bool TakeDamage(float amount)
        {
            // Edge cases: mara hua dobara nahi marega, dodge me hit nahi, negative damage nahi.
            if (IsDead || IsInvincible || amount <= 0f) return false;

            Current = Mathf.Max(0f, Current - amount);
            Damaged?.Invoke(amount);
            if (IsDead) Died?.Invoke();
            return true;
        }

        /// <summary>Jaan wapas (checkpoint / pickup). Max se upar nahi jayegi.</summary>
        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
        }

        /// <summary>Checkpoint se respawn par poori jaan.</summary>
        public void ResetFull() => Current = maxHealth;
    }
}
