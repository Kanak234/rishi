// =====================================================================
// Hud.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: GDD HUD: health bar (deepak shape), Jyoti count, current shastra
//       icon. Saath me boss ki jaan, tutorial hint, "save ho raha hai" ka
//       chhota deepak, aur kam jaan par kinare laal (contrast ke saath).
// KYA:  IMGUI overlay. Data Rishi ke Health/WeaponManager aur LevelDirector
//       se padhta hai — HUD kisi game logic ko nahi chhedta.
// AAGE: HintZone / LevelDirector ShowHint(). Checkpoint.Message sunta hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;
using Rishi.Core;
using Rishi.World;

namespace Rishi.UI
{
    public class Hud : MonoBehaviour
    {
        [SerializeField] Health playerHealth;
        [SerializeField] WeaponManager weapons;

        static string s_hint;
        static float s_hintUntil;
        static float s_jyotiPulse;
        float _shownHp = 1f, _shownBoss = 1f;

        public static void ShowHint(string text, float seconds)
        {
            s_hint = text;
            s_hintUntil = Time.unscaledTime + seconds;
        }

        public static void PulseJyoti() => s_jyotiPulse = Time.unscaledTime;

        static string s_b1, s_b2, s_b3;
        static float s_bannerAt = -100f;
        /// <summary>Mission shuru: "Level 2 · Rajrappa / Mission 3: … / lakshya".</summary>
        public static void ShowBanner(string level, string mission, string goal)
        {
            s_b1 = level; s_b2 = mission; s_b3 = goal;
            s_bannerAt = Time.unscaledTime;
        }

        void OnEnable() => Checkpoint.Message += OnCheckpoint;
        void OnDisable() => Checkpoint.Message -= OnCheckpoint;
        void OnCheckpoint(string msg) => ShowHint(msg, 4f);

        void Awake()
        {
            if (playerHealth == null) { var p = GameObject.FindWithTag("Player"); if (p != null) { playerHealth = p.GetComponent<Health>(); weapons = p.GetComponent<WeaponManager>(); } }
        }

        void OnGUI()
        {
            if (StoryCutscene.IsPlaying || playerHealth == null) return;
            var dir = LevelDirector.Instance;
            if (dir != null && dir.Victory) return; // jeet ke baad saaf screen — ending kahani
            float s = RishiStyle.S;

            // ---- Kam jaan: kinare laal ----
            float hp01 = playerHealth.Current / playerHealth.Max;
            _shownHp = Mathf.MoveTowards(_shownHp, hp01, Time.unscaledDeltaTime * 0.8f); // peeche ki safed patti dheere ghatti hai
            if (hp01 < 0.3f && !playerHealth.IsDead)
            {
                float pulse = 0.25f + 0.15f * Mathf.Sin(Time.unscaledTime * 6f);
                float e = 60 * s;
                Color c = new Color(0.6f, 0.05f, 0.02f, pulse);
                RishiStyle.Rect(new Rect(0, 0, Screen.width, e), c); RishiStyle.Rect(new Rect(0, Screen.height - e, Screen.width, e), c);
                RishiStyle.Rect(new Rect(0, 0, e, Screen.height), c); RishiStyle.Rect(new Rect(Screen.width - e, 0, e, Screen.height), c);
            }

            // ---- Health: deepak + lau wali patti ----
            Rect box = new Rect(28 * s, 26 * s, 560 * s, 120 * s);
            RishiStyle.Rect(box, new Color(0, 0, 0, 0.45f));
            float flameScale = 0.6f + 0.4f * hp01;
            RishiStyle.Flame(new Rect(box.x + 16 * s, box.y + 12 * s + (1f - flameScale) * 50 * s, 44 * s, 66 * s * flameScale), Color.white);
            RishiStyle.Rect(new Rect(box.x + 10 * s, box.y + 74 * s, 56 * s, 14 * s), RishiStyle.Kaala); // diya
            RishiStyle.Rect(new Rect(box.x + 14 * s, box.y + 74 * s, 48 * s, 6 * s), RishiStyle.Geru);
            Rect bar = new Rect(box.x + 84 * s, box.y + 22 * s, 450 * s, 30 * s);
            RishiStyle.Rect(bar, RishiStyle.Kaala);
            RishiStyle.Rect(new Rect(bar.x + 3 * s, bar.y + 3 * s, (bar.width - 6 * s) * _shownHp, bar.height - 6 * s), RishiStyle.Chuna);
            RishiStyle.Rect(new Rect(bar.x + 3 * s, bar.y + 3 * s, (bar.width - 6 * s) * hp01, bar.height - 6 * s), Color.Lerp(new Color(0.85f, 0.2f, 0.05f), RishiStyle.Sona, hp01));
            GUI.Label(new Rect(bar.x, bar.y + 36 * s, 200 * s, 34 * s), $"{playerHealth.Current:0} / {playerHealth.Max:0}", RishiStyle.Small);

            // ---- Jyoti ----
            int jyoti = dir != null ? dir.Jyoti : 0;
            float jp = Mathf.Clamp01(1f - (Time.unscaledTime - s_jyotiPulse) / 0.5f);
            Rect jr = new Rect(bar.x + 230 * s, bar.y + 36 * s, 220 * s, 34 * s);
            RishiStyle.Flame(new Rect(jr.x, jr.y - jp * 6 * s, 22 * s * (1f + jp * 0.4f), 32 * s * (1f + jp * 0.4f)), Color.white);
            var small = RishiStyle.Small;
            Color old = small.normal.textColor;
            small.normal.textColor = Color.Lerp(RishiStyle.Chuna, RishiStyle.Sona, jp + 0.3f);
            GUI.Label(new Rect(jr.x + 32 * s, jr.y, jr.width, jr.height), $"Jyoti  {jyoti}", small);
            small.normal.textColor = old;

            // ---- Shastra ----
            if (weapons != null)
            {
                Rect wr = new Rect(Screen.width - 360 * s, 26 * s, 332 * s, 92 * s);
                RishiStyle.Rect(wr, new Color(0, 0, 0, 0.45f));
                RishiStyle.Border(new Rect(wr.x, wr.yMax - 16 * s, wr.width, 16 * s), 0.8f);
                string lv = weapons.CurrentLevel > 0 ? $"  Lv {weapons.CurrentLevel}" : "";
                GUI.Label(new Rect(wr.x + 20 * s, wr.y + 6 * s, wr.width, 44 * s), $"<b>{weapons.Current}</b>{lv}", RishiStyle.Small);
                string other = weapons.IsUnlocked(WeaponId.Dhanush) ? "Q / E = badlo" : "";
                GUI.Label(new Rect(wr.x + 20 * s, wr.y + 40 * s, wr.width, 34 * s), other, RishiStyle.Small);
            }

            // ---- Ladai ka haal ----
            var zone = EncounterZone.Active;
            if (zone != null && !zone.IsBoss)
            {
                string w = zone.WaveCount > 1 ? $"  (lehar {zone.WaveIndex + 1}/{zone.WaveCount})" : "";
                RishiStyle.Shadowed(new Rect(0, 30 * s, Screen.width, 40 * s), $"{zone.Title} — asur baaki: {zone.Remaining}{w}", RishiStyle.Center);
            }

            // ---- Boss bar ----
            if (dir != null && dir.BossActive && dir.Boss != null)
            {
                var bh = dir.Boss.GetComponent<Health>();
                float b01 = bh.Current / bh.Max;
                _shownBoss = Mathf.MoveTowards(_shownBoss, b01, Time.unscaledDeltaTime * 0.5f);
                Rect bb = new Rect(Screen.width / 2f - 600 * s, Screen.height - 120 * s, 1200 * s, 30 * s);
                RishiStyle.Shadowed(new Rect(bb.x, bb.y - 46 * s, bb.width, 40 * s), $"{dir.Boss.DisplayName.ToUpper()} — {dir.Boss.Subtitle}   ·   charan {dir.Boss.Phase}/3", RishiStyle.Center);
                RishiStyle.Rect(bb, RishiStyle.Kaala);
                RishiStyle.Rect(new Rect(bb.x + 3 * s, bb.y + 3 * s, (bb.width - 6 * s) * _shownBoss, bb.height - 6 * s), RishiStyle.Chuna);
                RishiStyle.Rect(new Rect(bb.x + 3 * s, bb.y + 3 * s, (bb.width - 6 * s) * b01, bb.height - 6 * s), RishiStyle.Tamas * 1.6f);
                // phase ke nishaan 66% / 30%
                RishiStyle.Rect(new Rect(bb.x + bb.width * 0.66f, bb.y - 4 * s, 3 * s, bb.height + 8 * s), RishiStyle.Sona);
                RishiStyle.Rect(new Rect(bb.x + bb.width * 0.30f, bb.y - 4 * s, 3 * s, bb.height + 8 * s), RishiStyle.Sona);
                if (dir.Boss.InOpening)
                    RishiStyle.Shadowed(new Rect(bb.x, bb.yMax + 6 * s, bb.width, 40 * s), "<color=#FFC040>KHULA MAUKA — bhaari vaar (RMB / Y)!</color>", RishiStyle.Center);
            }

            // ---- Maha-deep lakshya ----
            var gate = ShrineGate.Active;
            if (gate != null && gate.Total > 0 && (gate.transform.position - playerHealth.transform.position).sqrMagnitude < 45f * 45f)
                RishiStyle.Shadowed(new Rect(0, 72 * s, Screen.width, 40 * s), $"{gate.Label} jale: {gate.Lit}/{gate.Total}", RishiStyle.Center);
            var near = ObjectiveShrine.Near;
            if (near != null && !near.IsLit)
            {
                Rect pr = new Rect(Screen.width / 2f - 200 * s, Screen.height * 0.6f, 400 * s, 22 * s);
                RishiStyle.Rect(pr, RishiStyle.Kaala);
                RishiStyle.Rect(new Rect(pr.x + 3 * s, pr.y + 3 * s, (pr.width - 6 * s) * near.Progress01, pr.height - 6 * s), RishiStyle.Sona);
                RishiStyle.Shadowed(new Rect(pr.x - 200 * s, pr.y - 44 * s, pr.width + 400 * s, 40 * s), "Deep jal raha hai… ruko (F / RB = jaldi)", RishiStyle.Center);
            }

            // ---- Mission banner ----
            float bt = Time.unscaledTime - s_bannerAt;
            if (bt < 5f && s_b2 != null)
            {
                float ba = Mathf.Clamp01(bt / 0.6f) * Mathf.Clamp01((5f - bt) / 0.8f);
                Rect br = new Rect(0, Screen.height * 0.12f, Screen.width, 230 * s); // Rishi (screen ke beech) ko na dhake
                RishiStyle.Rect(br, new Color(0, 0, 0, 0.55f * ba));
                RishiStyle.Border(new Rect(0, br.y, Screen.width, 22 * s), ba);
                RishiStyle.Border(new Rect(0, br.yMax - 22 * s, Screen.width, 22 * s), ba);
                Color o1 = RishiStyle.Center.normal.textColor, o2 = RishiStyle.Heading.normal.textColor;
                RishiStyle.Center.normal.textColor = new Color(o1.r, o1.g, o1.b, ba);
                RishiStyle.Heading.normal.textColor = new Color(o2.r, o2.g, o2.b, ba);
                GUI.Label(new Rect(0, br.y + 32 * s, Screen.width, 40 * s), s_b1, RishiStyle.Center);
                GUI.Label(new Rect(0, br.y + 76 * s, Screen.width, 70 * s), s_b2, RishiStyle.Heading);
                GUI.Label(new Rect(0, br.y + 150 * s, Screen.width, 40 * s), s_b3, RishiStyle.Center);
                RishiStyle.Center.normal.textColor = o1; RishiStyle.Heading.normal.textColor = o2;
            }

            // ---- Hint ----
            if (!string.IsNullOrEmpty(s_hint) && Time.unscaledTime < s_hintUntil)
            {
                float a = Mathf.Clamp01((s_hintUntil - Time.unscaledTime) / 0.5f);
                Rect hr = new Rect(Screen.width / 2f - 640 * s, Screen.height * 0.70f, 1280 * s, 120 * s);
                RishiStyle.Rect(hr, new Color(0, 0, 0, 0.55f * a));
                var st = RishiStyle.Center;
                Color oc = st.normal.textColor;
                st.normal.textColor = new Color(oc.r, oc.g, oc.b, a);
                GUI.Label(hr, s_hint, st);
                st.normal.textColor = oc;
            }

            // ---- Save ho raha hai: kone me deepak ----
            float since = Time.unscaledTime - SaveSystem.LastSaveTime;
            if (since < 2f)
                RishiStyle.Flame(new Rect(Screen.width - 60 * s, Screen.height - 80 * s, 30 * s, 46 * s), new Color(1, 1, 1, 1f - since / 2f));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { s_hint = null; s_hintUntil = 0f; s_b2 = null; s_bannerAt = -100f; }
    }
}
