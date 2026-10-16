// =====================================================================
// KillZone.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: GDD edge case — "arena / level se bahar girne par kinare wapas
//       spawn, health -10". Bina iske Rishi khaai me hamesha girta rahega.
// KYA:  Ek invisible trigger box (khaai ke neeche). Rishi andar aaye to
//       respawn point par bhejo aur 10 damage do.
// AAGE: PlayerController.Teleport() + Health.TakeDamage() bulata hai;
//       camera ko IsoCameraFollow.SnapToTarget() se turant laata hai.
//       M4 me har level ke checkpoint isi pattern ko use karenge.
// =====================================================================
using UnityEngine;
using Rishi.Combat;
using Rishi.Player;
using Rishi.CameraSystem;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        [SerializeField] Transform respawnPoint;
        [SerializeField] float fallDamage = 10f; // GDD
        [Tooltip("true = aakhri jalaya deepak (checkpoint) par wapas. Boss arena me false — GDD: arena ke kinare par.")]
        [SerializeField] bool useCheckpoint = true;

        public Transform RespawnPoint { get => respawnPoint; set => respawnPoint = value; }

        void Reset()
        {
            // Component lagate hi collider ko trigger bana do (bhoolne wala bug).
            GetComponent<Collider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null) return; // sirf Rishi ke liye
            Transform point = useCheckpoint && Checkpoint.Latest != null ? Checkpoint.Latest.transform : respawnPoint;
            if (point == null) return;

            player.Teleport(point.position, point.rotation);

            // Pehle teleport, phir damage: agar damage se mar gaye to bhi body
            // khaai me nahi, respawn par hogi (M4 ka game-over screen wahi se chalega).
            var hp = player.GetComponent<Health>();
            if (hp != null)
            {
                hp.IsInvincible = false;
                hp.TakeDamage(fallDamage);
            }

            var cam = Camera.main ? Camera.main.GetComponent<IsoCameraFollow>() : null;
            if (cam != null) cam.SnapToTarget();
        }
    }
}
