// =====================================================================
// EnemyAI.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: Teeno asur (Chhaya-bhoot, Pashan-rakshas, Vayu-pishach) ka dimaag
//       ek jaisa hai — dekho, peecha karo, maaro, chot khao, maro. Common
//       hissa yahan; har asur sirf apna farak (speed, guard, udna, teer) badalta hai.
// KYA:  State machine (GDD TRD): Idle → Chase → Attack → Hurt → Dead.
//       Attack ke 3 hisse: windup (rang badalta hai = "ab dodge karo" ka ishara)
//       → active (hit) → recovery. Ek saath max 3 asur attack karte hain
//       (attack token) — jhund me Rishi ko lagatar stun-lock na karein.
// KAISE: Zameen wale asur CharacterController se chalte hain (gravity, deewar).
//       NavMesh abhi nahi (M4 level ke saath) — seedha Rishi ki taraf, aur
//       kinare par ruk jaate hain taaki khaai me na girein.
// AAGE: Player ko "Player" tag se dhoondhta hai, uske Hurtbox par vaar.
//       Apne Hurtbox.Hit se knockback/hurt. Died event → M3/M4 me Jyoti drop.
// =====================================================================
using System;
using UnityEngine;
using Rishi.Combat;

namespace Rishi.Enemies
{
    public enum EnemyState { Idle, Chase, Attack, Hurt, Dead }

    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Hurtbox))]
    public abstract class EnemyAI : MonoBehaviour
    {
        [Header("Dekhna")]
        [SerializeField] protected float detectRange = 12f;
        [SerializeField] protected float loseRange = 20f;     // itna door bhaag gaye to peecha chhodo

        [Header("Chalna")]
        [SerializeField] protected float moveSpeed = 4f;
        [SerializeField] protected float turnSpeed = 540f;
        [SerializeField] protected float gravity = -25f;

        [Header("Attack")]
        [SerializeField] protected float attackRange = 1.6f;
        [SerializeField] protected float windup = 0.4f;       // telegraph — isi me player dodge kare
        [SerializeField] protected float activeTime = 0.15f;
        [SerializeField] protected float recovery = 0.5f;     // vaar ke baad khula mauka
        [SerializeField] protected float attackCooldown = 1.0f;
        [SerializeField] protected float damage = 8f;         // GDD numbers subclass me
        [SerializeField] protected float hitRadius = 0.9f;
        [SerializeField] protected float hitReach = 1.0f;

        [Header("Chot")]
        [SerializeField] protected float hurtStun = 0.35f;
        [SerializeField] protected float knockbackDecay = 8f;
        [SerializeField] protected bool superArmor;           // true = normal hit se stun nahi (sirf heavy)

        [Header("Dikhna")]
        [SerializeField] protected Renderer body;
        [SerializeField] Color telegraphColor = new Color(1f, 0.55f, 0.1f); // geru-naranga: "vaar aa raha hai"
        [SerializeField] Color hurtColor = Color.white;

        [Header("Inaam")]
        [SerializeField, Tooltip("GDD: asur maarne par Jyoti milti hai")] protected int jyotiReward = 1;
        public int JyotiReward => jyotiReward;

        // Jhund ka niyam: ek saath kitne asur attack kar sakte hain.
        public static int MaxSimultaneousAttackers = 3;
        static int s_attackers;
        /// <summary>Scene me zinda asur. GDD: ek screen par max 12 (spawner M4 me isko dekhega).</summary>
        public static int ActiveCount { get; private set; }

        public EnemyState State { get; private set; } = EnemyState.Idle;
        public event Action<EnemyAI> Died;
        /// <summary>Koi bhi asur mara — LevelDirector Jyoti ginta hai, EncounterZone gate kholta hai.</summary>
        public static event Action<EnemyAI> AnyDied;
        public event Action<EnemyState, EnemyState> StateChanged;

        protected Health Hp;
        protected Hurtbox Hb;
        protected CharacterController Cc;   // flying asur ke paas nahi hota
        protected Transform Target;
        protected Health TargetHp;
        protected float StateTimer;
        protected Vector3 Desired;          // is frame ki chahi gayi horizontal speed

        Vector3 _knock;
        float _vy;
        bool _hasToken;
        bool _hitDone;
        float _nextAttack;
        float _stunFor;
        float _flashUntil;
        MaterialPropertyBlock _mpb;
        Color _baseColor;
        readonly Collider[] _overlap = new Collider[8];

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        // =================================================================
        protected virtual void Awake()
        {
            Hp = GetComponent<Health>();
            Hb = GetComponent<Hurtbox>();
            Cc = GetComponent<CharacterController>();
            if (body == null) body = GetComponentInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _baseColor = body != null && body.sharedMaterial != null && body.sharedMaterial.HasProperty(BaseColorId)
                ? body.sharedMaterial.GetColor(BaseColorId) : Color.gray;
        }

        protected virtual void OnEnable()
        {
            Hb.Hit += OnHit;
            Hp.Died += OnDied;
            ActiveCount++;
            if (ActiveCount > 12) Debug.LogWarning($"[ASUR] {ActiveCount} asur zinda — GDD budget 12 se zyada (performance).");
        }

        protected virtual void OnDisable()
        {
            Hb.Hit -= OnHit;
            Hp.Died -= OnDied;
            ActiveCount--;
            ReleaseToken(); // edge case: attack ke beech disable hua to token atka na rahe
        }

        protected virtual void Start()
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) { Target = p.transform; TargetHp = p.GetComponent<Health>(); }
        }

        // =================================================================
        protected virtual void Update()
        {
            if (Time.timeScale <= 0f) return; // pause
            float dt = Time.deltaTime;
            StateTimer += dt;
            Desired = Vector3.zero;

            if (State == EnemyState.Dead) { UpdateDead(dt); return; }

            // Rishi mar gaya / scene me nahi → khade raho (game over screen M6).
            if (Target == null || (TargetHp != null && TargetHp.IsDead))
            {
                if (State != EnemyState.Idle && State != EnemyState.Hurt) SetState(EnemyState.Idle);
            }

            switch (State)
            {
                case EnemyState.Idle:   UpdateIdle();      break;
                case EnemyState.Chase:  UpdateChase(dt);   break;
                case EnemyState.Attack: UpdateAttack(dt);  break;
                case EnemyState.Hurt:   if (StateTimer >= _stunFor) SetState(EnemyState.Chase); break;
            }

            _knock = Vector3.Lerp(_knock, Vector3.zero, 1f - Mathf.Exp(-knockbackDecay * dt));
            MoveBody(Desired + _knock, dt);
            UpdateTint();

            // Edge case: khaai me gir gaya (knockback se) — hamesha girta na rahe.
            if (transform.position.y < -20f) Hp.TakeDamage(Hp.Max);
        }

        // ---------------- States ----------------
        void UpdateIdle()
        {
            if (Target == null || (TargetHp != null && TargetHp.IsDead)) return;
            if (FlatDistance(Target.position) <= detectRange) SetState(EnemyState.Chase);
        }

        void UpdateChase(float dt)
        {
            Vector3 to = Target.position - transform.position; to.y = 0f;
            float dist = to.magnitude;
            if (dist > loseRange) { SetState(EnemyState.Idle); return; }

            FaceTowards(to, dt);
            if (dist <= attackRange && Time.time >= _nextAttack && TryTakeToken())
            {
                SetState(EnemyState.Attack);
                return;
            }
            Desired = ChaseVelocity(to, dist);
        }

        void UpdateAttack(float dt)
        {
            // Windup ke pehle aadhe hisse tak Rishi ki taraf ghoomo; uske baad disha lock —
            // isse dodge karke side me nikal sakte ho (fair telegraph).
            if (StateTimer < windup * 0.5f && Target != null)
            {
                Vector3 to = Target.position - transform.position; to.y = 0f;
                FaceTowards(to, dt);
            }

            if (!_hitDone && StateTimer >= windup)
            {
                _hitDone = true;
                PerformAttack();
            }

            if (StateTimer >= windup + activeTime + recovery)
            {
                ReleaseToken();
                _nextAttack = Time.time + attackCooldown;
                SetState(EnemyState.Chase);
            }
        }

        void UpdateDead(float dt)
        {
            // Zameen me dheere dhans jao, phir gayab (M5 me Tamas dhuan VFX).
            if (StateTimer > 0.6f) transform.position += Vector3.down * (1.2f * dt);
            if (StateTimer > 2.0f) gameObject.SetActive(false);
        }

        // ---------------- Overridable ----------------
        /// <summary>Peecha karte waqt kitni speed/disha. Default: seedha Rishi ki taraf.</summary>
        protected virtual Vector3 ChaseVelocity(Vector3 toTarget, float dist)
        {
            return toTarget / Mathf.Max(dist, 0.001f) * moveSpeed;
        }

        /// <summary>Vaar ka active pal. Default: saamne sphere me melee hit.</summary>
        protected virtual void PerformAttack()
        {
            Vector3 center = transform.position + Vector3.up * 1f + transform.forward * hitReach;
            int n = Physics.OverlapSphereNonAlloc(center, hitRadius, _overlap, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < n; i++)
            {
                Hurtbox hb = _overlap[i].GetComponentInParent<Hurtbox>();
                if (hb == null || hb.Team != Team.Player) continue; // apne saathi asur ko nahi
                Vector3 dir = hb.transform.position - transform.position; dir.y = 0f;
                var info = new HitInfo
                {
                    Damage = damage,
                    Direction = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward,
                    Knockback = 0f,
                    AttackerTeam = Team.Asur,
                    Attacker = transform,
                };
                hb.ReceiveHit(info); // dodge ke i-frames me Health khud mana kar deta hai
                return;              // ek vaar = ek Rishi
            }
        }

        /// <summary>Is hit par kitni der stun. Default hurtStun; Pashan guard-break par lamba.</summary>
        protected virtual float StunDuration(in HitInfo info) => hurtStun;

        /// <summary>Zameen wale: CharacterController + gravity + kinara check. Flying override karta hai.</summary>
        protected virtual void MoveBody(Vector3 horizontal, float dt)
        {
            if (Cc == null || !Cc.enabled) return;

            // Edge case: aage zameen nahi (khaai/kinara) → chalna band (knockback se gir sakte hain, wo theek).
            Vector3 flat = new Vector3(Desired.x, 0f, Desired.z);
            if (flat.sqrMagnitude > 0.01f && !GroundAhead(flat.normalized))
                horizontal -= flat;

            _vy = Cc.isGrounded ? -2f : _vy + gravity * dt;
            Cc.Move(new Vector3(horizontal.x, _vy, horizontal.z) * dt);
        }

        // ---------------- Hit / death ----------------
        void OnHit(HitInfo info)
        {
            // Edge case: Health.Died, Hit se PEHLE chalta hai (Hurtbox order) — maarne wala hit Dead ko Hurt na bana de.
            if (State == EnemyState.Dead) return;
            _knock = info.Direction * info.Knockback;
            _flashUntil = Time.time + 0.12f;
            Rishi.Core.GameAudio.Play(Rishi.Core.Sfx.Hit, transform.position, info.IsFinisher || info.GuardBreak ? 1f : 0.75f);

            // Maar khaayi to Rishi ko dekh liya (peeche se teer aaya ho tab bhi).
            if (State == EnemyState.Idle) SetState(EnemyState.Chase);

            // Super armor (Pashan): sirf guard-break / heavy hit se rukta hai.
            if (superArmor && !info.GuardBreak) return;

            ReleaseToken(); // attack toota — token wapas
            _stunFor = StunDuration(info);
            SetState(EnemyState.Hurt);
        }

        void OnDied()
        {
            ReleaseToken();
            SetState(EnemyState.Dead);
            if (Cc != null) Cc.enabled = false;
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false; // teer/hitbox ab isse na takraayein
            Debug.Log($"[ASUR] {name} haara.");
            Rishi.Core.GameAudio.Play(Rishi.Core.Sfx.Death, transform.position);
            Died?.Invoke(this);
            AnyDied?.Invoke(this);
        }

        // ---------------- Helpers ----------------
        protected void SetState(EnemyState next)
        {
            if (next == State) return;
            EnemyState prev = State;
            State = next;
            StateTimer = 0f;
            if (next == EnemyState.Attack) _hitDone = false;
            StateChanged?.Invoke(prev, next);
        }

        bool TryTakeToken()
        {
            if (_hasToken) return true;
            if (s_attackers >= MaxSimultaneousAttackers) return false; // baaki ghere me intezaar karein
            s_attackers++;
            _hasToken = true;
            return true;
        }

        void ReleaseToken()
        {
            if (!_hasToken) return;
            _hasToken = false;
            s_attackers = Mathf.Max(0, s_attackers - 1);
        }

        protected void FaceTowards(Vector3 dir, float dt)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * dt);
        }

        protected float FlatDistance(Vector3 p)
        {
            Vector3 d = p - transform.position; d.y = 0f;
            return d.magnitude;
        }

        bool GroundAhead(Vector3 dir)
        {
            Vector3 probe = transform.position + dir * 0.8f + Vector3.up * 0.5f;
            return Physics.Raycast(probe, Vector3.down, 2.0f, ~0, QueryTriggerInteraction.Ignore);
        }

        void UpdateTint()
        {
            if (body == null) return;
            Color c = _baseColor;
            if (Time.time < _flashUntil) c = hurtColor;
            else if (State == EnemyState.Attack && StateTimer < windup)
                c = Color.Lerp(_baseColor, telegraphColor, StateTimer / Mathf.Max(windup, 0.01f)); // dheere garam hota rang = vaar aa raha
            else if (State == EnemyState.Dead) c = _baseColor * 0.3f;
            _mpb.SetColor(BaseColorId, c);
            body.SetPropertyBlock(_mpb);
        }

        /// <summary>Flash dikhana (jaise guard "clang") — subclass ke liye.</summary>
        protected void Flash(float seconds) => _flashUntil = Time.time + seconds;

        // Domain reload band ho (Enter Play Mode settings) to static counter purane run se na bache.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { s_attackers = 0; ActiveCount = 0; AnyDied = null; }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange);
            Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position + Vector3.up + transform.forward * hitReach, hitRadius);
        }
#endif
    }
}
