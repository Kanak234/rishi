# RISHI: The Tamas War — instructions for AI agents (Claude Code / Antigravity)

Source of truth: Game Design Doc → https://claude.ai/code/artifact/a34a5c3b-4325-4a40-ba34-453fb94ea72b
Agar code aur GDD me farak ho, GDD sahi hai — ya pehle GDD update karo, phir code.

## Project
- Unity 6 LTS, URP, C#. Input = Unity Input System (actions code me, `RishiInput.cs`).
- Raji-inspired 3D action-adventure; original story. Hero: Rishi. Final boss: Tamasur. Slice boss: Andhvir.
- Kahani Itihasa ke roop me — game text me "mythology" shabd kabhi nahi.
- Target hardware: HP Victus, RTX 3050 Laptop 4 GB, 16 GB RAM, Ubuntu. 60 fps @ 1080p Medium, VRAM < 3 GB.

## Folder layout
- `Assets/_Rishi/Scripts/Runtime/{Core,Player,Combat,Camera,World}` — game code (asmdef `Rishi.Runtime`)
- `Assets/_Rishi/Scripts/Editor` — editor tools (asmdef `Rishi.Editor`)
- `Assets/_Rishi/{Scenes,Materials,Prefabs,Art,Audio}`
- Third-party assets apne folder me (`Assets/ThirdParty/<name>`), `_Rishi` me nahi.

## Coding rules
- Har script ke upar header: KYUN / KYA / AAGE (kahan flow karta hai). Har important hisse par inline comment: kyun likha, kis kaam ka, aage kya bulata hai. Comments Hinglish (Roman script).
- Namespaces: `Rishi.Core`, `Rishi.Player`, `Rishi.Combat`, `Rishi.CameraSystem`, `Rishi.World`, `Rishi.Enemies`, `Rishi.UI`.
- Tuning values `[SerializeField]` me, GDD ke numbers ke saath comment.
- `Update` me allocation nahi (no LINQ, no `new` lists per frame).
- Events (`Action`) se jodo, direct references kam.
- Edge cases GDD ke "Edge cases" list se — har ek ka code me comment.

## Workflow
1. Milestone (M1–M6, GDD "Task breakdown") ke ek task par kaam karo, scope ke bahar nahi.
2. Scene/prefab ka kaam MCP for Unity tools se karo; console errors MCP se padho.
3. Har change ke baad: Unity console me 0 errors, Play Mode me test, phir report: kya badla, kaun si files, kaise verify kiya.
4. Architecture change ho to pehle batao, phir karo; GDD update karo.

## Git rules (strict)
- Author/committer: `Kanak Prabhakar <kanakprabhakar72@gmail.com>`.
- Commit/PR me koi Claude attribution nahi: no `Co-Authored-By: Claude`, no `Claude-Session:`, no "Generated with Claude Code".
- Push sirf Kanak ke kehne par.
