// =====================================================================
// BossAndhvir.cs  —  RISHI: The Tamas War   (Milestone M5)
// ---------------------------------------------------------------------
// KYUN: GDD boss: Andhvir, Tamasur ka senapati, bhari gada wala. 3 phase:
//       (1) gada ke ghoomte vaar — dodge sikhata hai;
//       (2) zameen todta hai aur Chhaya-bhoot bulata hai;
//       (3) health 30% par gussa, tez attacks.
//       "Har phase me ek khula mauka jab heavy attack zyada damage karta hai."
// KYA:  EnemyAI ki state machine (chase → attack → recovery) wahi; har
//       attack se pehle phase ke hisaab se agla vaar chunta hai:
//         Prahar (saamne gada), Chakkar (360° ghoomna), Bhoomi-bhed (slam + shockwave).
//       Zameen par laal ghera = "yahan vaar girega" (windup me badhta hai).
//       Chakkar/Bhoomi-bhed ke baad recovery = khula mauka: heavy x2, light x1.25.
//       Super armor — light combo se nahi rukta, khule mauke me heavy se stagger.
//       Yahi class teeno boss chalati hai (Andhvir, Shilaasur, Tamasur) —
//       naam, jaan, ghere ka naap aur bulaye asur builder me alag.
// AAGE: EncounterZone boss ko jagata hai. Hud BossHealth padhta hai.
//       LevelDirector Died sun ke jeet ka cutscene chalata hai.
// =====================================================================
using System;
using UnityEngine;
using Rishi.Combat;
using Rishi.Core;
using Rishi.CameraSystem;

namespace Rishi.Enemies
{
    public class BossAndhvir : EnemyAI, IHitFilter
    {
        enum Move { Prahar, Chakkar, BhoomiBhed }

        [Header("Senapati")]
        [SerializeField] string displayName = "Andhvir";
        [SerializeField] string subtitle = "Tamasur ka senapati";
        [SerializeField] float phase2At = 0.66f;          // HP fraction
        [SerializeField] float phase3At = 0.30f;          // GDD: 30% par gussa
        [SerializeField] float spinRadius = 3.4f;
        [SerializeField] float slamRadius = 5.5f;
        [SerializeField] float openingHeavyMult = 2.0f;   // khule mauke me heavy
        [SerializeField] float openingLightMult = 1.25f;
        [SerializeField] GameObject minionTemplate;       // Chhaya-bhoot (inactive) — phase 2 me bulata hai
        [SerializeField] Transform[] minionSpawns;
        [SerializeField] Renderer telegraphDisc;          // zameen par laal ghera
        [SerializeField] int maxMinions = 3;              // GDD performance: screen par 12 se kam

        public string DisplayName => displayName;
        public string Subtitle => subtitle;
        public int Phase { get; private set; } = 1;
        public bool InOpening { get; private set; }
        public event Action<int> PhaseChanged;

        Move _move;
        int _attackCount;
        float _baseMove, _baseWindupScale = 1f;
        readonly Collider[] _hits = new Collider[16];
        readonly GameObject[] _minions = new GameObject[6];

        void Reset()
        {
            detectRange = 30f; loseRange = 60f; moveSpeed = 3.2f; turnSpeed = 240f;
            attackRange = 2.6f; attackCooldown = 0.6f;
            damage = 15f; hitRadius = 1.6f; hitReach = 1.8f;
            hurtStun = 0.6f; superArmor = true; knockbackDecay = 16f;
            jyotiReward = 0;
        }

        protected override void Awake()
        {
            base.Awake();
            _baseMove = moveSpeed;
            StateChanged += OnStateChanged;
            if (telegraphDisc != null) telegraphDisc.enabled = false;
        }

        void OnDestroy() => StateChanged -= OnStateChanged;

        protected override void Update()
        {
            base.Update();
            UpdatePhase();
            UpdateTelegraph();
        }

        // ---------------- Phase ----------------
        void UpdatePhase()
        {
            if (Hp.IsDead) return;
            float f = Hp.Current / Hp.Max;
            int want = f <= phase3At ? 3 : f <= phase2At ? 2 : 1;
            if (want <= Phase) return;
            Phase = want;
            if (Phase == 3)
            {
                // Gussa: tez chaal, chhota windup (par telegraph abhi bhi dikhe — fair rahe)
                moveSpeed = _baseMove * 1.45f;
                _baseWindupScale = 0.7f;
                attackCooldown = 0.25f;
            }
            IsoCameraFollow.Shake(0.35f, 0.5f);
            GameAudio.Play(Sfx.Boom, transform.position);
            Debug.Log($"[BOSS] Andhvir phase {Phase}");
            PhaseChanged?.Invoke(Phase);
        }

        // ---------------- Attack chunna ----------------
        void OnStateChanged(EnemyState from, EnemyState to)
        {
            if (to == EnemyState.Attack) ChooseMove();
            // Khula mauka: Chakkar / Bhoomi-bhed ki recovery tak; attack khatam = band.
            if (from == EnemyState.Attack) InOpening = false;
            if (to == EnemyState.Dead)
            {
                if (telegraphDisc != null) telegraphDisc.enabled = false;
                // Boss haara — uske bulaye bhoot bhi khatam (jeet ke baad khaali arena me na maarein).
                for (int i = 0; i < _minions.Length; i++)
                {
                    if (_minions[i] == null || !_minions[i].activeSelf) continue;
                    var h = _minions[i].GetComponent<Health>();
                    if (h != null) h.TakeDamage(h.Max * 10f);
                }
            }
        }

        void ChooseMove()
        {
            _attackCount++;
            // Pattern: 1 = Prahar, Prahar, Chakkar…  2 = Prahar, Chakkar, Bhoomi-bhed…  3 = sab, tez
            if (Phase == 1) _move = _attackCount % 3 == 0 ? Move.Chakkar : Move.Prahar;
            else if (Phase == 2) _move = (_attackCount % 3) switch { 0 => Move.BhoomiBhed, 1 => Move.Prahar, _ => Move.Chakkar };
            else _move = (_attackCount % 2 == 0) ? Move.BhoomiBhed : Move.Chakkar;

            float k = _baseWindupScale;
            switch (_move)
            {
                case Move.Prahar:     windup = 0.65f * k; activeTime = 0.12f; recovery = 0.55f; damage = 15f; break;
                case Move.Chakkar:    windup = 0.95f * k; activeTime = 0.2f;  recovery = 1.3f;  damage = 18f; break;
                case Move.BhoomiBhed: windup = 1.15f * k; activeTime = 0.1f;  recovery = 2.0f;  damage = 20f; break;
            }
        }

        protected override void PerformAttack()
        {
            switch (_move)
            {
                case Move.Prahar:
                    base.PerformAttack();
                    GameAudio.Play(Sfx.Swoosh, transform.position);
                    break;
                case Move.Chakkar:
                    AreaHit(transform.position, spinRadius, knockback: 6f);
                    GameAudio.Play(Sfx.Swoosh, transform.position, 1f);
                    IsoCameraFollow.Shake(0.15f, 0.25f);
                    InOpening = true;
                    break;
                case Move.BhoomiBhed:
                    AreaHit(transform.position, slamRadius, knockback: 9f);
                    GameAudio.Play(Sfx.Boom, transform.position);
                    IsoCameraFollow.Shake(0.45f, 0.45f);
                    InOpening = true;
                    if (Phase >= 2) SummonMinions();
                    break;
            }
        }

        void AreaHit(Vector3 center, float radius, float knockback)
        {
            int n = Physics.OverlapSphereNonAlloc(center + Vector3.up, radius, _hits, ~0, QueryTriggerInteraction.Collide);
            for (int i = 0; i < n; i++)
            {
                Hurtbox hb = _hits[i].GetComponentInParent<Hurtbox>();
                if (hb == null || hb.Team != Team.Player) continue;
                // Edge case: Rishi hawa me (kooda) to shockwave neeche se nikal jaaye — koodna bhi bachav hai.
                if (_move == Move.BhoomiBhed && hb.transform.position.y > transform.position.y + 0.9f) return;
                Vector3 dir = hb.transform.position - transform.position; dir.y = 0f;
                hb.ReceiveHit(new HitInfo
                {
                    Damage = damage,
                    Direction = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward,
                    Knockback = knockback,
                    AttackerTeam = Team.Asur,
                    Attacker = transform,
                });
                return;
            }
        }

        void SummonMinions()
        {
            if (minionTemplate == null || minionSpawns == null || minionSpawns.Length == 0) return;
            int alive = 0;
            for (int i = 0; i < _minions.Length; i++) if (_minions[i] != null && _minions[i].activeSelf) alive++;
            int toSpawn = Mathf.Min(2, maxMinions - alive);
            for (int s = 0; s < toSpawn; s++)
            {
                int slot = Array.FindIndex(_minions, m => m == null || !m.activeSelf);
                if (slot < 0) break;
                if (_minions[slot] != null) Destroy(_minions[slot]);
                Transform sp = minionSpawns[(_attackCount + s) % minionSpawns.Length];
                GameObject m = Instantiate(minionTemplate, sp.position, sp.rotation, transform.parent);
                m.name = "Chhaya_Bhoot (Andhvir ka)";
                m.SetActive(true);
                _minions[slot] = m;
            }
        }

        // ---------------- Khula mauka ----------------
        public bool FilterHit(ref HitInfo info)
        {
            if (InOpening) info.Damage *= info.GuardBreak ? openingHeavyMult : openingLightMult;
            return true;
        }

        protected override float StunDuration(in HitInfo info) => InOpening && info.GuardBreak ? 0.9f : hurtStun;

        // ---------------- Telegraph ghera ----------------
        void UpdateTelegraph()
        {
            if (telegraphDisc == null) return;
            bool show = State == EnemyState.Attack && StateTimer < windup && _move != Move.Prahar;
            telegraphDisc.enabled = show;
            if (!show) return;
            float r = _move == Move.Chakkar ? spinRadius : slamRadius;
            float grow = Mathf.Lerp(0.35f, 1f, StateTimer / Mathf.Max(windup, 0.01f));
            telegraphDisc.transform.localScale = new Vector3(r * 2f * grow / transform.lossyScale.x, 0.01f, r * 2f * grow / transform.lossyScale.z);
        }
    }
}
