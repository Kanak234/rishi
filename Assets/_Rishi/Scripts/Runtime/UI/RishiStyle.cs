// =====================================================================
// RishiStyle.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: GDD art direction: UI Sohrai-Khovar chitrakala jaisa — geru (laal
//       mitti) zameen par chuna-safed lakeerein, kaale kinare, deepon ka sona.
//       Har screen (menu, HUD, pause, cutscene) ek hi look me dikhe.
// KYA:  Runtime me banne wale textures (Sohrai border: bindu + zigzag,
//       deepak ki lau, KP orbit ring) aur IMGUI styles. Koi image asset nahi
//       chahiye — sab code se, isliye repo halka aur license-free.
// AAGE: MainMenu, PauseMenu, Hud, StoryCutscene, SettingsPanel isko use karte hain.
//       MenuList keyboard/gamepad se menu chalane ka chhota helper hai.
// =====================================================================
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Core;

namespace Rishi.UI
{
    public static class RishiStyle
    {
        // GDD palette
        public static readonly Color Geru  = new Color(0.55f, 0.22f, 0.12f);
        public static readonly Color GeruDark = new Color(0.30f, 0.11f, 0.06f);
        public static readonly Color Chuna = new Color(0.95f, 0.92f, 0.84f);
        public static readonly Color Kaala = new Color(0.07f, 0.05f, 0.05f);
        public static readonly Color Sona  = new Color(0.98f, 0.72f, 0.22f);
        public static readonly Color Tamas = new Color(0.25f, 0.14f, 0.48f);

        static Texture2D _white, _border, _flame, _ring;
        static GUIStyle _title, _heading, _body, _item, _small, _center;
        static float _builtForScale = -1f;

        public static Texture2D White => _white != null ? _white : (_white = Solid(Color.white));

        /// <summary>Screen height ke hisaab se scale — 1080p par 1.0; text size setting bhi jodta hai.</summary>
        public static float S => Screen.height / 1080f;
        public static float FontScale => S * GameSettings.TextScale;

        public static GUIStyle Title   { get { Ensure(); return _title; } }
        public static GUIStyle Heading { get { Ensure(); return _heading; } }
        public static GUIStyle Body    { get { Ensure(); return _body; } }
        public static GUIStyle Item    { get { Ensure(); return _item; } }
        public static GUIStyle Small   { get { Ensure(); return _small; } }
        public static GUIStyle Center  { get { Ensure(); return _center; } }

        static void Ensure()
        {
            // Resolution / text size badla to styles dobara (font size pixel me hota hai).
            float fs = FontScale;
            if (_title != null && Mathf.Approximately(fs, _builtForScale)) return;
            _builtForScale = fs;
            _title   = Make(Mathf.RoundToInt(96 * fs), FontStyle.Bold, Chuna, TextAnchor.MiddleCenter);
            _heading = Make(Mathf.RoundToInt(48 * fs), FontStyle.Bold, Sona, TextAnchor.MiddleCenter);
            _body    = Make(Mathf.RoundToInt(30 * fs), FontStyle.Normal, Chuna, TextAnchor.UpperLeft);
            _body.wordWrap = true;
            _item    = Make(Mathf.RoundToInt(38 * fs), FontStyle.Bold, Chuna, TextAnchor.MiddleCenter);
            _small   = Make(Mathf.RoundToInt(24 * fs), FontStyle.Normal, Chuna, TextAnchor.MiddleLeft);
            _center  = Make(Mathf.RoundToInt(32 * fs), FontStyle.Normal, Chuna, TextAnchor.MiddleCenter);
            _center.wordWrap = true;
        }

        static GUIStyle Make(int size, FontStyle fs, Color c, TextAnchor a)
        {
            var st = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(10, size), fontStyle = fs, alignment = a, richText = true };
            st.normal.textColor = c;
            return st;
        }

        // ---------------- Drawing ----------------
        public static void Rect(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, White);
            GUI.color = old;
        }

        /// <summary>Sohrai panel: geru zameen, kaala kinara, andar safed bindu+zigzag ki patti.</summary>
        public static void Panel(Rect r, float alpha = 0.94f)
        {
            Rect(r, new Color(Kaala.r, Kaala.g, Kaala.b, alpha));
            Rect inner = new Rect(r.x + 4 * S, r.y + 4 * S, r.width - 8 * S, r.height - 8 * S);
            Rect(inner, new Color(Geru.r, Geru.g, Geru.b, alpha));
            float b = 26 * S;
            Border(new Rect(inner.x, inner.y, inner.width, b));
            Border(new Rect(inner.x, inner.yMax - b, inner.width, b));
        }

        /// <summary>Ek patti Sohrai border (texture repeat hota hai, khinchta nahi).</summary>
        public static void Border(Rect r, float alpha = 1f)
        {
            if (_border == null) _border = BuildBorder();
            Color old = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha);
            float tiles = r.width / Mathf.Max(1f, r.height * 2f);
            GUI.DrawTextureWithTexCoords(r, _border, new Rect(0, 0, tiles, 1));
            GUI.color = old;
        }

        public static void Flame(Rect r, Color tint)
        {
            if (_flame == null) _flame = BuildFlame();
            Color old = GUI.color; GUI.color = tint;
            GUI.DrawTexture(r, _flame, ScaleMode.ScaleToFit, true);
            GUI.color = old;
        }

        public static void Ring(Rect r, Color tint)
        {
            if (_ring == null) _ring = BuildRing();
            Color old = GUI.color; GUI.color = tint;
            GUI.DrawTexture(r, _ring, ScaleMode.StretchToFill, true);
            GUI.color = old;
        }

        /// <summary>Text ke peeche kaali parchhai — 3D duniya ke upar bhi padha jaye (contrast check).</summary>
        public static void Shadowed(Rect r, string text, GUIStyle st)
        {
            Color c = st.normal.textColor;
            st.normal.textColor = new Color(0, 0, 0, 0.85f);
            GUI.Label(new Rect(r.x + 2 * S, r.y + 2 * S, r.width, r.height), text, st);
            st.normal.textColor = c;
            GUI.Label(r, text, st);
        }

        // ---------------- Textures ----------------
        static Texture2D Solid(Color c)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.SetPixel(0, 0, c); t.Apply();
            return t;
        }

        // 64x32 tile: upar-neeche safed lakeer, beech me zigzag, zigzag ke khaanon me bindu (Sohrai ki pehchaan).
        static Texture2D BuildBorder()
        {
            const int w = 64, h = 32;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Color c = new Color(0, 0, 0, 0);
                    if (y <= 2 || y >= h - 3) c = Chuna;                              // kinare ki lakeer
                    else if (y == 3 || y == h - 4) c = Kaala;
                    float zig = Mathf.Abs((x % 32) - 16f) / 16f * (h - 14) + 7;      // 7..25 tak upar-neeche
                    if (Mathf.Abs(y - zig) < 1.6f) c = Chuna;
                    // bindu: zigzag ke upar aur neeche wale tikon me
                    Vector2 d1 = new Vector2((x % 32) - 16f, y - 22f);
                    Vector2 d2 = new Vector2((x % 32), y - 10f);
                    Vector2 d3 = new Vector2((x % 32) - 32f, y - 10f);
                    if (d1.sqrMagnitude < 5f || d2.sqrMagnitude < 5f || d3.sqrMagnitude < 5f) c = Sona;
                    px[y * w + x] = c;
                }
            t.SetPixels(px); t.Apply();
            return t;
        }

        // Deepak ki lau: aansu jaisi shape, beech me safed, bahar sona → laal.
        static Texture2D BuildFlame()
        {
            const int w = 32, h = 48;
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float v = y / (float)h;                         // 0 neeche, 1 upar
                    float width = Mathf.Sin(Mathf.PI * Mathf.Pow(1f - v, 0.7f)) * 0.5f * (1f - v * 0.6f);
                    float dx = Mathf.Abs(x / (float)w - 0.5f);
                    float a = Mathf.Clamp01((width - dx) * 18f);
                    float core = Mathf.Clamp01((width * 0.45f - dx) * 14f) * (1f - v);
                    Color c = Color.Lerp(new Color(0.9f, 0.25f, 0.05f), Sona, 1f - v);
                    c = Color.Lerp(c, Color.white, core);
                    c.a = a;
                    t.SetPixel(x, y, c);
                }
            t.Apply();
            return t;
        }

        static Texture2D BuildRing()
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = new Vector2(x - n / 2f + 0.5f, y - n / 2f + 0.5f).magnitude / (n / 2f);
                    float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.92f) * 40f);
                    t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            t.Apply();
            return t;
        }
    }

    /// <summary>
    /// Keyboard (W/S, arrows, Enter/Space), gamepad (stick/d-pad, A) aur mouse — teeno se chalne wala menu.
    /// IMGUI ke button keyboard se nahi chalte, isliye apna helper.
    /// </summary>
    public class MenuList
    {
        public int Selected;
        float _nextRepeat;
        int _lastHover = -1;

        /// <summary>Har frame Update() se bulao. Return: chuna gaya index, ya -1.</summary>
        public int Navigate(int count)
        {
            if (count <= 0) return -1;
            int dir = 0;
            var kb = Keyboard.current; var pad = Gamepad.current;
            if (kb != null && (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)) dir = -1;
            if (kb != null && (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)) dir = 1;
            if (pad != null)
            {
                if (pad.dpad.up.wasPressedThisFrame) dir = -1;
                if (pad.dpad.down.wasPressedThisFrame) dir = 1;
                float sy = pad.leftStick.ReadValue().y;
                if (Mathf.Abs(sy) > 0.6f && Time.unscaledTime >= _nextRepeat) { dir = sy > 0 ? -1 : 1; _nextRepeat = Time.unscaledTime + 0.25f; }
                if (Mathf.Abs(sy) < 0.3f) _nextRepeat = 0f;
            }
            if (dir != 0) { Selected = (Selected + dir + count) % count; GameAudio.PlayUi(Sfx.UiMove); }
            Selected = Mathf.Clamp(Selected, 0, count - 1);

            bool confirm = (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
                        || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            if (confirm) { GameAudio.PlayUi(Sfx.UiSelect); return Selected; }
            return -1;
        }

        public static bool BackPressed()
        {
            var kb = Keyboard.current; var pad = Gamepad.current;
            return (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame))
                || (pad != null && pad.buttonEast.wasPressedThisFrame);
        }

        /// <summary>OnGUI se: items ko column me draw karo. Mouse click ho to index return.</summary>
        public int Draw(Rect area, string[] items, float itemHeight)
        {
            int clicked = -1;
            float s = RishiStyle.S;
            for (int i = 0; i < items.Length; i++)
            {
                Rect r = new Rect(area.x, area.y + i * itemHeight, area.width, itemHeight - 8 * s);
                bool hover = r.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseMove && _lastHover != i) { Selected = i; _lastHover = i; }
                bool sel = i == Selected;
                if (sel)
                {
                    RishiStyle.Rect(r, new Color(0, 0, 0, 0.55f));
                    RishiStyle.Flame(new Rect(r.x + 12 * s, r.y + 6 * s, 28 * s, r.height - 12 * s), Color.white);
                    RishiStyle.Flame(new Rect(r.xMax - 40 * s, r.y + 6 * s, 28 * s, r.height - 12 * s), Color.white);
                }
                var st = RishiStyle.Item;
                Color old = st.normal.textColor;
                st.normal.textColor = sel ? RishiStyle.Sona : RishiStyle.Chuna;
                GUI.Label(r, items[i], st);
                st.normal.textColor = old;
                if (hover && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    Selected = i; clicked = i; GameAudio.PlayUi(Sfx.UiSelect);
                    Event.current.Use();
                }
            }
            return clicked;
        }
    }
}
