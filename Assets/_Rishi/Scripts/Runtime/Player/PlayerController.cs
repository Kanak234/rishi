// =====================================================================
// PlayerController.cs  —  RISHI: The Tamas War   (Milestone M2)
// ---------------------------------------------------------------------
// KYUN: Rishi ka poora movement — daudna, koodna, dodge roll, deewar par
//       daudna (wall-run), aur kinare (ledge) pakad kar chadhna. Raji jaisa
//       feel isi script se aata hai.
// KYA:  Ek "state machine" hai. Har waqt Rishi ek hi state me hota hai:
//         Grounded → Airborne → WallRunning / LedgeHanging → Climbing
//         aur kahin se bhi Dodging (jab allowed ho).
//       Har state ka apna Update function hai — isse bugs dhoondhna aasaan.
// KAISE: Unity ka CharacterController use hota hai (Rigidbody nahi), kyunki
//       action game me physics ka "phisalna" nahi, exact control chahiye.
// AAGE: RishiInput se input aata hai, IsoCameraFollow ki direction se WASD
//       screen-relative banta hai, Health ke "Damaged" event se ledge par
//       hit lagne par Rishi girta hai. "State"/"StateChanged" ko PlayerAnimator
//       (Mixamo animations) aur DebugHud padhte hain. CombatSystem (M3) MoveScale/
//       ExtraVelocity/JumpLocked se attack ke time movement rokta/lunge karta hai.
// GDD NUMBERS: dodge 0.4 s i-frames, 1.0 s cooldown; wall-run max 2 s.
// =====================================================================
using System;
using UnityEngine;
using Rishi.Core;
using Rishi.Combat;

namespace Rishi.Player
{
    // Saari possible states. Ek waqt par sirf ek.
    public enum MoveState { Grounded, Airborne, Dodging, WallRunning, LedgeHanging, Climbing }

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(RishiInput))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        // ---------------- Tuning (Inspector me badal sakte ho) ----------------
        [Header("Run / Jump")]
        [SerializeField] float moveSpeed = 6f;
        [SerializeField] float turnSpeed = 900f;      // degree per second
        [SerializeField] float jumpHeight = 1.6f;     // metre
        [SerializeField] float gravity = -25f;        // asli -9.8 se zyada: game me "bhari" aur crisp lagta hai
        [SerializeField] float airControl = 0.6f;     // hawa me kitna mod sakte ho (0..1)
        [SerializeField] float coyoteTime = 0.12f;    // kinare se girne ke baad bhi thodi der jump allowed

        [Header("Dodge (GDD)")]
        [SerializeField] float dodgeSpeed = 13f;
        [SerializeField] float dodgeDuration = 0.4f;  // = i-frames
        [SerializeField] float dodgeCooldown = 1.0f;

        [Header("Wall-run (GDD: max 2 s)")]
        [SerializeField] float wallRunMaxTime = 2f;
        [SerializeField] float wallRunSpeed = 8f;
        [SerializeField] float wallRunGravity = -2.5f;   // deewar par dheere dheere neeche aata hai
        [SerializeField] float wallCheckDistance = 0.9f;
        [SerializeField] float wallRunMinHeight = 1.0f;  // zameen se itna upar ho tabhi wall-run
        [SerializeField] float wallJumpPush = 7f;        // deewar se kitni zor se door koode
        [SerializeField] float wallReattachDelay = 0.35f;

        [Header("Ledge climb")]
        [SerializeField] float ledgeReach = 0.8f;        // samne kitni door tak kinara dhoondhe
        [SerializeField] float hangDepth = 1.75f;        // latakte waqt kinara pairon se kitna upar ho
        [SerializeField] float climbDuration = 0.55f;
        [SerializeField] float ledgeRegrabDelay = 0.4f;  // chhodne ke turant baad dobara na pakde

        [Header("Hit reaction")]
        [SerializeField] float fallKnockback = 3f;       // ledge par hit lagne par peeche dhakka

        [SerializeField] LayerMask environmentMask = ~0; // kin layers ko deewar/zameen maane

        // ---------------- Public read-only info ----------------
        public MoveState State { get; private set; } = MoveState.Grounded;
        public Vector3 Velocity => _velocity;
        public float DodgeCooldownLeft => Mathf.Max(0f, _nextDodgeTime - Time.time);
        public float WallRunTimeLeft => State == MoveState.WallRunning ? Mathf.Max(0f, wallRunMaxTime - _stateTimer) : 0f;
        public event Action<MoveState, MoveState> StateChanged; // (purana, naya) — animation script ke liye
        // PlayerAnimator ko ye numbers chahiye: run blend normalize karna, roll/climb clip ki
        // speed state ki length se milana, aur wall-run me kis taraf jhukna hai.
        public float MoveSpeed => moveSpeed;
        public float DodgeDuration => dodgeDuration;
        public float ClimbDuration => climbDuration;
        public Vector3 WallNormal => _wallNormal;

        // ---------------- Combat hooks (M3) ----------------
        // CombatSystem attack ke time ye set karta hai. PlayerController ko combat ka
        // pata nahi — bas "kitna chal sakte ho" aur "kitna dhakka" milta hai.
        /// <summary>Zameen par input movement ka multiplier (1 = normal, 0 = attack me khade).</summary>
        public float MoveScale { get; set; } = 1f;
        /// <summary>Attack lunge jaisi extra horizontal speed (world space), zameen par lagti hai.</summary>
        public Vector3 ExtraVelocity { get; set; }
        /// <summary>true ho to Jump nahi (attack ke beech). Dodge hamesha allowed — attack cancel karta hai.</summary>
        public bool JumpLocked { get; set; }
        /// <summary>true ho to PlayerController khud nahi ghoomta — Dhanush aim karte waqt BowCombat disha tay karta hai (strafe).</summary>
        public bool LockFacing { get; set; }

        // ---------------- Internal state ----------------
        CharacterController _cc;
        RishiInput _input;
        Health _health;
        Transform _cam;

        Vector3 _velocity;          // abhi ki speed (world space)
        float _stateTimer;          // current state me kitna time hua
        float _lastGroundedTime;    // coyote time ke liye
        float _nextDodgeTime;       // cooldown khatam kab hoga
        float _nextWallRunTime;     // deewar chhodne ke baad delay
        float _nextLedgeTime;       // ledge chhodne ke baad delay
        Vector3 _dodgeDir;
        Vector3 _wallNormal, _wallRunDir;
        Vector3 _hangPos, _climbTarget; // ledge: latakne ki jagah, chadhne ke baad ki jagah
        Vector3 _climbStart;

        // =================================================================
        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _input = GetComponent<RishiInput>();
            _health = GetComponent<Health>();
        }

        void OnEnable()  { _health.Damaged += OnDamaged; }   // subscribe
        void OnDisable() { _health.Damaged -= OnDamaged; }   // unsubscribe (memory leak se bachao)

        void Start()
        {
            // Camera.main Start me — Awake me camera abhi bana na ho (edge case).
            if (Camera.main != null) _cam = Camera.main.transform;
        }

        // =================================================================
        void Update()
        {
            // Pause (timeScale 0) me kuch mat karo — warna pause me dabaya Jump
            // resume par achanak chal jata.
            if (Time.timeScale <= 0f) return;
            if (_health.IsDead) return;

            float dt = Time.deltaTime;
            _stateTimer += dt;

            // State ke hisaab se sahi function. Switch = saaf, ek jagah sab states.
            switch (State)
            {
                case MoveState.Grounded:     UpdateGrounded(dt);  break;
                case MoveState.Airborne:     UpdateAirborne(dt);  break;
                case MoveState.Dodging:      UpdateDodging(dt);   break;
                case MoveState.WallRunning:  UpdateWallRun(dt);   break;
                case MoveState.LedgeHanging: UpdateHanging();     break;
                case MoveState.Climbing:     UpdateClimbing();    break;
            }
        }

        // ---------------- STATE: Grounded ----------------
        void UpdateGrounded(float dt)
        {
            Vector3 wish = CameraRelativeInput();

            // Zameen par seedha speed set (instant, responsive feel).
            // MoveScale/ExtraVelocity = CombatSystem ka hissa (attack me rukna, lunge).
            _velocity.x = wish.x * moveSpeed * MoveScale + ExtraVelocity.x;
            _velocity.z = wish.z * moveSpeed * MoveScale + ExtraVelocity.z;
            _velocity.y = -2f; // halka neeche dabav: slope/seedhi par chipka rahe
            if (MoveScale > 0f && !LockFacing) FaceDirection(wish, dt); // attack ke beech ghoomna nahi — CombatSystem swing ki disha tay karta hai

            if (_input.Dodge.WasPressedThisFrame() && TryStartDodge(wish)) return;
            if (_input.Jump.WasPressedThisFrame() && !JumpLocked) { DoJump(); return; }

            _cc.Move(_velocity * dt);

            if (_cc.isGrounded) _lastGroundedTime = Time.time;
            else SetState(MoveState.Airborne); // kinare se gir gaye
        }

        // ---------------- STATE: Airborne ----------------
        void UpdateAirborne(float dt)
        {
            Vector3 wish = CameraRelativeInput();

            // Hawa me thoda control — poora nahi, warna "floaty" lagta hai.
            Vector3 horiz = new Vector3(_velocity.x, 0f, _velocity.z);
            Vector3 target = wish * moveSpeed;
            horiz = Vector3.MoveTowards(horiz, target, moveSpeed * airControl * 6f * dt);
            _velocity.x = horiz.x; _velocity.z = horiz.z;
            _velocity.y += gravity * dt;
            FaceDirection(wish, dt);

            // Coyote jump: kinare se abhi abhi gire ho to bhi jump chale.
            if (_input.Jump.WasPressedThisFrame() && Time.time - _lastGroundedTime <= coyoteTime && _velocity.y <= 0f)
            { DoJump(); return; }

            // Pehle ledge check (zyada important), phir wall-run.
            if (TryGrabLedge()) return;
            if (wish.sqrMagnitude > 0.1f && TryStartWallRun()) return;

            CollisionFlags flags = _cc.Move(_velocity * dt);
            if ((flags & CollisionFlags.Above) != 0 && _velocity.y > 0f) _velocity.y = 0f; // sar takraya

            if (_cc.isGrounded && _velocity.y <= 0f)
            {
                _lastGroundedTime = Time.time;
                SetState(MoveState.Grounded);
            }
        }

        // ---------------- STATE: Dodging ----------------
        void UpdateDodging(float dt)
        {
            // Dodge ke time fixed direction me tez chalo; gravity bhi lagao taaki
            // kinare se dodge karke gir sako (asli game jaisa).
            _velocity.x = _dodgeDir.x * dodgeSpeed;
            _velocity.z = _dodgeDir.z * dodgeSpeed;
            _velocity.y = _cc.isGrounded ? -2f : _velocity.y + gravity * dt;
            _cc.Move(_velocity * dt);

            if (_stateTimer >= dodgeDuration)
            {
                _health.IsInvincible = false; // i-frames khatam
                SetState(_cc.isGrounded ? MoveState.Grounded : MoveState.Airborne);
            }
        }

        bool TryStartDodge(Vector3 wish)
        {
            if (Time.time < _nextDodgeTime) return false; // cooldown chal raha hai
            // Input nahi diya to Rishi jidhar dekh raha hai udhar roll.
            _dodgeDir = wish.sqrMagnitude > 0.01f ? wish.normalized : transform.forward;
            transform.rotation = Quaternion.LookRotation(_dodgeDir);
            _nextDodgeTime = Time.time + dodgeCooldown;
            _health.IsInvincible = true; // GDD: 0.4 s invincibility
            SetState(MoveState.Dodging);
            return true;
        }

        // ---------------- STATE: WallRunning ----------------
        bool TryStartWallRun()
        {
            if (Time.time < _nextWallRunTime) return false;
            if (_velocity.y < -6f) return false;                     // bahut tez gir rahe ho to nahi
            if (HeightAboveGround() < wallRunMinHeight) return false; // zameen ke paas nahi

            Vector3 origin = transform.position + Vector3.up * (_cc.height * 0.5f);
            // Daaye aur baaye deewar dhoondo.
            if (!CheckWall(origin, transform.right, out RaycastHit hit) &&
                !CheckWall(origin, -transform.right, out hit)) return false;

            _wallNormal = hit.normal;
            // Deewar ke saath-saath direction = normal aur upar ka cross product.
            _wallRunDir = Vector3.Cross(_wallNormal, Vector3.up);
            if (Vector3.Dot(_wallRunDir, transform.forward) < 0f) _wallRunDir = -_wallRunDir;

            _velocity = _wallRunDir * wallRunSpeed;
            _velocity.y = Mathf.Max(_velocity.y, 1.5f); // shuru me halka upar uchhal (achha lagta hai)
            transform.rotation = Quaternion.LookRotation(_wallRunDir);
            SetState(MoveState.WallRunning);
            return true;
        }

        void UpdateWallRun(float dt)
        {
            // Edge case (GDD): wall-run ke beech Dodge ya Jump = deewar se kood jao, ruko mat.
            if (_input.Jump.WasPressedThisFrame() || _input.Dodge.WasPressedThisFrame())
            {
                WallJump();
                return;
            }

            Vector3 origin = transform.position + Vector3.up * (_cc.height * 0.5f);
            bool stillOnWall = CheckWall(origin, -_wallNormal, out RaycastHit hit);
            bool noInput = CameraRelativeInput().sqrMagnitude < 0.1f;

            // Khatam karne ki conditions: time poora, deewar khatam, zameen aa gayi, input chhoda.
            if (_stateTimer >= wallRunMaxTime || !stillOnWall || _cc.isGrounded || noInput)
            {
                _nextWallRunTime = Time.time + wallReattachDelay;
                SetState(_cc.isGrounded ? MoveState.Grounded : MoveState.Airborne);
                return;
            }

            _wallNormal = hit.normal; // curved deewar par bhi chale
            Vector3 v = _wallRunDir * wallRunSpeed - _wallNormal * 1f; // halka deewar ki taraf khinchav
            _velocity.y = Mathf.Max(_velocity.y + wallRunGravity * dt, -3f);
            _velocity.x = v.x; _velocity.z = v.z;
            _cc.Move(_velocity * dt);
        }

        void WallJump()
        {
            // Deewar se door + upar + aage ki taraf.
            float up = Mathf.Sqrt(2f * -gravity * jumpHeight * 0.85f);
            _velocity = _wallNormal * wallJumpPush + _wallRunDir * (wallRunSpeed * 0.6f) + Vector3.up * up;
            _nextWallRunTime = Time.time + wallReattachDelay; // usi deewar par turant wapas na chipke
            Vector3 face = new Vector3(_velocity.x, 0f, _velocity.z);
            if (face.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(face);
            SetState(MoveState.Airborne);
        }

        bool CheckWall(Vector3 origin, Vector3 dir, out RaycastHit hit)
        {
            // Sirf seedhi deewar maano (normal ka y ~0), slope/zameen nahi.
            return Physics.Raycast(origin, dir, out hit, wallCheckDistance, environmentMask, QueryTriggerInteraction.Ignore)
                   && Mathf.Abs(hit.normal.y) < 0.2f;
        }

        // ---------------- STATE: LedgeHanging ----------------
        bool TryGrabLedge()
        {
            if (Time.time < _nextLedgeTime) return false;
            if (_velocity.y > 2f) return false; // tez upar ja rahe ho to nahi pakdo (jump ki chhoti ki taraf ho tab pakdo)

            Vector3 fwd = transform.forward;
            Vector3 pos = transform.position;

            // 1) Seene ki height par samne deewar honi chahiye.
            Vector3 chest = pos + Vector3.up * 1.4f;
            if (!Physics.Raycast(chest, fwd, out RaycastHit wallHit, ledgeReach, environmentMask, QueryTriggerInteraction.Ignore)) return false;
            if (Mathf.Abs(wallHit.normal.y) > 0.3f) return false;

            // 2) Sar ke upar samne KHALI hona chahiye — yaani deewar yahan khatam (kinara hai).
            Vector3 aboveHead = pos + Vector3.up * (_cc.height + 0.4f);
            if (Physics.Raycast(aboveHead, fwd, ledgeReach + 0.3f, environmentMask, QueryTriggerInteraction.Ignore)) return false;

            // 3) Upar se neeche ray maar kar kinare ki chhat (top surface) dhoondo.
            Vector3 downFrom = aboveHead + fwd * (wallHit.distance + 0.25f);
            if (!Physics.Raycast(downFrom, Vector3.down, out RaycastHit topHit, 1.6f, environmentMask, QueryTriggerInteraction.Ignore)) return false;
            if (topHit.normal.y < 0.7f) return false; // chhat lagbhag flat honi chahiye

            // 4) Upar chadhne ki jagah par Rishi fit hoga? (chhat ke neeche tak ki jagah check)
            Vector3 standPos = topHit.point + fwd * 0.45f;
            Vector3 p1 = standPos + Vector3.up * (_cc.radius + 0.05f);
            Vector3 p2 = standPos + Vector3.up * (_cc.height - _cc.radius);
            if (Physics.CheckCapsule(p1, p2, _cc.radius * 0.9f, environmentMask, QueryTriggerInteraction.Ignore)) return false;

            // Sab theek — latak jao.
            Vector3 wallFace = -wallHit.normal;
            _hangPos = new Vector3(wallHit.point.x, topHit.point.y - hangDepth, wallHit.point.z)
                       + wallHit.normal * (_cc.radius + 0.05f);
            _climbTarget = standPos;

            _velocity = Vector3.zero;
            _cc.enabled = false; // manual position ke time CharacterController band (warna wo position overwrite karta hai)
            transform.SetPositionAndRotation(_hangPos, Quaternion.LookRotation(new Vector3(wallFace.x, 0f, wallFace.z)));
            SetState(MoveState.LedgeHanging);
            return true;
        }

        void UpdateHanging()
        {
            // Jump/Interact = chadho. Dodge ya peeche (S) = chhod do.
            if (_input.Jump.WasPressedThisFrame() || _input.Interact.WasPressedThisFrame())
            {
                _climbStart = transform.position;
                SetState(MoveState.Climbing);
                return;
            }

            bool pullBack = _input.MoveValue.y < -0.6f;
            if (_input.Dodge.WasPressedThisFrame() || pullBack) DropFromLedge(Vector3.zero);
        }

        // ---------------- STATE: Climbing ----------------
        void UpdateClimbing()
        {
            // Do hisse: pehle seedha upar, phir aage — asli chadhne jaisa arc.
            float t = Mathf.Clamp01(_stateTimer / climbDuration);
            Vector3 upPoint = new Vector3(_climbStart.x, _climbTarget.y + 0.1f, _climbStart.z);
            Vector3 p = t < 0.6f
                ? Vector3.Lerp(_climbStart, upPoint, Mathf.SmoothStep(0f, 1f, t / 0.6f))
                : Vector3.Lerp(upPoint, _climbTarget, Mathf.SmoothStep(0f, 1f, (t - 0.6f) / 0.4f));
            transform.position = p;

            if (t >= 1f)
            {
                _cc.enabled = true;
                _velocity = Vector3.zero;
                _lastGroundedTime = Time.time;
                SetState(MoveState.Grounded);
            }
        }

        void DropFromLedge(Vector3 push)
        {
            _cc.enabled = true;
            _velocity = push;
            _nextLedgeTime = Time.time + ledgeRegrabDelay;
            SetState(MoveState.Airborne);
        }

        // ---------------- Hit reaction ----------------
        void OnDamaged(float amount)
        {
            // Edge case (GDD): ledge par latakte ya chadhte waqt hit lage to Rishi
            // neeche gire — ledge par atka na rahe.
            if (State == MoveState.LedgeHanging || State == MoveState.Climbing)
                DropFromLedge(-transform.forward * fallKnockback);
        }

        // ---------------- Helpers ----------------
        void DoJump()
        {
            // v = sqrt(2 * g * h)  →  exactly jumpHeight tak jaata hai.
            _velocity.y = Mathf.Sqrt(2f * -gravity * jumpHeight);
            _lastGroundedTime = -999f; // double coyote jump na ho
            SetState(MoveState.Airborne);
        }

        Vector3 CameraRelativeInput()
        {
            // WASD ko camera ke hisaab se badlo: W = screen me upar, chahe camera tircha ho.
            Vector2 m = _input.MoveValue;
            if (m.sqrMagnitude < 0.01f) return Vector3.zero;
            Vector3 f = _cam ? _cam.forward : Vector3.forward;
            Vector3 r = _cam ? _cam.right : Vector3.right;
            f.y = 0f; r.y = 0f; f.Normalize(); r.Normalize();
            Vector3 v = f * m.y + r * m.x;
            return v.sqrMagnitude > 1f ? v.normalized : v; // diagonal par tez na chale
        }

        void FaceDirection(Vector3 dir, float dt)
        {
            if (dir.sqrMagnitude < 0.01f) return;
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * dt);
        }

        float HeightAboveGround()
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit h, 50f, environmentMask, QueryTriggerInteraction.Ignore)
                ? h.distance : 50f;
        }

        void SetState(MoveState next)
        {
            if (next == State) return;
            MoveState prev = State;
            // Dodge beech me toot jaye (jaise gir gaye) to i-frames hamesha band karo.
            if (prev == MoveState.Dodging) _health.IsInvincible = false;
            State = next;
            _stateTimer = 0f;
            StateChanged?.Invoke(prev, next);
        }

        /// <summary>KillZone / checkpoint ke liye: Rishi ko seedha kisi jagah bhejo.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _cc.enabled = false; // CharacterController on ho to teleport kaam nahi karta
            transform.SetPositionAndRotation(position, rotation);
            _cc.enabled = true;
            _velocity = Vector3.zero;
            _health.IsInvincible = false;
            SetState(MoveState.Airborne); // agle frame zameen detect hogi
        }

#if UNITY_EDITOR
        // Scene view me ledge/wall rays dikhte hain — debugging ke liye.
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 p = transform.position;
            Gizmos.DrawRay(p + Vector3.up * 1.4f, transform.forward * ledgeReach);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(p + Vector3.up * 1f, transform.right * wallCheckDistance);
            Gizmos.DrawRay(p + Vector3.up * 1f, -transform.right * wallCheckDistance);
        }
#endif
    }
}
