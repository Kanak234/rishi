// =====================================================================
// GameSettings.cs  —  RISHI: The Tamas War   (Milestone M6)
// ---------------------------------------------------------------------
// KYUN: GDD Settings screen: graphics preset, resolution, volume
//       (music / SFX / voice), aur accessibility (subtitles, camera shake).
//       Menu aur pause dono ek hi settings badlein — isliye ek jagah.
// KYA:  PlayerPrefs me values rakhta hai, Apply() se QualitySettings,
//       fullscreen aur AudioListener.volume par lagata hai.
// AAGE: SettingsPanel (UI) values badalta hai. GameAudio MusicVolume /
//       SfxVolume padhta hai. StoryCutscene Subtitles padhta hai.
// =====================================================================
using UnityEngine;

namespace Rishi.Core
{
    public static class GameSettings
    {
        public static float MasterVolume { get => PlayerPrefs.GetFloat("vol_master", 0.9f); set => PlayerPrefs.SetFloat("vol_master", value); }
        public static float MusicVolume  { get => PlayerPrefs.GetFloat("vol_music", 0.6f);  set => PlayerPrefs.SetFloat("vol_music", value); }
        public static float SfxVolume    { get => PlayerPrefs.GetFloat("vol_sfx", 0.9f);    set => PlayerPrefs.SetFloat("vol_sfx", value); }
        public static int Quality        { get => PlayerPrefs.GetInt("quality", -1);        set => PlayerPrefs.SetInt("quality", value); }
        public static bool Fullscreen    { get => PlayerPrefs.GetInt("fullscreen", 1) == 1; set => PlayerPrefs.SetInt("fullscreen", value ? 1 : 0); }
        public static bool Subtitles     { get => PlayerPrefs.GetInt("subtitles", 1) == 1;  set => PlayerPrefs.SetInt("subtitles", value ? 1 : 0); } // GDD: on by default
        public static bool CameraShake   { get => PlayerPrefs.GetInt("shake", 1) == 1;      set => PlayerPrefs.SetInt("shake", value ? 1 : 0); }  // GDD: off option
        public static int TextSize       { get => PlayerPrefs.GetInt("textsize", 1);        set => PlayerPrefs.SetInt("textsize", value); }       // 0 chhota, 1 normal, 2 bada

        /// <summary>Text size setting ke hisaab se font scale (UI isse multiply karta hai).</summary>
        public static float TextScale => TextSize == 0 ? 0.85f : TextSize == 2 ? 1.25f : 1f;

        public static void Apply()
        {
            // Pehli baar: Medium jaisa preset (GDD target 1080p Medium) — beech wala level.
            int q = Quality;
            if (q < 0 || q >= QualitySettings.names.Length) q = Mathf.Clamp(QualitySettings.names.Length / 2, 0, QualitySettings.names.Length - 1);
            if (QualitySettings.GetQualityLevel() != q) QualitySettings.SetQualityLevel(q, true);

            if (!Application.isEditor && Screen.fullScreen != Fullscreen)
                Screen.fullScreenMode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

            AudioListener.volume = MasterVolume;
            QualitySettings.vSyncCount = 1; // tearing nahi, laptop thanda rahe
            PlayerPrefs.Save();
        }

        // Game shuru hote hi (kisi bhi scene se) settings lagao.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void ApplyOnBoot() => Apply();
    }
}
