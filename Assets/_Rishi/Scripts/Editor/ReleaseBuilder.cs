// =====================================================================
// ReleaseBuilder.cs  —  RISHI: The Tamas War   (Editor-only, M6)
// ---------------------------------------------------------------------
// KYUN: GDD M6: "Linux + Windows build". Har baar haath se Build Settings
//       kholna, naam/version bharna — galti ka mauka. Ek menu / ek command.
// KYA:  Player settings (naam, company, version, 1080p fullscreen window,
//       Unity splash band) lagata hai, phir Builds/Linux aur Builds/Windows
//       me build banata hai. Command line se bhi chalta hai:
//         Unity -batchmode -quit -projectPath . -executeMethod Rishi.EditorTools.ReleaseBuilder.BuildAllCI
// AAGE: Builds/ folder .gitignore me hai; zip karke itch.io par.
// =====================================================================
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Rishi.EditorTools
{
    public static class ReleaseBuilder
    {
        public const string Version = "1.0.0";

        [MenuItem("Tools/RISHI/Release/Build Linux + Windows")]
        public static void BuildAll()
        {
            ApplyPlayerSettings();
            Build(BuildTarget.StandaloneLinux64, "Builds/Linux/RISHI.x86_64");
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/RISHI.exe");
        }

        [MenuItem("Tools/RISHI/Release/Build Linux")]
        public static void BuildLinux() { ApplyPlayerSettings(); Build(BuildTarget.StandaloneLinux64, "Builds/Linux/RISHI.x86_64"); }

        [MenuItem("Tools/RISHI/Release/Build Windows")]
        public static void BuildWindows() { ApplyPlayerSettings(); Build(BuildTarget.StandaloneWindows64, "Builds/Windows/RISHI.exe"); }

        /// <summary>CI / terminal: scenes dobara banao, phir dono build. Fail ho to exit code 1.</summary>
        public static void BuildAllCI()
        {
            LevelBuilder.BuildAll();
            ApplyPlayerSettings();
            bool ok = Build(BuildTarget.StandaloneLinux64, "Builds/Linux/RISHI.x86_64")
                    & Build(BuildTarget.StandaloneWindows64, "Builds/Windows/RISHI.exe");
            EditorApplication.Exit(ok ? 0 : 1);
        }

        /// <summary>Sirf scenes banao (CI) — editor khole bina.</summary>
        public static void BuildScenesCI()
        {
            LevelBuilder.BuildAll();
            AssetDatabase.SaveAssets();
            EditorApplication.Exit(0);
        }

        static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = "RISHI: The Tamas War";
            PlayerSettings.companyName = "Kanak Prabhakar";
            PlayerSettings.bundleVersion = Version;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;  // Alt-Tab par pause (GDD edge case)
            PlayerSettings.SplashScreen.show = false; // apna KP splash MainMenu me
            PlayerSettings.SplashScreen.showUnityLogo = false;
            AssetDatabase.SaveAssets(); // batch mode me bina save ke build "unknown" naam le leta tha
        }

        static bool Build(BuildTarget target, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var scenes = LevelBuilder.AllScenePaths();
            var opts = new BuildPlayerOptions { scenes = scenes, locationPathName = path, target = target, options = BuildOptions.None };
            BuildReport r = BuildPipeline.BuildPlayer(opts);
            bool ok = r.summary.result == BuildResult.Succeeded;
            Debug.Log($"[RISHI] {target} build: {r.summary.result}, {r.summary.totalSize / (1024 * 1024)} MB, {r.summary.totalErrors} errors → {path}");
            return ok;
        }
    }
}
