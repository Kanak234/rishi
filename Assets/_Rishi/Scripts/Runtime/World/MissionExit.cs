// =====================================================================
// MissionExit.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: Har mission sirf "aage chalo, lado" na ho. Kashi ki aarti aur
//       Ellora ki gufaon me kaam hai: maha-deep jalao. Aur har mission ka
//       ek saaf ant chahiye jahan se kahani agle mission me jaaye.
// KYA:  ObjectiveShrine — bujha maha-deep; Rishi paas aakar F / RB dabaye
//       (ya 1.2 s khada rahe) to jalta hai. ShrineGate — iske saare deep
//       jal jaayein to Tamas ki deewar hat jaati hai.
//       MissionExit — mission ka ant (trigger) → LevelDirector.CompleteMission().
// AAGE: LevelBuilder in teeno ko mission ke hisaab se lagata hai. Hud
//       "Maha-deep: 2/3" ShrineGate.Active se padhta hai.
// =====================================================================
using UnityEngine;
using Rishi.Combat;
using Rishi.Core;

namespace Rishi.World
{
    [RequireComponent(typeof(Collider))]
    public class MissionExit : MonoBehaviour
    {
        bool _done;
        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (_done || other.GetComponentInParent<WeaponManager>() == null) return;
            _done = true;
            LevelDirector.Instance?.CompleteMission();
        }
    }
}
