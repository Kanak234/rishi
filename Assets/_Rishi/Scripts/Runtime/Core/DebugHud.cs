// =====================================================================
// DebugHud.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: M2 test karte waqt screen par dikhna chahiye ki Rishi kis state
//       me hai, kitni jaan hai, dodge ready hai ya nahi, FPS kitne hain.
//       Isse bug turant samajh aata hai ("wall-run start hi nahi hua").
// KYA:  Kone me chhota text panel. F1 se on/off. Release build me band.
// AAGE: PlayerController, Health aur CombatSystem (M3) se data padhta hai. M6 me asli HUD
//       (deepak health bar) aane ke baad ye sirf dev builds me rahega.
// =====================================================================
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Player;
using Rishi.Combat;

namespace Rishi.Core
{
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] bool visible = true;

        PlayerController _pc;
        Health _hp;
        CombatSystem _combat; // M3 — na ho to combat line nahi dikhegi
        float _fps;

        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _hp = GetComponent<Health>();
            _combat = GetComponent<CombatSystem>();
            // Release build me debug panel nahi chahiye.
            if (!Debug.isDebugBuild) visible = false;
        }

        void Update()
        {
            // FPS ko smooth karo (har frame ka number bahut uchhalta hai).
            if (Time.unscaledDeltaTime > 0f)
                _fps = Mathf.Lerp(_fps, 1f / Time.unscaledDeltaTime, 0.05f);
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || _pc == null) return;
            GUILayout.BeginArea(new Rect(12, 12, 300, 225), GUI.skin.box);
            GUILayout.Label($"FPS: {_fps:0}   (target 60)");
            GUILayout.Label($"State: {_pc.State}");
            GUILayout.Label($"HP: {_hp.Current:0}/{_hp.Max:0}   {( _hp.IsInvincible ? "[I-FRAMES]" : "")}");
            GUILayout.Label($"Dodge cooldown: {_pc.DodgeCooldownLeft:0.00}s");
            GUILayout.Label($"Wall-run left: {_pc.WallRunTimeLeft:0.00}s");
            GUILayout.Label($"Speed: {new Vector3(_pc.Velocity.x, 0, _pc.Velocity.z).magnitude:0.0} m/s");
            if (_combat != null)
            {
                string atk = _combat.IsCharging ? $"Heavy charge {_combat.Charge01 * 100f:0}%"
                           : _combat.CurrentKind == AttackKind.Light ? $"Light {_combat.ComboStep}/3"
                           : _combat.CurrentKind.ToString();
                GUILayout.Label($"Attack: {atk} {(_combat.HitboxActive ? "[HITBOX]" : "")}");
            }
            GUILayout.Label("F1 = hide");
            GUILayout.EndArea();
        }
    }
}
