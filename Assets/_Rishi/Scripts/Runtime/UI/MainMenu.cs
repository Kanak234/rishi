// =====================================================================
// MainMenu.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: GDD screens: Splash (KP orbit logo, phir game title) aur Main menu
//       (New game, Continue, Settings, Credits, Exit).
// KYA:  Teen hisse: splash (KP logo ke charon taraf ghoomta deepak = orbit,
//       ~3 s, koi bhi button = skip) → title + menu → Settings / Controls /
//       Credits panels. Peeche 3D mandir ka chhota drishya, camera dheere ghoomta hai.
//       Continue sirf tab jab save file ho.
// AAGE: GameFlow.NewGame() / Continue() Rajrappa scene kholte hain.
// =====================================================================
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Core;

namespace Rishi.UI
{
    public class MainMenu : MonoBehaviour
    {
        enum Page { Splash, Main, Missions, Settings, Controls, Credits }

        [SerializeField] Transform orbitCenter;          // camera iske charon taraf ghoomta hai
        [SerializeField] float orbitSpeed = 4f;          // degree / sec — dheema, shant
        [SerializeField] float splashTime = 3.2f;

        Page _page = Page.Splash;
        float _t;
        readonly MenuList _menu = new MenuList();
        readonly MenuList _missionMenu = new MenuList();
        string[] _missionItems;
        readonly SettingsPanel _settings = new SettingsPanel();
        string[] _items;
        System.Action[] _actions;
        Camera _cam;

        void Start()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _cam = Camera.main;
            GameAudio.SetMusic(MusicTrack.Menu);
            BuildItems();
        }

        void BuildItems()
        {
            // Continue sirf save ho tab — aur tab wahi pehla (sabse zyada use hota hai).
            if (SaveSystem.HasSave)
            {
                SaveSystem.Load();
                _items = new[] { SaveSystem.Current.finished ? "Gaatha dobara" : "Continue", "Mission chuno", "Naya khel", "Settings", "Controls", "Credits", "Exit" };
                _actions = new System.Action[] { SaveSystem.Current.finished ? (System.Action)(() => GameFlow.StartMission(0)) : GameFlow.Continue, OpenMissions, GameFlow.NewGame, () => _page = Page.Settings, () => _page = Page.Controls, () => _page = Page.Credits, Quit };
            }
            else
            {
                _items = new[] { "Naya khel", "Settings", "Controls", "Credits", "Exit" };
                _actions = new System.Action[] { GameFlow.NewGame, () => _page = Page.Settings, () => _page = Page.Controls, () => _page = Page.Credits, Quit };
            }
        }

        void OpenMissions()
        {
            int open = Mathf.Clamp(SaveSystem.Current.unlocked, 0, Rishi.World.Missions.Count - 1);
            _missionItems = new string[open + 2];
            for (int i = 0; i <= open; i++)
            {
                var m = Rishi.World.Missions.All[i];
                _missionItems[i] = $"{i + 1}. {m.Title}  ·  {m.Level}";
            }
            _missionItems[open + 1] = "Wapas";
            _missionMenu.Selected = open;
            _page = Page.Missions;
        }

        void ChooseMission(int i)
        {
            if (i < 0) return;
            if (i >= _missionItems.Length - 1) { _page = Page.Main; return; }
            GameFlow.StartMission(i);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void Update()
        {
            _t += Time.unscaledDeltaTime;
            if (_cam != null && orbitCenter != null)
                _cam.transform.RotateAround(orbitCenter.position, Vector3.up, orbitSpeed * Time.unscaledDeltaTime);

            switch (_page)
            {
                case Page.Splash:
                    if (_t > splashTime || AnyPressed()) { _page = Page.Main; _t = 0f; }
                    break;
                case Page.Main:
                    int i = _menu.Navigate(_items.Length);
                    if (i >= 0) _actions[i]();
                    break;
                case Page.Missions:
                    if (MenuList.BackPressed()) { _page = Page.Main; break; }
                    ChooseMission(_missionMenu.Navigate(_missionItems.Length));
                    break;
                case Page.Settings:
                    if (_settings.Tick() || _settings.ConsumeMouseClose()) _page = Page.Main;
                    break;
                case Page.Controls:
                case Page.Credits:
                    if (MenuList.BackPressed() || ConfirmPressed()) _page = Page.Main;
                    break;
            }
        }

        static bool AnyPressed()
        {
            var kb = Keyboard.current; var pad = Gamepad.current; var m = Mouse.current;
            return (kb != null && kb.anyKey.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame)
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));
        }

        static bool ConfirmPressed()
        {
            var kb = Keyboard.current; var pad = Gamepad.current;
            return (kb != null && kb.enterKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame);
        }

        void OnGUI()
        {
            float s = RishiStyle.S;
            if (_page == Page.Splash) { DrawSplash(s); return; }

            // Title fade-in
            float a = Mathf.Clamp01(_t / 0.8f);
            RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.35f));
            RishiStyle.Border(new Rect(0, 0, Screen.width, 34 * s), a);
            RishiStyle.Border(new Rect(0, Screen.height - 34 * s, Screen.width, 34 * s), a);

            switch (_page)
            {
                case Page.Main: DrawMain(s); break;
                case Page.Missions: DrawMissions(s); break;
                case Page.Settings: _settings.Draw(); break;
                case Page.Controls: ControlsCard.Draw(); break;
                case Page.Credits: DrawCredits(s); break;
            }
        }

        void DrawSplash(float s)
        {
            RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), Color.black);
            float fade = Mathf.Clamp01(_t / 0.6f) * Mathf.Clamp01((splashTime - _t) / 0.6f);
            Vector2 c = new Vector2(Screen.width / 2f, Screen.height / 2f - 40 * s);
            float r = 150 * s;
            RishiStyle.Ring(new Rect(c.x - r, c.y - r * 0.42f, r * 2f, r * 0.84f), new Color(RishiStyle.Chuna.r, RishiStyle.Chuna.g, RishiStyle.Chuna.b, 0.7f * fade));
            // Orbit par ghoomta deepak
            float ang = _t * 2.2f;
            Vector2 p = c + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r * 0.42f);
            RishiStyle.Flame(new Rect(p.x - 14 * s, p.y - 24 * s, 28 * s, 42 * s), new Color(1, 1, 1, fade));
            var t = RishiStyle.Title;
            Color old = t.normal.textColor;
            t.normal.textColor = new Color(RishiStyle.Sona.r, RishiStyle.Sona.g, RishiStyle.Sona.b, fade);
            GUI.Label(new Rect(0, c.y - 70 * s, Screen.width, 140 * s), "KP", t);
            t.normal.textColor = old;
            var st = RishiStyle.Center;
            Color o2 = st.normal.textColor;
            st.normal.textColor = new Color(o2.r, o2.g, o2.b, fade);
            GUI.Label(new Rect(0, c.y + 110 * s, Screen.width, 50 * s), "Kanak Prabhakar prastut karte hain", st);
            st.normal.textColor = o2;
        }

        void DrawMain(float s)
        {
            Rect title = new Rect(0, Screen.height * 0.08f, Screen.width, 130 * s);
            RishiStyle.Shadowed(title, "RISHI", RishiStyle.Title);
            RishiStyle.Shadowed(new Rect(0, title.yMax - 10 * s, Screen.width, 60 * s), "The Tamas War", RishiStyle.Heading);
            GUI.Label(new Rect(0, title.yMax + 50 * s, Screen.width, 40 * s), "Ek Itihasa gaatha  ·  6 level · 8 mission", RishiStyle.Center);

            float w = 560 * s, ih = 84 * s;
            Rect panel = new Rect(Screen.width / 2f - w / 2f, Screen.height * 0.42f, w, ih * _items.Length + 80 * s);
            RishiStyle.Panel(panel, 0.85f);
            int c = _menu.Draw(new Rect(panel.x + 30 * s, panel.y + 40 * s, panel.width - 60 * s, ih * _items.Length), _items, ih);
            if (c >= 0) _actions[c]();

            GUI.Label(new Rect(24 * s, Screen.height - 80 * s, Screen.width, 36 * s), "v1.0  ·  W/S + Enter  ·  gamepad bhi chalega", RishiStyle.Small);
        }

        void DrawMissions(float s)
        {
            float ih = 70 * s;
            Rect panel = new Rect(Screen.width / 2f - 560 * s, Screen.height / 2f - (ih * _missionItems.Length + 180 * s) / 2f, 1120 * s, ih * _missionItems.Length + 180 * s);
            RishiStyle.Panel(panel);
            GUI.Label(new Rect(panel.x, panel.y + 36 * s, panel.width, 70 * s), "Mission chuno", RishiStyle.Heading);
            int c = _missionMenu.Draw(new Rect(panel.x + 40 * s, panel.y + 120 * s, panel.width - 80 * s, ih * _missionItems.Length), _missionItems, ih);
            ChooseMission(c);
        }

        void DrawCredits(float s)
        {
            Rect panel = new Rect(Screen.width / 2f - 600 * s, Screen.height / 2f - 380 * s, 1200 * s, 760 * s);
            RishiStyle.Panel(panel);
            GUI.Label(new Rect(panel.x, panel.y + 40 * s, panel.width, 70 * s), "Credits", RishiStyle.Heading);
            string text =
                "<b>Game design, code, kahani</b>\nKanak Prabhakar\n\n" +
                "<b>Engine</b>\nUnity 6 (URP)\n\n" +
                "<b>Character</b>\nUnity Starter Assets (Unity Companion License)\n\n" +
                "<b>Kala prerna</b>\nHazaribagh ki Sohrai aur Khovar chitrakala — gaon ki un mahila chitrakaron ko naman\n\n" +
                "<b>Sangeet aur awaaz</b>\nCode se bane (procedural) — tanpura drone aur dhol taal\n\n" +
                "Ye kahani kalpanik hai, Bharat ki Itihasa parampara se prerit.";
            GUI.Label(new Rect(panel.x + 80 * s, panel.y + 130 * s, panel.width - 160 * s, panel.height - 220 * s), text, RishiStyle.Center);
            GUI.Label(new Rect(panel.x, panel.yMax - 80 * s, panel.width, 40 * s), "Esc / B = wapas", RishiStyle.Center);
        }
    }
}
