// =====================================================================
// SaveSystem.cs  —  RISHI: The Tamas War   (Milestone M4)
// ---------------------------------------------------------------------
// KYUN: GDD: "checkpoint save" + main menu ka "Continue". Mar gaye ya game
//       band kiya to Rishi wahi se shuru kare jahan aakhri deepak jalaya tha.
// KYA:  Chhota JSON file (Application.persistentDataPath/rishi_save.json):
//       kaunsa checkpoint, Dhanush mila ya nahi, Jyoti kitni, shastra level.
//       Static class — scene badalne par bhi data bana rahe.
// AAGE: Checkpoint.cs Save() bulata hai. LevelDirector level shuru par
//       Load() karke Rishi ko checkpoint par bhejta hai. MainMenu HasSave
//       dekh ke "Continue" button dikhata hai.
// =====================================================================
using System;
using System.IO;
using UnityEngine;

namespace Rishi.Core
{
    [Serializable]
    public class SaveData
    {
        public int mission;             // abhi kaunsa mission (0..7)
        public int unlocked;            // sabse aage khula mission — "Mission chuno" isse
        public int checkpoint = -1;     // is mission me; -1 = mission ki shuruaat
        public bool hasDhanush;
        public int jyoti;
        public int trishulLevel;
        public int dhanushLevel;
        public int introMask;           // bit i = mission i ki shuru ki kahani dekh li (Continue par dobara nahi)
        public int bossMask;            // bit i = boss se pehle ki kahani dekh li
        public bool finished;           // poora khel khatam
    }

    public static class SaveSystem
    {
        static string FilePath => Path.Combine(Application.persistentDataPath, "rishi_save.json");

        /// <summary>Abhi chal rahe game ka data. New Game par khaali, Continue par file se.</summary>
        public static SaveData Current { get; private set; } = new SaveData();

        public static bool HasSave => File.Exists(FilePath);

        public static void NewGame()
        {
            Current = new SaveData();
            Delete(); // purana save hatao — warna Continue purani jagah le jaata
        }

        public static bool Load()
        {
            try
            {
                if (!HasSave) { Current = new SaveData(); return false; }
                Current = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath)) ?? new SaveData();
                return true;
            }
            catch (Exception e)
            {
                // Edge case: file kharab (beech me power gaya) — crash nahi, naya game.
                Debug.LogWarning($"[SAVE] Save padh nahi paye, naya game: {e.Message}");
                Current = new SaveData();
                return false;
            }
        }

        public static void Save()
        {
            try
            {
                // Pehle temp file, phir rename — likhte waqt band hua to purana save bacha rahe.
                string tmp = FilePath + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(Current, true));
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tmp, FilePath);
                LastSaveTime = Time.unscaledTime; // HUD kone me chhota deepak jalata hai
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SAVE] Save nahi hua: {e.Message}");
            }
        }

        public static void Delete()
        {
            try { if (HasSave) File.Delete(FilePath); } catch (Exception) { /* read-only disk — ignore */ }
        }

        /// <summary>GDD UI states: "save ho raha hai to kone me chhota deepak jale".</summary>
        public static float LastSaveTime { get; private set; } = -100f;
    }
}
