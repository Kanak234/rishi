// =====================================================================
// StoryCutscene.cs  —  RISHI: The Tamas War   (Milestone M5)
// ---------------------------------------------------------------------
// KYUN: GDD: "Sohrai-style intro cutscene + 1 mid cutscene, narrator".
//       Kahani budhe chitrakar sunate hain jo deewar par Rishi ki gaatha
//       bana rahe hain — isliye har slide deewar par "banti" dikhti hai.
// KYA:  Slides ki list (chitra + narration line). Har slide: baayein se
//       daayein chitra ubharta hai (painting banti hui), halka zoom/pan
//       (parallax jaisa), neeche subtitle. Space/Enter/A = agli slide,
//       Esc/B = poora skip. Chalte waqt duniya ruki rehti hai (timeScale 0).
// AAGE: LevelDirector intro / mid (boss se pehle) / ending yahan se chalata hai.
//       Narrator ki awaaz (voice-over) aaye to Slide me AudioClip jod dena.
// =====================================================================
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Core;

namespace Rishi.UI
{
    [Serializable]
    public struct Slide
    {
        public Painting painting;
        [TextArea] public string line;
        public Slide(Painting p, string l) { painting = p; line = l; }
    }

    public class StoryCutscene : MonoBehaviour
    {
        public static bool IsPlaying { get; private set; }

        Slide[] _slides;
        Action _onDone;
        int _index;
        float _t;           // is slide ka unscaled time
        float _fade;        // 0..1 kaala parda (shuru / ant)
        bool _ending;

        const float RevealTime = 1.6f;    // chitra kitni der me "banta" hai
        const float AutoAdvance = 7.5f;   // koi button na dabaye to agli slide

        /// <summary>Chal rahi kahani skip karo (Esc jaisa) — callback phir bhi chalta hai.</summary>
        public static void SkipCurrent()
        {
            var c = FindAnyObjectByType<StoryCutscene>();
            if (c != null) c._ending = true;
        }

        public static void Play(Slide[] slides, Action onDone)
        {
            if (slides == null || slides.Length == 0) { onDone?.Invoke(); return; }
            var go = new GameObject("StoryCutscene");
            var c = go.AddComponent<StoryCutscene>();
            c._slides = slides;
            c._onDone = onDone;
            IsPlaying = true;
            GamePause.Locked = true;
            Time.timeScale = 0f; // duniya ruko — asur cutscene ke beech na maarein
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _t += dt;
            _fade = _ending ? Mathf.MoveTowards(_fade, 1f, dt * 2.5f) : Mathf.MoveTowards(_fade, 0f, dt * 2f);
            if (_ending) { if (_fade >= 1f) Finish(); return; }

            var kb = Keyboard.current; var pad = Gamepad.current; var mouse = Mouse.current;
            bool next = (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                     || (pad != null && pad.buttonSouth.wasPressedThisFrame)
                     || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            bool skip = (kb != null && kb.escapeKey.wasPressedThisFrame) || (pad != null && (pad.buttonEast.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));

            if (skip) { _ending = true; return; }
            if (next && _t < RevealTime) { _t = RevealTime; return; } // pehla dabana = chitra poora dikhao
            if (next || _t > AutoAdvance)
            {
                _index++;
                _t = 0f;
                if (_index >= _slides.Length) { _index = _slides.Length - 1; _ending = true; }
                else GameAudio.PlayUi(Sfx.UiMove);
            }
        }

        void Finish()
        {
            IsPlaying = false;
            GamePause.Locked = false;
            Time.timeScale = 1f;
            Destroy(gameObject);
            _onDone?.Invoke();
        }

        void OnDestroy()
        {
            // Edge case: cutscene ke beech scene badla — game ruka na rahe.
            if (IsPlaying) { IsPlaying = false; GamePause.Locked = false; Time.timeScale = 1f; }
        }

        void OnGUI()
        {
            GUI.depth = -100; // HUD/menu ke upar
            float s = RishiStyle.S;
            RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), RishiStyle.Kaala);

            Slide sl = _slides[_index];
            Texture2D tex = SohraiPainter.Get(sl.painting);

            // 16:9 chitra, screen ke beech, subtitle ke liye neeche jagah
            float h = Screen.height * 0.74f, w = h * 16f / 9f;
            if (w > Screen.width * 0.94f) { w = Screen.width * 0.94f; h = w * 9f / 16f; }
            Rect area = new Rect((Screen.width - w) / 2f, Screen.height * 0.05f, w, h);

            // Halka Ken-Burns zoom/pan = parallax jaisa jeevan
            float k = Mathf.Clamp01(_t / AutoAdvance);
            float zoom = 1f - 0.06f * k;
            Rect uv = new Rect((1f - zoom) * (_index % 2 == 0 ? k : 1f - k), (1f - zoom) * 0.5f, zoom, zoom);

            // Painting "banti" hai: baayein se daayein reveal
            float reveal = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_t / RevealTime));
            Rect part = new Rect(area.x, area.y, area.width * reveal, area.height);
            Rect partUv = new Rect(uv.x, uv.y, uv.width * reveal, uv.height);
            GUI.DrawTextureWithTexCoords(part, tex, partUv);
            if (reveal < 1f) // "brush" ki chamakti lakeer
                RishiStyle.Rect(new Rect(part.xMax - 3 * s, area.y, 6 * s, area.height), new Color(RishiStyle.Chuna.r, RishiStyle.Chuna.g, RishiStyle.Chuna.b, 0.8f));

            if (GameSettings.Subtitles)
            {
                Rect sub = new Rect(area.x, area.yMax + 16 * s, area.width, Screen.height - area.yMax - 60 * s);
                RishiStyle.Shadowed(sub, sl.line, RishiStyle.Center);
            }
            var small = RishiStyle.Small;
            GUI.Label(new Rect(24 * s, Screen.height - 46 * s, Screen.width - 48 * s, 36 * s),
                $"{_index + 1}/{_slides.Length}     Space / A = aage     Esc / B = skip", small);

            if (_fade > 0.001f) RishiStyle.Rect(new Rect(0, 0, Screen.width, Screen.height), new Color(0, 0, 0, _fade));
        }
    }
}
