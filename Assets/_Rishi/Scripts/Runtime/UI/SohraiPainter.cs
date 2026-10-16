// =====================================================================
// SohraiPainter.cs  —  RISHI: The Tamas War   (Milestone M5)
// ---------------------------------------------------------------------
// KYUN: GDD: cutscenes "Sohrai/Khovar style ke 2D chitra" — Hazaribagh ki
//       deewar-chitrakala. Haath se bane chitra abhi nahi hain, isliye ye
//       class code se chitra "banati" hai: geru deewar, chuna-safed lakeerein,
//       kaale kinare, bindu, tikon wale manav, nadi ki lehrein.
// KYA:  Chhota software rasterizer (circle, line, triangle, dots, lehar) ek
//       Color32[] par; har scene ka ek function (gaon, Tamasur, guru, yatra,
//       mandir, Andhvir, jyoti). Ek baar bana ke Texture2D cache.
// AAGE: StoryCutscene slides isse texture leti hain. Asli artist ke chitra
//       aayein to bas StoryCutscene me Texture badal do — ye class hat jaayegi.
// =====================================================================
using UnityEngine;

namespace Rishi.UI
{
    public enum Painting { Village, TamasRises, GuruTaken, Journey, Mandir, Andhvir, JyotiReturns, Kashi, Aarti, Hampi, Shilaasur, Ellora, GuruFreed, TamasLok, Dawn }

    public static class SohraiPainter
    {
        const int W = 640, H = 360;
        static readonly Texture2D[] s_cache = new Texture2D[System.Enum.GetValues(typeof(Painting)).Length];

        static Color32[] _px;
        static readonly Color32 Geru  = new Color32(150, 62, 34, 255);
        static readonly Color32 GeruD = new Color32(112, 42, 22, 255);
        static readonly Color32 Chuna = new Color32(242, 234, 214, 255);
        static readonly Color32 Kaala = new Color32(22, 16, 14, 255);
        static readonly Color32 Sona  = new Color32(250, 184, 56, 255);
        static readonly Color32 Tamas = new Color32(52, 30, 100, 255);

        public static Texture2D Get(Painting p)
        {
            int i = (int)p;
            if (s_cache[i] != null) return s_cache[i];
            _px = new Color32[W * H];
            Wall();
            switch (p)
            {
                case Painting.Village: Village(); break;
                case Painting.TamasRises: TamasRises(); break;
                case Painting.GuruTaken: GuruTaken(); break;
                case Painting.Journey: Journey(); break;
                case Painting.Mandir: Mandir(false); break;
                case Painting.Andhvir: AndhvirScene(); break;
                case Painting.JyotiReturns: Mandir(true); break;
                case Painting.Kashi: Kashi(); break;
                case Painting.Aarti: Aarti(); break;
                case Painting.Hampi: Hampi(); break;
                case Painting.Shilaasur: ShilaasurScene(); break;
                case Painting.Ellora: Ellora(); break;
                case Painting.GuruFreed: GuruFreed(); break;
                case Painting.TamasLok: TamasLok(); break;
                case Painting.Dawn: Dawn(); break;
            }
            Frame();
            var t = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            // Rasterizer me y = 0 upar hai; Texture2D me neeche — palat ke daalo.
            var flipped = new Color32[W * H];
            for (int y = 0; y < H; y++) System.Array.Copy(_px, y * W, flipped, (H - 1 - y) * W, W);
            t.SetPixels32(flipped); t.Apply();
            _px = null;
            return s_cache[i] = t;
        }

        // ---------------- Scenes ----------------
        static void Village()
        {
            Moon(520, 70, 34);
            River(270);
            for (int i = 0; i < 3; i++) Hut(90 + i * 120, 230, 1f);
            for (int i = 0; i < 5; i++) Person(380 + i * 34, 245, 0.8f, Chuna, i % 2 == 0); // Navami ka naach
            for (int i = 0; i < 6; i++) Deepak(60 + i * 100, 262);
            Tree(600, 210);
        }

        static void TamasRises()
        {
            River(250);
            // Andhkar nadi se uthta hai: bada baingani dhuan + sona aankhein
            FillCircle(320, 210, 120, Tamas);
            FillCircle(250, 170, 70, Tamas);
            FillCircle(400, 160, 80, Tamas);
            FillCircle(320, 110, 70, Tamas);
            Ring(320, 180, 150, 3, Kaala);
            FillCircle(290, 120, 12, Sona); FillCircle(350, 120, 12, Sona);
            FillCircle(290, 120, 5, Kaala); FillCircle(350, 120, 5, Kaala);
            Line(270, 98, 305, 108, 4, Sona); Line(370, 98, 335, 108, 4, Sona);
            // Bujhte deepak (sirf dhuan, lau nahi)
            for (int i = 0; i < 4; i++) { int x = 70 + i * 160; Lamp(x, 300); Wavy(x, 290, 40, Chuna, vertical: true); }
        }

        static void GuruTaken()
        {
            Moon(110, 70, 28);
            River(280);
            // Tamas ka haath guru ko le ja raha hai
            FillTriangle(380, 40, 620, 60, 520, 300, Tamas);
            FillCircle(500, 120, 60, Tamas);
            FillCircle(480, 100, 8, Sona); FillCircle(520, 100, 8, Sona);
            Person(430, 190, 1.1f, Chuna, false); // Acharya Vasudev
            Line(410, 160, 460, 150, 3, Kaala);
            // Rishi neeche se dekh raha hai, haath aage
            Person(180, 250, 1.2f, Sona, true);
        }

        static void Journey()
        {
            Sun(520, 80, 40);
            // Pahadi raasta (tikon pahad) aur door mandir
            FillTriangle(0, 300, 160, 120, 320, 300, GeruD);
            FillTriangle(240, 300, 420, 100, 600, 300, GeruD);
            Outline(240, 300, 420, 100, 600, 300);
            Outline(0, 300, 160, 120, 320, 300);
            Shikhara(470, 180, 0.6f, false);
            Dots(40, 320, 600, 320, 18, Chuna);
            Person(140, 290, 1.2f, Sona, true);
            Trishul(165, 290, 1.2f);
        }

        static void Mandir(bool lit)
        {
            if (lit) Sun(320, 70, 46); else Moon(540, 60, 26);
            River(300);
            Shikhara(320, 270, 1.5f, lit);
            for (int i = 0; i < 6; i++)
            {
                int x = 60 + i * 104;
                if (Mathf.Abs(x - 320) < 90) continue;
                if (lit) Deepak(x, 290); else Lamp(x, 296);
            }
            if (lit) { Person(130, 285, 1f, Sona, true); Person(510, 285, 1f, Chuna, false); } // Rishi + guru
        }

        static void AndhvirScene()
        {
            Moon(90, 60, 24);
            Shikhara(110, 260, 0.9f, false);
            // Andhvir: bada Tamas manav + gada
            Person(420, 300, 2.4f, Tamas, false);
            Line(470, 180, 560, 90, 8, Kaala);
            FillCircle(565, 82, 26, Kaala); Ring(565, 82, 26, 3, Sona);
            FillCircle(410, 150, 6, Sona); FillCircle(432, 150, 6, Sona);
            Person(230, 300, 1.1f, Sona, true);
            Trishul(255, 300, 1.1f);
            Dots(20, 320, 620, 320, 24, Chuna);
        }

        static void Kashi()
        {
            Moon(530, 60, 26);
            River(240);
            // Ghat ki seedhiyan, upar mandir, seedhiyon par deepak
            for (int s = 0; s < 5; s++) Rect(0, 240 - s * 18, 640, 18, s % 2 == 0 ? GeruD : Geru);
            for (int s = 0; s < 5; s++) Line(0, 240 - s * 18, 640, 240 - s * 18, 2, Chuna);
            for (int i = 0; i < 8; i++) Deepak(40 + i * 80, 236 - (i % 3) * 18);
            Shikhara(160, 150, 0.6f, false); Shikhara(460, 150, 0.7f, false);
            for (int i = 0; i < 9; i++) Deepak(30 + i * 70, 300 + (i % 2) * 12); // nadi me tairte deep
        }

        static void Aarti()
        {
            Moon(560, 50, 22);
            River(250);
            for (int s = 0; s < 4; s++) { Rect(0, 250 - s * 20, 640, 20, s % 2 == 0 ? GeruD : Geru); Line(0, 250 - s * 20, 640, 250 - s * 20, 2, Chuna); }
            // Teen maha-deep (oonche stambh par), beech me pujari
            for (int i = 0; i < 3; i++)
            {
                int x = 160 + i * 160;
                Rect(x - 6, 120, 12, 70, Chuna);
                Lamp(x, 118);
                FillTriangle(x - 12, 110, x + 12, 110, x, 70, Sona);
                FillCircle(x, 100, 5, Chuna);
                Ring(x, 92, 34, 2, Sona);
            }
            Person(320, 245, 1f, Chuna, true);
            for (int i = 0; i < 10; i++) Deepak(20 + i * 64, 300 + (i % 2) * 14);
        }

        static void Hampi()
        {
            Sun(540, 70, 36);
            // Gol pathar (boulders) aur toote mandap
            FillCircle(90, 250, 70, GeruD); Ring(90, 250, 70, 2, Kaala);
            FillCircle(170, 270, 45, GeruD); Ring(170, 270, 45, 2, Kaala);
            FillCircle(580, 260, 60, GeruD); Ring(580, 260, 60, 2, Kaala);
            for (int i = 0; i < 4; i++) Rect(250 + i * 40, 170, 14, 110, Chuna);
            Rect(240, 160, 170, 14, Chuna);
            // Pathar ka rath: chabootra + pahiye + shikhar
            Rect(420, 210, 110, 50, Chuna);
            Ring(445, 268, 18, 3, Kaala); Ring(505, 268, 18, 3, Kaala);
            FillTriangle(425, 210, 525, 210, 475, 140, Chuna); Outline(425, 210, 525, 210, 475, 140);
            Dots(30, 320, 610, 320, 26, Chuna);
        }

        static void ShilaasurScene()
        {
            Sun(80, 60, 26);
            // Patthar ka asur: bade gol pathar jode hue
            FillCircle(420, 250, 80, new Color32(90, 80, 100, 255));
            FillCircle(420, 140, 50, new Color32(90, 80, 100, 255));
            FillCircle(340, 200, 34, new Color32(90, 80, 100, 255)); FillCircle(500, 200, 34, new Color32(90, 80, 100, 255));
            Ring(420, 250, 80, 3, Kaala); Ring(420, 140, 50, 3, Kaala);
            FillCircle(402, 132, 9, Sona); FillCircle(438, 132, 9, Sona);
            for (int i = 0; i < 5; i++) Line(380 + i * 18, 230, 372 + i * 20, 300, 2, Kaala); // daraarein
            Person(170, 300, 1.1f, Sona, true);
            Trishul(195, 300, 1.1f);
        }

        static void Ellora()
        {
            // Pahad me kaati gufa: bada kaala muh, andar mandir ki rekha
            Rect(0, 60, 640, 260, GeruD);
            FillTriangle(0, 60, 640, 60, 320, 0, GeruD);
            Rect(150, 120, 340, 200, Kaala);
            FillTriangle(150, 120, 490, 120, 320, 70, Kaala);
            Shikhara(320, 300, 1.1f, false);
            for (int i = 0; i < 5; i++) { Rect(170 + i * 72, 130, 10, 190, Chuna); }
            Person(560, 305, 1f, Sona, true);
            Dots(20, 330, 620, 330, 26, Chuna);
        }

        static void GuruFreed()
        {
            Rect(0, 40, 640, 290, Kaala);
            FillTriangle(0, 40, 640, 40, 320, 12, Kaala);
            for (int i = 0; i < 3; i++) Deepak(160 + i * 160, 200);
            Person(260, 300, 1.3f, Sona, true);   // Rishi
            Person(380, 300, 1.3f, Chuna, true);  // Acharya Vasudev
            // tooti zanjeer
            for (int i = 0; i < 6; i++) Ring(420 + i * 16, 250 + (i % 2) * 6, 6, 2, Chuna);
            Sun(320, 110, 28);
        }

        static void TamasLok()
        {
            Rect(0, 0, 640, 360, new Color32(30, 18, 56, 255));
            for (int i = 0; i < 30; i++) FillCircle((i * 97) % 640, (i * 53) % 300 + 20, 2 + i % 3, Chuna); // bujhe deepon ki chingari
            FillCircle(320, 330, 210, Tamas);
            Ring(320, 330, 210, 3, Kaala);
            for (int i = 0; i < 5; i++) { int x = 200 + i * 60; FillCircle(x, 210 - (i % 2) * 20, 8, Sona); FillCircle(x, 210 - (i % 2) * 20, 3, Kaala); }
            Person(320, 340, 0.9f, Sona, true);
        }

        static void Dawn()
        {
            Sun(320, 230, 70);
            River(260);
            Shikhara(110, 260, 0.8f, true); Shikhara(530, 260, 0.8f, true);
            for (int i = 0; i < 9; i++) Deepak(40 + i * 70, 300 + (i % 2) * 14);
            Person(300, 258, 1.1f, Sona, true); Person(345, 258, 1.1f, Chuna, true);
            for (int i = 0; i < 5; i++) Person(80 + i * 26, 258, 0.6f, Chuna, i % 2 == 0);
        }

        // ---------------- Motifs ----------------
        static void Wall()
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    // Mitti ki deewar: halka daag-dhabba (noise) — sapaat rang na lage
                    float n = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.6f + Mathf.PerlinNoise(x * 0.11f, y * 0.11f) * 0.4f;
                    _px[y * W + x] = Color32.Lerp(GeruD, Geru, 0.55f + n * 0.45f);
                }
        }

        static void Frame()
        {
            // Sohrai border: safed patti, kaali lakeer, bindu ki maala
            Rect(0, 0, W, 10, Chuna); Rect(0, H - 10, W, 10, Chuna);
            Rect(0, 0, 10, H, Chuna); Rect(W - 10, 0, 10, H, Chuna);
            Line(14, 14, W - 14, 14, 2, Kaala); Line(14, H - 15, W - 14, H - 15, 2, Kaala);
            Line(14, 14, 14, H - 15, 2, Kaala); Line(W - 15, 14, W - 15, H - 15, 2, Kaala);
            Dots(24, 22, W - 24, 22, 40, Sona);
            Dots(24, H - 23, W - 24, H - 23, 40, Sona);
        }

        static void River(int y)
        {
            Rect(0, y, W, H - y, new Color32(80, 34, 22, 255));
            for (int r = 0; r < 4; r++) Wavy(0, y + 14 + r * 18, W, Chuna, vertical: false);
        }

        static void Moon(int x, int y, int r) { FillCircle(x, y, r, Chuna); Ring(x, y, r + 8, 2, Chuna); Dots(x - r - 30, y, x + r + 30, y, 0, Chuna); }

        static void Sun(int x, int y, int r)
        {
            FillCircle(x, y, r, Sona);
            Ring(x, y, r, 3, Kaala);
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI / 8f;
                Line(x + (int)(Mathf.Cos(a) * (r + 8)), y + (int)(Mathf.Sin(a) * (r + 8)),
                     x + (int)(Mathf.Cos(a) * (r + 26)), y + (int)(Mathf.Sin(a) * (r + 26)), 3, Sona);
            }
        }

        static void Hut(int x, int baseY, float s)
        {
            int w = (int)(80 * s), h = (int)(50 * s);
            Rect(x - w / 2, baseY - h, w, h, Chuna);
            FillTriangle(x - w / 2 - 12, baseY - h, x + w / 2 + 12, baseY - h, x, baseY - h - (int)(44 * s), Kaala);
            Rect(x - 10, baseY - 30, 20, 30, GeruD);
            Dots(x - w / 2 + 6, baseY - h + 12, x + w / 2 - 6, baseY - h + 12, 7, Geru);
        }

        /// <summary>Sohrai manav: do tikon (seena + ghaghra/dhoti), gol sar, lakeer haath-pair.</summary>
        static void Person(int x, int footY, float s, Color32 c, bool armsUp)
        {
            int h = (int)(60 * s), w = (int)(18 * s);
            int waist = footY - (int)(h * 0.45f), shoulder = footY - (int)(h * 0.85f);
            FillTriangle(x - w, shoulder, x + w, shoulder, x, waist, c);               // upar ka tikon (ulta)
            FillTriangle(x, waist, x - w, footY - (int)(h * 0.12f), x + w, footY - (int)(h * 0.12f), c); // neeche
            FillCircle(x, shoulder - (int)(9 * s), (int)(8 * s), c);
            int th = Mathf.Max(2, (int)(3 * s));
            Line(x - w / 2, footY - (int)(h * 0.12f), x - w / 2, footY, th, c);
            Line(x + w / 2, footY - (int)(h * 0.12f), x + w / 2, footY, th, c);
            if (armsUp) { Line(x - w, shoulder, x - w - (int)(12 * s), shoulder - (int)(18 * s), th, c); Line(x + w, shoulder, x + w + (int)(12 * s), shoulder - (int)(18 * s), th, c); }
            else { Line(x - w, shoulder, x - w - (int)(8 * s), waist, th, c); Line(x + w, shoulder, x + w + (int)(8 * s), waist, th, c); }
        }

        static void Trishul(int x, int footY, float s)
        {
            int top = footY - (int)(90 * s);
            Line(x, footY, x, top, 3, Sona);
            Line(x - (int)(10 * s), top + (int)(12 * s), x + (int)(10 * s), top + (int)(12 * s), 3, Sona);
            Line(x - (int)(10 * s), top + (int)(12 * s), x - (int)(10 * s), top, 3, Sona);
            Line(x + (int)(10 * s), top + (int)(12 * s), x + (int)(10 * s), top, 3, Sona);
            Line(x, top, x, top - (int)(8 * s), 3, Sona);
        }

        static void Shikhara(int x, int baseY, float s, bool lit)
        {
            int w = (int)(90 * s), h = (int)(150 * s);
            Rect(x - w / 2, baseY - (int)(50 * s), w, (int)(50 * s), Chuna);
            FillTriangle(x - w / 2 + 6, baseY - (int)(50 * s), x + w / 2 - 6, baseY - (int)(50 * s), x, baseY - h, Chuna);
            Outline(x - w / 2 + 6, baseY - (int)(50 * s), x + w / 2 - 6, baseY - (int)(50 * s), x, baseY - h);
            for (int i = 1; i < 5; i++) { int yy = baseY - (int)(50 * s) - i * (int)(20 * s); int half = (int)((w / 2 - 6) * (1f - i * 20f * s / (h - 50f * s))); Line(x - half, yy, x + half, yy, 2, Kaala); }
            Rect(x - (int)(12 * s), baseY - (int)(34 * s), (int)(24 * s), (int)(34 * s), Kaala);
            FillCircle(x, baseY - h - (int)(8 * s), (int)(7 * s), lit ? Sona : Kaala); // kalash
            if (lit) Deepak(x, baseY - (int)(36 * s));
        }

        static void Deepak(int x, int y) { Lamp(x, y); FillTriangle(x - 5, y - 6, x + 5, y - 6, x, y - 24, Sona); FillCircle(x, y - 9, 3, Chuna); }
        static void Lamp(int x, int y) { FillTriangle(x - 12, y - 6, x + 12, y - 6, x, y + 4, Kaala); Line(x - 12, y - 6, x + 12, y - 6, 2, Chuna); }

        static void Tree(int x, int baseY)
        {
            Line(x, baseY, x, baseY - 70, 5, Kaala);
            for (int i = 0; i < 18; i++)
            {
                float a = i * 0.7f; int rr = 16 + (i % 4) * 8;
                FillCircle(x + (int)(Mathf.Cos(a) * rr), baseY - 80 + (int)(Mathf.Sin(a) * rr * 0.7f), 4, Chuna);
            }
        }

        // ---------------- Rasterizer ----------------
        static void Put(int x, int y, Color32 c) { if ((uint)x < W && (uint)y < H) _px[y * W + x] = c; }

        static void Rect(int x, int y, int w, int h, Color32 c)
        {
            for (int yy = Mathf.Max(0, y); yy < Mathf.Min(H, y + h); yy++)
                for (int xx = Mathf.Max(0, x); xx < Mathf.Min(W, x + w); xx++) _px[yy * W + xx] = c;
        }

        static void FillCircle(int cx, int cy, int r, Color32 c)
        {
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= r * r) Put(cx + x, cy + y, c);
        }

        static void Ring(int cx, int cy, int r, int th, Color32 c)
        {
            int ro = r + th, ri = Mathf.Max(0, r - th);
            for (int y = -ro; y <= ro; y++)
                for (int x = -ro; x <= ro; x++)
                {
                    int d = x * x + y * y;
                    if (d <= ro * ro && d >= ri * ri) Put(cx + x, cy + y, c);
                }
        }

        static void Line(int x0, int y0, int x1, int y1, int th, Color32 c)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0), 1);
            int r = Mathf.Max(0, th / 2);
            for (int i = 0; i <= steps; i++)
            {
                int x = x0 + (x1 - x0) * i / steps, y = y0 + (y1 - y0) * i / steps;
                for (int dy = -r; dy <= r; dy++) for (int dx = -r; dx <= r; dx++) Put(x + dx, y + dy, c);
            }
        }

        static void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Color32 c)
        {
            int minX = Mathf.Max(0, Mathf.Min(x0, Mathf.Min(x1, x2))), maxX = Mathf.Min(W - 1, Mathf.Max(x0, Mathf.Max(x1, x2)));
            int minY = Mathf.Max(0, Mathf.Min(y0, Mathf.Min(y1, y2))), maxY = Mathf.Min(H - 1, Mathf.Max(y0, Mathf.Max(y1, y2)));
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    float d1 = Edge(x, y, x0, y0, x1, y1), d2 = Edge(x, y, x1, y1, x2, y2), d3 = Edge(x, y, x2, y2, x0, y0);
                    bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
                    if (!(neg && pos)) _px[y * W + x] = c;
                }
        }

        static float Edge(int px, int py, int ax, int ay, int bx, int by) => (px - bx) * (ay - by) - (ax - bx) * (py - by);

        static void Outline(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            Line(x0, y0, x1, y1, 3, Kaala); Line(x1, y1, x2, y2, 3, Kaala); Line(x2, y2, x0, y0, 3, Kaala);
        }

        static void Dots(int x0, int y0, int x1, int y1, int count, Color32 c)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                FillCircle(Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), 3, c);
            }
        }

        static void Wavy(int x, int y, int len, Color32 c, bool vertical)
        {
            for (int i = 0; i < len; i++)
            {
                int o = Mathf.RoundToInt(Mathf.Sin(i * 0.18f) * 4f);
                if (vertical) { Put(x + o, y - i, c); Put(x + o + 1, y - i, c); }
                else { Put(x + i, y + o, c); Put(x + i, y + o + 1, c); }
            }
        }
    }
}
