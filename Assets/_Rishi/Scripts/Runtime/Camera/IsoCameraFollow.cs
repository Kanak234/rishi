// =====================================================================
// IsoCameraFollow.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: Raji jaisa "upar se tirchha" fixed camera chahiye jo Rishi ke
//       peeche smooth tarike se chale. Player camera ghumata nahi, taaki
//       combat aur platforming saaf dikhe.
// KYA:  Ek fixed angle (pitch, yaw) aur doori se target ko follow karta hai,
//       SmoothDamp se jhatka nahi lagta.
// FAISLA: GDD me Cinemachine likha tha; M2 ke liye apna chhota script
//       rakha kyunki ye kisi package version par depend nahi karta aur
//       samajhna aasaan hai. Level me camera zones chahiye honge to M4 me
//       Cinemachine par shift kar sakte hain.
// AAGE: PlayerController isi camera ki direction se WASD ko "screen ke
//       hisaab se" movement me badalta hai.
// =====================================================================
using UnityEngine;

namespace Rishi.CameraSystem
{
    [RequireComponent(typeof(Camera))]
    public class IsoCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField, Range(20f, 80f)] float pitch = 45f;   // kitna upar se dekhe
        [SerializeField] float yaw = 45f;                      // kis kone se dekhe
        [SerializeField] float distance = 12f;                 // kitni door (14 par Rishi bahut chhota dikhta tha)
        [SerializeField] float heightOffset = 1.2f;            // Rishi ke pairon ki jagah seene ko dekhe
        [SerializeField] float smoothTime = 0.15f;

        Vector3 _vel; // SmoothDamp ka internal state
        Vector3 _base; // shake se pehle ki jagah — shake smoothing me na ghule
        static float s_shakeAmount, s_shakeUntil;

        public Transform Target { get => target; set => target = value; }
        /// <summary>Boss arena me camera thoda door (poora akhada dikhe). LevelDirector set karta hai.</summary>
        public float Distance { get => distance; set => distance = value; }

        /// <summary>Bhaari vaar / boss slam par jhatka. GDD accessibility: settings me band ho sakta hai.</summary>
        public static void Shake(float amount, float seconds)
        {
            if (!Rishi.Core.GameSettings.CameraShake) return;
            s_shakeAmount = Mathf.Max(amount, Time.time < s_shakeUntil ? s_shakeAmount : 0f);
            s_shakeUntil = Time.time + seconds;
        }

        void LateUpdate()
        {
            // LateUpdate: player ke move hone ke BAAD camera chale, warna jitter aata hai.
            if (target == null) return;

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * heightOffset;
            Vector3 desired = focus - rot * Vector3.forward * distance;

            if (_base == Vector3.zero) _base = transform.position;
            _base = Vector3.SmoothDamp(_base, desired, ref _vel, smoothTime);
            Vector3 shake = Time.time < s_shakeUntil ? Random.insideUnitSphere * s_shakeAmount * ((s_shakeUntil - Time.time) * 4f) : Vector3.zero;
            transform.position = _base + shake;
            transform.rotation = rot;
        }

        // Teleport/respawn par camera ko turant le aao (warna lamba smooth slide dikhega).
        public void SnapToTarget()
        {
            if (target == null) return;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            _base = target.position + Vector3.up * heightOffset - rot * Vector3.forward * distance;
            transform.SetPositionAndRotation(_base, rot);
            _vel = Vector3.zero;
        }
    }
}
