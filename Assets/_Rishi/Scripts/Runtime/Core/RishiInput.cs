// =====================================================================
// RishiInput.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: Game ke saare controls (keyboard+mouse aur gamepad) ek jagah
//       define karne ke liye. Baaki scripts ko pata nahi hona chahiye ki
//       button kaunsa hai — wo sirf "Jump dabaya?" puchte hain.
// KYA:  Unity Input System ke InputAction code me hi banata hai
//       (koi .inputactions asset nahi chahiye), taaki AI aur insaan dono
//       seedha text me bindings padh/badal sakein.
// AAGE: PlayerController (movement, jump, dodge, climb) aur aage chal ke
//       CombatSystem (light/heavy attack) isi component se input padhte hain.
// =====================================================================
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rishi.Core
{
    // Ye component Player GameObject par lagta hai.
    public class RishiInput : MonoBehaviour
    {
        // --- Actions: har ek "kaam" ka ek InputAction -------------------
        // public getter isliye ki doosri scripts padh sakein, private set
        // isliye ki koi bahar se action badal na de.
        public InputAction Move { get; private set; }        // WASD / left stick
        public InputAction Jump { get; private set; }        // Space / A
        public InputAction Dodge { get; private set; }       // Left Shift / B
        public InputAction LightAttack { get; private set; } // LMB / X  (M3 me use hoga)
        public InputAction HeavyAttack { get; private set; } // RMB / Y  (M3 me use hoga)
        public InputAction Interact { get; private set; }    // F / RB   (ledge climb, switch)
        public InputAction PrevWeapon { get; private set; }  // Q / D-pad left  (M3)
        public InputAction NextWeapon { get; private set; }  // E / D-pad right (M3)
        public InputAction Aim { get; private set; }         // Left Ctrl hold / LT (M3, Dhanush)
        public InputAction Pause { get; private set; }       // Esc / Start

        // Move ka value seedha Vector2 me — PlayerController har frame padhta hai.
        public Vector2 MoveValue => Move.ReadValue<Vector2>();

        void Awake()
        {
            // Awake me banate hain taaki kisi bhi Start/Update se pehle ready ho.

            // Move: WASD ko ek 2D vector me jodna (composite), plus gamepad stick.
            Move = new InputAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            Move.AddBinding("<Gamepad>/leftStick");

            // Button actions: Make() helper ek keyboard/mouse aur ek gamepad binding deta hai.
            Jump        = Make("Jump",        "<Keyboard>/space",      "<Gamepad>/buttonSouth");
            Dodge       = Make("Dodge",       "<Keyboard>/leftShift",  "<Gamepad>/buttonEast");
            LightAttack = Make("LightAttack", "<Mouse>/leftButton",    "<Gamepad>/buttonWest");
            HeavyAttack = Make("HeavyAttack", "<Mouse>/rightButton",   "<Gamepad>/buttonNorth");
            Interact    = Make("Interact",    "<Keyboard>/f",          "<Gamepad>/rightShoulder");
            PrevWeapon  = Make("PrevWeapon",  "<Keyboard>/q",          "<Gamepad>/dpad/left");
            NextWeapon  = Make("NextWeapon",  "<Keyboard>/e",          "<Gamepad>/dpad/right");
            Aim         = Make("Aim",         "<Keyboard>/leftCtrl",   "<Gamepad>/leftTrigger");
            Pause       = Make("Pause",       "<Keyboard>/escape",     "<Gamepad>/start");
        }

        // Chhota helper: duplicate code kam karta hai.
        static InputAction Make(string name, string kbm, string pad)
        {
            var a = new InputAction(name, InputActionType.Button);
            a.AddBinding(kbm);
            a.AddBinding(pad);
            return a;
        }

        // Enable/Disable: component on/off hone par input bhi on/off.
        // (Cutscene ke time PlayerInput disable karna ho to bas component band karo.)
        void OnEnable()  { ForEach(a => a.Enable()); }
        void OnDisable() { ForEach(a => a.Disable()); }
        void OnDestroy() { ForEach(a => a.Dispose()); } // memory leak se bachne ke liye

        void ForEach(System.Action<InputAction> f)
        {
            // Awake se pehle OnDisable call ho sakta hai (edge case) — null check zaroori.
            InputAction[] all = { Move, Jump, Dodge, LightAttack, HeavyAttack, Interact, PrevWeapon, NextWeapon, Aim, Pause };
            foreach (var a in all) if (a != null) f(a);
        }
    }
}
