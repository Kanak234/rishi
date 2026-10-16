// =====================================================================
// HintZone.cs  —  RISHI: The Tamas War   (Milestone M4)
// ---------------------------------------------------------------------
// KYUN: GDD acceptance: "Naya player 2 minute ke andar combo, dodge aur
//       climb samajh le (tutorial ke saath)". Lambi tutorial screen ki jagah
//       sahi jagah par chhota ishara — deewar ke paas "wall-run", kinare ke
//       paas "chadho".
// KYA:  Trigger. Rishi pehli baar aaye to Hud par hint (kuch second).
// AAGE: Hud.ShowHint(). LevelBuilder level me jagah-jagah lagata hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;
using Rishi.UI;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class HintZone : MonoBehaviour
    {
        [SerializeField, TextArea] string text = "";
        [SerializeField] float seconds = 6f;
        bool _shown;

        public string Text { get => text; set => text = value; }

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (_shown || other.GetComponentInParent<WeaponManager>() == null) return;
            _shown = true;
            Hud.ShowHint(text, seconds);
        }
    }
}
