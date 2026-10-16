// =====================================================================
// WeaponManager.cs  —  RISHI: The Tamas War   (Milestone M3)
// ---------------------------------------------------------------------
// KYUN: GDD: har teerth par ek divya shastra milta hai. Slice me do —
//       Trishul (shuru se) aur Dhanush (level ke beech). Kaunsa haath me hai,
//       kaunsa mil chuka hai, kitna upgrade hua — ye ek jagah sambhalna hai.
// KYA:  Q/E (D-pad) se agle/pichhle MILE HUE shastra par switch.
//       Trishul = CombatSystem (melee) on; Dhanush = BowCombat on.
//       Unlock() (pickup se), Upgrade() (Jyoti se — M4/M6 UI bulayega).
//       Haath me shastra ka model on/off.
// AAGE: RishiInput se Prev/NextWeapon. CombatSystem/BowCombat ko enable/disable
//       aur DamageMultiplier. WeaponPickup Unlock() bulata hai. Events HUD
//       (M6 "current shastra icon") aur PlayerAnimator ke liye.
// =====================================================================
using System;
using UnityEngine;
using Rishi.Core;

namespace Rishi.Combat
{
    public enum WeaponId { Trishul, Dhanush }

    [Serializable]
    public class WeaponSlot
    {
        public WeaponId id;
        public bool unlocked;
        [Range(0, 2)] public int level;     // 0 = base; GDD: Jyoti se upgrade
        public GameObject visual;           // haath me dikhne wala model (placeholder abhi)
    }

    [RequireComponent(typeof(RishiInput))]
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] WeaponSlot[] slots =
        {
            new WeaponSlot { id = WeaponId.Trishul, unlocked = true },  // GDD: shuru se
            new WeaponSlot { id = WeaponId.Dhanush, unlocked = false }, // GDD: level ke beech milega
        };
        // Upgrade level → damage multiplier. (GDD me number nahi tha — M3 me tay.)
        [SerializeField] float[] levelMultiplier = { 1f, 1.25f, 1.5f };

        public WeaponId Current => slots[_index].id;
        public int CurrentLevel => slots[_index].level;

        public event Action<WeaponId> WeaponChanged;
        public event Action<WeaponId> WeaponUnlocked;
        public event Action<WeaponId, int> WeaponUpgraded;

        RishiInput _input;
        CombatSystem _melee;
        BowCombat _bow;
        int _index;

        void Awake()
        {
            _input = GetComponent<RishiInput>();
            _melee = GetComponent<CombatSystem>();
            _bow = GetComponent<BowCombat>();
        }

        void Start() => Equip(FirstUnlocked(), notify: false);

        void Update()
        {
            if (Time.timeScale <= 0f) return;
            int dir = _input.NextWeapon.WasPressedThisFrame() ? 1 : _input.PrevWeapon.WasPressedThisFrame() ? -1 : 0;
            if (dir == 0) return;

            // Edge case: attack / charge / aim ke beech switch nahi — warna aadha vaar atak jaata.
            if ((_melee != null && _melee.IsBusy) || (_bow != null && _bow.IsBusy)) return;

            int next = NextUnlocked(dir);
            if (next != _index) Equip(next, notify: true);
        }

        // ---------------- Public API ----------------
        public bool IsUnlocked(WeaponId id) { int i = IndexOf(id); return i >= 0 && slots[i].unlocked; }

        /// <summary>Pickup se shastra mila → unlock aur turant haath me (naya shastra aazmao).</summary>
        public void Unlock(WeaponId id)
        {
            int i = IndexOf(id);
            if (i < 0 || slots[i].unlocked) return; // dobara pickup — kuch nahi
            slots[i].unlocked = true;
            WeaponUnlocked?.Invoke(id);
            if ((_melee == null || !_melee.IsBusy) && (_bow == null || !_bow.IsBusy)) Equip(i, notify: true);
        }

        /// <summary>Level +1 (max 2). Jyoti ka hisaab bulane wala karega. true = upgrade hua.</summary>
        public bool Upgrade(WeaponId id)
        {
            int i = IndexOf(id);
            if (i < 0 || !slots[i].unlocked || slots[i].level >= levelMultiplier.Length - 1) return false;
            slots[i].level++;
            if (i == _index) ApplyMultiplier();
            WeaponUpgraded?.Invoke(id, slots[i].level);
            return true;
        }

        public int LevelOf(WeaponId id) { int i = IndexOf(id); return i >= 0 ? slots[i].level : 0; }

        /// <summary>Save se wapas lana (Continue) — bina events ke, seedha level.</summary>
        public void RestoreLevel(WeaponId id, int level)
        {
            int i = IndexOf(id);
            if (i < 0) return;
            slots[i].level = Mathf.Clamp(level, 0, levelMultiplier.Length - 1);
            if (i == _index) ApplyMultiplier();
        }

        // ---------------- Internal ----------------
        void Equip(int index, bool notify)
        {
            _index = index;
            WeaponId id = slots[index].id;

            // Ek waqt par ek hi combat script on — dono LMB padhte hain.
            if (_melee != null) _melee.enabled = id == WeaponId.Trishul;
            if (_bow != null) _bow.enabled = id == WeaponId.Dhanush;

            for (int i = 0; i < slots.Length; i++)
                if (slots[i].visual != null) slots[i].visual.SetActive(i == index);

            ApplyMultiplier();
            if (notify) WeaponChanged?.Invoke(id);
        }

        void ApplyMultiplier()
        {
            float m = levelMultiplier[Mathf.Clamp(slots[_index].level, 0, levelMultiplier.Length - 1)];
            if (_melee != null) _melee.DamageMultiplier = m;
            if (_bow != null) _bow.DamageMultiplier = m;
        }

        int NextUnlocked(int dir)
        {
            for (int step = 1; step <= slots.Length; step++)
            {
                int i = ((_index + dir * step) % slots.Length + slots.Length) % slots.Length;
                if (slots[i].unlocked) return i;
            }
            return _index;
        }

        int FirstUnlocked()
        {
            for (int i = 0; i < slots.Length; i++) if (slots[i].unlocked) return i;
            return 0;
        }

        int IndexOf(WeaponId id)
        {
            for (int i = 0; i < slots.Length; i++) if (slots[i].id == id) return i;
            return -1;
        }
    }
}
