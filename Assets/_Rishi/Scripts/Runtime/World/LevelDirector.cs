// =====================================================================
// LevelDirector.cs  —  RISHI: The Tamas War   (Milestone M4–M6)
// ---------------------------------------------------------------------
// KYUN: Har mission ka "sutradhar": shuru se ant tak ka kram ek jagah —
//       mission ki kahani, checkpoint se wapsi, Jyoti ginti, game over, boss
//       se pehle ki kahani, ant ki kahani jo agle mission se judti hai.
//       Har script ek-doosre ko na dhoondhe, sab isse puchhein.
// KYA:  missionIndex se Missions.All[i] padhta hai. Start: save se Rishi ko
//       aakhri deepak par, Dhanush/upgrade wapas. Intro (pehli baar).
//       Rishi mara → Game over (Retry = checkpoint, Main menu).
//       Boss zone → boss kahani → boss music. Boss haara ya MissionExit →
//       ant ki kahani → agla mission. Aakhri mission ke baad "Samaapt" screen.
// AAGE: Hud yahan se Jyoti / boss padhta hai. Checkpoint WriteWeaponLevels.
//       EncounterZone BeginBoss(). GameFlow scene badalta hai.
// =====================================================================
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.CameraSystem;
using Rishi.Combat;
using Rishi.Core;
using Rishi.Enemies;
using Rishi.Player;
using Rishi.UI;

namespace Rishi.World
{
    public class LevelDirector : MonoBehaviour
    {
        public static LevelDirector Instance { get; private set; }

        [SerializeField] int missionIndex;
        [SerializeField] Transform player;
        [SerializeField] Checkpoint[] checkpoints;   // index ke kram me
        [SerializeField] BossAndhvir boss;
        [SerializeField] float bossCameraDistance = 19f;

        public BossAndhvir Boss => boss;
        public int MissionIndex => missionIndex;
        public MissionDef Mission => Missions.All[Mathf.Clamp(missionIndex, 0, Missions.Count - 1)];
        public bool BossActive { get; private set; }
        public int Jyoti => SaveSystem.Current.jyoti;
        public bool GameOver { get; private set; }
        public bool Victory { get; private set; }
        public float RunTime { get; private set; }

        Health _hp;
        WeaponManager _wm;
        RishiInput _input;
        CombatSystem _melee;
        BowCombat _bow;
        float _deadAt = -1f;
        readonly MenuList _overMenu = new MenuList();
        static readonly string[] OverItems = { "Deepak se dobara", "Main menu" };
        bool _endScreen;

        // =================================================================
        void Awake()
        {
            Instance = this;
            if (player == null) { var p = GameObject.FindWithTag("Player"); if (p != null) player = p.transform; }
            _hp = player.GetComponent<Health>();
            _wm = player.GetComponent<WeaponManager>();
            _input = player.GetComponent<RishiInput>();
            _melee = player.GetComponent<CombatSystem>();
            _bow = player.GetComponent<BowCombat>();
        }

        void OnEnable()
        {
            EnemyAI.AnyDied += OnEnemyDied;
            if (_hp != null) { _hp.Died += OnPlayerDied; _hp.Damaged += OnPlayerDamaged; }
            if (boss != null) boss.Died += OnBossDied;
            if (_melee != null) { _melee.AttackStarted += OnAttack; _melee.HitLanded += OnHitLanded; }
            if (_bow != null) _bow.Fired += OnBowFired;
        }

        void OnDisable()
        {
            EnemyAI.AnyDied -= OnEnemyDied;
            if (_hp != null) { _hp.Died -= OnPlayerDied; _hp.Damaged -= OnPlayerDamaged; }
            if (boss != null) boss.Died -= OnBossDied;
            if (_melee != null) { _melee.AttackStarted -= OnAttack; _melee.HitLanded -= OnHitLanded; }
            if (_bow != null) _bow.Fired -= OnBowFired;
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            Cursor.visible = true;
            SaveData d = SaveSystem.Current;

            // --- Continue: aakhri deepak par wapas ---
            if (d.checkpoint >= 0 && checkpoints != null)
            {
                Checkpoint cp = null;
                foreach (var c in checkpoints) if (c != null && c.Index <= d.checkpoint) { c.LightSilently(); if (cp == null || c.Index > cp.Index) cp = c; }
                if (cp != null)
                {
                    player.GetComponent<PlayerController>().Teleport(cp.transform.position, cp.transform.rotation);
                    var cam = Camera.main != null ? Camera.main.GetComponent<IsoCameraFollow>() : null;
                    if (cam != null) cam.SnapToTarget();
                    cp.LightSilently(); // Latest = sabse aage wala
                }
            }
            // Mission 3 ke baad Dhanush hamesha (menu se seedha aage ka mission chuna ho tab bhi)
            if (missionIndex >= 3) d.hasDhanush = true;
            if (_wm != null)
            {
                if (d.hasDhanush) _wm.Unlock(WeaponId.Dhanush);
                _wm.RestoreLevel(WeaponId.Trishul, d.trishulLevel);
                _wm.RestoreLevel(WeaponId.Dhanush, d.dhanushLevel);
                // Dhanush pehle mil chuka to pickup dobara na dikhe
                if (d.hasDhanush) foreach (var p in FindObjectsByType<WeaponPickup>(FindObjectsInactive.Exclude)) p.gameObject.SetActive(false);
            }

            GameAudio.SetMusic(MusicTrack.Explore);
            MissionDef m = Mission;
            void Banner() => Hud.ShowBanner($"Level {m.LevelNumber} · {m.Level}", $"Mission {missionIndex + 1}: {m.Title}", m.Goal);
            int bit = 1 << missionIndex;
            if ((d.introMask & bit) == 0 && d.checkpoint < 0)
            {
                d.introMask |= bit;
                StoryCutscene.Play(m.Intro, Banner);
            }
            else Banner();
        }

        void Update()
        {
            if (!GameOver && !Victory && !StoryCutscene.IsPlaying) RunTime += Time.deltaTime;

            if (GameOver && Time.unscaledTime - _deadAt > 1.5f)
            {
                int i = _overMenu.Navigate(OverItems.Length);
                if (i >= 0) ChooseOver(i);
            }
            if (_endScreen)
            {
                var kb = Keyboard.current; var pad = Gamepad.current;
                if ((kb != null && (kb.enterKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)))
                    GameFlow.CompleteMission(missionIndex); // aakhri: save "finished" karke menu
            }
        }

        void ChooseOver(int i)
        {
            if (i == 0) GameFlow.Retry(); else GameFlow.ToMenu();
        }

        // ---------------- Events ----------------
        void OnEnemyDied(EnemyAI e)
        {
            if (e.JyotiReward <= 0) return;
            SaveSystem.Current.jyoti += e.JyotiReward;
            Hud.PulseJyoti();
        }

        // Rishi ke vaar ki awaaz / jhatka — CombatSystem ko audio ka pata nahi hona chahiye.
        void OnAttack(AttackKind kind, int step)
        {
            if (kind == AttackKind.Heavy && step == 0) return; // charge shuru — abhi swing nahi
            GameAudio.Play(Sfx.Swoosh, player.position, kind == AttackKind.Heavy ? 1f : 0.6f);
        }

        void OnHitLanded(Hurtbox hb, HitInfo info)
        {
            if (info.GuardBreak || info.IsFinisher) IsoCameraFollow.Shake(0.12f, 0.15f);
        }

        void OnBowFired(bool charged) => GameAudio.Play(Sfx.Arrow, player.position, charged ? 1f : 0.7f);

        void OnPlayerDamaged(float amount)
        {
            GameAudio.Play(Sfx.Hurt, player.position);
            IsoCameraFollow.Shake(Mathf.Clamp(amount * 0.012f, 0.05f, 0.3f), 0.25f);
        }

        void OnPlayerDied()
        {
            if (Victory) return;
            GameOver = true;
            _deadAt = Time.unscaledTime;
            GamePause.Locked = true;
            if (_input != null) _input.enabled = false; // mara hua Rishi chale nahi
            GameAudio.Play(Sfx.Death, player.position);
            GameAudio.SetMusic(MusicTrack.Menu);
        }

        /// <summary>EncounterZone (isBoss) bulata hai: pehli baar mid kahani, phir ladai.</summary>
        public void BeginBoss(EncounterZone zone, Action begin)
        {
            void Go()
            {
                BossActive = true;
                GameAudio.SetMusic(MusicTrack.Boss);
                var cam = Camera.main != null ? Camera.main.GetComponent<IsoCameraFollow>() : null;
                if (cam != null) cam.Distance = bossCameraDistance; // poora akhada dikhe
                Hud.ShowHint($"{(boss != null ? boss.DisplayName : "Senapati")} — laal ghere se bacho (dodge / kood).\nBada vaar chookne par wo thakta hai: tab RMB bhaari vaar!", 7f);
                begin();
            }
            int bit = 1 << missionIndex;
            var bi = Mission.BossIntro;
            if ((SaveSystem.Current.bossMask & bit) == 0 && bi != null) { SaveSystem.Current.bossMask |= bit; StoryCutscene.Play(bi, Go); }
            else Go();
        }

        void OnBossDied(EnemyAI _)
        {
            BossActive = false;
            CompleteMission(2.5f); // boss ka girna dekhne do
        }

        /// <summary>Mission poora (boss haara ya MissionExit). Ant ki kahani → agla mission.</summary>
        public void CompleteMission(float delay = 0.3f)
        {
            if (Victory || GameOver) return;
            Victory = true;
            GamePause.Locked = true;
            if (_input != null) _input.enabled = false;
            GameAudio.SetMusic(MusicTrack.Explore);
            SaveSystem.Current.hasDhanush = _wm != null && _wm.IsUnlocked(WeaponId.Dhanush);
            if (_wm != null) WriteWeaponLevels(_wm, SaveSystem.Current);
            Invoke(nameof(PlayEnding), delay);
        }

        void PlayEnding()
        {
            bool last = missionIndex >= Missions.Count - 1;
            StoryCutscene.Play(Mission.Outro, () =>
            {
                if (last) { _endScreen = true; GamePause.Locked = true; GameAudio.SetMusic(MusicTrack.Menu); SaveSystem.Current.finished = true; SaveSystem.Current.unlocked = Missions.Count - 1; SaveSystem.Save(); }
                else GameFlow.CompleteMission(missionIndex);
            });
        }

        public static void WriteWeaponLevels(WeaponManager wm, SaveData d)
        {
            d.trishulLevel = wm.LevelOf(WeaponId.Trishul);
            d.dhanushLevel = wm.LevelOf(WeaponId.Dhanush);
        }

        // ---------------- Screens ----------------
        void OnGUI()
        {
            float s = RishiStyle.S;
            if (GameOver)
            {
                float a = Mathf.Clamp01((Time.unscaledTime - _deadAt) / 1.5f);
                RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), new Color(0.05f, 0.02f, 0.08f, 0.75f * a));
                if (a < 1f) return;
                Rect panel = new Rect(Screen.width / 2f - 400 * s, Screen.height / 2f - 230 * s, 800 * s, 460 * s);
                RishiStyle.Panel(panel);
                GUI.Label(new Rect(panel.x, panel.y + 50 * s, panel.width, 80 * s), "Andhkar jeet gaya… is baar", RishiStyle.Heading);
                int c = _overMenu.Draw(new Rect(panel.x + 80 * s, panel.y + 180 * s, panel.width - 160 * s, 200 * s), OverItems, 90 * s);
                if (c >= 0) ChooseOver(c);
            }
            else if (_endScreen)
            {
                RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), RishiStyle.Kaala);
                RishiStyle.Border(new Rect(0, Screen.height * 0.2f, Screen.width, 34 * s));
                RishiStyle.Border(new Rect(0, Screen.height * 0.8f - 34 * s, Screen.width, 34 * s));
                RishiStyle.Shadowed(new Rect(0, Screen.height * 0.28f, Screen.width, 120 * s), "Rishi ki gaatha samaapt", RishiStyle.Heading);
                GUI.Label(new Rect(0, Screen.height * 0.42f, Screen.width, 160 * s),
                    $"Tamasur haara, saare teerthon ki jyoti laut aayi.\nKhelne ke liye dhanyavaad!     Jyoti: {Jyoti}\n\nRISHI: The Tamas War — Kanak Prabhakar", RishiStyle.Center);
                GUI.Label(new Rect(0, Screen.height * 0.7f, Screen.width, 40 * s), "Enter / A = main menu", RishiStyle.Center);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Instance = null;
    }
}
