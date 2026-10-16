// =====================================================================
// Checkpoint.cs  —  RISHI: The Tamas War   (Milestone M4)
// ---------------------------------------------------------------------
// KYUN: GDD: checkpoint save + game over par "checkpoint se dobara".
//       Kahani se juda: har checkpoint ek bujha hua deepak hai — Rishi paas
//       aata hai to jyoti wapas jalti hai (yahi game ka maqsad bhi hai).
// KYA:  Trigger. Rishi aaye to: deepak jalao (light + lau), poori jaan,
//       SaveSystem me index + shastra + Jyoti save. Jyoti kaafi ho to
//       haath wale shastra ka upgrade (GDD: Jyoti se shastra upgrade).
// AAGE: LevelDirector level shuru par index se Rishi ko yahan bhejta hai.
//       KillZone khaai me girne par Checkpoint.Latest par laata hai. Hud message dikhata hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;
using Rishi.Core;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] int index;
        [SerializeField] GameObject litVisual;   // lau + Light — bujha ho to band
        [SerializeField] int[] upgradeCost = { 8, 16 }; // Lv1, Lv2 ke liye Jyoti

        public int Index => index;
        public static Checkpoint Latest { get; private set; }
        public static event System.Action<string> Message; // Hud sunta hai

        bool _lit;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void Awake()
        {
            if (litVisual != null) litVisual.SetActive(false);
        }

        void OnDestroy() { if (Latest == this) Latest = null; }

        /// <summary>Continue par: bina save/sound ke jalao (pehle jal chuka tha).</summary>
        public void LightSilently()
        {
            _lit = true;
            Latest = this;
            if (litVisual != null) litVisual.SetActive(true);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_lit) return;
            var wm = other.GetComponentInParent<WeaponManager>();
            if (wm == null) return; // sirf Rishi

            LightSilently();
            var hp = other.GetComponentInParent<Health>();
            if (hp != null) hp.ResetFull();
            GameAudio.Play(Sfx.Deepak, transform.position);

            SaveData d = SaveSystem.Current;
            string msg = "Deepak jala — yahan se dobara shuru hoga";

            // Jyoti se upgrade: pehle haath wala shastra, warna doosra
            int lvl = wm.CurrentLevel;
            if (lvl < upgradeCost.Length && d.jyoti >= upgradeCost[lvl] && wm.Upgrade(wm.Current))
            {
                d.jyoti -= upgradeCost[lvl];
                msg = $"{wm.Current} ka ashirvad badha — Lv {lvl + 1} (damage x{(lvl == 0 ? 1.25f : 1.5f)})";
                GameAudio.Play(Sfx.Pickup, transform.position);
            }

            d.checkpoint = Mathf.Max(d.checkpoint, index);
            d.hasDhanush = wm.IsUnlocked(WeaponId.Dhanush);
            LevelDirector.WriteWeaponLevels(wm, d);
            SaveSystem.Save();
            Message?.Invoke(msg);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Latest = null; Message = null; }
    }
}
