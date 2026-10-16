// =====================================================================
// Arrow.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: Dhanush ka teer. Tez chalta hai (30 m/s), isliye Rigidbody/collision
//       par bharosa nahi — ek frame me 0.5 m laangh kar patli deewar ke paar
//       nikal jaata (tunneling). Har frame "pichhli jagah → nayi jagah" ray
//       maar ke dekhte hain kya beech me aaya.
// KYA:  Launch() se udta hai, Hurtbox lage to ReceiveHit(), deewar lage to
//       wahin atak jaata hai (thodi der dikhta hai), phir wapas pool me.
// AAGE: BowCombat ka pool isko banata aur dobara use karta hai — har shot
//       par Instantiate nahi (allocation rule). M4 me switch/target bhi isi
//       Hurtbox raste se hit honge.
// =====================================================================
using UnityEngine;

namespace Rishi.Combat
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] float stuckTime = 1.5f;   // deewar me atka teer kitni der dikhe
        [SerializeField] float gravity = -4f;       // halka neeche jhukna — asli gravity se kam, aim aasaan rahe

        Vector3 _velocity;
        HitInfo _hit;
        float _dieAt;
        bool _flying;
        LayerMask _mask;

        public bool InUse => gameObject.activeSelf;

        public void Launch(Vector3 pos, Vector3 dir, float speed, float lifetime, in HitInfo hit, LayerMask mask)
        {
            transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir));
            _velocity = dir * speed;
            _hit = hit;
            _mask = mask;
            _dieAt = Time.time + lifetime;   // range ki hadd — hawa me hamesha na ude
            _flying = true;
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (Time.time >= _dieAt) { gameObject.SetActive(false); return; } // wapas pool me
            if (!_flying) return; // deewar me atka hai

            float dt = Time.deltaTime;
            _velocity.y += gravity * dt;
            Vector3 from = transform.position;
            Vector3 step = _velocity * dt;

            // Triggers (KillZone, Tamas dhuan) ignore — teer unke paar jaata hai.
            if (Physics.Raycast(from, step.normalized, out RaycastHit h, step.magnitude, _mask, QueryTriggerInteraction.Ignore))
            {
                Hurtbox hb = h.collider.GetComponentInParent<Hurtbox>();
                if (hb != null)
                {
                    // Edge case: apni team (Rishi) ka collider — ReceiveHit false deta hai, teer phir bhi ruk jaaye
                    // taaki player ke andar se nikalte waqt ajeeb na lage. Muzzle Rishi ke collider se bahar hai.
                    _hit.Direction = new Vector3(_velocity.x, 0f, _velocity.z).normalized;
                    hb.ReceiveHit(_hit);
                    gameObject.SetActive(false); // dushman me ghus gaya — gayab (M5 me hit VFX)
                    return;
                }
                // Deewar/zameen: wahin atak jao.
                transform.position = h.point;
                _flying = false;
                _dieAt = Time.time + stuckTime;
                return;
            }

            transform.position = from + step;
            if (_velocity.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(_velocity);
        }
    }
}
