// =====================================================================
// PauseMenu.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: GDD Pause screen: Resume, Settings, Main menu. GDD edge case:
//       Alt-Tab ke baad game sahi resume ho — GamePause pause karta hai,
//       ye menu dikhata hai, player khud Resume chunta hai.
// KYA:  GamePause.PauseChanged sunta hai. Keyboard/gamepad/mouse menu.
//       Esc = resume (ya settings se wapas). Controller nikla to bhi pause
//       + message (GDD UI states).
// AAGE: GameFlow.ToMenu() se main menu. SettingsPanel shared hai.
// =====================================================================
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Core;

namespace Rishi.UI
{
    public class PauseMenu : MonoBehaviour
    {
        readonly MenuList _menu = new MenuList();
        readonly SettingsPanel _settings = new SettingsPanel();
        readonly string[] _items = { "Resume", "Settings", "Controls", "Main menu" };
        bool _open, _inSettings, _inControls, _padLost;
        int _openedFrame;

        void OnEnable()
        {
            GamePause.PauseChanged += OnPause;
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnDisable()
        {
            GamePause.PauseChanged -= OnPause;
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        void OnPause(bool paused)
        {
            _open = paused;
            _inSettings = _inControls = false;
            _menu.Selected = 0;
            _openedFrame = Time.frameCount; // isi frame ka Esc dobara band na kar de
            if (!paused) _padLost = false;
        }

        // GDD: "controller disconnect par pause aur message"
        void OnDeviceChange(InputDevice d, InputDeviceChange change)
        {
            if (d is Gamepad && change == InputDeviceChange.Disconnected && !GamePause.Locked)
            {
                GamePause.SetPaused(true);
                _padLost = true;
            }
        }

        void Update()
        {
            if (!_open || Time.frameCount == _openedFrame) return;

            if (_inSettings)
            {
                if (_settings.Tick() || _settings.ConsumeMouseClose()) _inSettings = false;
                return;
            }
            if (_inControls)
            {
                if (MenuList.BackPressed() || ConfirmPressed()) _inControls = false;
                return;
            }
            if (MenuList.BackPressed()) { GamePause.SetPaused(false); return; }
            Choose(_menu.Navigate(_items.Length));
        }

        static bool ConfirmPressed()
        {
            var kb = Keyboard.current; var pad = Gamepad.current;
            return (kb != null && kb.enterKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame);
        }

        void Choose(int i)
        {
            switch (i)
            {
                case 0: GamePause.SetPaused(false); break;
                case 1: _inSettings = true; break;
                case 2: _inControls = true; break;
                case 3: GameFlow.ToMenu(); break;
            }
        }

        void OnGUI()
        {
            if (!_open) return;
            GUI.depth = -50;
            float s = RishiStyle.S;
            RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, 0.6f));
            if (_inSettings) { _settings.Draw(); return; }
            if (_inControls) { ControlsCard.Draw(); return; }

            Rect panel = new Rect(Screen.width / 2f - 360 * s, Screen.height / 2f - 330 * s, 720 * s, 660 * s);
            RishiStyle.Panel(panel);
            GUI.Label(new Rect(panel.x, panel.y + 50 * s, panel.width, 80 * s), "Viraam", RishiStyle.Heading); // = pause
            if (_padLost)
                GUI.Label(new Rect(panel.x, panel.y + 125 * s, panel.width, 40 * s), "Controller nikal gaya — jodo, phir Resume", RishiStyle.Center);
            int c = _menu.Draw(new Rect(panel.x + 60 * s, panel.y + 180 * s, panel.width - 120 * s, 400 * s), _items, 90 * s);
            if (c >= 0) Choose(c);
        }
    }

    /// <summary>Controls ki chhoti tasveer — pause aur main menu dono me.</summary>
    public static class ControlsCard
    {
        static readonly string[,] Rows =
        {
            { "Chalna", "WASD", "Left stick" },
            { "Koodna", "Space", "A / Cross" },
            { "Dodge (i-frames)", "Left Shift", "B / Circle" },
            { "Halka vaar (3-hit combo)", "Left click", "X / Square" },
            { "Bhaari vaar (hold = charge)", "Right click", "Y / Triangle" },
            { "Chadhna / ledge", "F / Space", "RB / A" },
            { "Shastra badlo", "Q / E", "D-pad left/right" },
            { "Dhanush nishana", "Left Ctrl (hold)", "LT (hold)" },
            { "Viraam (pause)", "Esc", "Start" },
        };

        public static void Draw()
        {
            float s = RishiStyle.S;
            Rect panel = new Rect(Screen.width / 2f - 640 * s, Screen.height / 2f - 400 * s, 1280 * s, 800 * s);
            RishiStyle.Panel(panel);
            GUI.Label(new Rect(panel.x, panel.y + 40 * s, panel.width, 70 * s), "Controls", RishiStyle.Heading);
            float y = panel.y + 140 * s, rh = 56 * s;
            var st = RishiStyle.Small;
            for (int i = 0; i < Rows.GetLength(0); i++)
            {
                if (i % 2 == 0) RishiStyle.Rect(new Rect(panel.x + 60 * s, y + i * rh, panel.width - 120 * s, rh), new Color(0, 0, 0, 0.25f));
                GUI.Label(new Rect(panel.x + 90 * s, y + i * rh, 520 * s, rh), Rows[i, 0], st);
                GUI.Label(new Rect(panel.x + 640 * s, y + i * rh, 280 * s, rh), Rows[i, 1], st);
                GUI.Label(new Rect(panel.x + 940 * s, y + i * rh, 300 * s, rh), Rows[i, 2], st);
            }
            GUI.Label(new Rect(panel.x, panel.yMax - 80 * s, panel.width, 40 * s), "Esc / B = wapas", RishiStyle.Center);
        }
    }
}
