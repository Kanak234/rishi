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
        [SerializeField] float distance = 14f;                 // kitni door
        [SerializeField] float heightOffset = 1.2f;            // Rishi ke pairon ki jagah seene ko dekhe
        [SerializeField] float smoothTime = 0.15f;

        Vector3 _vel; // SmoothDamp ka internal state

        public Transform Target { get => target; set => target = value; }

        void LateUpdate()
        {
            // LateUpdate: player ke move hone ke BAAD camera chale, warna jitter aata hai.
            if (target == null) return;

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * heightOffset;
            Vector3 desired = focus - rot * Vector3.forward * distance;

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);
            transform.rotation = rot;
        }

        // Teleport/respawn par camera ko turant le aao (warna lamba smooth slide dikhega).
        public void SnapToTarget()
        {
            if (target == null) return;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(
                target.position + Vector3.up * heightOffset - rot * Vector3.forward * distance, rot);
            _vel = Vector3.zero;
        }
    }
}
