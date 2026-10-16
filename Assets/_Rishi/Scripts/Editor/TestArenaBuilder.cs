// =====================================================================
// TestArenaBuilder.cs  —  RISHI: The Tamas War   (Editor-only tool)
// ---------------------------------------------------------------------
// KYUN: M2 ka movement test karne ke liye ek "training ground" chahiye —
//       deewarein (wall-run), alag-alag unchai ke kinare (ledge climb),
//       khaai (KillZone), Tamas dhuan (DamageZone). Haath se banane me
//       aadha ghanta; ye menu ek click me banata hai, har baar same.
// KYA:  Menu: Tools ▸ RISHI ▸ Build M2 Test Arena
//       Naya scene banata hai, materials (URP Lit) save karta hai, Rishi
//       (Mixamo model, ya na ho to capsule) + camera + light + obstacles daalta hai, aur
//       scene ko Build Settings me add karta hai.
// AAGE: Play dabao aur PlayerController test karo. Mixamo body
//       MixamoRishiSetup.TryAttachModel() lagata hai; M4 me asli Rajrappa level.
// =====================================================================
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rishi.Core;
using Rishi.Combat;
using Rishi.Player;
using Rishi.World;
using Rishi.CameraSystem;

namespace Rishi.EditorTools
{
    public static class TestArenaBuilder
    {
        const string Root = "Assets/_Rishi";
        const string MatDir = Root + "/Materials";
        const string SceneDir = Root + "/Scenes";
        const string ScenePath = SceneDir + "/M2_TestArena.unity";

        [MenuItem("Tools/RISHI/Build M2 Test Arena")]
        public static void Build()
        {
            // Unsaved changes ho to pehle poochho — kaam na khoye (consequential operation).
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Directory.CreateDirectory(MatDir);
            Directory.CreateDirectory(SceneDir);

            // Palette GDD ke art direction se: geru (laal mitti), chuna-safed, kaala, sona, Tamas-neela.
            Material geru   = Mat("M_Geru",   new Color(0.62f, 0.27f, 0.16f));
            Material chuna  = Mat("M_Chuna",  new Color(0.90f, 0.87f, 0.80f));
            Material stone  = Mat("M_Stone",  new Color(0.35f, 0.33f, 0.31f));
            Material sona   = Mat("M_Sona",   new Color(0.95f, 0.66f, 0.18f));
            Material tamas  = Mat("M_Tamas",  new Color(0.20f, 0.12f, 0.45f, 0.45f), transparent: true);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Light: shaam/raat jaisa garam rang ---
            var lightGo = new GameObject("Sun (warm)");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.85f, 0.7f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // --- Zameen (do hisse, beech me khaai) ---
            Block("Ground_A", new Vector3(0, -0.5f, 0), new Vector3(40, 1, 30), geru);
            Block("Ground_B", new Vector3(0, -0.5f, 32), new Vector3(40, 1, 22), geru);
            // Khaai: z = 15..21 (6 m chaudi) — wall-run se paar karna hai.

            // --- Wall-run deewarein khaai ke dono taraf ---
            Block("WallRun_Left",  new Vector3(-4f, 2.5f, 18f), new Vector3(0.6f, 5f, 14f), chuna);
            Block("WallRun_Right", new Vector3( 8f, 2.5f, 18f), new Vector3(0.6f, 5f, 14f), chuna);

            // --- Ledge climb: alag unchai ke block (2.2 m, 2.8 m, 3.4 m) ---
            Block("Ledge_2.2m", new Vector3(-14f, 1.1f, 0f), new Vector3(4, 2.2f, 4), stone);
            Block("Ledge_2.8m", new Vector3(-14f, 1.4f, 7f), new Vector3(4, 2.8f, 4), stone);
            Block("Ledge_3.4m", new Vector3(-14f, 1.7f, -7f), new Vector3(4, 3.4f, 4), stone);
            // Chhote steps (CharacterController ka stepOffset test)
            Block("Step_0.3m", new Vector3(14f, 0.15f, 0f), new Vector3(3, 0.3f, 3), stone);
            Block("Step_0.6m", new Vector3(14f, 0.3f, 4f), new Vector3(3, 0.6f, 3), stone);

            // --- Mandir jaisa khambha (dekhne ke liye scale; M3 me pillar-kick) ---
            for (int i = 0; i < 4; i++)
                Cylinder($"Pillar_{i}", new Vector3(14f + (i % 2) * 5f, 2f, 10f + (i / 2) * 5f), new Vector3(1f, 2f, 1f), sona);

            // --- Respawn + KillZone (khaai ke neeche) ---
            var respawn = new GameObject("Respawn_Point").transform;
            respawn.position = new Vector3(2f, 0.1f, 10f);

            var kill = Block("KillZone", new Vector3(2f, -8f, 18f), new Vector3(60f, 2f, 12f), null);
            kill.GetComponent<MeshRenderer>().enabled = false; // dikhna nahi chahiye
            kill.GetComponent<Collider>().isTrigger = true;
            kill.AddComponent<KillZone>().RespawnPoint = respawn;

            // --- Tamas dhuan (DamageZone) — 2.8 m ledge ke saamne, hit-while-climb test ---
            var smoke = Block("Tamas_DamageZone", new Vector3(-11f, 1.5f, 7f), new Vector3(2f, 3f, 4f), tamas);
            smoke.GetComponent<Collider>().isTrigger = true;
            smoke.AddComponent<DamageZone>();

            // --- Rishi ---
            // Pivot pairon par chahiye (PlayerController ke rays pairon se naapte hain),
            // isliye khaali "Rishi" root banaya; dikhne wala body uska child hai.
            var root = new GameObject("Rishi").transform;
            root.position = new Vector3(0f, 0.05f, 0f);
            root.tag = "Player";

            var cc = root.gameObject.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.4f; cc.center = new Vector3(0f, 1f, 0f);
            cc.stepOffset = 0.4f; cc.slopeLimit = 45f;
            root.gameObject.AddComponent<RishiInput>();
            root.gameObject.AddComponent<Health>();
            root.gameObject.AddComponent<PlayerController>();
            root.gameObject.AddComponent<DebugHud>();

            // Mixamo model + Animator ready ho (Tools ▸ RISHI ▸ Setup Mixamo Rishi) to wahi lagao,
            // warna capsule placeholder — arena bina Mixamo files ke bhi ban sake.
            if (!MixamoRishiSetup.TryAttachModel(root)) AddCapsuleBody(root, sona, geru);

            // --- M3 combat: Rishi par CombatSystem + Hurtbox, aur akhade ke putle ---
            AddCombatTest(root, stone, sona);
            AddWeaponTest(root, stone, sona, geru);

            // --- Camera ---
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.05f, 0.12f); // raat ka aasmaan
            cam.fieldOfView = 40f;
            camGo.AddComponent<AudioListener>();
            var follow = camGo.AddComponent<IsoCameraFollow>();
            follow.Target = root;
            follow.SnapToTarget();

            // --- Pause system ---
            new GameObject("GamePause").AddComponent<GamePause>();

            // --- Save + Build Settings ---
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Selection.activeGameObject = root.gameObject;
            Debug.Log("[RISHI] M2 Test Arena ready → Play dabao. WASD, Space jump, Shift dodge, F/Space ledge climb, Esc pause, F1 debug.");
        }

        /// <summary>Khule scene me combat test cheezein jodo (arena dobara banaye bina).</summary>
        [MenuItem("Tools/RISHI/Add M3 Combat Test (current scene)")]
        public static void AddCombatTestToOpenScene()
        {
            GameObject rishi = GameObject.Find("Rishi");
            if (rishi == null) { Debug.LogError("[RISHI] Scene me 'Rishi' nahi mila."); return; }
            AddCombatTest(rishi.transform, Mat("M_Stone", Color.gray), Mat("M_Sona", Color.yellow));
            EditorSceneManager.MarkSceneDirty(rishi.scene);
            EditorSceneManager.SaveScene(rishi.scene);
            Debug.Log("[RISHI] M3 combat test ready → Play: LMB = 3-hit combo, RMB hold = heavy charge, Shift = dodge cancel.");
        }

        [MenuItem("Tools/RISHI/Add M3 Weapons Test (current scene)")]
        public static void AddWeaponTestToOpenScene()
        {
            GameObject rishi = GameObject.Find("Rishi");
            if (rishi == null) { Debug.LogError("[RISHI] Scene me 'Rishi' nahi mila."); return; }
            AddCombatTest(rishi.transform, Mat("M_Stone", Color.gray), Mat("M_Sona", Color.yellow));
            AddWeaponTest(rishi.transform, Mat("M_Stone", Color.gray), Mat("M_Sona", Color.yellow), Mat("M_Geru", Color.red));
            EditorSceneManager.MarkSceneDirty(rishi.scene);
            EditorSceneManager.SaveScene(rishi.scene);
            Debug.Log("[RISHI] M3 weapons test ready → sone ke Dhanush ko chhuo, Q/E switch, Ctrl hold = aim, LMB teer, RMB hold = charged teer.");
        }

        static void AddWeaponTest(Transform rishi, Material stone, Material sona, Material geru)
        {
            // --- Rishi par BowCombat + WeaponManager ---
            // Note: "??" nahi — editor me missing component "fake null" hota hai, ?? usse null nahi maanta.
            var bow = rishi.GetComponent<BowCombat>();
            if (bow == null) bow = rishi.gameObject.AddComponent<BowCombat>();
            var bowSo = new SerializedObject(bow);
            bowSo.FindProperty("arrowMaterial").objectReferenceValue = sona;
            bowSo.ApplyModifiedPropertiesWithoutUndo();

            var wm = rishi.GetComponent<WeaponManager>();
            if (wm == null) wm = rishi.gameObject.AddComponent<WeaponManager>();

            // --- Haath me placeholder shastra (asli model M4/M5 me) ---
            // Humanoid ho to daayein haath ki haddi par, warna Rishi ke daayein taraf.
            var anim = rishi.GetComponentInChildren<Animator>();
            Transform hand = anim != null && anim.isHuman ? anim.GetBoneTransform(HumanBodyBones.RightHand) : null;
            Transform holder = hand != null ? hand : rishi;
            GameObject trishul = FindOrBuild(holder, "Weapon_Trishul", () => BuildTrishul(sona, geru));
            GameObject dhanush = FindOrBuild(holder, "Weapon_Dhanush", () => BuildDhanush(geru));
            if (hand == null)
            {
                trishul.transform.localPosition = new Vector3(0.45f, 1.0f, 0.1f);
                dhanush.transform.localPosition = new Vector3(0.45f, 1.1f, 0.1f);
            }

            var wmSo = new SerializedObject(wm);
            var slots = wmSo.FindProperty("slots");
            for (int i = 0; i < slots.arraySize; i++)
            {
                var slot = slots.GetArrayElementAtIndex(i);
                bool isTrishul = slot.FindPropertyRelative("id").enumValueIndex == (int)WeaponId.Trishul;
                slot.FindPropertyRelative("visual").objectReferenceValue = isTrishul ? trishul : dhanush;
            }
            wmSo.ApplyModifiedPropertiesWithoutUndo();
            dhanush.SetActive(false); // shuru me sirf Trishul

            // --- Dhanush pickup (GDD: level ke beech milta hai) ---
            if (GameObject.Find("Pickup_Dhanush") == null)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.name = "Pickup_Dhanush";
                p.transform.position = new Vector3(5f, 1f, -3f);
                p.transform.localScale = new Vector3(0.3f, 1.2f, 0.3f);
                p.GetComponent<MeshRenderer>().sharedMaterial = sona;
                p.GetComponent<Collider>().isTrigger = true;
                p.AddComponent<WeaponPickup>();
            }

            // --- Door ka dummy (teer test, ~12 m) ---
            if (GameObject.Find("Training_Dummy_Far") == null)
            {
                var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                d.name = "Training_Dummy_Far";
                d.transform.position = new Vector3(8f, 1f, -9f);
                d.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
                d.GetComponent<MeshRenderer>().sharedMaterial = stone;
                d.AddComponent<Health>();
                d.AddComponent<Hurtbox>();
                d.AddComponent<TrainingDummy>();
            }
        }

        static GameObject FindOrBuild(Transform parent, string name, System.Func<GameObject> build)
        {
            Transform t = parent.Find(name);
            if (t != null) return t.gameObject; // dobara chalane par duplicate nahi
            GameObject go = build();
            go.name = name;
            go.transform.SetParent(parent, false);
            return go;
        }

        // Trishul: lamba danda + upar teen nok. Colliders hata do — hathiyar physics me na takraye.
        static GameObject BuildTrishul(Material sona, Material geru)
        {
            var root = new GameObject("Weapon_Trishul");
            Part(root.transform, PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.035f, 0.75f, 0.035f), geru);
            Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 1.15f, 0f), new Vector3(0.03f, 0.3f, 0.03f), sona);
            Part(root.transform, PrimitiveType.Cube, new Vector3(-0.09f, 1.08f, 0f), new Vector3(0.03f, 0.18f, 0.03f), sona);
            Part(root.transform, PrimitiveType.Cube, new Vector3(0.09f, 1.08f, 0f), new Vector3(0.03f, 0.18f, 0.03f), sona);
            Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.99f, 0f), new Vector3(0.22f, 0.03f, 0.03f), sona);
            return root;
        }

        // Dhanush: patla lamba dhanush (do jhuke hisse) — placeholder.
        static GameObject BuildDhanush(Material geru)
        {
            var root = new GameObject("Weapon_Dhanush");
            var top = Part(root.transform, PrimitiveType.Cube, new Vector3(0f, 0.3f, -0.05f), new Vector3(0.03f, 0.62f, 0.03f), geru);
            top.localRotation = Quaternion.Euler(-15f, 0f, 0f);
            var bottom = Part(root.transform, PrimitiveType.Cube, new Vector3(0f, -0.3f, -0.05f), new Vector3(0.03f, 0.62f, 0.03f), geru);
            bottom.localRotation = Quaternion.Euler(15f, 0f, 0f);
            return root;
        }

        static Transform Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material m)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            return go.transform;
        }

        static void AddCombatTest(Transform rishi, Material stone, Material sona)
        {
            // Rishi: CombatSystem (hitbox) + Hurtbox (aage asur ke vaar ke liye, Team Player).
            if (rishi.GetComponent<CombatSystem>() == null) rishi.gameObject.AddComponent<CombatSystem>();
            if (rishi.GetComponent<Hurtbox>() == null)
            {
                var hb = rishi.gameObject.AddComponent<Hurtbox>();
                var so = new SerializedObject(hb);
                so.FindProperty("team").enumValueIndex = (int)Team.Player;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Teen putle spawn ke paas, ek line me — combo ke beech disha badal ke test karo.
            for (int i = 0; i < 3; i++)
            {
                string n = $"Training_Dummy_{i + 1}";
                if (GameObject.Find(n) != null) continue; // dobara chalane par duplicate nahi
                var d = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                d.name = n;
                d.transform.position = new Vector3(-3f + i * 3f, 1f, 5f);
                d.transform.localScale = new Vector3(0.7f, 1f, 0.7f); // ~2 m lamba, Rishi jitna
                d.GetComponent<MeshRenderer>().sharedMaterial = i == 1 ? sona : stone;
                d.AddComponent<Health>();      // default 100 HP
                d.AddComponent<Hurtbox>();     // default Team.Asur
                d.AddComponent<TrainingDummy>();
                // Static nahi — dummy hit par hilta hai.
            }
        }

        // ---------------- helpers ----------------
        static void AddCapsuleBody(Transform root, Material sona, Material geru)
        {
            // Capsule mesh child ko 1 m upar rakha (pivot pairon par rahe).
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body_Placeholder";
            Object.DestroyImmediate(capsule.GetComponent<CapsuleCollider>()); // CharacterController apna collider laata hai
            capsule.GetComponent<MeshRenderer>().sharedMaterial = sona;
            var body = capsule.transform;
            body.SetParent(root, false);
            body.localPosition = new Vector3(0f, 1f, 0f);
            // "Naak" — kis taraf dekh raha hai wo dikhe
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Facing";
            Object.DestroyImmediate(nose.GetComponent<BoxCollider>());
            nose.transform.SetParent(body, false);
            nose.transform.localPosition = new Vector3(0f, 0.5f, 0.45f);
            nose.transform.localScale = new Vector3(0.25f, 0.15f, 0.3f);
            nose.GetComponent<MeshRenderer>().sharedMaterial = geru;
        }

        static GameObject Block(string name, Vector3 pos, Vector3 size, Material m)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = size;
            if (m != null) go.GetComponent<MeshRenderer>().sharedMaterial = m;
            go.isStatic = true; // baked lighting (M4) ke liye
            return go;
        }

        static GameObject Cylinder(string name, Vector3 pos, Vector3 scale, Material m)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            go.isStatic = true;
            return go;
        }

        static Material Mat(string name, Color c, bool transparent = false)
        {
            string path = $"{MatDir}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing; // dobara chalane par duplicate na bane

            // URP Lit shader; agar project URP nahi hai to Standard par fallback (warna pink dikhega).
            Shader sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(sh);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);

            if (transparent && mat.HasProperty("_Surface"))
            {
                // URP Lit ko transparent banana (dhuan ke liye)
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static void AddToBuildSettings(string path)
        {
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (list.Exists(s => s.path == path)) return;
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
