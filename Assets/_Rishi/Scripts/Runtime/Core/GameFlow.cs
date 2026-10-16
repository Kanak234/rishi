// =====================================================================
// GameFlow.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: Menu → Mission 1 … Mission 8 → menu: scene ke naam aur save ka
//       hisaab ek hi jagah, taaki koi script galat scene na khole.
// KYA:  NewGame, Continue, StartMission (menu ka "Mission chuno"),
//       CompleteMission (agla mission kholo + load), Retry, ToMenu.
//       Load se pehle pause hatata hai (warna timeScale 0 me naya scene atak jaata).
// AAGE: MainMenu, PauseMenu, LevelDirector isko bulate hain. Scene ke naam
//       Rishi.World.Missions.Scene(i) se.
// =====================================================================
using UnityEngine.SceneManagement;
using Rishi.World;

namespace Rishi.Core
{
    public static class GameFlow
    {
        public const string MenuScene = "MainMenu";

        public static void NewGame()
        {
            SaveSystem.NewGame();
            SaveSystem.Save();
            Load(Missions.Scene(0));
        }

        public static void Continue()
        {
            SaveSystem.Load();
            Load(Missions.Scene(UnityEngine.Mathf.Clamp(SaveSystem.Current.mission, 0, Missions.Count - 1)));
        }

        /// <summary>Menu se koi khula mission dobara khelo (Jyoti/shastra wahi rehte hain).</summary>
        public static void StartMission(int i)
        {
            SaveSystem.Load();
            var d = SaveSystem.Current;
            d.mission = i;
            d.checkpoint = -1;
            SaveSystem.Save();
            Load(Missions.Scene(i));
        }

        /// <summary>Mission poora: agla khulo aur seedha wahi chalo. Aakhri ke baad menu.</summary>
        public static void CompleteMission(int i)
        {
            var d = SaveSystem.Current;
            int next = i + 1;
            d.unlocked = UnityEngine.Mathf.Max(d.unlocked, UnityEngine.Mathf.Min(next, Missions.Count - 1));
            d.checkpoint = -1;
            if (next >= Missions.Count) { d.finished = true; d.mission = Missions.Count - 1; SaveSystem.Save(); Load(MenuScene); return; }
            d.mission = next;
            SaveSystem.Save();
            Load(Missions.Scene(next));
        }

        /// <summary>Game over ke baad: aakhri checkpoint se (save file se) dobara.</summary>
        public static void Retry() => Continue();

        public static void ToMenu() => Load(MenuScene);

        static void Load(string scene)
        {
            GamePause.SetPaused(false);
            GamePause.Locked = false;
            SceneManager.LoadScene(scene);
        }
    }
}
