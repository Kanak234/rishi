// =====================================================================
// TrainingDummy.cs  —  RISHI: The Tamas War   (Milestone M3, test-only)
// ---------------------------------------------------------------------
// KYUN: Asur AI abhi bana nahi, par combo/heavy/hitbox test karna hai.
//       Akhade ka putla (dummy) hit leta hai aur saaf dikhata hai ki vaar
//       laga, kitna laga, aur finisher/heavy ka dhakka sahi hai ya nahi.
// KYA:  Hurtbox.Hit sunta hai → rang chamakta hai (laal; heavy = sona),
//       dhakke ki disha me thoda hilta hai, console me "[DUMMY]" log,
//       sar ke upar HP. Mar jaye to 2 s baad poori jaan ke saath wapas.
// AAGE: Sirf test arena me. Asli dushman EnemyAI (M3) me banenge.
// =====================================================================
using UnityEngine;
using Rishi.Combat;

namespace Rishi.World
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Hurtbox))]
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] Renderer body;
        [SerializeField] float flashTime = 0.15f;
        [SerializeField] float respawnDelay = 2f;
        [SerializeField] float shakePerKnockback = 0.04f; // knockback 7 → ~0.28 m jhatka

        Health _hp;
        Hurtbox _hb;
        MaterialPropertyBlock _mpb;   // material copy kiye bina rang badalna (no new material per hit)
        Color _baseColor;
        Color _flashColor;
        float _flashUntil;
        float _respawnAt = -1f;
        Vector3 _home;
        Vector3 _offset;              // dhakke ka visual offset, dheere wapas 0
        float _lastDamage;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        void Awake()
        {
            _hp = GetComponent<Health>();
            _hb = GetComponent<Hurtbox>();
            if (body == null) body = GetComponentInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _baseColor = body != null && body.sharedMaterial != null && body.sharedMaterial.HasProperty(BaseColorId)
                ? body.sharedMaterial.GetColor(BaseColorId) : Color.gray;
            _home = transform.position;
        }

        void OnEnable()  { _hb.Hit += OnHit; _hp.Died += OnDied; }
        void OnDisable() { _hb.Hit -= OnHit; _hp.Died -= OnDied; }

        void OnHit(HitInfo info)
        {
            _lastDamage = info.Damage;
            _flashColor = info.GuardBreak ? new Color(1f, 0.75f, 0.2f) : new Color(1f, 0.25f, 0.2f);
            _flashUntil = Time.time + flashTime;
            _offset += info.Direction * info.Knockback * shakePerKnockback;
            Debug.Log($"[DUMMY] {name} hit: {info.Damage:0} dmg{(info.IsFinisher ? " FINISHER" : "")}{(info.GuardBreak ? " HEAVY/guard-break" : "")} → HP {_hp.Current:0}/{_hp.Max:0}");
        }

        void OnDied()
        {
            Debug.Log($"[DUMMY] {name} toot gaya — {respawnDelay:0} s me wapas.");
            _respawnAt = Time.time + respawnDelay;
        }

        void Update()
        {
            if (_respawnAt > 0f && Time.time >= _respawnAt)
            {
                _respawnAt = -1f;
                _hp.ResetFull();
            }

            // Jhatka dheere wapas (spring jaisa), position kabhi home se door na bhatke.
            _offset = Vector3.Lerp(_offset, Vector3.zero, 1f - Mathf.Exp(-10f * Time.deltaTime));
            transform.position = _home + _offset;

            if (body != null)
            {
                Color c = Time.time < _flashUntil ? _flashColor : (_hp.IsDead ? _baseColor * 0.35f : _baseColor);
                _mpb.SetColor(BaseColorId, c);
                body.SetPropertyBlock(_mpb);
            }
        }

        void OnGUI()
        {
            // Sar ke upar HP — sirf debug ke liye (M6 HUD alag).
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.4f);
            if (sp.z < 0f) return;
            GUI.Label(new Rect(sp.x - 50f, Screen.height - sp.y, 100f, 22f),
                      _hp.IsDead ? "TOOTA" : $"HP {_hp.Current:0}  (-{_lastDamage:0})");
        }
    }
}
