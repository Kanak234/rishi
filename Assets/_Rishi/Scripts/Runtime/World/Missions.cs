// =====================================================================
// Missions.cs  —  RISHI: The Tamas War
// ---------------------------------------------------------------------
// KYUN: Poora khel 6 level (jagah) aur 8 mission me bata hai, aur har
//       mission ki kahani pichhle se judti hai. Ye list ek jagah ho to
//       menu (mission chuno), LevelDirector (kahani, agla mission) aur
//       builder (scene ke naam) sab ek hi sach padhein.
// KYA:  Har mission: level ka naam, mission ka naam, scene, shuru ki
//       kahani, boss se pehle ki kahani (agar boss hai), aur ant ki kahani
//       jo agle mission ki taraf le jaati hai.
//
//   Level 1  Damodar Gaon   M1 Navami ki Raat
//   Level 2  Rajrappa       M2 Sangam ki Chattanein · M3 Chhinnamastika ka Aangan (boss Andhvir)
//   Level 3  Kashi          M4 Deepon ki Seedhiyan · M5 Ganga Aarti ki Raksha
//   Level 4  Hampi          M6 Vijayanagara ke Khandahar (boss Shilaasur)
//   Level 5  Ellora         M7 Kailasa ki Gufayein (guru ki mukti)
//   Level 6  Tamas-lok      M8 Damodar ke Neeche (final boss Tamasur)
//
// AAGE: LevelDirector.missionIndex se apna mission padhta hai. MainMenu
//       "Mission chuno" isi se list banata hai. LevelBuilder Scene() naam.
// =====================================================================
using Rishi.UI;

namespace Rishi.World
{
    public class MissionDef
    {
        public string Level;      // jagah (Level 1..6)
        public int LevelNumber;
        public string Title;      // mission ka naam
        public string Goal;       // HUD par shuru me
        public Slide[] Intro;
        public Slide[] BossIntro; // null = boss nahi
        public Slide[] Outro;
    }

    public static class Missions
    {
        public static string Scene(int i) => $"Mission_{i + 1}";
        public static int Count => All.Length;

        public static readonly MissionDef[] All =
        {
            // ---------------- Level 1: Damodar Gaon ----------------
            new MissionDef
            {
                Level = "Damodar Gaon", LevelNumber = 1, Title = "Navami ki Raat",
                Goal = "Gaon ko Chhaya-bhooton se bachao, ghat tak pahuncho",
                Intro = new[]
                {
                    new Slide(Painting.Village, "Damodar ke kinare ek chhota gaon. Navami ki raat — har ghar me deepak, har aangan me naach."),
                    new Slide(Painting.TamasRises, "Usi raat nadi ke neeche ke andhkar se Tamasur jaaga. Uski chhaayaayein gaon ki taraf badhne lagin."),
                    new Slide(Painting.Journey, "Acharya Vasudev ka shishya Rishi, guru ka diya Trishul uthata hai. \"Pehle gaon, phir kuch aur.\""),
                },
                Outro = new[]
                {
                    new Slide(Painting.GuruTaken, "Ghat par pahunch kar Rishi ne dekha — Tamas ki chhaaya Acharya Vasudev ko utha le ja rahi thi."),
                    new Slide(Painting.Journey, "Guru ki aakhri awaaz: \"Rajrappa… jyoti bachao…\" Rishi sangam ki or chal pada."),
                },
            },
            // ---------------- Level 2: Rajrappa ----------------
            new MissionDef
            {
                Level = "Rajrappa", LevelNumber = 2, Title = "Sangam ki Chattanein",
                Goal = "Damodar-Bhairavi sangam ki chattanein paar karo",
                Intro = new[]
                {
                    new Slide(Painting.Journey, "Rajrappa — jahan Bhairavi, Damodar se milti hai. Par raasta nadi ki fisalti chattanon se hokar jaata hai."),
                },
                Outro = new[]
                {
                    new Slide(Painting.Mandir, "Chattanon ke paar mandir dikha. Uski akhand jyoti ab bas ek kaanpti lau thi."),
                },
            },
            new MissionDef
            {
                Level = "Rajrappa", LevelNumber = 2, Title = "Chhinnamastika ka Aangan",
                Goal = "Mandir ke aangan me Dhanush dhoondho, jyoti bachao",
                Intro = new[]
                {
                    new Slide(Painting.Mandir, "Mandir ke aangan me sona chamak raha tha — kisi purane yoddha ka Dhanush."),
                },
                BossIntro = new[]
                {
                    new Slide(Painting.Andhvir, "Wahan Tamasur ka senapati Andhvir khada tha — bhari gada, aur aankhon me sirf andhkar."),
                },
                Outro = new[]
                {
                    new Slide(Painting.JyotiReturns, "Andhvir gira, aur Rajrappa ki jyoti phir jal uthi."),
                    new Slide(Painting.Kashi, "Girte hue usne kaha: \"Agla deep Kashi ka bujhega… aur tera guru Ellora ki gufaon me hai.\""),
                },
            },
            // ---------------- Level 3: Kashi ----------------
            new MissionDef
            {
                Level = "Kashi", LevelNumber = 3, Title = "Deepon ki Seedhiyan",
                Goal = "Ghaton ki seedhiyan chadho, aasmaan ke Vayu-pishach giraao",
                Intro = new[]
                {
                    new Slide(Painting.Kashi, "Kashi ke ghat — hazaaron deepon wali seedhiyan. Par aaj aasmaan Vayu-pishachon se kaala tha."),
                },
                Outro = new[]
                {
                    new Slide(Painting.Aarti, "Seedhiyon ke upar maha-aarti ke teen bade deep bujhe pade the. Aarti ruki, to Kashi andhere me doob jaayegi."),
                },
            },
            new MissionDef
            {
                Level = "Kashi", LevelNumber = 3, Title = "Ganga Aarti ki Raksha",
                Goal = "Aarti ke teeno maha-deep jalao — asur rokne aayenge",
                Intro = new[]
                {
                    new Slide(Painting.Aarti, "Rishi ko teeno maha-deep jalane the — aur Tamas ki sena use rokne ko taiyaar thi."),
                },
                Outro = new[]
                {
                    new Slide(Painting.Aarti, "Teeno deep jale, aur Ganga par aarti ki roshni phail gayi."),
                    new Slide(Painting.Hampi, "Ek naavik ne bataya: dakshin me Hampi ke khandaharon me patthar ke asur jama ho rahe hain — Shilaasur ki sena."),
                },
            },
            // ---------------- Level 4: Hampi ----------------
            new MissionDef
            {
                Level = "Hampi", LevelNumber = 4, Title = "Vijayanagara ke Khandahar",
                Goal = "Khandaharon se raasta banao, Shilaasur ko roko",
                Intro = new[]
                {
                    new Slide(Painting.Hampi, "Vijayanagara ke khandahar — pathar ka rath, toote mandap. Har chattan ke peeche Pashan-rakshas."),
                },
                BossIntro = new[]
                {
                    new Slide(Painting.Shilaasur, "Rath ke saamne Shilaasur utha — Tamasur ka doosra senapati, jiska sharir hi patthar hai."),
                },
                Outro = new[]
                {
                    new Slide(Painting.Shilaasur, "Shilaasur toota. Uske patthar par Ellora ki gufaon ka naksha khuda tha."),
                    new Slide(Painting.Ellora, "Rishi samajh gaya — guru wahin hain, Kailasa ki gufaon me."),
                },
            },
            // ---------------- Level 5: Ellora ----------------
            new MissionDef
            {
                Level = "Ellora", LevelNumber = 5, Title = "Kailasa ki Gufayein",
                Goal = "Gufaon ke teen deep jalao, guru tak pahuncho",
                Intro = new[]
                {
                    new Slide(Painting.Ellora, "Ellora — ek hi pahad se kaata gaya Kailasa mandir. Andar ki gufaayein Tamas se bhari thin."),
                },
                Outro = new[]
                {
                    new Slide(Painting.GuruFreed, "Aakhri gufa me Acharya Vasudev the. Teeno deep jalte hi Tamas ki zanjeerein toot gayin."),
                    new Slide(Painting.TamasLok, "Guru bole: \"Tamasur ka hriday wahin hai jahan se wo jaaga — Damodar ke neeche. Wahan jaana hoga.\""),
                },
            },
            // ---------------- Level 6: Tamas-lok ----------------
            new MissionDef
            {
                Level = "Tamas-lok", LevelNumber = 6, Title = "Damodar ke Neeche",
                Goal = "Andhkar ke lok me utro, Tamasur ka saamna karo",
                Intro = new[]
                {
                    new Slide(Painting.TamasLok, "Nadi ke neeche ek doosra lok — na din, na raat, sirf Tamas. Rishi guru ka ashirvad le kar utra."),
                },
                BossIntro = new[]
                {
                    new Slide(Painting.TamasRises, "Aur wahan, apne andhkar ke singhasan par, Tamasur ne aankhein kholin."),
                },
                Outro = new[]
                {
                    new Slide(Painting.Dawn, "Tamasur ka andhkar bikhar gaya. Ek-ek karke Bharat ke saare teerthon ki jyoti laut aayi."),
                    new Slide(Painting.Village, "Navami ki subah gaon laut kar Rishi ne dekha — budhe chitrakar deewar par uski gaatha ka aakhri chitra bana rahe the."),
                },
            },
        };
    }
}
