// =====================================================================
// DamageZone.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: M2 testing ke liye — asur abhi (M3) bane nahi hain, lekin hume
//       check karna hai ki (1) dodge ke i-frames kaam karte hain, aur
//       (2) ledge par latakte waqt hit lagne se Rishi girta hai.
// KYA:  Ek trigger area (jaise Tamas ka kaala dhuan). Andar raho to har
//       "interval" second par "damage" lagta hai. Baad me level me asli
//       hazard (aag, kaante) ke liye bhi kaam aayega.
// AAGE: Health.TakeDamage() → Health.Damaged event → PlayerController
//       ledge se gira deta hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class DamageZone : MonoBehaviour
    {
        [SerializeField] float damage = 8f;     // GDD: Chhaya-bhoot ka hit = 8
        [SerializeField] float interval = 1f;

        float _nextTick;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerStay(Collider other)
        {
            if (Time.time < _nextTick) return;
            var hp = other.GetComponent<Health>();
            if (hp == null) return;
            hp.TakeDamage(damage);       // dodge ke time ye false lautata hai — yahi test hai
            _nextTick = Time.time + interval;
        }
    }
}
