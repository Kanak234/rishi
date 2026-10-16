// =====================================================================
// CombatSystem.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD ka "bahta hua combat" — 3-hit light combo, charge wala heavy
//       vaar, aur dodge se attack cancel. Ye Rishi ka asli gameplay hai.
// KYA:  Ek chhoti state machine: Idle → Light(1/2/3) / HeavyCharge → HeavyStrike.
//       Har attack ke 3 hisse: windup (tayyari) → active (hitbox on) →
//       recovery (wapas). Active ke time Rishi ke saamne ek sphere (hitbox)
//       me Hurtbox dhoondhte hain — har swing me ek target ek hi baar.
//       Input buffer: attack ke beech dabaya light agla hit ban jaata hai.
// KAISE: Hitbox = Physics.OverlapSphereNonAlloc (pehle se bana array) —
//       Update me koi allocation nahi.
// AAGE: RishiInput se LightAttack/HeavyAttack padhta hai, PlayerController ke
//       MoveScale/ExtraVelocity/JumpLocked se attack ke time chalna rokta hai,
//       Hurtbox.ReceiveHit() se damage deta hai. Events (AttackStarted, HitLanded)
//       PlayerAnimator (attack clips) aur aage HUD/SFX ke liye.
//       WeaponManager (agla M3 task) Trishul/Dhanush ke hisaab se AttackData badlega.
// GDD: light combo 3 hit, teesra dhakelta hai; heavy charge, guard todta hai;
//      dodge (0.4 s i-frames) kabhi bhi attack cancel kar sakta hai.
// =====================================================================
using System;
using UnityEngine;
using Rishi.Core;
using Rishi.Player;

namespace Rishi.Combat
{
    public enum AttackKind { None, Light, Heavy }

    /// <summary>Ek vaar ki timing aur hitbox. Inspector me tune karo.</summary>
    [Serializable]
    public struct AttackData
    {
        public float duration;      // poora attack (sec)
        public float activeStart;   // hitbox kab on (sec, attack shuru se)
        public float activeEnd;     // hitbox kab off
        public float damage;
        public float knockback;     // target ko dhakka (m/s)
        public float radius;        // hitbox sphere ka size
        public float reach;         // Rishi ke saamne kitni door sphere ka center
        public float lunge;         // windup+active me aage khisakne ki speed (m/s)

        public AttackData(float duration, float activeStart, float activeEnd, float damage,
                          float knockback, float radius, float reach, float lunge)
        {
            this.duration = duration; this.activeStart = activeStart; this.activeEnd = activeEnd;
            this.damage = damage; this.knockback = knockback; this.radius = radius;
            this.reach = reach; this.lunge = lunge;
        }
    }

    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(RishiInput))]
    [RequireComponent(typeof(Health))]
    public class CombatSystem : MonoBehaviour
    {
        // ---------------- Tuning ----------------
        // Damage numbers GDD me nahi the — M3 me tay kiye (GDD "Combat" section me likhe hain):
        // Chhaya-bhoot ko 3-hit combo (10+10+18 = 38) se ~2 combo me maarna.
        [Header("Light combo (GDD: 3 hit, teesra dhakelta hai)")]
        [SerializeField] AttackData[] lightCombo =
        {
            new AttackData(0.42f, 0.12f, 0.24f, 10f, 1.5f, 0.9f, 1.0f, 3.0f),
            new AttackData(0.42f, 0.12f, 0.24f, 10f, 1.5f, 0.9f, 1.0f, 3.0f),
            new AttackData(0.60f, 0.20f, 0.34f, 18f, 7.0f, 1.1f, 1.1f, 4.5f), // finisher: bada dhakka
        };
        [SerializeField] float comboBufferWindow = 0.25f; // attack ke aakhri itne sec me dabaya = agla hit pakka
        [SerializeField] float comboResetTime = 0.35f;    // attack khatam hone ke baad itni der me dabao to combo chalta rahe

        [Header("Heavy (GDD: charge karke, guard todta hai)")]
        [SerializeField] AttackData heavy = new AttackData(0.65f, 0.18f, 0.34f, 25f, 6f, 1.2f, 1.2f, 5f);
        [SerializeField] float heavyMaxDamage = 45f;     // full charge par
        [SerializeField] float heavyMinCharge = 0.25f;   // isse jaldi chhoda to bhi itna charge maana jayega
        [SerializeField] float heavyMaxCharge = 1.0f;    // isse zyada hold = apne aap vaar
        [SerializeField] float chargeMoveScale = 0f;     // charge ke time chal sakte ho? (0 = jagah par)

        [SerializeField] LayerMask hurtboxMask = ~0;

        // ---------------- Public read-only ----------------
        public AttackKind CurrentKind => _kind;
        public int ComboStep => _comboIndex + 1;          // 1..3 (light ke liye)
        public bool IsCharging => _charging;
        public float Charge01 => _charging ? Mathf.Clamp01(_timer / heavyMaxCharge) : 0f;
        public bool HitboxActive { get; private set; }

        public event Action<AttackKind, int> AttackStarted; // (kind, combo step 1..3) — PlayerAnimator
        public event Action<Hurtbox, HitInfo> HitLanded;      // HUD/SFX/hit-stop ke liye
        public event Action AttackEnded;

        // ---------------- Internal ----------------
        PlayerController _pc;
        RishiInput _input;
        Health _health;

        AttackKind _kind = AttackKind.None;
        AttackData _current;
        int _comboIndex = -1;       // abhi kaunsa light hit (0..2); -1 = combo shuru nahi
        float _timer;               // current attack / charge me kitna time
        float _sinceAttackEnd = 999f;
        bool _bufferedLight;
        bool _charging;
        float _heavyDamage;

        // Pehle se bane arrays — har frame naya nahi (allocation rule).
        readonly Collider[] _overlap = new Collider[16];
        readonly Hurtbox[] _alreadyHit = new Hurtbox[16];
        int _alreadyHitCount;

        // =================================================================
        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _input = GetComponent<RishiInput>();
            _health = GetComponent<Health>();
        }

        void OnEnable()
        {
            _pc.StateChanged += OnMoveStateChanged;
            _health.Died += CancelAttack;
        }

        void OnDisable()
        {
            _pc.StateChanged -= OnMoveStateChanged;
            _health.Died -= CancelAttack;
            CancelAttack(); // component band ho to Rishi atka na rahe (MoveScale 0)
        }

        // =================================================================
        void Update()
        {
            // Pause (timeScale 0) me kuch nahi — resume par attack wahi se chale.
            if (Time.timeScale <= 0f || _health.IsDead) return;
            float dt = Time.deltaTime;

            bool lightPressed = _input.LightAttack.WasPressedThisFrame();
            bool heavyPressed = _input.HeavyAttack.WasPressedThisFrame();
            bool canStart = _pc.State == MoveState.Grounded; // hawa / ledge / dodge me attack nahi (air attack M3 ke scope me nahi)

            if (_charging) { UpdateCharge(dt); return; }

            if (_kind == AttackKind.None)
            {
                _sinceAttackEnd += dt;
                if (!canStart) return;
                if (heavyPressed) { StartCharge(); return; }
                if (lightPressed)
                {
                    // Combo chalu rakhna: pichhla hit abhi abhi khatam hua ho to agla step, warna 1 se.
                    int next = (_sinceAttackEnd <= comboResetTime && _comboIndex >= 0 && _comboIndex < lightCombo.Length - 1)
                        ? _comboIndex + 1 : 0;
                    StartLight(next);
                }
                return;
            }

            // --- Attack chal raha hai ---
            float prevTimer = _timer;
            _timer += dt;

            // Input buffer: aakhri hisse me light dabaya = agla hit yaad rakho.
            if (lightPressed && _kind == AttackKind.Light && _timer >= _current.duration - comboBufferWindow)
                _bufferedLight = true;

            // Lunge sirf active khatam hone tak — uske baad Rishi ruk jaye (recovery).
            _pc.ExtraVelocity = _timer < _current.activeEnd ? transform.forward * _current.lunge : Vector3.zero;

            // Hitbox window. Edge case: lamba frame (lag spike) poora window (0.12 s) laangh sakta hai —
            // isliye "pichhle frame se is frame ke beech window aaya?" check, sirf "abhi window me?" nahi.
            HitboxActive = _timer >= _current.activeStart && prevTimer <= _current.activeEnd;
            if (HitboxActive) DoHitboxCheck();
            if (_timer > _current.activeEnd) HitboxActive = false; // HUD/gizmo ke liye sahi dikhe

            if (_timer >= _current.duration)
            {
                bool chain = _bufferedLight && _kind == AttackKind.Light && _comboIndex < lightCombo.Length - 1 && canStart;
                int nextIndex = _comboIndex + 1;
                FinishAttack();
                if (chain) StartLight(nextIndex);
                else if (_kind == AttackKind.None && _comboIndex >= lightCombo.Length - 1) _comboIndex = -1; // teesre ke baad naya combo
            }
        }

        // ---------------- Light ----------------
        void StartLight(int index)
        {
            _comboIndex = index;
            _current = lightCombo[index];
            BeginAttack(AttackKind.Light);
            AttackStarted?.Invoke(AttackKind.Light, index + 1);
        }

        // ---------------- Heavy ----------------
        void StartCharge()
        {
            _charging = true;
            _timer = 0f;
            _comboIndex = -1; // heavy combo tod deta hai
            FaceInputDirection();
            _pc.MoveScale = chargeMoveScale;
            _pc.JumpLocked = true;
            AttackStarted?.Invoke(AttackKind.Heavy, 0); // 0 = charge shuru (animator charge pose)
        }

        void UpdateCharge(float dt)
        {
            _timer += dt;
            bool released = !_input.HeavyAttack.IsPressed();
            if (!released && _timer < heavyMaxCharge) return;

            // Charge → damage: min charge se kam par bhi base damage (jaldi tap = halka heavy).
            float t = Mathf.InverseLerp(heavyMinCharge, heavyMaxCharge, Mathf.Max(_timer, heavyMinCharge));
            _heavyDamage = Mathf.Lerp(heavy.damage, heavyMaxDamage, t);
            _charging = false;
            _current = heavy;
            BeginAttack(AttackKind.Heavy);
            AttackStarted?.Invoke(AttackKind.Heavy, 1); // 1 = strike
        }

        // ---------------- Common ----------------
        void BeginAttack(AttackKind kind)
        {
            _kind = kind;
            _timer = 0f;
            _bufferedLight = false;
            _alreadyHitCount = 0;
            FaceInputDirection();       // swing us taraf jidhar stick/WASD hai — combo me disha badal sakte ho
            _pc.MoveScale = 0f;         // attack me khade (lunge alag se)
            _pc.JumpLocked = true;
        }

        void FinishAttack()
        {
            _kind = AttackKind.None;
            HitboxActive = false;
            _sinceAttackEnd = 0f;
            _pc.MoveScale = 1f;
            _pc.ExtraVelocity = Vector3.zero;
            _pc.JumpLocked = false;
            AttackEnded?.Invoke();
        }

        /// <summary>Dodge / girna / maut — attack turant band, movement wapas normal.</summary>
        public void CancelAttack()
        {
            bool wasBusy = _kind != AttackKind.None || _charging;
            _charging = false;
            _comboIndex = -1;
            if (_pc != null)
            {
                _pc.MoveScale = 1f;
                _pc.ExtraVelocity = Vector3.zero;
                _pc.JumpLocked = false;
            }
            _kind = AttackKind.None;
            HitboxActive = false;
            _sinceAttackEnd = 999f;
            if (wasBusy) AttackEnded?.Invoke();
        }

        void OnMoveStateChanged(MoveState prev, MoveState next)
        {
            // GDD: dodge se attack cancel. Kinare se gir gaye / teleport hue to bhi cancel.
            if (next != MoveState.Grounded) CancelAttack();
        }

        void FaceInputDirection()
        {
            // Camera ke hisaab se WASD — PlayerController jaisa hi.
            Vector2 m = _input.MoveValue;
            if (m.sqrMagnitude < 0.04f) return; // input nahi to jidhar dekh rahe the wahi
            Transform cam = Camera.main != null ? Camera.main.transform : null;
            Vector3 f = cam ? cam.forward : Vector3.forward;
            Vector3 r = cam ? cam.right : Vector3.right;
            f.y = 0f; r.y = 0f;
            Vector3 dir = f.normalized * m.y + r.normalized * m.x;
            if (dir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(dir);
        }

        // ---------------- Hitbox ----------------
        Vector3 HitboxCenter => transform.position + Vector3.up * 1.0f + transform.forward * _current.reach;

        void DoHitboxCheck()
        {
            int n = Physics.OverlapSphereNonAlloc(HitboxCenter, _current.radius, _overlap, hurtboxMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < n; i++)
            {
                Collider col = _overlap[i];
                if (!col.TryGetComponent(out Hurtbox hb)) hb = col.GetComponentInParent<Hurtbox>();
                if (hb == null || WasHit(hb)) continue;     // ek swing me ek target ek hi baar

                if (_alreadyHitCount < _alreadyHit.Length) _alreadyHit[_alreadyHitCount++] = hb;

                Vector3 dir = hb.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;

                bool finisher = _kind == AttackKind.Light && _comboIndex == lightCombo.Length - 1;
                var info = new HitInfo
                {
                    Damage = _kind == AttackKind.Heavy ? _heavyDamage : _current.damage,
                    Direction = dir.normalized,
                    Knockback = _current.knockback,
                    GuardBreak = _kind == AttackKind.Heavy,
                    IsFinisher = finisher,
                    AttackerTeam = Team.Player,
                    Attacker = transform,
                };
                if (hb.ReceiveHit(info)) HitLanded?.Invoke(hb, info);
            }
        }

        bool WasHit(Hurtbox hb)
        {
            for (int i = 0; i < _alreadyHitCount; i++) if (_alreadyHit[i] == hb) return true;
            return false;
        }

#if UNITY_EDITOR
        // Scene view me hitbox dikhe: laal = active, peela = attack chal raha par hitbox off.
        void OnDrawGizmos()
        {
            if (_kind == AttackKind.None) return;
            Gizmos.color = HitboxActive ? new Color(1f, 0.2f, 0.1f, 0.6f) : new Color(1f, 0.85f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(HitboxCenter, _current.radius);
        }
#endif
    }
}
