// =====================================================================
// LevelBuilder.cs  —  RISHI: The Tamas War   (Editor-only, M4–M6)
// ---------------------------------------------------------------------
// KYUN: 6 level, 8 mission — haath se greybox banana dino ka kaam aur har
//       baar thoda alag. Ye tool ek click me Main Menu + saare 8 mission
//       scene banata hai — same naap, same jagah, dobara chalao to same result.
// KYA:  Har mission "hisson" (sections) ki list hai jo +Z disha me aage
//       judte jaate hain: Start, Floor, Stones (koodna), WallRun, Ledge,
//       Steps, Fight (asur lehar + Tamas gate), Checkpoint, Dhanush,
//       Shrines (maha-deep), Boss, Exit. Har level ki apni "theme":
//       zameen ka rang, kohra, roshni, aas-paas ki sajavat.
//       Neeche nadi / shoonya = KillZone (aakhri deepak par wapas).
// MENU: Tools ▸ RISHI ▸ Build All Scenes  (ya ReleaseBuilder.BuildScenesCI)
// AAGE: Rishi / asur TestArenaBuilder ke helpers se (ek hi tarika).
//       ReleaseBuilder yahi 9 scenes Linux/Windows build me daalta hai.
// =====================================================================
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Rishi.CameraSystem;
using Rishi.Combat;
using Rishi.Core;
using Rishi.Enemies;
using Rishi.Player;
using Rishi.UI;
using Rishi.World;
using T = Rishi.EditorTools.TestArenaBuilder;

namespace Rishi.EditorTools
{
    public static class LevelBuilder
    {
        const string SceneDir = "Assets/_Rishi/Scenes";
        public const string MenuPath = SceneDir + "/MainMenu.unity";
        public static string MissionPath(int i) => $"{SceneDir}/{Missions.Scene(i)}.unity";

        enum Theme { Gaon, Rajrappa, Kashi, Hampi, Ellora, TamasLok }
        enum BossKind { Andhvir, Shilaasur, Tamasur }

        static Material _geru, _chuna, _stone, _sona, _tamas, _asur, _pashan, _water, _flame, _wood, _telegraph, _andhvir, _void, _ghat, _sand, _cave;
        static Transform _env, _lights, _enemies, _logic;

        // ---- mission build state (cursor) ----
        static float Z, Y, W;              // aage ka kinara, zameen ki unchai, raaste ki chaudai
        static Theme _theme;
        static int _cpIndex, _seed, _nameId;
        static List<Checkpoint> _cps;
        static BossAndhvir _boss;
        static float _arenaStartZ = -1f;
        static Transform _arenaRespawn;
        static Transform _rishi;
        static Vector3 _spawn;

        // =================================================================
        [MenuItem("Tools/RISHI/Build All Scenes (Menu + 8 Missions)")]
        public static void BuildAll()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildMenuScene();
            for (int i = 0; i < Missions.Count; i++) BuildMission(i);
            SetBuildScenes();
            Debug.Log("[RISHI] MainMenu + 8 missions ready.");
        }

        public static string[] AllScenePaths()
        {
            var list = new List<string> { MenuPath };
            for (int i = 0; i < Missions.Count; i++) list.Add(MissionPath(i));
            return list.ToArray();
        }

        static void SetBuildScenes()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var p in AllScenePaths()) list.Add(new EditorBuildSettingsScene(p, true));
            list.Add(new EditorBuildSettingsScene("Assets/_Rishi/Scenes/M2_TestArena.unity", false));
            EditorBuildSettings.scenes = list.ToArray();
        }

        // =================================================================
        //  MISSIONS — yahan har mission ka naksha hai
        // =================================================================
        static void BuildMission(int i)
        {
            switch (i)
            {
                case 0: // Level 1 · Damodar Gaon · Navami ki Raat (tutorial)
                    Begin(i, Theme.Gaon, 22f);
                    Hint("WASD = chalo  ·  Shift = dodge roll (0.4 s tak koi vaar nahi lagta)\nAsur naranga chamke = vaar aa raha hai, tab dodge!");
                    Floor(14f);
                    Hint("LMB x3 = Trishul combo (teesra vaar dhakka deta hai)\nRMB hold = bhaari vaar — chhodo to prahar");
                    Fight("Gaon me Chhaya-bhoot", "C3");
                    CP();
                    Hint("Space = koodo. Chattanon par koodte jao —\nnadi me gire to aakhri deepak par wapas (-10 jaan)");
                    Stones(3, 12f);
                    Floor(6f);
                    Fight("Ghat ki raksha", "C3|C4");
                    Floor(6f);
                    Exit();
                    break;

                case 1: // Level 2 · Rajrappa · Sangam ki Chattanein
                    Begin(i, Theme.Rajrappa, 12f);
                    Stones(4, 7f);
                    CP();
                    Hint("Bahut chaudi khaai! Baayein deewar ki taraf daudo, Space se koodo —\nRishi deewar par daudega (2 s). Aage Space = deewar se kood.");
                    WallRun();
                    Floor(6f);
                    Hint("Oonchi chattan: kinare ki taraf koodo — Rishi latak jaayega.\nF / Space = upar chadho");
                    Ledge(2.4f);
                    Hint("Pashan-rakshas saamne se vaar rok leta hai (clang!).\nRMB hold karke bhaari vaar se guard todo — ya peeche se maaro");
                    Fight("Sangam par Pashan-rakshas", "P1C2");
                    CP();
                    Stones(3, 8f);
                    WallRun();
                    Ledge(2.4f);
                    Fight("Chattanon ke rakhwale", "C3|P1C2");
                    Exit();
                    break;

                case 2: // Level 2 · Rajrappa · Chhinnamastika ka Aangan (Dhanush + Andhvir)
                    Begin(i, Theme.Rajrappa, 22f);
                    Floor(8f);
                    Hint("Chabootre par sone ka Dhanush chamak raha hai — chadho aur utha lo!");
                    Dhanush();
                    Hint("Vayu-pishach hawa me hain — Trishul nahi pahunchta.\nE dabao, Ctrl hold = nishana, LMB = teer, RMB hold = charged teer");
                    Fight("Mandir ke aangan me asur", "V2C2|P1V1C2");
                    CP();
                    Boss(BossKind.Andhvir);
                    break;

                case 3: // Level 3 · Kashi · Deepon ki Seedhiyan
                    Begin(i, Theme.Kashi, 20f);
                    Hint("Ghaton ki seedhiyan — Space se ek-ek seedhi chadho");
                    Steps(4, 1.1f);
                    Fight("Ghat par Vayu-pishach", "V2C2");
                    CP();
                    Stones(3, 8f);
                    Fight("Naavon ke ghat", "C3|V2C2");
                    CP();
                    WallRun();
                    Steps(3, 1.1f);
                    Fight("Aasmaan ka jhund", "V3C2|P1V2");
                    Exit();
                    break;

                case 4: // Level 3 · Kashi · Ganga Aarti ki Raksha (maha-deep)
                    Begin(i, Theme.Kashi, 24f);
                    Fight("Aarti ke raste", "C4");
                    CP();
                    Hint("Teen maha-deep! Har deep ke paas khade raho (F / RB = jaldi) —\nasur rokne aayenge, pehle saaf karo ya jaldi jalao");
                    Shrines("Maha-deep", 3, "C3V1|V2C2|P1V2C2");
                    CP();
                    Steps(3, 1.1f);
                    Floor(6f);
                    Exit();
                    break;

                case 5: // Level 4 · Hampi · Vijayanagara ke Khandahar (Shilaasur)
                    Begin(i, Theme.Hampi, 14f);
                    Stones(3, 8f);
                    Ledge(2.4f);
                    Fight("Khandaharon ke Pashan", "P2C2");
                    CP();
                    WallRun();
                    Floor(4f);
                    Fight("Toote mandap", "P1V2C2|P2V1");
                    CP();
                    Boss(BossKind.Shilaasur);
                    break;

                case 6: // Level 5 · Ellora · Kailasa ki Gufayein (guru ki mukti)
                    Begin(i, Theme.Ellora, 16f);
                    Fight("Gufa ka dwaar", "C4");
                    Ledge(2.4f);
                    CP();
                    Hint("Gufa ke teen deep jalao — Tamas ki zanjeer tabhi tootegi");
                    Shrines("Gufa-deep", 3, "V2C3|P1V2C2|P2V1");
                    CP();
                    WallRun();
                    Ledge(2.4f);
                    Fight("Kailasa ke rakshak", "P1V2C3");
                    Floor(6f);
                    Exit();
                    break;

                case 7: // Level 6 · Tamas-lok · Damodar ke Neeche (Tamasur)
                    Begin(i, Theme.TamasLok, 12f);
                    Stones(4, 7f);
                    Fight("Andhkar ki sena", "C4V2");
                    CP();
                    WallRun();
                    Ledge(2.4f);
                    Fight("Tamas ke dwaar", "P2V2C2|V3C3");
                    CP();
                    Boss(BossKind.Tamasur);
                    break;
            }
            Finish(i);
        }

        // =================================================================
        //  Mission scaffolding
        // =================================================================
        static void Begin(int mission, Theme theme, float width)
        {
            LoadMaterials();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _env = new GameObject("Environment").transform;
            _lights = new GameObject("Lights").transform;
            _enemies = new GameObject("Asur").transform;
            _logic = new GameObject("Logic").transform;
            _theme = theme;
            _cps = new List<Checkpoint>();
            _cpIndex = 0; _seed = 1234 + mission * 97; _nameId = 0;
            _boss = null; _arenaStartZ = -1f; _arenaRespawn = null;
            Z = -4f; Y = 0f; W = width;

            ThemeAtmosphere();
            // Shuruaat: 14 m zameen + pehla deepak (checkpoint 0)
            float z0 = Z;
            Floor(14f);
            MakeCheckpoint(new Vector3(0f, Y + 0.05f, z0 + 9f));
            _spawn = new Vector3(0f, Y + 0.05f, z0 + 3f);
        }

        static void Finish(int mission)
        {
            // Nadi / shoonya — gire to wapas. Boss arena ka alag (arena ke kinare par).
            float end = Z + 10f;
            float killEnd = _arenaStartZ > 0f ? _arenaStartZ - 2f : end;
            Kill("KillZone_Main", -12f, killEnd, null, true);
            if (_arenaStartZ > 0f) Kill("KillZone_Akhada", _arenaStartZ - 2f, end, _arenaRespawn, false);

            if (_theme != Theme.TamasLok && _theme != Theme.Ellora)
            {
                var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
                water.name = "Nadi_Water";
                water.transform.SetParent(_env);
                water.transform.position = new Vector3(0f, -1.6f, (end - 12f) / 2f);
                water.transform.localScale = new Vector3(20f, 1f, (end + 40f) / 10f);
                water.GetComponent<MeshRenderer>().sharedMaterial = _water;
                Object.DestroyImmediate(water.GetComponent<Collider>());
            }
            else
            {
                var voidP = GameObject.CreatePrimitive(PrimitiveType.Plane);
                voidP.name = "Andhkar";
                voidP.transform.SetParent(_env);
                voidP.transform.position = new Vector3(0f, -9f, (end - 12f) / 2f);
                voidP.transform.localScale = new Vector3(20f, 1f, (end + 40f) / 10f);
                voidP.GetComponent<MeshRenderer>().sharedMaterial = _void;
                Object.DestroyImmediate(voidP.GetComponent<Collider>());
            }

            _rishi = BuildRishi(_spawn);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = RenderSettings.fogColor;
            cam.fieldOfView = 40f;
            cam.farClipPlane = 160f;
            camGo.AddComponent<AudioListener>();
            var follow = camGo.AddComponent<IsoCameraFollow>();
            follow.Target = _rishi;
            follow.SnapToTarget();

            var sys = new GameObject("Systems");
            sys.AddComponent<GamePause>();
            sys.AddComponent<PauseMenu>();
            var hud = sys.AddComponent<Hud>();
            var hudSo = new SerializedObject(hud);
            hudSo.FindProperty("playerHealth").objectReferenceValue = _rishi.GetComponent<Health>();
            hudSo.FindProperty("weapons").objectReferenceValue = _rishi.GetComponent<WeaponManager>();
            hudSo.ApplyModifiedPropertiesWithoutUndo();
            var dir = sys.AddComponent<LevelDirector>();
            var dso = new SerializedObject(dir);
            dso.FindProperty("missionIndex").intValue = mission;
            dso.FindProperty("player").objectReferenceValue = _rishi;
            dso.FindProperty("boss").objectReferenceValue = _boss;
            var cps = dso.FindProperty("checkpoints");
            cps.arraySize = _cps.Count;
            for (int k = 0; k < _cps.Count; k++) cps.GetArrayElementAtIndex(k).objectReferenceValue = _cps[k];
            dso.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(SceneDir);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), MissionPath(mission));
            Debug.Log($"[RISHI] Mission {mission + 1} saved ({Z:0} m lamba, {_cps.Count} deepak)");
        }

        // =================================================================
        //  Sections
        // =================================================================
        /// <summary>Seedha raasta (zameen ka block, top = Y).</summary>
        static void Floor(float len, float width = -1f, Material m = null)
        {
            float w = width > 0f ? width : W;
            Slab($"Floor_{_nameId++}", 0f, Z, len, w, Y, m ?? FloorMat());
            Decorate(Z, len, w);
            Z += len;
        }

        /// <summary>Koodne wali chattanein: 2–2.5 m khaai, thoda daayein-baayein.</summary>
        static void Stones(int count, float stoneLen)
        {
            for (int i = 0; i < count; i++)
            {
                Z += 2f + Rand01() * 0.6f;
                float x = (Rand01() - 0.5f) * Mathf.Min(4f, W * 0.3f);
                float w = Mathf.Max(5f, W * 0.5f);
                float h = Y + (i % 2 == 1 ? 0.2f : 0f);
                Slab($"Chattan_{_nameId++}", x, Z, stoneLen, w, h, _stone);
                Z += stoneLen;
            }
            Z += 2.2f;
            Floor(4f);
        }

        /// <summary>5 m chaudi khaai + baayein deewar (M2 test arena jaisa naap). Raasta 12 m.</summary>
        static void WallRun()
        {
            float w = 12f;
            Floor(8f, w);
            float gapStart = Z;
            var wall = T.Block($"WallRun_{_nameId++}", new Vector3(-5.6f, Y + 2.5f, gapStart + 2.5f), new Vector3(0.6f, 5f, 14f), _chuna);
            wall.transform.SetParent(_env, true);
            Z += 5f; // seedhi kood (~4.3 m) se thoda zyada — wall-run se aasaan
            Floor(8f, w);
        }

        /// <summary>Oonchi chattan — kinara pakad ke chadhna (2.4 m). Y badhta hai.</summary>
        static void Ledge(float h)
        {
            Floor(4f);
            Y += h;
            Floor(8f);
        }

        /// <summary>Ghat ki seedhiyan — har seedhi koodne layak (≤ 1.2 m).</summary>
        static void Steps(int n, float h)
        {
            for (int i = 0; i < n; i++)
            {
                Y += h;
                Floor(3.5f, -1f, i % 2 == 0 ? _ghat : _chuna);
            }
            Floor(4f);
        }

        static void CP()
        {
            float z0 = Z;
            Floor(8f);
            MakeCheckpoint(new Vector3(0f, Y + 0.05f, z0 + 4f));
        }

        /// <summary>Asur ki ladai: zameen + lehar (waves "C3|P1V2") + aage Tamas gate.</summary>
        static void Fight(string title, string waves, float len = 24f)
        {
            float z0 = Z;
            Floor(len);
            string[] specs = waves.Split('|');
            var all = new GameObject[specs.Length][];
            for (int w = 0; w < specs.Length; w++) all[w] = SpawnWave(specs[w], z0 + len * 0.45f, len * 0.5f).ToArray();
            var gate = Gate(new Vector3(0f, Y + 3f, Z + 0.2f));
            Encounter($"Ladai_{_nameId++}", title, new Vector3(0f, Y + 2f, z0 + len / 2f + 1f), new Vector3(W, 4f, len - 2f), all, new[] { gate }, false);
        }

        /// <summary>Oonche chabootre par Dhanush — poori chaudai me, bina chadhe aage nahi (agli ladai me Vayu-pishach).</summary>
        static void Dhanush()
        {
            Floor(4f);
            float z0 = Z;
            Slab("Chabootra", 0f, Z, 6f, W, Y + 2.4f, _geru);
            var pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pickup.name = "Pickup_Dhanush";
            pickup.transform.SetParent(_logic);
            pickup.transform.position = new Vector3(0f, Y + 3.5f, z0 + 3f);
            pickup.transform.localScale = new Vector3(0.3f, 1.2f, 0.3f);
            pickup.GetComponent<MeshRenderer>().sharedMaterial = _sona;
            var pcol = pickup.GetComponent<BoxCollider>(); pcol.isTrigger = true; pcol.size = new Vector3(14f, 2.5f, 14f);
            pickup.AddComponent<WeaponPickup>();
            PointLight(new Vector3(0f, Y + 5f, z0 + 3f), new Color(1f, 0.75f, 0.3f), 9f, 3f);
            HintAt(new Vector3(0f, Y + 3.6f, z0 + 3f), new Vector3(W, 2.5f, 5f), "Dhanush mila! Q / E = shastra badlo.\nCtrl hold = nishana, LMB = teer, RMB hold = charged teer");
            Z += 6f;
            Floor(6f);
        }

        /// <summary>Maha-deep: chaudi jagah me n deep, asur lehar; saare jale to aage ki Tamas deewar hat-ti hai.</summary>
        static void Shrines(string label, int n, string waves)
        {
            float len = 34f;
            float z0 = Z;
            Floor(len, W + 6f);
            var shrines = new List<ObjectiveShrine>();
            for (int i = 0; i < n; i++)
            {
                float x = n == 1 ? 0f : Mathf.Lerp(-(W + 6f) / 2f + 4f, (W + 6f) / 2f - 4f, i / (float)(n - 1));
                float z = z0 + 10f + (i % 2) * 12f;
                shrines.Add(MakeShrine(new Vector3(x, Y, z)));
            }
            string[] specs = waves.Split('|');
            var all = new GameObject[specs.Length][];
            for (int w = 0; w < specs.Length; w++) all[w] = SpawnWave(specs[w], z0 + len * 0.5f, len * 0.6f).ToArray();
            Encounter($"Ladai_{_nameId++}", label + " ke rakshak", new Vector3(0f, Y + 2f, z0 + len / 2f), new Vector3(W + 6f, 4f, len - 2f), all, new GameObject[0], false);

            var wall = Gate(new Vector3(0f, Y + 3f, Z + 0.2f));
            var sg = new GameObject("ShrineGate").AddComponent<ShrineGate>();
            sg.transform.SetParent(_logic);
            sg.transform.position = wall.transform.position;
            var so = new SerializedObject(sg);
            so.FindProperty("label").stringValue = label;
            var sp = so.FindProperty("shrines"); sp.arraySize = shrines.Count;
            for (int i = 0; i < shrines.Count; i++) sp.GetArrayElementAtIndex(i).objectReferenceValue = shrines[i];
            var wp = so.FindProperty("walls"); wp.arraySize = 1; wp.GetArrayElementAtIndex(0).objectReferenceValue = wall;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Boss(BossKind kind)
        {
            // Pul + deepak (poori jaan) + gate, phir gol akhada
            float savedW = W;
            W = 8f;
            Floor(6f);
            HintAt(new Vector3(0f, Y + 1.5f, Z - 3f), new Vector3(8f, 3f, 3f), "Aage senapati hai. Deepak ne jaan lauta di — taiyaar raho.");
            Floor(6f);
            W = savedW;
            float arenaR = 18f;
            float center = Z + arenaR - 0.5f;
            _arenaStartZ = Z;

            var arena = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arena.name = "Akhada_Floor";
            arena.transform.SetParent(_env);
            arena.transform.position = new Vector3(0f, Y - 0.5f, center);
            arena.transform.localScale = new Vector3(arenaR * 2f, 0.5f, arenaR * 2f);
            arena.GetComponent<MeshRenderer>().sharedMaterial = FloorMat();
            Object.DestroyImmediate(arena.GetComponent<Collider>());
            arena.AddComponent<MeshCollider>().sharedMesh = arena.GetComponent<MeshFilter>().sharedMesh;
            arena.isStatic = true;
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Akhada_Ring";
            ring.transform.SetParent(_env);
            ring.transform.position = new Vector3(0f, Y + 0.01f, center);
            ring.transform.localScale = new Vector3(arenaR * 1.66f, 0.005f, arenaR * 1.66f);
            ring.GetComponent<MeshRenderer>().sharedMaterial = _theme == Theme.TamasLok ? _tamas : _geru;
            Object.DestroyImmediate(ring.GetComponent<Collider>());
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f + Mathf.PI / 8f;
                Vector3 p = new Vector3(Mathf.Sin(a) * (arenaR - 2.5f), Y, center + Mathf.Cos(a) * (arenaR - 2.5f));
                if (p.z < center - arenaR + 6f && Mathf.Abs(p.x) < 6f) continue; // pravesh khula
                Pillar(p);
                Torch(p + (new Vector3(0f, Y, center) - p).normalized * 1.6f);
            }
            if (_theme != Theme.TamasLok) Shikhara(new Vector3(0f, Y, center + arenaR + 12f));
            else for (int i = 0; i < 5; i++) PointLight(new Vector3(-16f + i * 8f, Y + 6f, center + arenaR + 4f), new Color(0.55f, 0.25f, 1f), 16f, 3f);

            _arenaRespawn = new GameObject("Akhada_Respawn").transform;
            _arenaRespawn.SetParent(_logic);
            _arenaRespawn.position = new Vector3(0f, Y + 0.05f, Z + 3f);

            _boss = MakeBoss(kind, new Vector3(0f, Y + 0.05f, center + 7f), center);
            var gate = Gate(new Vector3(0f, Y + 3f, Z - 0.3f));
            Encounter("Ladai_Boss", _boss.DisplayName, new Vector3(0f, Y + 2f, center + 1.5f), new Vector3(arenaR * 1.7f, 5f, arenaR * 1.6f),
                new[] { new[] { _boss.gameObject } }, new[] { gate }, true);
            Z = center + arenaR;
        }

        static void Exit()
        {
            float z0 = Z;
            Floor(10f);
            // Roshni ka dwaar: do stambh + upar patti + garam roshni
            foreach (float x in new[] { -3f, 3f })
            {
                var p = T.Block("Dwaar_Stambh", new Vector3(x, Y + 2.5f, z0 + 6f), new Vector3(0.8f, 5f, 0.8f), _chuna);
                p.transform.SetParent(_env, true);
            }
            T.Block("Dwaar_Patti", new Vector3(0f, Y + 5.3f, z0 + 6f), new Vector3(7.6f, 0.6f, 1f), _geru).transform.SetParent(_env, true);
            var glow = T.Block("Dwaar_Roshni", new Vector3(0f, Y + 2.5f, z0 + 6f), new Vector3(5.2f, 5f, 0.1f), _flame);
            Object.DestroyImmediate(glow.GetComponent<Collider>());
            glow.transform.SetParent(_env, true);
            PointLight(new Vector3(0f, Y + 3f, z0 + 4.5f), new Color(1f, 0.7f, 0.35f), 12f, 4f);
            var ex = new GameObject("Mission_Exit");
            ex.transform.SetParent(_logic);
            ex.transform.position = new Vector3(0f, Y + 1.5f, z0 + 6f);
            var c = ex.AddComponent<BoxCollider>(); c.isTrigger = true; c.size = new Vector3(5f, 4f, 1.5f);
            ex.AddComponent<MissionExit>();
            HintAt(new Vector3(0f, Y + 1.5f, z0 + 2f), new Vector3(W, 3f, 2f), "Roshni ke dwaar se aage badho — mission poora");
        }

        // =================================================================
        //  Pieces
        // =================================================================
        /// <summary>Block jiska top = topY, neeche -1 tak (nadi ke andar dooba dikhe).</summary>
        static void Slab(string name, float x, float z0, float len, float w, float topY, Material m)
        {
            float bottom = _theme == Theme.TamasLok ? topY - 1.5f : -1f; // Tamas-lok: hawa me tairti chattanein
            float h = topY - bottom;
            var b = T.Block(name, new Vector3(x, bottom + h / 2f, z0 + len / 2f), new Vector3(w, h, len), m);
            b.transform.SetParent(_env, true);
        }

        static List<GameObject> SpawnWave(string spec, float zc, float spread)
        {
            var list = new List<GameObject>();
            int i = 0;
            while (i < spec.Length)
            {
                char kind = spec[i++];
                int n = 0;
                while (i < spec.Length && char.IsDigit(spec[i])) n = n * 10 + (spec[i++] - '0');
                for (int k = 0; k < n; k++)
                {
                    float x = (Rand01() - 0.5f) * (W - 4f);
                    float z = zc + (Rand01() - 0.3f) * spread;
                    string nm = $"{kind}_{_nameId++}";
                    switch (kind)
                    {
                        case 'C': list.Add(Park(T.GroundEnemy<ChhayaBhoot>("Chhaya_" + nm, new Vector3(x, Y + 0.05f, z), 0.8f, 30f, _asur))); break;
                        case 'P': list.Add(Park(T.GroundEnemy<PashanRakshas>("Pashan_" + nm, new Vector3(x * 0.6f, Y + 0.05f, z), 1.4f, 120f, _pashan))); break;
                        case 'V': list.Add(Vayu("Vayu_" + nm, new Vector3(x, Y + 3.2f, z + 3f))); break;
                    }
                }
            }
            return list;
        }

        static GameObject Vayu(string name, Vector3 pos)
        {
            var v = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            v.name = name;
            v.transform.position = pos;
            v.transform.localScale = Vector3.one * 0.9f;
            v.GetComponent<MeshRenderer>().sharedMaterial = _asur;
            T.Part(v.transform, PrimitiveType.Cube, new Vector3(-0.8f, 0.1f, 0f), new Vector3(1.0f, 0.05f, 0.4f), _asur);
            T.Part(v.transform, PrimitiveType.Cube, new Vector3(0.8f, 0.1f, 0f), new Vector3(1.0f, 0.05f, 0.4f), _asur);
            T.Part(v.transform, PrimitiveType.Cube, new Vector3(0f, 0.15f, 0.45f), new Vector3(0.5f, 0.08f, 0.1f), _sona);
            T.SetMaxHp(v.AddComponent<Health>(), 40f);
            v.AddComponent<Hurtbox>();
            var vp = v.AddComponent<VayuPishach>();
            var so = new SerializedObject(vp);
            so.FindProperty("arrowMaterial").objectReferenceValue = _asur;
            so.FindProperty("body").objectReferenceValue = v.GetComponent<MeshRenderer>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return Park(v);
        }

        static GameObject Park(GameObject e)
        {
            e.transform.SetParent(_enemies, true);
            e.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Rishi ki taraf (–Z) dekhein
            e.SetActive(false); // EncounterZone jagayega
            return e;
        }

        static BossAndhvir MakeBoss(BossKind kind, Vector3 pos, float arenaCenterZ)
        {
            string name; string sub; float size, hp, spin, slam, speed; int maxMin; Material mat; char minion;
            switch (kind)
            {
                default:
                case BossKind.Andhvir:   name = "Andhvir";   sub = "Tamasur ka senapati";       size = 1.9f; hp = 520f; spin = 3.4f; slam = 5.5f; speed = 3.2f; maxMin = 3; mat = _andhvir; minion = 'C'; break;
                case BossKind.Shilaasur: name = "Shilaasur"; sub = "patthar ka senapati";        size = 2.2f; hp = 650f; spin = 3.8f; slam = 6.5f; speed = 2.6f; maxMin = 2; mat = _pashan;  minion = 'P'; break;
                case BossKind.Tamasur:   name = "Tamasur";   sub = "andhkar ka asur-raja";       size = 2.6f; hp = 900f; spin = 4.2f; slam = 7.0f; speed = 3.4f; maxMin = 3; mat = _andhvir; minion = 'V'; break;
            }
            var go = T.GroundEnemy<BossAndhvir>(name, pos, size, hp, mat);
            var bodyT = go.transform.Find("Body");
            var gada = new GameObject("Gada").transform;
            gada.SetParent(bodyT, false);
            gada.localPosition = new Vector3(0.75f, 0.1f, 0.25f);
            gada.localRotation = Quaternion.Euler(20f, 0f, 0f);
            T.Part(gada, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.08f, 0.55f, 0.08f), _wood);
            T.Part(gada, PrimitiveType.Sphere, new Vector3(0f, 0.6f, 0f), new Vector3(0.38f, 0.38f, 0.38f), kind == BossKind.Tamasur ? _tamas : _sona);
            T.Part(bodyT, PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0f), new Vector3(0.45f, 0.08f, 0.45f), _sona); // mukut
            if (kind == BossKind.Tamasur)
            {
                // Tamas ka ghera (dikhawa) + sing
                T.Part(bodyT, PrimitiveType.Cube, new Vector3(-0.3f, 1.2f, 0f), new Vector3(0.1f, 0.4f, 0.1f), _sona).localRotation = Quaternion.Euler(0f, 0f, 25f);
                T.Part(bodyT, PrimitiveType.Cube, new Vector3(0.3f, 1.2f, 0f), new Vector3(0.1f, 0.4f, 0.1f), _sona).localRotation = Quaternion.Euler(0f, 0f, -25f);
                var aura = new GameObject("Tamas_Aura").AddComponent<Light>();
                aura.transform.SetParent(go.transform, false);
                aura.transform.localPosition = new Vector3(0f, 3f, 0f);
                aura.type = LightType.Point; aura.color = new Color(0.6f, 0.3f, 1f); aura.range = 10f; aura.intensity = 3f; aura.shadows = LightShadows.None;
            }

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Telegraph_Disc";
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            disc.transform.SetParent(go.transform, false);
            disc.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            disc.transform.localScale = new Vector3(1f, 0.01f, 1f);
            var dr = disc.GetComponent<MeshRenderer>();
            dr.sharedMaterial = _telegraph;
            dr.shadowCastingMode = ShadowCastingMode.Off;

            GameObject template;
            Vector3 tp = pos + new Vector3(0f, -30f, 0f); // template chhupa (inactive) — sirf copy ke liye
            if (minion == 'P') template = T.GroundEnemy<PashanRakshas>(name + "_Minion", tp, 1.4f, 120f, _pashan);
            else if (minion == 'V') { template = Vayu(name + "_Minion", tp); }
            else template = T.GroundEnemy<ChhayaBhoot>(name + "_Minion", tp, 0.8f, 30f, _asur);
            template.transform.SetParent(_enemies, true);
            template.SetActive(false);
            var spawns = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI / 2f + Mathf.PI / 4f;
                spawns[i] = new GameObject($"Minion_Spawn_{i}").transform;
                spawns[i].SetParent(_logic);
                spawns[i].position = new Vector3(Mathf.Sin(a) * 10f, minion == 'V' ? Y + 3.2f : Y + 0.05f, arenaCenterZ + Mathf.Cos(a) * 10f);
            }

            var b = go.GetComponent<BossAndhvir>();
            var so = new SerializedObject(b);
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("subtitle").stringValue = sub;
            so.FindProperty("spinRadius").floatValue = spin;
            so.FindProperty("slamRadius").floatValue = slam;
            so.FindProperty("maxMinions").intValue = maxMin;
            so.FindProperty("moveSpeed").floatValue = speed;
            so.FindProperty("hitReach").floatValue = 1.2f + size * 0.35f;
            so.FindProperty("attackRange").floatValue = 1.8f + size * 0.45f;
            so.FindProperty("minionTemplate").objectReferenceValue = template;
            so.FindProperty("telegraphDisc").objectReferenceValue = dr;
            var sp = so.FindProperty("minionSpawns");
            sp.arraySize = spawns.Length;
            for (int i = 0; i < spawns.Length; i++) sp.GetArrayElementAtIndex(i).objectReferenceValue = spawns[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Park(go);
            return b;
        }

        static Checkpoint MakeCheckpoint(Vector3 pos)
        {
            int index = _cpIndex++;
            var root = new GameObject($"Checkpoint_{index}");
            root.transform.SetParent(_logic);
            root.transform.position = pos;
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true; col.center = new Vector3(0f, 1.5f, 0f); col.size = new Vector3(Mathf.Max(6f, W), 3f, 4f);

            var lit = Diya(root.transform, new Vector3(-2.2f, 0f, 0f), 1f);
            var cp = root.AddComponent<Checkpoint>();
            var so = new SerializedObject(cp);
            so.FindProperty("index").intValue = index;
            so.FindProperty("litVisual").objectReferenceValue = lit;
            so.ApplyModifiedPropertiesWithoutUndo();
            _cps.Add(cp);
            return cp;
        }

        static ObjectiveShrine MakeShrine(Vector3 pos)
        {
            var root = new GameObject("Maha_Deep");
            root.transform.SetParent(_logic);
            root.transform.position = pos;
            var col = root.AddComponent<BoxCollider>();
            col.isTrigger = true; col.center = new Vector3(0f, 1.5f, 0f); col.size = new Vector3(4f, 3f, 4f);
            var stambh = T.Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.2f, 0f), new Vector3(0.6f, 1.2f, 0.6f), _chuna);
            stambh.gameObject.AddComponent<BoxCollider>().size = new Vector3(1f, 2f, 1f); // asli khamba — takraaye
            var lit = Diya(root.transform, new Vector3(0f, 1.6f, 0f), 1.8f);
            var sh = root.AddComponent<ObjectiveShrine>();
            var so = new SerializedObject(sh);
            so.FindProperty("litVisual").objectReferenceValue = lit;
            so.ApplyModifiedPropertiesWithoutUndo();
            // Bujha hua bhi dikhe: halka thanda nishaan
            PointLight(pos + new Vector3(0f, 4f, 0f), new Color(0.4f, 0.4f, 0.9f), 5f, 1f).transform.SetParent(root.transform, true);
            return sh;
        }

        /// <summary>Pathar ka stambh + diya; return = "Lit" child (lau + roshni) jo jalne par on hota hai.</summary>
        static GameObject Diya(Transform parent, Vector3 local, float scale)
        {
            T.Part(parent, PrimitiveType.Cylinder, local + new Vector3(0f, 0.5f * scale, 0f), new Vector3(0.5f, 0.5f, 0.5f) * scale, _chuna);
            T.Part(parent, PrimitiveType.Cylinder, local + new Vector3(0f, 1.05f * scale, 0f), new Vector3(0.7f, 0.05f, 0.7f) * scale, _geru);
            var lit = new GameObject("Lit");
            lit.transform.SetParent(parent, false);
            var f = T.Part(lit.transform, PrimitiveType.Sphere, local + new Vector3(0f, 1.3f * scale, 0f), new Vector3(0.22f, 0.4f, 0.22f) * scale, _flame);
            f.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            var l = new GameObject("Light").AddComponent<Light>();
            l.transform.SetParent(lit.transform, false);
            l.transform.localPosition = local + new Vector3(0f, 1.8f * scale, 0f);
            l.type = LightType.Point; l.range = 10f * scale; l.intensity = 3f; l.color = new Color(1f, 0.7f, 0.35f);
            l.shadows = LightShadows.None;
            return lit;
        }

        static void Hint(string text) => HintAt(new Vector3(0f, Y + 1.5f, Z + 1f), new Vector3(W + 4f, 3f, 2.5f), text);

        static void HintAt(Vector3 center, Vector3 size, string text)
        {
            var go = new GameObject("Hint");
            go.transform.SetParent(_logic);
            go.transform.position = center;
            var c = go.AddComponent<BoxCollider>(); c.isTrigger = true; c.size = size;
            var h = go.AddComponent<HintZone>();
            h.Text = text;
            EditorUtility.SetDirty(h);
        }

        static GameObject Gate(Vector3 pos)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = "Tamas_Gate";
            g.transform.SetParent(_logic);
            g.transform.position = pos;
            g.transform.localScale = new Vector3(W + 24f, 6f, 0.4f); // sajavat ke upar se bhi na nikal sakein
            var r = g.GetComponent<MeshRenderer>();
            r.sharedMaterial = _tamas;
            r.shadowCastingMode = ShadowCastingMode.Off;
            g.SetActive(false);
            return g;
        }

        static void Encounter(string name, string title, Vector3 center, Vector3 size, GameObject[][] waves, GameObject[] gates, bool isBoss)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_logic);
            go.transform.position = center;
            var c = go.AddComponent<BoxCollider>(); c.isTrigger = true; c.size = size;
            var ez = go.AddComponent<EncounterZone>();
            var so = new SerializedObject(ez);
            so.FindProperty("title").stringValue = title;
            so.FindProperty("isBoss").boolValue = isBoss;
            var wp = so.FindProperty("waves");
            wp.arraySize = waves.Length;
            for (int w = 0; w < waves.Length; w++)
            {
                var ep = wp.GetArrayElementAtIndex(w).FindPropertyRelative("enemies");
                ep.arraySize = waves[w].Length;
                for (int i = 0; i < waves[w].Length; i++) ep.GetArrayElementAtIndex(i).objectReferenceValue = waves[w][i];
            }
            var gp = so.FindProperty("gates");
            gp.arraySize = gates.Length;
            for (int i = 0; i < gates.Length; i++) gp.GetArrayElementAtIndex(i).objectReferenceValue = gates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Kill(string name, float z0, float z1, Transform respawn, bool useCheckpoint)
        {
            var k = new GameObject(name);
            k.transform.SetParent(_logic);
            k.transform.position = new Vector3(0f, -6f, (z0 + z1) / 2f);
            var c = k.AddComponent<BoxCollider>(); c.isTrigger = true; c.size = new Vector3(200f, 4f, Mathf.Max(1f, z1 - z0));
            var kz = k.AddComponent<KillZone>();
            kz.RespawnPoint = respawn;
            var so = new SerializedObject(kz);
            so.FindProperty("useCheckpoint").boolValue = useCheckpoint;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // =================================================================
        //  Theme: rang, kohra, sajavat
        // =================================================================
        static Material FloorMat()
        {
            switch (_theme)
            {
                case Theme.Gaon: return _geru;
                case Theme.Kashi: return _ghat;
                case Theme.Hampi: return _sand;
                case Theme.Ellora: return _cave;
                case Theme.TamasLok: return _andhvir;
                default: return _stone;
            }
        }

        static void ThemeAtmosphere()
        {
            switch (_theme)
            {
                case Theme.Gaon:     Atmosphere(new Color(0.06f, 0.04f, 0.10f), new Color(0.55f, 0.6f, 0.9f), 0.55f, 0.016f); break;
                case Theme.Rajrappa: Atmosphere(new Color(0.05f, 0.05f, 0.10f), new Color(0.55f, 0.62f, 0.95f), 0.5f, 0.018f); break;
                case Theme.Kashi:    Atmosphere(new Color(0.10f, 0.05f, 0.06f), new Color(1f, 0.75f, 0.55f), 0.45f, 0.014f); break;
                case Theme.Hampi:    Atmosphere(new Color(0.32f, 0.17f, 0.10f), new Color(1f, 0.72f, 0.45f), 1.1f, 0.012f); break;   // shaam
                case Theme.Ellora:   Atmosphere(new Color(0.07f, 0.05f, 0.04f), new Color(0.8f, 0.65f, 0.5f), 0.35f, 0.022f); break;
                case Theme.TamasLok: Atmosphere(new Color(0.09f, 0.03f, 0.16f), new Color(0.6f, 0.4f, 1f), 0.4f, 0.02f); break;
            }
        }

        static void Decorate(float z0, float len, float w)
        {
            // Har ~11 m par dono taraf ek mashal (baari-baari), aur theme ki badi cheezein bahar (bina collider).
            for (float z = z0 + 3f; z < z0 + len - 1f; z += 11f)
            {
                float side = ((int)(z / 11f) % 2 == 0) ? 1f : -1f;
                Torch(new Vector3(side * (w / 2f - 0.8f), Y, z));
                float far = side * -(w / 2f + 7f + Rand01() * 4f);
                Vector3 p = new Vector3(far, _theme == Theme.TamasLok ? Y - 2f : 0f, z + Rand01() * 6f);
                switch (_theme)
                {
                    case Theme.Gaon: Prop(PrimitiveType.Cube, p + Vector3.up * 1.5f, new Vector3(5f, 3f, 4f), _chuna); Prop(PrimitiveType.Cube, p + Vector3.up * 3.5f, new Vector3(6f, 0.6f, 5f), _wood); break;
                    case Theme.Rajrappa: Prop(PrimitiveType.Sphere, p, Vector3.one * (3f + Rand01() * 3f), _stone); break;
                    case Theme.Kashi: MiniShikhara(p + Vector3.up * (Y * 0.5f)); break;
                    case Theme.Hampi: Prop(PrimitiveType.Sphere, p + Vector3.up * 1f, Vector3.one * (4f + Rand01() * 4f), _sand); Prop(PrimitiveType.Cylinder, p + new Vector3(-side * 3f, 2.5f, 3f), new Vector3(1f, 2.5f, 1f), _chuna); break;
                    case Theme.Ellora:
                        Prop(PrimitiveType.Cube, new Vector3(side * (w / 2f + 3f), Y + 4f, z + 5f), new Vector3(4f, 10f, 12f), _cave);
                        Prop(PrimitiveType.Cube, new Vector3(-side * (w / 2f + 3f), Y + 4f, z + 5f), new Vector3(4f, 10f, 12f), _cave); break;
                    case Theme.TamasLok: Prop(PrimitiveType.Cube, p, new Vector3(2f + Rand01() * 3f, 1f, 2f + Rand01() * 3f), _andhvir); break;
                }
            }
        }

        static void Prop(PrimitiveType t, Vector3 pos, Vector3 scale, Material m)
        {
            var go = GameObject.CreatePrimitive(t);
            go.name = "Sajavat";
            Object.DestroyImmediate(go.GetComponent<Collider>()); // sirf dikhawa — raasta na roke, shortcut na bane
            go.transform.SetParent(_env);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            go.isStatic = true;
        }

        static void MiniShikhara(Vector3 basePos)
        {
            Prop(PrimitiveType.Cube, basePos + Vector3.up * 2f, new Vector3(4f, 4f, 4f), _chuna);
            for (int i = 0; i < 3; i++) Prop(PrimitiveType.Cube, basePos + Vector3.up * (4.6f + i * 1.2f), new Vector3(3.4f - i, 1.2f, 3.4f - i), i % 2 == 0 ? _geru : _chuna);
        }

        static void Torch(Vector3 basePos)
        {
            var root = new GameObject("Mashal").transform;
            root.SetParent(_lights);
            root.position = basePos;
            T.Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 0.8f, 0.12f), _wood);
            T.Part(root, PrimitiveType.Cylinder, new Vector3(0f, 1.62f, 0f), new Vector3(0.35f, 0.04f, 0.35f), _geru);
            var f = T.Part(root, PrimitiveType.Sphere, new Vector3(0f, 1.85f, 0f), new Vector3(0.22f, 0.4f, 0.22f), _flame);
            f.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            Color c = _theme == Theme.TamasLok ? new Color(0.65f, 0.4f, 1f) : new Color(1f, 0.62f, 0.28f);
            PointLight(basePos + new Vector3(0f, 2.2f, 0f), c, 10f, 2.6f).transform.SetParent(root, true);
        }

        static void Pillar(Vector3 basePos)
        {
            var p = T.Cylinder("Stambh", basePos + new Vector3(0f, 2.5f, 0f), new Vector3(1.1f, 2.5f, 1.1f), _theme == Theme.TamasLok ? _andhvir : _chuna);
            p.transform.SetParent(_env, true);
            Object.DestroyImmediate(p.GetComponent<Collider>());
            p.AddComponent<BoxCollider>().size = new Vector3(1f, 2f, 1f); // capsule collider par Rishi phisalta tha
            T.Block("Stambh_Shir", basePos + new Vector3(0f, 5.1f, 0f), new Vector3(1.6f, 0.3f, 1.6f), _geru).transform.SetParent(_env, true);
        }

        static Light PointLight(Vector3 pos, Color c, float range, float intensity)
        {
            var l = new GameObject("Point Light").AddComponent<Light>();
            l.transform.SetParent(_lights);
            l.transform.position = pos;
            l.type = LightType.Point; l.color = c; l.range = range; l.intensity = intensity;
            l.shadows = LightShadows.None; // GDD budget: real-time shadow sirf chaand par
            return l;
        }

        static void Shikhara(Vector3 basePos)
        {
            var parent = new GameObject("Mandir_Shikhara").transform;
            parent.SetParent(_env);
            T.Block("Garbhagriha", basePos + new Vector3(0f, 3f, 0f), new Vector3(14f, 6f, 10f), _chuna).transform.SetParent(parent, true);
            T.Block("Dwar", basePos + new Vector3(0f, 2f, -5.05f), new Vector3(3f, 4f, 0.2f), _wood).transform.SetParent(parent, true);
            for (int i = 0; i < 7; i++)
            {
                float w = 12f - i * 1.5f;
                T.Block($"Shikhara_{i}", basePos + new Vector3(0f, 6.9f + i * 1.8f, 0f), new Vector3(w, 1.8f, w * 0.8f), i % 2 == 0 ? _chuna : _geru).transform.SetParent(parent, true);
            }
            var kalash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            kalash.name = "Kalash";
            kalash.transform.SetParent(parent);
            kalash.transform.position = basePos + new Vector3(0f, 20f, 0f);
            kalash.transform.localScale = Vector3.one * 1.4f;
            kalash.GetComponent<MeshRenderer>().sharedMaterial = _sona;
            T.Block("Dhwaja", basePos + new Vector3(0.9f, 22f, 0f), new Vector3(1.6f, 0.9f, 0.05f), _geru).transform.SetParent(parent, true);
            PointLight(basePos + new Vector3(0f, 4f, -7f), new Color(1f, 0.6f, 0.3f), 14f, 3f);
        }

        static void Atmosphere(Color fog, Color moonColor, float moonIntensity, float density)
        {
            var moon = new GameObject("Chaand / Suraj").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = moonColor;
            moon.intensity = moonIntensity * 1.6f; // screenshot check: raat bahut kaali thi, Rishi/raasta dikhe
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            RenderSettings.sun = moon;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.36f, 0.33f, 0.48f), fog * 3f, 0.35f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.26f, 0.24f);
            RenderSettings.ambientGroundColor = new Color(0.14f, 0.10f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = density;
            RenderSettings.fogColor = fog;
            RenderSettings.skybox = null;
        }

        static float Rand01()
        {
            // Apna chhota LCG — Random.value har build me alag; ye har baar same level banata hai.
            _seed = _seed * 1103515245 + 12345;
            return ((_seed >> 8) & 0xFFFF) / 65535f;
        }

        // ---------------- Rishi ----------------
        static Transform BuildRishi(Vector3 pos)
        {
            var root = new GameObject("Rishi").transform;
            root.position = pos;
            root.tag = "Player";
            var cc = root.gameObject.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.4f; cc.center = new Vector3(0f, 1f, 0f);
            cc.stepOffset = 0.4f; cc.slopeLimit = 45f;
            root.gameObject.AddComponent<RishiInput>();
            root.gameObject.AddComponent<Health>();
            root.gameObject.AddComponent<PlayerController>();
            root.gameObject.AddComponent<DebugHud>();
            if (!MixamoRishiSetup.TryAttachModel(root)) T.AddCapsuleBody(root, _sona, _geru);

            root.gameObject.AddComponent<CombatSystem>();
            var hb = root.gameObject.AddComponent<Hurtbox>();
            var hso = new SerializedObject(hb);
            hso.FindProperty("team").enumValueIndex = (int)Team.Player;
            hso.ApplyModifiedPropertiesWithoutUndo();

            var bow = root.gameObject.AddComponent<BowCombat>();
            var bso = new SerializedObject(bow);
            bso.FindProperty("arrowMaterial").objectReferenceValue = _sona;
            bso.ApplyModifiedPropertiesWithoutUndo();
            var wm = root.gameObject.AddComponent<WeaponManager>();

            var anim = root.GetComponentInChildren<Animator>();
            Transform hand = anim != null && anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.RightHand) : null;
            Transform holder = hand != null ? hand : root;
            GameObject trishul = T.FindOrBuild(holder, "Weapon_Trishul", () => T.BuildTrishul(_sona, _geru));
            GameObject dhanush = T.FindOrBuild(holder, "Weapon_Dhanush", () => T.BuildDhanush(_geru));
            if (hand == null)
            {
                trishul.transform.localPosition = new Vector3(0.45f, 1.0f, 0.1f);
                dhanush.transform.localPosition = new Vector3(0.45f, 1.1f, 0.1f);
            }
            var wso = new SerializedObject(wm);
            var slots = wso.FindProperty("slots");
            for (int i = 0; i < slots.arraySize; i++)
            {
                var slot = slots.GetArrayElementAtIndex(i);
                bool isTrishul = slot.FindPropertyRelative("id").enumValueIndex == (int)WeaponId.Trishul;
                slot.FindPropertyRelative("visual").objectReferenceValue = isTrishul ? trishul : dhanush;
            }
            wso.ApplyModifiedPropertiesWithoutUndo();
            dhanush.SetActive(false);

            var glow = new GameObject("Rishi_Glow").AddComponent<Light>();
            glow.transform.SetParent(root, false);
            glow.transform.localPosition = new Vector3(0f, 3f, -1f);
            glow.type = LightType.Point; glow.range = 11f; glow.intensity = 2.6f;
            glow.color = new Color(1f, 0.8f, 0.6f); glow.shadows = LightShadows.None;
            return root;
        }

        // ---------------- Materials ----------------
        static void LoadMaterials()
        {
            Directory.CreateDirectory("Assets/_Rishi/Materials");
            _geru   = T.Mat("M_Geru",   new Color(0.62f, 0.27f, 0.16f));
            _chuna  = T.Mat("M_Chuna",  new Color(0.90f, 0.87f, 0.80f));
            _stone  = T.Mat("M_Stone",  new Color(0.35f, 0.33f, 0.31f));
            _sona   = T.Mat("M_Sona",   new Color(0.95f, 0.66f, 0.18f));
            _tamas  = T.Mat("M_Tamas",  new Color(0.20f, 0.12f, 0.45f, 0.45f), transparent: true);
            _asur   = T.Mat("M_Asur",   new Color(0.18f, 0.10f, 0.32f));
            _pashan = T.Mat("M_Pashan", new Color(0.30f, 0.27f, 0.36f));
            _wood   = T.Mat("M_Wood",   new Color(0.28f, 0.17f, 0.10f));
            _andhvir = T.Mat("M_Andhvir", new Color(0.12f, 0.06f, 0.22f));
            _ghat   = T.Mat("M_Ghat",   new Color(0.66f, 0.52f, 0.40f));
            _sand   = T.Mat("M_HampiSand", new Color(0.72f, 0.45f, 0.26f));
            _cave   = T.Mat("M_Cave",   new Color(0.30f, 0.24f, 0.20f));
            _void   = T.Mat("M_Void",   new Color(0.05f, 0.02f, 0.09f));
            _water  = T.Mat("M_Water",  new Color(0.05f, 0.10f, 0.13f));
            if (_water.HasProperty("_Smoothness")) { _water.SetFloat("_Smoothness", 0.92f); EditorUtility.SetDirty(_water); }
            _telegraph = T.Mat("M_Telegraph", new Color(0.95f, 0.15f, 0.05f, 0.35f), transparent: true);
            _flame = Emissive("M_Flame", new Color(1f, 0.62f, 0.18f), 4f);
        }

        static Material Emissive(string name, Color c, float intensity)
        {
            string path = $"Assets/_Rishi/Materials/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * intensity);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        // =================================================================
        //  MAIN MENU
        // =================================================================
        public static void BuildMenuScene()
        {
            LoadMaterials();
            _theme = Theme.Rajrappa;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _env = new GameObject("Environment").transform;
            _lights = new GameObject("Lights").transform;
            _logic = new GameObject("Logic").transform;
            Atmosphere(new Color(0.06f, 0.04f, 0.10f), new Color(0.55f, 0.6f, 0.9f), 0.55f, 0.012f);

            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Damodar_Water";
            water.transform.SetParent(_env);
            water.transform.position = new Vector3(0f, -0.6f, 0f);
            water.transform.localScale = new Vector3(20f, 1f, 20f);
            water.GetComponent<MeshRenderer>().sharedMaterial = _water;
            T.Block("Ghat", new Vector3(0f, -0.5f, 0f), new Vector3(36f, 1f, 30f), _stone).transform.SetParent(_env, true);
            for (int i = 0; i < 3; i++)
                T.Block($"Seedhi_{i}", new Vector3(0f, -0.25f - i * 0.25f, -15.5f - i * 1.2f), new Vector3(36f, 0.5f, 1.2f), i % 2 == 0 ? _chuna : _geru).transform.SetParent(_env, true);
            Y = 0f;
            Shikhara(new Vector3(0f, 0f, 6f));
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                Pillar(new Vector3(Mathf.Sin(a) * 13f, 0f, Mathf.Cos(a) * 9f - 2f));
            }
            for (int i = 0; i < 8; i++) Torch(new Vector3(-14f + i * 4f, 0f, -12.5f));

            var center = new GameObject("Orbit_Center").transform;
            center.SetParent(_logic);
            center.position = new Vector3(0f, 6f, 4f);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = RenderSettings.fogColor;
            cam.fieldOfView = 45f;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0f, 7f, -32f);
            camGo.transform.LookAt(center.position + Vector3.up * 2f);

            var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
            var so = new SerializedObject(menu);
            so.FindProperty("orbitCenter").objectReferenceValue = center;
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(SceneDir);
            EditorSceneManager.SaveScene(scene, MenuPath);
            Debug.Log("[RISHI] Main menu saved: " + MenuPath);
        }
    }
}
