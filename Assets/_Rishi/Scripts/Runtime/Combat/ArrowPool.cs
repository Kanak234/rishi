// =====================================================================
// ArrowPool.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: Rishi ka Dhanush aur Vayu-pishach dono teer chalate hain. Pool ka
//       code ek jagah — har shot par Instantiate/Destroy nahi (allocation
//       rule + GC spikes se frame drop).
// KYA:  Shuru me N teer bana ke chhupa deta hai; Get() khaali teer deta hai.
//       Pool ka root scene me alag object hai (shooter ka child nahi) —
//       warna shooter ghoome to udte teer bhi ghoomte.
// AAGE: BowCombat (Rishi) aur VayuPishach (asur) isko Awake me banate hain,
//       OnDestroy me Dispose().
// =====================================================================
using UnityEngine;

namespace Rishi.Combat
{
    public class ArrowPool
    {
        readonly Arrow[] _arrows;
        readonly GameObject _root;

        public ArrowPool(string name, int size, Material material, Vector3 scale)
        {
            _root = new GameObject(name);
            _arrows = new Arrow[size];
            for (int i = 0; i < size; i++)
            {
                // Placeholder teer: patla lamba cube (asli model M4/M5 me).
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Arrow_" + i;
                Object.Destroy(go.GetComponent<Collider>()); // teer khud collider nahi — ray se hit dhoondhta hai
                go.transform.SetParent(_root.transform, false);
                go.transform.localScale = scale;
                if (material != null) go.GetComponent<MeshRenderer>().sharedMaterial = material;
                go.SetActive(false);
                _arrows[i] = go.AddComponent<Arrow>();
            }
        }

        /// <summary>Khaali teer, ya null (sab ud rahe hain — shot skip, crash nahi).</summary>
        public Arrow Get()
        {
            for (int i = 0; i < _arrows.Length; i++) if (!_arrows[i].InUse) return _arrows[i];
            return null;
        }

        public void Dispose()
        {
            if (_root != null) Object.Destroy(_root);
        }
    }
}
