// =====================================================================
// MixamoRishiSetup.cs  —  RISHI: The Tamas War   (Editor-only tool, M2)
// ---------------------------------------------------------------------
// KYUN: Mixamo se aaye FBX ko haath se set karna (Humanoid rig, avatar copy,
//       loop, bake-into-pose, Animator Controller, scene me lagana) lamba aur
//       galti wala kaam hai. Ye menu ek click me sab karta hai, har baar same —
//       naya character ya animation aaye to bas dobara chalao.
// KYA:  Menu: Tools ▸ RISHI ▸ Setup Mixamo Rishi
//         1) Assets/ThirdParty/Mixamo/ ke FBX ki import settings set karta hai
//            (Mixamo na ho to Assets/ThirdParty/StarterAssets/ — Unity Starter
//            Assets ka humanoid, sirf temporary placeholder; same file naam)
//         2) Assets/_Rishi/Art/Animation/AC_Rishi.controller banata hai
//         3) M2_TestArena me "Rishi" ka capsule hata kar Mixamo model lagata hai
// AAGE: Runtime me PlayerAnimator is controller ke states (RishiAnim naam) chalata
//       hai. TestArenaBuilder bhi TryAttachModel() bulata hai, taaki arena
//       dobara banao to capsule nahi, model aaye.
// FILES (Mixamo se download karke yahi naam do):
//       Rishi_Character.fbx  (zaroori — T-pose, "With Skin")
//       Idle.fbx, Run.fbx    (zaroori — "Without Skin", Run "In Place")
//       Walk, Jump, Fall, Roll, WallRun, Hang, Climb .fbx (optional — na ho to fallback)
//       Attack1/2/3, HeavyCharge, Heavy .fbx (optional, M3 — na ho to attack state nahi)
// =====================================================================
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Rishi.Player;

namespace Rishi.EditorTools
{
    public static class MixamoRishiSetup
    {
        // Third-party assets apne folder me (project coding rule), hamara generated controller _Rishi me.
        // Pehla folder jisme Rishi_Character.fbx ho wahi source — Mixamo hamesha pehle (GDD choice),
        // StarterAssets sirf tab tak jab tak Mixamo files nahi aatin.
        static readonly string[] SourceDirs = { "Assets/ThirdParty/Mixamo", "Assets/ThirdParty/StarterAssets" };
        static string MixamoDir
        {
            get
            {
                foreach (string d in SourceDirs) if (File.Exists(d + "/Rishi_Character.fbx")) return d;
                return SourceDirs[0]; // kuch nahi mila — error message Mixamo folder ka naam dikhaye
            }
        }
        static string CharacterPath => MixamoDir + "/Rishi_Character.fbx";
        const string AnimDir = "Assets/_Rishi/Art/Animation";
        const string ControllerPath = AnimDir + "/AC_Rishi.controller";
        const string ScenePath = "Assets/_Rishi/Scenes/M2_TestArena.unity";

        // Har animation file: naam, loop hogi ya nahi, aur root motion pose me "bake" ho ya nahi.
        // bakeRoot = true  → clip jaisa bana waisa chale (idle/run/hang — jagah par hi).
        // bakeRoot = false → root ki upar/aage ki movement nikaal kar phenk do (root motion band hai),
        //                    kyunki jump/roll/climb me position PlayerController khud badalta hai —
        //                    warna body do baar hilegi (clip + code).
        struct ClipSpec { public string file; public bool loop; public bool bakeRoot; public ClipSpec(string f, bool l, bool b) { file = f; loop = l; bakeRoot = b; } }
        static readonly ClipSpec[] Clips =
        {
            new ClipSpec("Idle",    true,  true),
            new ClipSpec("Walk",    true,  true),
            new ClipSpec("Run",     true,  true),
            new ClipSpec("Jump",    false, false),
            new ClipSpec("Fall",    true,  true),
            new ClipSpec("Roll",    false, false),
            new ClipSpec("WallRun", true,  true),
            new ClipSpec("Hang",    true,  true),
            new ClipSpec("Climb",   false, false),
            // M3 combat — optional; na ho to attack state banti hi nahi (PlayerAnimator skip karta hai)
            new ClipSpec("Attack1",     false, true),
            new ClipSpec("Attack2",     false, true),
            new ClipSpec("Attack3",     false, true),
            new ClipSpec("HeavyCharge", true,  true),
            new ClipSpec("Heavy",       false, true),
        };
        // Inke liye fallback nahi — galat clip (jaise Run) attack me ajeeb lagta hai.
        static readonly string[] NoFallback = { "Attack1", "Attack2", "Attack3", "HeavyCharge", "Heavy" };

        // =================================================================
        [MenuItem("Tools/RISHI/Setup Mixamo Rishi")]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; // kaam na khoye

            // --- Zaroori files check ---
            var missing = new List<string>();
            if (!File.Exists(CharacterPath)) missing.Add("Rishi_Character.fbx");
            if (!File.Exists(ClipPath("Idle"))) missing.Add("Idle.fbx");
            if (!File.Exists(ClipPath("Run"))) missing.Add("Run.fbx");
            if (missing.Count > 0)
            {
                EditorUtility.DisplayDialog("RISHI — Mixamo files nahi mile",
                    $"{MixamoDir}/ me ye files chahiye:\n  • " + string.Join("\n  • ", missing) +
                    "\n\nBaaki (Walk, Jump, Fall, Roll, WallRun, Hang, Climb) optional hain.", "Theek hai");
                return;
            }

            // 1) Import settings
            Avatar avatar = SetupCharacter();
            if (avatar == null || !avatar.isHuman)
            {
                Debug.LogError("[RISHI] Rishi_Character.fbx ka Humanoid avatar nahi bana. FBX ko Mixamo se 'T-pose, With Skin' download karo.");
                return;
            }
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var spec in Clips)
            {
                AnimationClip c = SetupClip(spec, avatar);
                if (c != null) clips[spec.file] = c;
            }

            // 2) Animator Controller
            AnimatorController ac = BuildController(clips);

            // 3) Scene me lagao
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject rishi = GameObject.Find("Rishi");
            if (rishi == null)
            {
                Debug.LogError("[RISHI] M2_TestArena me 'Rishi' nahi mila — pehle Tools ▸ RISHI ▸ Build M2 Test Arena chalao.");
                return;
            }
            TryAttachModel(rishi.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = rishi;

            Debug.Log($"[RISHI] Rishi model ready from {MixamoDir} ({clips.Count} clips: {string.Join(", ", clips.Keys)}). Play dabao.");
        }

        /// <summary>
        /// Rishi root par Mixamo model lagata hai (capsule hata kar). Model ya controller na ho to false —
        /// tab TestArenaBuilder capsule placeholder hi rakhta hai.
        /// </summary>
        public static bool TryAttachModel(Transform root)
        {
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);
            var ac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (modelAsset == null || ac == null) return false;

            // Purana body hatao: capsule placeholder ya pichhla model (dobara chalane par duplicate na bane).
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform ch = root.GetChild(i);
                if (ch.name == "Body_Placeholder" || ch.name == "Body") Object.DestroyImmediate(ch.gameObject);
            }

            // Prefab instance — FBX dobara import ho (naya mesh) to scene me apne aap update ho.
            var body = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, root);
            body.name = "Body";
            body.transform.localPosition = Vector3.zero; // Mixamo ka pivot pairon par — PlayerController ke rays bhi pairon se
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = Vector3.one;

            var anim = body.GetComponent<Animator>();
            if (anim == null) anim = body.AddComponent<Animator>();
            anim.runtimeAnimatorController = ac;
            anim.applyRootMotion = false;        // position sirf CharacterController se
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // camera ke kinare par bhi pose sahi rahe

            // Body par koi collider na ho — CharacterController aur ledge/wall rays se takraayega.
            foreach (var col in body.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(col);

            var pa = root.GetComponent<PlayerAnimator>();
            if (pa == null) pa = root.gameObject.AddComponent<PlayerAnimator>();
            var so = new SerializedObject(pa);
            so.FindProperty("animator").objectReferenceValue = anim;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        // ---------------- Import settings ----------------
        static Avatar SetupCharacter()
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(CharacterPath);
            imp.animationType = ModelImporterAnimationType.Human;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; // Mixamo rig Unity khud map kar leta hai
            imp.importAnimation = false;       // character file me sirf mesh + skeleton
            imp.importCameras = false;
            imp.importLights = false;
            imp.SaveAndReimport();

            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(CharacterPath))
                if (o is Avatar a) return a;
            return null;
        }

        static AnimationClip SetupClip(ClipSpec spec, Avatar avatar)
        {
            string path = ClipPath(spec.file);
            if (!File.Exists(path)) return null; // optional clip — fallback BuildController me

            var imp = (ModelImporter)AssetImporter.GetAtPath(path);
            imp.animationType = ModelImporterAnimationType.Human;
            // Sab clips ek hi avatar par — warna har FBX ka alag skeleton map hota aur pose bigadta.
            imp.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            imp.sourceAvatar = avatar;
            imp.importAnimation = true;
            imp.importCameras = false;
            imp.importLights = false;
            imp.materialImportMode = ModelImporterMaterialImportMode.None; // "Without Skin" — material nahi chahiye

            // Pehle default clip lo (Mixamo ka naam "mixamo.com" hota hai) phir hamari settings.
            ModelImporterClipAnimation[] defs = imp.defaultClipAnimations;
            if (defs == null || defs.Length == 0)
            {
                // Edge case: FBX me animation hi nahi (galti se "With Skin" T-pose aa gaya).
                imp.SaveAndReimport();
                defs = imp.defaultClipAnimations;
                if (defs == null || defs.Length == 0) { Debug.LogWarning($"[RISHI] {spec.file}.fbx me animation nahi mili — skip."); return null; }
            }
            ModelImporterClipAnimation c = defs[0];
            c.name = "Rishi_" + spec.file;          // PlayerAnimator isi naam se roll/climb speed match karta hai
            c.loopTime = spec.loop;
            c.loopPose = spec.loop;
            c.lockRootRotation = true;              // ghoomna hamesha PlayerController ka (rotation drift nahi)
            c.keepOriginalOrientation = true;
            c.lockRootHeightY = spec.bakeRoot;
            c.keepOriginalPositionY = true;
            c.lockRootPositionXZ = spec.bakeRoot;
            c.keepOriginalPositionXZ = true;
            imp.clipAnimations = new[] { c };
            imp.SaveAndReimport();

            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
                if (o is AnimationClip clip && !clip.name.StartsWith("__preview__")) return clip;
            return null;
        }

        // ---------------- Animator Controller ----------------
        static AnimatorController BuildController(Dictionary<string, AnimationClip> clips)
        {
            Directory.CreateDirectory(AnimDir);
            // Har baar naya banao — purane states/transitions ka kachra na rahe.
            // (Scene ka Animator path se reference rakhta hai; GUID bachane ke liye asset delete nahi, khaali karte hain.)
            var ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (ac == null) ac = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            ClearController(ac);

            ac.AddParameter(RishiAnim.SpeedParam, AnimatorControllerParameterType.Float);
            AddFloat(ac, RishiAnim.RollSpeedParam, 1f);
            AddFloat(ac, RishiAnim.ClimbSpeedParam, 1f);

            AnimatorStateMachine sm = ac.layers[0].stateMachine;

            // Locomotion: Speed 0 → Idle, 0.5 → Walk (agar hai), 1 → Run.
            var loco = ac.CreateBlendTreeInController(RishiAnim.Locomotion, out BlendTree tree, 0);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = RishiAnim.SpeedParam;
            tree.useAutomaticThresholds = false;
            tree.AddChild(clips["Idle"], 0f);
            if (clips.TryGetValue("Walk", out var walk)) tree.AddChild(walk, 0.45f);
            tree.AddChild(clips["Run"], 1f);
            sm.defaultState = loco;

            // Baaki states — transitions nahi, PlayerAnimator code se CrossFade karta hai
            // (PlayerController ki state machine hi asli "sach" hai; do jagah logic nahi).
            AnimationClip fall = Pick(clips, "Fall", "Idle");
            AddState(sm, RishiAnim.Jump,    Pick(clips, "Jump", "Fall", "Idle"), null);
            AddState(sm, RishiAnim.Fall,    fall, null);
            AddState(sm, RishiAnim.Roll,    Pick(clips, "Roll", "Run"), clips.ContainsKey("Roll") ? RishiAnim.RollSpeedParam : null);
            AddState(sm, RishiAnim.WallRun, Pick(clips, "WallRun", "Run"), null);
            AddState(sm, RishiAnim.Hang,    Pick(clips, "Hang", "Fall", "Idle"), null);
            AddState(sm, RishiAnim.Climb,   Pick(clips, "Climb", "Hang", "Fall", "Idle"), clips.ContainsKey("Climb") ? RishiAnim.ClimbSpeedParam : null);

            // Attack states sirf tab jab clip ho.
            foreach (string a in NoFallback)
                if (clips.TryGetValue(a, out var ac2)) AddState(sm, a, ac2, null);

            foreach (var spec in Clips)
                if (!clips.ContainsKey(spec.file) && System.Array.IndexOf(NoFallback, spec.file) < 0)
                    Debug.LogWarning($"[RISHI] {spec.file}.fbx nahi hai — fallback clip use hua. Mixamo se laao to dobara Setup chalao.");

            EditorUtility.SetDirty(ac);
            AssetDatabase.SaveAssets();
            return ac;
        }

        static void ClearController(AnimatorController ac)
        {
            for (int i = ac.parameters.Length - 1; i >= 0; i--) ac.RemoveParameter(i);
            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            foreach (ChildAnimatorState s in sm.states)
            {
                // Blend tree sub-asset ke roop me controller me save hota hai — usko bhi hatao.
                if (s.state.motion is BlendTree bt && AssetDatabase.IsSubAsset(bt)) Object.DestroyImmediate(bt, true);
                sm.RemoveState(s.state);
            }
        }

        static void AddFloat(AnimatorController ac, string name, float def)
        {
            ac.AddParameter(new AnimatorControllerParameter { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = def });
        }

        static void AddState(AnimatorStateMachine sm, string name, AnimationClip clip, string speedParam)
        {
            AnimatorState st = sm.AddState(name);
            st.motion = clip;
            st.writeDefaultValues = true;
            if (speedParam != null) { st.speedParameterActive = true; st.speedParameter = speedParam; }
        }

        // Pehla jo clip mile wo — optional animation na ho to game phir bhi chale.
        static AnimationClip Pick(Dictionary<string, AnimationClip> clips, params string[] order)
        {
            foreach (string k in order) if (clips.TryGetValue(k, out var c)) return c;
            return null;
        }

        static string ClipPath(string file) => $"{MixamoDir}/{file}.fbx";
    }
}
