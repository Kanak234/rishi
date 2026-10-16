// =====================================================================
// SettingsPanel.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: GDD Settings: graphics preset, volume (master/music/SFX), aur
//       accessibility — subtitles, text size, camera shake. Main menu aur
//       pause menu dono me same panel (do baar code nahi).
// KYA:  Plain C# class (MonoBehaviour nahi). Upar/neeche se row chuno,
//       left/right (A/D, d-pad, stick) se value badlo. Mouse se bhi.
// AAGE: MainMenu / PauseMenu Update() me Tick(), OnGUI me Draw() bulate hain.
//       Values GameSettings me jaati hain, Apply() turant lagta hai.
// =====================================================================
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Core;

namespace Rishi.UI
{
    public class SettingsPanel
    {
        int _row;
        float _nextRepeat;
        const int Rows = 9; // aakhri row = "Wapas"

        /// <summary>true = panel band karo (Back / Wapas).</summary>
        public bool Tick()
        {
            if (MenuList.BackPressed()) { GameSettings.Apply(); return true; }
            var kb = Keyboard.current; var pad = Gamepad.current;
            int v = 0, h = 0;
            if (kb != null)
            {
                if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) v = -1;
                if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) v = 1;
                if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) h = -1;
                if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) h = 1;
            }
            if (pad != null)
            {
                if (pad.dpad.up.wasPressedThisFrame) v = -1;
                if (pad.dpad.down.wasPressedThisFrame) v = 1;
                if (pad.dpad.left.wasPressedThisFrame) h = -1;
                if (pad.dpad.right.wasPressedThisFrame) h = 1;
                Vector2 st = pad.leftStick.ReadValue();
                if (st.magnitude > 0.6f && Time.unscaledTime >= _nextRepeat)
                {
                    if (Mathf.Abs(st.y) > Mathf.Abs(st.x)) v = st.y > 0 ? -1 : 1; else h = st.x > 0 ? 1 : -1;
                    _nextRepeat = Time.unscaledTime + 0.22f;
                }
                if (st.magnitude < 0.3f) _nextRepeat = 0f;
            }
            if (v != 0) { _row = (_row + v + Rows) % Rows; GameAudio.PlayUi(Sfx.UiMove); }
            if (h != 0) Change(_row, h);

            bool confirm = (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            if (confirm)
            {
                if (_row == Rows - 1) { GameSettings.Apply(); GameAudio.PlayUi(Sfx.UiSelect); return true; }
                Change(_row, 1);
            }
            return false;
        }

        void Change(int row, int dir)
        {
            switch (row)
            {
                case 0: GameSettings.MasterVolume = Step(GameSettings.MasterVolume, dir); break;
                case 1: GameSettings.MusicVolume = Step(GameSettings.MusicVolume, dir); break;
                case 2: GameSettings.SfxVolume = Step(GameSettings.SfxVolume, dir); break;
                case 3:
                    int n = QualitySettings.names.Length;
                    GameSettings.Quality = (QualitySettings.GetQualityLevel() + dir + n) % n; break;
                case 4: GameSettings.Fullscreen = !GameSettings.Fullscreen; break;
                case 5: GameSettings.Subtitles = !GameSettings.Subtitles; break;
                case 6: GameSettings.TextSize = (GameSettings.TextSize + dir + 3) % 3; break;
                case 7: GameSettings.CameraShake = !GameSettings.CameraShake; break;
                default: return;
            }
            GameSettings.Apply();
            GameAudio.PlayUi(Sfx.UiMove);
        }

        static float Step(float v, int dir) => Mathf.Clamp01(Mathf.Round((v + dir * 0.1f) * 10f) / 10f);

        public void Draw()
        {
            float s = RishiStyle.S;
            Rect panel = new Rect(Screen.width / 2f - 520 * s, Screen.height / 2f - 400 * s, 1040 * s, 800 * s);
            RishiStyle.Panel(panel);
            GUI.Label(new Rect(panel.x, panel.y + 40 * s, panel.width, 70 * s), "Settings", RishiStyle.Heading);

            string[] labels = { "Master volume", "Music", "SFX", "Graphics", "Fullscreen", "Subtitles", "Text size", "Camera shake", "Wapas" };
            string[] values =
            {
                Bar(GameSettings.MasterVolume), Bar(GameSettings.MusicVolume), Bar(GameSettings.SfxVolume),
                QualitySettings.names[QualitySettings.GetQualityLevel()],
                GameSettings.Fullscreen ? "On" : "Off", GameSettings.Subtitles ? "On" : "Off",
                GameSettings.TextSize == 0 ? "Chhota" : GameSettings.TextSize == 2 ? "Bada" : "Normal",
                GameSettings.CameraShake ? "On" : "Off", "",
            };

            float rowH = 62 * s, top = panel.y + 130 * s;
            for (int i = 0; i < Rows; i++)
            {
                Rect r = new Rect(panel.x + 70 * s, top + i * rowH, panel.width - 140 * s, rowH - 6 * s);
                bool hover = r.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseMove) _row = i;
                if (hover && Event.current.type == EventType.MouseDown)
                {
                    _row = i;
                    if (i == Rows - 1) { GameSettings.Apply(); _closeByMouse = true; }
                    else Change(i, Event.current.button == 1 ? -1 : 1); // right-click = ulta
                    Event.current.Use();
                }
                if (i == _row) RishiStyle.Rect(r, new Color(0, 0, 0, 0.5f));
                var st = RishiStyle.Small;
                Color old = st.normal.textColor;
                st.normal.textColor = i == _row ? RishiStyle.Sona : RishiStyle.Chuna;
                GUI.Label(new Rect(r.x + 20 * s, r.y, r.width * 0.5f, r.height), labels[i], st);
                if (values[i].Length > 0) GUI.Label(new Rect(r.x + r.width * 0.5f, r.y, r.width * 0.5f - 20 * s, r.height), "<   " + values[i] + "   >", st);
                st.normal.textColor = old;
            }
            GUI.Label(new Rect(panel.x, panel.yMax - 80 * s, panel.width, 40 * s), "W/S chuno  ·  A/D badlo  ·  Esc wapas", RishiStyle.Center);
        }

        bool _closeByMouse;
        /// <summary>Mouse se "Wapas" dabaya — caller agle Update me padhta hai.</summary>
        public bool ConsumeMouseClose() { bool c = _closeByMouse; _closeByMouse = false; return c; }

        static string Bar(float v)
        {
            int n = Mathf.RoundToInt(v * 10f);
            return new string('|', n * 2) + new string('.', (10 - n) * 2);
        }
    }
}
