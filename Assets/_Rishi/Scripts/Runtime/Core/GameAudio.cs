// =====================================================================
// GameAudio.cs  —  RISHI: The Tamas War   (Milestone M5)
// ---------------------------------------------------------------------
// KYUN: GDD M5 "Music + SFX". Abhi koi recorded audio asset nahi hai, aur
//       bina awaaz ke combat feeka lagta hai. Isliye awaaz code me hi
//       banti hai (synthesis) — koi license ka jhanjhat nahi, repo halka.
// KYA:  Pehli baar me saare clips ek baar bana ke cache karta hai:
//       hit, clang (guard), swoosh, hurt, death, pickup, deepak (checkpoint),
//       boom (boss slam). Music: tanpura jaisa drone (Sa–Pa) + boss ke liye
//       dhol jaisi taal. Ek singleton GameObject, scene badalne par bhi zinda.
// AAGE: Kahin se bhi GameAudio.Play(Sfx.Hit, pos). LevelDirector music
//       badalta hai (explore ↔ boss). Volume GameSettings se.
// =====================================================================
using UnityEngine;

namespace Rishi.Core
{
    public enum Sfx { Hit, Clang, Swoosh, Hurt, Death, Pickup, Deepak, Boom, Arrow, UiMove, UiSelect }
    public enum MusicTrack { None, Menu, Explore, Boss }

    public class GameAudio : MonoBehaviour
    {
        const int Rate = 44100;
        static GameAudio s_instance;

        AudioClip[] _sfx;
        AudioClip _drone, _bossTaal;
        AudioSource _music, _musicTaal;
        AudioSource[] _voices;      // chhota pool — har SFX ke liye naya AudioSource nahi
        int _nextVoice;
        MusicTrack _track = MusicTrack.None;

        static GameAudio Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("GameAudio");
                    DontDestroyOnLoad(go);
                    s_instance = go.AddComponent<GameAudio>();
                    s_instance.Build();
                }
                return s_instance;
            }
        }

        public static void Play(Sfx s, Vector3 position, float volume = 1f) => Instance.PlayInternal(s, position, volume);
        public static void PlayUi(Sfx s) => Instance.PlayInternal(s, Vector3.zero, 0.7f, ui: true);
        public static void SetMusic(MusicTrack t) => Instance.SetMusicInternal(t);

        // ---------------------------------------------------------------
        void Build()
        {
            _sfx = new AudioClip[System.Enum.GetValues(typeof(Sfx)).Length];
            _sfx[(int)Sfx.Hit]      = Make("hit", 0.18f, (t, i) => Noise(i) * Env(t, 0.002f, 0.08f) * 0.6f + Sine(90f - 200f * t, t) * Env(t, 0.002f, 0.12f));
            _sfx[(int)Sfx.Clang]    = Make("clang", 0.5f, (t, i) => (Sine(820f, t) + 0.6f * Sine(1230f, t) + 0.4f * Sine(1977f, t)) * Env(t, 0.001f, 0.35f) * 0.45f);
            _sfx[(int)Sfx.Swoosh]   = Make("swoosh", 0.22f, (t, i) => Noise(i) * Mathf.Sin(Mathf.PI * t / 0.22f) * 0.35f);
            _sfx[(int)Sfx.Hurt]     = Make("hurt", 0.25f, (t, i) => (Sine(140f - 120f * t, t) * 0.9f + Noise(i) * 0.3f) * Env(t, 0.003f, 0.2f));
            _sfx[(int)Sfx.Death]    = Make("death", 0.8f, (t, i) => (Sine(220f - 180f * t, t) + Noise(i) * 0.4f * (1f - t)) * Env(t, 0.01f, 0.7f) * 0.6f);
            _sfx[(int)Sfx.Pickup]   = Make("pickup", 0.7f, (t, i) => Bell(t < 0.12f ? 587f : t < 0.24f ? 740f : 880f, t % 0.12f + (t >= 0.24f ? t - 0.24f : 0f)) * 0.5f);
            _sfx[(int)Sfx.Deepak]   = Make("deepak", 1.4f, (t, i) => (Bell(440f, t) + 0.5f * Bell(660f, t)) * 0.45f);
            _sfx[(int)Sfx.Boom]     = Make("boom", 0.9f, (t, i) => (Sine(55f - 25f * t, t) * 1.0f + Noise(i) * 0.5f * Mathf.Exp(-t * 8f)) * Env(t, 0.003f, 0.8f));
            _sfx[(int)Sfx.Arrow]    = Make("arrow", 0.15f, (t, i) => Noise(i) * Env(t, 0.001f, 0.12f) * 0.3f + Sine(1400f - 4000f * t, t) * Env(t, 0.001f, 0.06f) * 0.2f);
            _sfx[(int)Sfx.UiMove]   = Make("ui_move", 0.06f, (t, i) => Sine(660f, t) * Env(t, 0.001f, 0.05f) * 0.3f);
            _sfx[(int)Sfx.UiSelect] = Make("ui_select", 0.3f, (t, i) => Bell(880f, t) * 0.4f);

            _drone = MakeDrone();
            _bossTaal = MakeTaal();

            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true; _music.playOnAwake = false; _music.spatialBlend = 0f;
            _musicTaal = gameObject.AddComponent<AudioSource>();
            _musicTaal.loop = true; _musicTaal.playOnAwake = false; _musicTaal.spatialBlend = 0f;

            _voices = new AudioSource[12];
            for (int i = 0; i < _voices.Length; i++)
            {
                var v = new GameObject("Voice" + i).AddComponent<AudioSource>();
                v.transform.SetParent(transform);
                v.playOnAwake = false;
                v.spatialBlend = 0.6f;      // isometric camera door hai — poora 3D hoga to awaaz bahut dheemi
                v.minDistance = 8f; v.maxDistance = 60f;
                _voices[i] = v;
            }
        }

        void Update()
        {
            // Settings menu me volume badla to turant suno.
            float mv = GameSettings.MusicVolume;
            if (_music != null) _music.volume = mv * 0.55f;
            if (_musicTaal != null) _musicTaal.volume = mv * 0.7f;
        }

        void PlayInternal(Sfx s, Vector3 pos, float volume, bool ui = false)
        {
            AudioSource v = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            v.transform.position = pos;
            v.spatialBlend = ui ? 0f : 0.6f;
            v.ignoreListenerPause = ui; // pause menu ki awaaz pause me bhi sunai de
            v.pitch = ui ? 1f : Random.Range(0.93f, 1.07f); // har hit thoda alag — robotic na lage
            v.PlayOneShot(_sfx[(int)s], volume * GameSettings.SfxVolume);
        }

        void SetMusicInternal(MusicTrack t)
        {
            if (t == _track) return;
            _track = t;
            if (t == MusicTrack.None) { _music.Stop(); _musicTaal.Stop(); return; }
            if (_music.clip != _drone || !_music.isPlaying) { _music.clip = _drone; _music.Play(); }
            _music.pitch = t == MusicTrack.Menu ? 0.94f : 1f; // menu thoda gehra
            if (t == MusicTrack.Boss) { _musicTaal.clip = _bossTaal; _musicTaal.Play(); }
            else _musicTaal.Stop();
        }

        // ---------------- Synthesis helpers ----------------
        delegate float SampleFn(float t, int i);

        static AudioClip Make(string name, float seconds, SampleFn f)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)Rate, i), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Sine(float hz, float t) => Mathf.Sin(2f * Mathf.PI * hz * t);
        static float Env(float t, float attack, float decay) => t < attack ? t / attack : Mathf.Exp(-(t - attack) / Mathf.Max(decay * 0.35f, 0.001f));
        static float Bell(float hz, float t) => (Sine(hz, t) + 0.4f * Sine(hz * 2.76f, t) + 0.2f * Sine(hz * 5.4f, t)) * Env(t, 0.002f, 0.9f);

        // Sasta deterministic noise (System.Random per sample mehenga hai)
        static float Noise(int i)
        {
            uint x = (uint)i * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
            return ((x >> 22) ^ x) / (float)uint.MaxValue * 2f - 1f;
        }

        // Tanpura jaisa drone: Pa – Sa – Sa – Sa(neeche) pluck, har pluck me bahut saare harmonics.
        // D (Sa = 146.8 Hz). 8 second ka loop.
        static AudioClip MakeDrone()
        {
            const float len = 8f;
            int n = (int)(len * Rate);
            var data = new float[n];
            float sa = 146.83f;
            float[] notes = { sa * 1.5f / 2f, sa, sa, sa / 2f };
            for (int k = 0; k < 8; k++)
            {
                float freq = notes[k % 4];
                int start = (int)(k * (len / 8f) * Rate);
                for (int i = start; i < n; i++)
                {
                    float t = (i - start) / (float)Rate;
                    float env = Mathf.Exp(-t * 0.55f);
                    if (env < 0.01f) break;
                    float s = 0f;
                    for (int h = 1; h <= 7; h++) s += Mathf.Sin(2f * Mathf.PI * freq * h * t + h) / (h * 1.3f);
                    data[i] += s * env * 0.11f;
                }
            }
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i] + Mathf.Sin(2f * Mathf.PI * sa / 2f * i / Rate) * 0.04f, -1f, 1f);
            var clip = AudioClip.Create("drone", n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Dhol jaisi taal (boss): 8 beat, 120 bpm — "dha . dhin dha . dha dhin ."
        static AudioClip MakeTaal()
        {
            const float beat = 0.25f;
            int n = (int)(beat * 16 * Rate);
            var data = new float[n];
            int[] pattern = { 2, 0, 1, 0, 2, 1, 0, 1, 2, 0, 1, 0, 2, 1, 1, 0 }; // 2 = bhaari (bayan), 1 = halki (dayan)
            for (int b = 0; b < pattern.Length; b++)
            {
                if (pattern[b] == 0) continue;
                int start = (int)(b * beat * Rate);
                bool heavy = pattern[b] == 2;
                for (int i = start; i < n && i < start + Rate / 2; i++)
                {
                    float t = (i - start) / (float)Rate;
                    float s = heavy
                        ? Mathf.Sin(2f * Mathf.PI * (70f + 60f * Mathf.Exp(-t * 30f)) * t) * Mathf.Exp(-t * 7f)
                        : (Mathf.Sin(2f * Mathf.PI * 330f * t) * 0.6f + Noise(i) * 0.4f) * Mathf.Exp(-t * 22f);
                    data[i] += s * (heavy ? 0.55f : 0.35f);
                }
            }
            var clip = AudioClip.Create("taal", n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
