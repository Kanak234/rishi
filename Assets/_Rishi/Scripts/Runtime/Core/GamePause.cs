// =====================================================================
// GamePause.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: GDD edge case — "Pause aur Alt-Tab par game sahi resume ho".
//       Laptop par window se bahar click karte hi game chalta rahe aur
//       Rishi mar jaye, ye nahi hona chahiye.
// KYA:  Esc / Start se pause toggle. Window ka focus jaane par apne aap
//       pause. Time.timeScale = 0 karke poori duniya rok deta hai.
// AAGE: PlayerController timeScale 0 dekh ke input ignore karta hai.
//       PauseMenu (UI) "PauseChanged" sun ke menu dikhata hai aur
//       wahi Esc se resume karta hai.
// =====================================================================
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rishi.Core
{
    public class GamePause : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }
        public static event Action<bool> PauseChanged; // PauseMenu yahan subscribe karta hai

        /// <summary>Cutscene / game over / jeet ke time pause toggle band (dono ek saath timeScale na chhedein).</summary>
        public static bool Locked { get; set; }

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
        void OnDestroy() { _toggle.Dispose(); SetPaused(false); Locked = false; } // scene badle to game atka na rahe

        void Update()
        {
            // Pause menu khula ho to Esc wahi sambhalta hai (settings se wapas, phir resume).
            if (_toggle.WasPressedThisFrame() && !Locked && !IsPaused) SetPaused(true);
        }

        // Alt-Tab / doosri window par click → pause. Wapas aane par khud resume NAHI
        // (player ko ready hone ka mauka mile, wo khud Esc dabaye).
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !Application.isEditor && !Locked) SetPaused(true);
        }

        public static void SetPaused(bool paused)
        {
            if (IsPaused == paused) return;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            AudioListener.pause = paused; // pause me awaaz bhi ruke
            PauseChanged?.Invoke(paused);
        }
    }
}
