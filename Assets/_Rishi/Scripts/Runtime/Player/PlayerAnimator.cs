// =====================================================================
// PlayerAnimator.cs  —  RISHI: The Tamas War   (Milestone M2)
// ---------------------------------------------------------------------
// KYUN: PlayerController sirf "Rishi kahan hai aur kis state me hai" tay
//       karta hai. Dikhne wala body (Mixamo model) alag script se chalaya
//       jaata hai, taaki movement ka logic animation par depend na kare —
//       animation badlo, model badlo, movement ka code nahi chhedna padega.
// KYA:  PlayerController ke StateChanged event par sahi Animator state me
//       CrossFade karta hai, har frame "Speed" param (idle↔run blend) bharta
//       hai, hawa me Jump→Fall badalta hai, roll/climb clip ki speed GDD ke
//       time (0.4 s dodge) se milata hai, aur wall-run me body ko jhukata hai.
// KAISE: Root motion BAND hai — position hamesha CharacterController/
//       PlayerController hi badalta hai (exact control, GDD feel). Animator
//       sirf haddiyan (bones) hilata hai.
// AAGE: Animator Controller (AC_Rishi) Editor tool MixamoRishiSetup banata
//       hai; state ke naam yahan aur wahan same hone chahiye (RishiAnim class).
//       CombatSystem (M3) ke AttackStarted/AttackEnded par attack states chalata hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;

namespace Rishi.Player
{
    /// <summary>Animator state/param naam — Editor tool aur runtime dono yahi use karte hain (typo se bachao).</summary>
    public static class RishiAnim
    {
        public const string Locomotion = "Locomotion";
        public const string Jump = "Jump";
        public const string Fall = "Fall";
        public const string Roll = "Roll";
        public const string WallRun = "WallRun";
        public const string Hang = "Hang";
        public const string Climb = "Climb";
        // M3 attack states — clip ho tabhi controller me bante hain (Attack1.fbx ... Heavy.fbx).
        public const string Attack1 = "Attack1";
        public const string Attack2 = "Attack2";
        public const string Attack3 = "Attack3";
        public const string HeavyCharge = "HeavyCharge";
        public const string Heavy = "Heavy";
        public const string BowShoot = "BowShoot"; // Dhanush (optional clip)

        public const string SpeedParam = "Speed";          // 0 = khada, 1 = full run
        public const string RollSpeedParam = "RollSpeed";  // roll clip ka playback multiplier
        public const string ClimbSpeedParam = "ClimbSpeed";
    }

    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimator : MonoBehaviour
    {
        [Tooltip("Mixamo model (child 'Body') par laga Animator. Khaali chhodo to child me khud dhoondh lega.")]
        [SerializeField] Animator animator;

        [Header("Blend / fade")]
        [SerializeField] float speedDamp = 0.1f;        // Speed param kitna smooth badle (sec) — jhatka na lage
        [SerializeField] float defaultFade = 0.12f;     // state badalne par cross-fade (sec)
        [SerializeField] float landFade = 0.08f;        // zameen par girte hi jaldi idle/run — late lagta hai warna

        [Header("Wall-run lean")]
        [SerializeField] float wallLeanAngle = 25f;     // degree — pair deewar par, body deewar se door jhuki
        [SerializeField] float leanLerpSpeed = 10f;

        [Header("Ledge hang")]
        [Tooltip("Hanging Idle clip me haath ledge se mile, iske liye model ko thoda khiskao (Play Mode me tune karo).")]
        [SerializeField] Vector3 hangModelOffset = Vector3.zero;

        PlayerController _pc;
        CombatSystem _combat;        // optional — M3 se pehle wale scene me nahi hota
        BowCombat _bow;              // optional — Dhanush
        Transform _model;            // Animator wala child — lean/offset isi par
        Quaternion _modelBaseRot;    // model ka original local rotation
        Vector3 _modelBasePos;
        float _lean;                 // abhi ka lean angle (smooth)
        bool _rising;                // Airborne me upar ja rahe hain (Jump clip) ya gir rahe (Fall)

        // String ki jagah hash — Update me har frame string compare/alloc nahi (project coding rule).
        static readonly int HLocomotion = Animator.StringToHash(RishiAnim.Locomotion);
        static readonly int HJump = Animator.StringToHash(RishiAnim.Jump);
        static readonly int HFall = Animator.StringToHash(RishiAnim.Fall);
        static readonly int HRoll = Animator.StringToHash(RishiAnim.Roll);
        static readonly int HWallRun = Animator.StringToHash(RishiAnim.WallRun);
        static readonly int HHang = Animator.StringToHash(RishiAnim.Hang);
        static readonly int HClimb = Animator.StringToHash(RishiAnim.Climb);
        static readonly int HAttack1 = Animator.StringToHash(RishiAnim.Attack1);
        static readonly int HAttack2 = Animator.StringToHash(RishiAnim.Attack2);
        static readonly int HAttack3 = Animator.StringToHash(RishiAnim.Attack3);
        static readonly int HHeavyCharge = Animator.StringToHash(RishiAnim.HeavyCharge);
        static readonly int HHeavy = Animator.StringToHash(RishiAnim.Heavy);
        static readonly int HBowShoot = Animator.StringToHash(RishiAnim.BowShoot);
        static readonly int PSpeed = Animator.StringToHash(RishiAnim.SpeedParam);
        static readonly int PRollSpeed = Animator.StringToHash(RishiAnim.RollSpeedParam);
        static readonly int PClimbSpeed = Animator.StringToHash(RishiAnim.ClimbSpeedParam);

        // =================================================================
        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _combat = GetComponent<CombatSystem>();
            _bow = GetComponent<BowCombat>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                // Edge case: capsule placeholder par ye script lagi ho — chup-chaap band, game chalta rahe.
                Debug.LogWarning("[RISHI] PlayerAnimator: koi Animator nahi mila (Mixamo model lagao: Tools ▸ RISHI ▸ Setup Mixamo Rishi).", this);
                enabled = false;
                return;
            }

            // Position sirf PlayerController badlega; Animator ko root hilane ki ijazat nahi.
            animator.applyRootMotion = false;
            _model = animator.transform;
            _modelBaseRot = _model.localRotation;
            _modelBasePos = _model.localPosition;

            MatchClipSpeeds();
        }

        void OnEnable()
        {
            if (_pc != null) _pc.StateChanged += OnStateChanged;
            if (_combat != null) { _combat.AttackStarted += OnAttackStarted; _combat.AttackEnded += OnAttackEnded; }
            if (_bow != null) _bow.Fired += OnBowFired;
        }

        void OnDisable()
        {
            if (_pc != null) _pc.StateChanged -= OnStateChanged;
            if (_combat != null) { _combat.AttackStarted -= OnAttackStarted; _combat.AttackEnded -= OnAttackEnded; }
            if (_bow != null) _bow.Fired -= OnBowFired;
        }

        void Start()
        {
            // Scene shuru hote hi sahi state (Teleport/Airborne se start ho sakta hai).
            OnStateChanged(_pc.State, _pc.State);
        }

        // =================================================================
        void Update()
        {
            // Pause me Animator khud ruk jaata hai (timeScale 0); hum bhi kuch na badlein.
            if (Time.timeScale <= 0f) return;
            float dt = Time.deltaTime;

            // 1) Idle↔Run blend: zameen par horizontal speed / max speed.
            Vector3 v = _pc.Velocity;
            float horiz = new Vector2(v.x, v.z).magnitude;
            float speed01 = _pc.State == MoveState.Grounded ? Mathf.Clamp01(horiz / Mathf.Max(0.01f, _pc.MoveSpeed)) : 0f;
            animator.SetFloat(PSpeed, speed01, speedDamp, dt);

            // 2) Hawa me: upar jaate waqt Jump, chhoti (peak) ke baad Fall.
            if (_pc.State == MoveState.Airborne)
            {
                bool risingNow = v.y > 0.5f;
                if (_rising && !risingNow) animator.CrossFadeInFixedTime(HFall, 0.2f);
                _rising = risingNow;
            }

            // Teer chalane ka clip khatam → wapas idle/run (bow ka "AttackEnded" jaisa event nahi hai).
            AnimatorStateInfo cur = animator.GetCurrentAnimatorStateInfo(0);
            if (cur.shortNameHash == HBowShoot && cur.normalizedTime >= 1f && !animator.IsInTransition(0))
                animator.CrossFadeInFixedTime(HLocomotion, defaultFade);

            // 3) Wall-run lean: deewar daayi taraf ho to body baayi taraf jhuke (pair deewar par).
            float targetLean = 0f;
            if (_pc.State == MoveState.WallRunning)
            {
                float side = Vector3.Dot(_pc.WallNormal, transform.right); // <0 = deewar daayein
                targetLean = -Mathf.Sign(side) * wallLeanAngle;
            }
            _lean = Mathf.Lerp(_lean, targetLean, 1f - Mathf.Exp(-leanLerpSpeed * dt));
            _model.localRotation = _modelBaseRot * Quaternion.Euler(0f, 0f, _lean);

            // 4) Ledge par latakte/chadhte waqt model ko offset do (haath kinare par dikhen).
            bool onLedge = _pc.State == MoveState.LedgeHanging || _pc.State == MoveState.Climbing;
            _model.localPosition = onLedge ? _modelBasePos + hangModelOffset : _modelBasePos;
        }

        // =================================================================
        void OnStateChanged(MoveState prev, MoveState next)
        {
            switch (next)
            {
                case MoveState.Grounded:
                    // Landing par fade chhota — warna pair zameen me "tairte" lagte hain.
                    animator.CrossFadeInFixedTime(HLocomotion, prev == MoveState.Airborne ? landFade : defaultFade);
                    break;

                case MoveState.Airborne:
                    // Jump dabaya (ya wall-jump) → Jump clip; kinare se gire / hit se ledge chhoota → seedha Fall.
                    // Edge case (GDD): climb ke beech hit → DropFromLedge → Airborne → yahan Fall chalega.
                    _rising = _pc.Velocity.y > 0.5f;
                    animator.CrossFadeInFixedTime(_rising ? HJump : HFall, defaultFade);
                    break;

                case MoveState.Dodging:
                    // Roll turant shuru ho — 0.4 s ki dodge me lamba fade roll ko kha jaata.
                    animator.CrossFadeInFixedTime(HRoll, 0.05f, 0, 0f);
                    break;

                case MoveState.WallRunning:
                    animator.CrossFadeInFixedTime(HWallRun, defaultFade);
                    break;

                case MoveState.LedgeHanging:
                    animator.CrossFadeInFixedTime(HHang, 0.08f);
                    break;

                case MoveState.Climbing:
                    // Climb clip shuru se — PlayerController ka arc bhi abhi shuru hua.
                    animator.CrossFadeInFixedTime(HClimb, 0.05f, 0, 0f);
                    break;
            }
        }

        // ---------------- Combat (M3) ----------------
        void OnAttackStarted(AttackKind kind, int step)
        {
            int h = kind == AttackKind.Heavy
                ? (step == 0 ? HHeavyCharge : HHeavy)
                : (step == 1 ? HAttack1 : step == 2 ? HAttack2 : HAttack3);
            // Clip abhi nahi aaya (state hi nahi) to chup — combat logic animation par depend nahi karta.
            if (animator.HasState(0, h)) animator.CrossFadeInFixedTime(h, 0.05f, 0, 0f);
        }

        void OnBowFired(bool charged)
        {
            if (animator.HasState(0, HBowShoot)) animator.CrossFadeInFixedTime(HBowShoot, 0.05f, 0, 0f);
        }

        void OnAttackEnded()
        {
            // Attack khatam/cancel → zameen par ho to wapas idle/run. (Dodge cancel par OnStateChanged Roll chalata hai.)
            // Pehle se Locomotion me ho (attack clip tha hi nahi) to dobara fade nahi — warna idle restart jhatka deta.
            if (_pc.State == MoveState.Grounded && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != HLocomotion)
                animator.CrossFadeInFixedTime(HLocomotion, defaultFade);
        }

        /// <summary>
        /// Mixamo ka roll ~1 s aur climb ~2–3 s ka hota hai, par GDD me dodge 0.4 s hai.
        /// Clip ko state ki length me fit karne ke liye uski playback speed set karo.
        /// Awake me ek baar — animationClips array banata hai, isliye Update me nahi.
        /// </summary>
        void MatchClipSpeeds()
        {
            var rac = animator.runtimeAnimatorController;
            if (rac == null) return;
            foreach (AnimationClip c in rac.animationClips)
            {
                if (c == null || c.length <= 0f) continue;
                if (c.name == "Rishi_" + RishiAnim.Roll)  animator.SetFloat(PRollSpeed,  c.length / Mathf.Max(0.05f, _pc.DodgeDuration));
                if (c.name == "Rishi_" + RishiAnim.Climb) animator.SetFloat(PClimbSpeed, c.length / Mathf.Max(0.05f, _pc.ClimbDuration));
            }
        }
    }
}
