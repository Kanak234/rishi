// =====================================================================
// EncounterZone.cs  —  RISHI: The Tamas War   (Milestone M4)
// ---------------------------------------------------------------------
// KYUN: GDD core loop: "explore → asur jhund se lado → …". Asur pehle se
//       chalte rahein to CPU bhi khaate hain aur player ko achanak gher lete
//       hain. Isliye har ladai ek "zone" hai: andar aao → Tamas ki deewar
//       uthti hai → sab asur haaro → deewar hat jaati hai.
// KYA:  Trigger. enemies[] shuru me band (inactive); Rishi aaye to on.
//       Waves: pehli wave khatam to agli (jhund ek saath 12 se zyada na ho).
//       gates[] (Tamas dhuan ki deewar) ladai ke dauran on.
//       isBoss = mid cutscene (pehli baar) + boss music + camera door.
// AAGE: LevelDirector se cutscene/music. Hud "Asur baaki: N" dikhata hai.
// =====================================================================
using System;
using UnityEngine;
using Rishi.Combat;
using Rishi.Core;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class EncounterZone : MonoBehaviour
    {
        [Serializable]
        public class Wave { public GameObject[] enemies; }

        [SerializeField] string title = "Asur jhund";
        [SerializeField] Wave[] waves;
        [SerializeField] GameObject[] gates;
        [SerializeField] bool isBoss;

        public static EncounterZone Active { get; private set; }
        public static event Action<EncounterZone> Started, Cleared;

        public string Title => title;
        public bool IsBoss => isBoss;
        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => waves != null ? waves.Length : 0;

        bool _started, _done;
        Health[] _current = Array.Empty<Health>();

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void Awake()
        {
            // Shuru me sab band — scene me dikhein nahi, chalein nahi.
            if (waves != null) foreach (var w in waves) foreach (var e in w.enemies) if (e != null) e.SetActive(false);
            SetGates(false);
        }

        void OnDestroy() { if (Active == this) Active = null; }

        void OnTriggerEnter(Collider other)
        {
            if (_started || other.GetComponentInParent<WeaponManager>() == null) return;
            _started = true;
            if (isBoss) LevelDirector.Instance?.BeginBoss(this, Begin);
            else Begin();
        }

        void Begin()
        {
            Active = this;
            SetGates(true);
            Started?.Invoke(this);
            NextWave();
        }

        void NextWave()
        {
            WaveIndex++;
            if (waves == null || WaveIndex >= waves.Length) { Finish(); return; }
            var list = waves[WaveIndex].enemies;
            _current = new Health[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null) continue;
                list[i].SetActive(true);
                _current[i] = list[i].GetComponent<Health>();
            }
            if (WaveIndex > 0) GameAudio.Play(Sfx.Boom, transform.position, 0.5f);
        }

        void Update()
        {
            if (!_started || _done || Active != this) return;
            for (int i = 0; i < _current.Length; i++)
                if (_current[i] != null && !_current[i].IsDead) return;
            NextWave();
        }

        /// <summary>Is wave me kitne zinda — Hud ke liye.</summary>
        public int Remaining
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _current.Length; i++) if (_current[i] != null && !_current[i].IsDead) n++;
                return n;
            }
        }

        void Finish()
        {
            _done = true;
            SetGates(false);
            if (Active == this) Active = null;
            GameAudio.Play(Sfx.Deepak, transform.position, 0.6f);
            Cleared?.Invoke(this);
        }

        void SetGates(bool on)
        {
            if (gates == null) return;
            foreach (var g in gates) if (g != null) g.SetActive(on);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Active = null; Started = null; Cleared = null; }
    }
}
