// =====================================================================
// ShrineGate.cs  —  RISHI: The Tamas War
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
    public class ShrineGate : MonoBehaviour
    {
        [SerializeField] ObjectiveShrine[] shrines;
        [SerializeField] GameObject[] walls;
        [SerializeField] string label = "Maha-deep";

        public static ShrineGate Active { get; private set; }
        public int Lit { get { int n = 0; foreach (var s in shrines) if (s != null && s.IsLit) n++; return n; } }
        public int Total => shrines != null ? shrines.Length : 0;
        public string Label => label;
        bool _open;

        void Start()
        {
            foreach (var w in walls) if (w != null) w.SetActive(true);
            if (Active == null) Active = this;
        }

        void OnDestroy() { if (Active == this) Active = null; }

        public static void NotifyLit()
        {
            foreach (var g in FindObjectsByType<ShrineGate>(FindObjectsSortMode.None)) g.Check();
        }

        void Check()
        {
            if (_open || Lit < Total) return;
            _open = true;
            foreach (var w in walls) if (w != null) w.SetActive(false);
            GameAudio.Play(Sfx.Boom, transform.position, 0.6f);
            Rishi.UI.Hud.ShowHint($"Saare {label} jal gaye — raasta khul gaya!", 4f);
            if (Active == this) Active = null;
            // Agla gate (agar ho) ab HUD par dikhe
            foreach (var g in FindObjectsByType<ShrineGate>(FindObjectsSortMode.None)) if (!g._open) { Active = g; break; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Active = null;
    }
}
