// =====================================================================
// GamePause.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: GDD edge case — "Pause aur Alt-Tab par game sahi resume ho".
//       Laptop par window se bahar click karte hi game chalta rahe aur
//       Rishi mar jaye, ye nahi hona chahiye.
// KYA:  Esc / Start se pause toggle. Window ka focus jaane par apne aap
//       pause. Time.timeScale = 0 karke poori duniya rok deta hai.
// AAGE: PlayerController timeScale 0 dekh ke input ignore karta hai.
//       M6 me UIManager ka pause menu "Paused" event sun ke dikhega;
//       abhi ke liye screen par simple text aata hai.
// =====================================================================
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rishi.Core
{
    public class GamePause : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }
        public static event Action<bool> PauseChanged; // M6 ka pause menu yahan subscribe karega

        InputAction _toggle;

        void Awake()
        {
            // Pause apna alag action rakhta hai — player ka input band ho tab bhi chale.
            _toggle = new InputAction("PauseToggle", InputActionType.Button);
            _toggle.AddBinding("<Keyboard>/escape");
            _toggle.AddBinding("<Gamepad>/start");
        }

        void OnEnable()  => _toggle.Enable();
        void OnDisable() => _toggle.Disable();
        void OnDestroy() { _toggle.Dispose(); SetPaused(false); } // scene badle to game atka na rahe

        void Update()
        {
            if (_toggle.WasPressedThisFrame()) SetPaused(!IsPaused);
        }

        // Alt-Tab / doosri window par click → pause. Wapas aane par khud resume NAHI
        // (player ko ready hone ka mauka mile, wo khud Esc dabaye).
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !Application.isEditor) SetPaused(true);
        }

        public static void SetPaused(bool paused)
        {
            if (IsPaused == paused) return;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused; // pause me awaaz bhi ruke
            PauseChanged?.Invoke(paused);
        }

        void OnGUI()
        {
            // Temporary pause text — M6 me asli Sohrai-style menu aayega.
            if (!IsPaused) return;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 42, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "PAUSED  (Esc / Start)", style);
        }
    }
}
