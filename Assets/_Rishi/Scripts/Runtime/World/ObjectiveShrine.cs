// =====================================================================
// ObjectiveShrine.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: Har mission sirf "aage chalo, lado" na ho. Kashi ki aarti aur
//       Ellora ki gufaon me kaam hai: maha-deep jalao. Aur har mission ka
//       ek saaf ant chahiye jahan se kahani agle mission me jaaye.
// KYA:  ObjectiveShrine — bujha maha-deep; Rishi paas aakar F / RB dabaye
//       (ya 1.2 s khada rahe) to jalta hai. ShrineGate — iske saare deep
//       jal jaayein to Tamas ki deewar hat jaati hai.
//       MissionExit — mission ka ant (trigger) → LevelDirector.CompleteMission().
// AAGE: LevelBuilder in teeno ko mission ke hisaab se lagata hai. Hud
//       "Maha-deep: 2/3" ShrineGate.Active se padhta hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;
using Rishi.Core;

namespace Rishi.World
{

    [RequireComponent(typeof(Collider))]
    public class ObjectiveShrine : MonoBehaviour
    {
        [SerializeField] GameObject litVisual;
        [SerializeField] float holdTime = 1.2f;

        public bool IsLit { get; private set; }
        public float Progress01 => Mathf.Clamp01(_inside / holdTime);
        public static ObjectiveShrine Near { get; private set; } // Hud "jalane ke liye ruko" dikhata hai

        float _inside;
        bool _playerIn;

        void Reset() => GetComponent<Collider>().isTrigger = true;
        void Awake() { if (litVisual != null) litVisual.SetActive(false); }

        void OnTriggerEnter(Collider other) { if (other.GetComponentInParent<WeaponManager>() != null) { _playerIn = true; Near = this; } }
        void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<WeaponManager>() == null) return;
            _playerIn = false; _inside = 0f;
            if (Near == this) Near = null;
        }

        void Update()
        {
            if (IsLit || !_playerIn || Time.timeScale <= 0f) return;
            // Khade raho (ya F dabao) — ladai ke beech jaldi jalana ek risk/reward faisla hai.
            _inside += Time.deltaTime;
            var kb = UnityEngine.InputSystem.Keyboard.current; var pad = UnityEngine.InputSystem.Gamepad.current;
            if ((kb != null && kb.fKey.isPressed) || (pad != null && pad.rightShoulder.isPressed)) _inside += Time.deltaTime * 1.5f;
            if (_inside >= holdTime) Light();
        }

        void Light()
        {
            IsLit = true;
            if (Near == this) Near = null;
            if (litVisual != null) litVisual.SetActive(true);
            GameAudio.Play(Sfx.Deepak, transform.position);
            ShrineGate.NotifyLit();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Near = null;
    }
}
