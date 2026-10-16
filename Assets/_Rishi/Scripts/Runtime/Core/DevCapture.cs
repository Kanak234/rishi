// =====================================================================
// DevCapture.cs  —  RISHI: The Tamas War   (dev tool)
// ---------------------------------------------------------------------
// KYUN: Release se pehle har screen ki tasveer chahiye (menu, kahani,
//       level ke charon hisse, boss) — haath se khel ke har baar lena
//       dheema hai. Command line se chalao, ye khud ghoom ke screenshot leta hai.
// KYA:  Sirf tab chalta hai jab argument ho:  --capture=<folder>
//       Menu → New game → har mission: intro, har deepak, boss → quit. Normal khel par asar nahi.
// AAGE: Release checklist / itch.io page ke screenshots.
// =====================================================================
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rishi.Player;
using Rishi.CameraSystem;

namespace Rishi.Core
{
    public class DevCapture : MonoBehaviour
    {
        string _dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            foreach (string a in System.Environment.GetCommandLineArgs())
            {
                if (!a.StartsWith("--capture=")) continue;
                var go = new GameObject("DevCapture");
                DontDestroyOnLoad(go);
                go.AddComponent<DevCapture>()._dir = a.Substring("--capture=".Length);
                return;
            }
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(_dir);
            yield return new WaitForSecondsRealtime(1.5f);
            Shot("01_splash");
            yield return new WaitForSecondsRealtime(3.5f);
            Shot("02_menu");
            GameFlow.NewGame();
            yield return new WaitForSecondsRealtime(3.5f);
            Shot("03_intro_slide");
            // kahani skip (StoryCutscene ko seedha band) — Esc dabane jaisa
            Rishi.UI.StoryCutscene.SkipCurrent();
            yield return new WaitForSecondsRealtime(1.5f);

            // Har mission: shuru, beech, boss/ant ke paas — LevelDirector ke checkpoints se jagah
            for (int m = 0; m < Rishi.World.Missions.Count; m++)
            {
                if (m > 0)
                {
                    GameFlow.StartMission(m);
                    yield return new WaitForSecondsRealtime(3f);
                    Shot($"m{m + 1}_0_intro");
                    Rishi.UI.StoryCutscene.SkipCurrent();
                    yield return new WaitForSecondsRealtime(1.5f);
                }
                var dir = Rishi.World.LevelDirector.Instance;
                var pc = FindAnyObjectByType<PlayerController>();
                Shot($"m{m + 1}_1_start");
                var cps = FindObjectsByType<Rishi.World.Checkpoint>(FindObjectsSortMode.None);
                System.Array.Sort(cps, (a, b) => a.Index.CompareTo(b.Index));
                for (int c = 1; c < cps.Length && pc != null; c++)
                {
                    pc.Teleport(cps[c].transform.position + Vector3.forward * 4f, Quaternion.identity);
                    var cf = Camera.main.GetComponent<IsoCameraFollow>(); if (cf != null) cf.SnapToTarget();
                    yield return new WaitForSecondsRealtime(1.5f);
                    Shot($"m{m + 1}_2_cp{c}");
                }
                if (dir != null && dir.Boss != null && pc != null)
                {
                    pc.Teleport(dir.Boss.transform.position + new Vector3(0f, 0f, -12f), Quaternion.identity);
                    yield return new WaitForSecondsRealtime(2.5f);
                    Shot($"m{m + 1}_3_bossintro");
                    Rishi.UI.StoryCutscene.SkipCurrent();
                    yield return new WaitForSecondsRealtime(4f);
                    Shot($"m{m + 1}_4_boss");
                }
            }
            Debug.Log("[CAPTURE] done, fps ~" + (1f / Time.smoothDeltaTime).ToString("0"));
            Application.Quit();
        }

        void Shot(string name)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(_dir, name + ".png"));
            Debug.Log($"[CAPTURE] {name} scene={SceneManager.GetActiveScene().name}");
        }
    }
}
