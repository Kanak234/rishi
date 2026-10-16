// =====================================================================
// WeaponPickup.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD: Dhanush level ke beech milta hai. Rishi us jagah pahunche to
//       shastra unlock ho — ek trigger se.
// KYA:  Trigger collider. Player (WeaponManager wala) ghuse to Unlock(),
//       pickup gayab. Dheere ghoomta aur upar-neeche tairta hai taaki dikhe.
// AAGE: WeaponManager.Unlock() → WeaponUnlocked event (M6 HUD "naya shastra"
//       message). M4 me Rajrappa level me asli jagah par rakha jayega.
// =====================================================================
using UnityEngine;
using Rishi.Combat;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour
    {
        [SerializeField] WeaponId weapon = WeaponId.Dhanush;
        [SerializeField] float spinSpeed = 90f;
        [SerializeField] float bobHeight = 0.15f;

        Vector3 _start;

        void Reset() => GetComponent<Collider>().isTrigger = true;
        void Awake() => _start = transform.position;

        void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            transform.position = _start + Vector3.up * (Mathf.Sin(Time.time * 2f) * bobHeight);
        }

        void OnTriggerEnter(Collider other)
        {
            var wm = other.GetComponentInParent<WeaponManager>();
            if (wm == null) return; // sirf Rishi utha sakta hai
            wm.Unlock(weapon);
            Debug.Log($"[RISHI] {weapon} mila! Q/E se badlo.");
            gameObject.SetActive(false);
        }
    }
}
