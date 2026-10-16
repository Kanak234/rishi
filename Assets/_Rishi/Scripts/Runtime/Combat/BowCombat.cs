// =====================================================================
// BowCombat.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD: Dhanush — door ke dushman (Vayu-pishach udte hain, paas nahi
//       aate) aur switch ke liye. Isliye melee CombatSystem se alag script:
//       aim, charge aur teer ka pool.
// KYA:  Dhanush equip ho tab on (WeaponManager karta hai).
//         Aim (Ctrl / LT hold)  → dheere chalo, mouse / stick ki disha me nishana
//         Light (LMB / X)       → jaldi teer (bina aim ke: saamne ke asur par auto-aim)
//         Heavy hold (RMB / Y)  → charge, chhodo to bhaari teer
//       Dodge / girna aim aur charge cancel karta hai.
// AAGE: RishiInput se input, PlayerController ke MoveScale/LockFacing/JumpLocked
//       se chalna, Arrow pool se teer, Arrow → Hurtbox.ReceiveHit().
//       Fired/AimChanged events PlayerAnimator (bow clips) ke liye.
// =====================================================================
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Rishi.Core;
using Rishi.Player;

namespace Rishi.Combat
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(RishiInput))]
    [RequireComponent(typeof(Health))]
    public class BowCombat : MonoBehaviour
    {
        // Damage GDD me nahi tha — M3 me tay (GDD Combat section me likha): Vayu-pishach ko ~3 teer.
        [Header("Teer")]
        [SerializeField] float quickDamage = 12f;
        [SerializeField] float chargedMinDamage = 16f;
        [SerializeField] float chargedMaxDamage = 32f;
        [SerializeField] float chargeTime = 0.8f;      // itna hold = full charge
        [SerializeField] float fireCooldown = 0.35f;   // do teer ke beech kam se kam
        [SerializeField] float arrowSpeed = 30f;
        [SerializeField] float arrowLifetime = 1.2f;   // ~35 m range
        [SerializeField] float chargedKnockback = 4f;

        [Header("Aim")]
        [SerializeField] float aimMoveScale = 0.4f;    // aim me dheere chalo (strafe)
        [SerializeField] float autoAimRange = 16f;
        [SerializeField] float autoAimAngle = 35f;     // bina aim ke: saamne itne degree ke cone me asur dhoondho
        [SerializeField] float muzzleHeight = 1.4f;    // kandhe ki unchai
        [SerializeField] float muzzleForward = 0.6f;   // CharacterController (radius 0.4) ke bahar se teer nikle

        [Header("Pool")]
        [SerializeField] int poolSize = 10;
        [SerializeField] Material arrowMaterial;
        [SerializeField] LayerMask hitMask = ~0;

        public bool IsAiming { get; private set; }
        public bool IsCharging => _charging;
        public float Charge01 => _charging ? Mathf.Clamp01(_chargeTimer / chargeTime) : 0f;
        public bool IsBusy => IsAiming || _charging;
        public float DamageMultiplier { get; set; } = 1f;   // WeaponManager upgrade

        public event Action<bool> Fired;        // (charged?) — animator / SFX
        public event Action<bool> AimChanged;   // aim on/off

        PlayerController _pc;
        RishiInput _input;
        Health _health;
        Arrow[] _pool;
        Transform _poolRoot;
        float _nextFire;
        bool _charging;
        float _chargeTimer;
        Vector3 _aimDir;

        readonly Collider[] _overlap = new Collider[24];

        // =================================================================
        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _input = GetComponent<RishiInput>();
            _health = GetComponent<Health>();
            BuildPool();
        }

        void OnEnable()  { _pc.StateChanged += OnMoveStateChanged; _health.Died += ResetBow; }
        void OnDisable() { _pc.StateChanged -= OnMoveStateChanged; _health.Died -= ResetBow; ResetBow(); }
        void OnDestroy() { if (_poolRoot != null) Destroy(_poolRoot.gameObject); }

        void BuildPool()
        {
            // Pool Rishi ka child nahi — warna Rishi ghoome to udte teer bhi ghoomte.
            _poolRoot = new GameObject("ArrowPool (Rishi)").transform;
            _pool = new Arrow[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                // Placeholder teer: patla lamba cube (M4 me asli model).
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Arrow_" + i;
                Destroy(go.GetComponent<Collider>()); // teer khud collider nahi — ray se hit dhoondhta hai
                go.transform.SetParent(_poolRoot, false);
                go.transform.localScale = new Vector3(0.04f, 0.04f, 0.7f);
                if (arrowMaterial != null) go.GetComponent<MeshRenderer>().sharedMaterial = arrowMaterial;
                go.SetActive(false);
                _pool[i] = go.AddComponent<Arrow>();
            }
        }

        // =================================================================
        void Update()
        {
            if (Time.timeScale <= 0f || _health.IsDead) return;
            bool grounded = _pc.State == MoveState.Grounded;

            // --- Aim ---
            bool wantAim = grounded && _input.Aim.IsPressed();
            if (wantAim != IsAiming) SetAiming(wantAim);
            // Sirf aim me cursor/stick se nishana; bina aim ke charge me start wala auto-aim hi rahe.
            if (IsAiming) UpdateAimDirection();

            // --- Charge (heavy) ---
            if (_charging)
            {
                _chargeTimer += Time.deltaTime;
                if (!_input.HeavyAttack.IsPressed())
                {
                    float t = Mathf.Clamp01(_chargeTimer / chargeTime);
                    Shoot(Mathf.Lerp(chargedMinDamage, chargedMaxDamage, t), t >= 1f ? chargedKnockback : 0f, true);
                    EndCharge();
                }
                return;
            }

            if (!grounded) return; // hawa / ledge / dodge me teer nahi (slice scope)

            if (_input.HeavyAttack.WasPressedThisFrame() && Time.time >= _nextFire)
            {
                _charging = true;
                _chargeTimer = 0f;
                ApplyMoveLock();
                if (!IsAiming) _aimDir = AutoAimDirection();
                return;
            }

            if (_input.LightAttack.WasPressedThisFrame() && Time.time >= _nextFire)
            {
                if (!IsAiming) _aimDir = AutoAimDirection();
                Shoot(quickDamage, 0f, false);
            }
        }

        void SetAiming(bool on)
        {
            IsAiming = on;
            if (on) ApplyMoveLock();
            else if (!_charging) ReleaseMoveLock();
            AimChanged?.Invoke(on);
        }

        void ApplyMoveLock()
        {
            _pc.MoveScale = aimMoveScale;
            _pc.LockFacing = true;      // ghoomna hum tay karenge (nishane ki taraf), chalna strafe
            _pc.JumpLocked = true;
        }

        void ReleaseMoveLock()
        {
            _pc.MoveScale = 1f;
            _pc.LockFacing = false;
            _pc.JumpLocked = false;
        }

        void EndCharge()
        {
            _charging = false;
            _chargeTimer = 0f;
            if (!IsAiming) ReleaseMoveLock();
        }

        void ResetBow()
        {
            bool wasAiming = IsAiming;
            _charging = false;
            IsAiming = false;
            if (_pc != null) ReleaseMoveLock();
            if (wasAiming) AimChanged?.Invoke(false);
        }

        void OnMoveStateChanged(MoveState prev, MoveState next)
        {
            // Dodge / gir gaye → aim aur charge band (dodge hamesha jeetta hai, GDD).
            if (next != MoveState.Grounded) ResetBow();
        }

        // ---------------- Aim direction ----------------
        void UpdateAimDirection()
        {
            Vector3 dir = Vector3.zero;
            Camera cam = Camera.main;

            // Mouse: cursor jahan zameen (Rishi ki kandhe wali unchai) ko chhuye, udhar nishana.
            Mouse mouse = Mouse.current;
            bool padActive = Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.04f;
            if (!padActive && mouse != null && cam != null)
            {
                Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
                var plane = new Plane(Vector3.up, transform.position + Vector3.up * muzzleHeight);
                if (plane.Raycast(ray, out float enter)) dir = ray.GetPoint(enter) - transform.position;
            }
            else
            {
                // Gamepad: left stick ki disha (camera ke hisaab se).
                Vector2 m = _input.MoveValue;
                if (m.sqrMagnitude > 0.04f && cam != null)
                {
                    Vector3 f = cam.transform.forward; f.y = 0f;
                    Vector3 r = cam.transform.right; r.y = 0f;
                    dir = f.normalized * m.y + r.normalized * m.x;
                }
            }

            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                _aimDir = dir.normalized;
                transform.rotation = Quaternion.LookRotation(_aimDir);
            }
            else if (_aimDir.sqrMagnitude < 0.01f) _aimDir = transform.forward;
        }

        /// <summary>Bina aim ke: saamne ke cone me sabse paas ka asur. Koi nahi to seedha saamne.</summary>
        Vector3 AutoAimDirection()
        {
            Vector3 fwd = transform.forward;
            Vector3 origin = transform.position;
            int n = Physics.OverlapSphereNonAlloc(origin, autoAimRange, _overlap, hitMask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            Vector3 bestDir = fwd;
            float cosLimit = Mathf.Cos(autoAimAngle * Mathf.Deg2Rad);
            for (int i = 0; i < n; i++)
            {
                Hurtbox hb = _overlap[i].GetComponentInParent<Hurtbox>();
                if (hb == null || hb.Team == Team.Player || (hb.Health != null && hb.Health.IsDead)) continue;
                Vector3 to = _overlap[i].bounds.center - origin;
                to.y = 0f;
                float d = to.magnitude;
                if (d < 0.01f || Vector3.Dot(to / d, fwd) < cosLimit) continue;
                if (d < best) { best = d; bestDir = to / d; }
            }
            if (best < float.MaxValue) transform.rotation = Quaternion.LookRotation(bestDir);
            return bestDir;
        }

        // ---------------- Fire ----------------
        void Shoot(float damage, float knockback, bool charged)
        {
            Arrow a = GetFreeArrow();
            if (a == null) return; // edge case: pool khatam (bahut tez spam) — shot skip, crash nahi

            Vector3 dir = _aimDir.sqrMagnitude > 0.01f ? _aimDir : transform.forward;
            Vector3 muzzle = transform.position + Vector3.up * muzzleHeight + dir * muzzleForward;
            var info = new HitInfo
            {
                Damage = damage * DamageMultiplier,
                Direction = dir,
                Knockback = knockback,
                GuardBreak = false,       // guard sirf heavy melee todta hai (GDD)
                IsFinisher = false,
                AttackerTeam = Team.Player,
                Attacker = transform,
            };
            a.Launch(muzzle, dir, arrowSpeed, arrowLifetime, info, hitMask);
            _nextFire = Time.time + fireCooldown;
            Fired?.Invoke(charged);
        }

        Arrow GetFreeArrow()
        {
            for (int i = 0; i < _pool.Length; i++) if (!_pool[i].InUse) return _pool[i];
            return null;
        }
    }
}
