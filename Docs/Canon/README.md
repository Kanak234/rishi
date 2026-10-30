<div align="center">

# RISHI: The Tamas War

**A Raji-inspired 3D action-adventure rooted in Bharat's Itihasa — built in Unity 6.**

On the night of Navami, Tamasur rises from the darkness beneath the Damodar and begins extinguishing the eternal flames of the tirthas. Rishi, a young warrior from a riverside village, sets out to rekindle them and rescue his guru.

`Status: v1.0.0 — 6 levels, 8 missions (Linux build)`

</div>

---

## Features

- 6 levels, 8 connected missions: Damodar Gaon → Rajrappa → Kashi → Hampi → Ellora → Tamas-lok
- 3 bosses with 3 phases each: Andhvir, Shilaasur, Tamasur
- Sohrai-style story slides before and after every mission
- Run, jump, dodge roll with i-frames, wall-run, ledge climb
- Trishul 3-hit combo + charged heavy attack; Dhanush with aim and charged arrows
- Enemies: Chhaya-bhoot, Pashan-rakshas (guard), Vayu-pishach (flying archers)
- Checkpoint deepaks with save/continue, Jyoti upgrades, mission select
- Main menu, pause, settings (volume, graphics, subtitles, text size, camera shake), HUD
- Procedural music and sound effects

## Missions

| Level | Mission |
| --- | --- |
| 1 · Damodar Gaon | 1. Navami ki Raat |
| 2 · Rajrappa | 2. Sangam ki Chattanein · 3. Chhinnamastika ka Aangan (boss: Andhvir) |
| 3 · Kashi | 4. Deepon ki Seedhiyan · 5. Ganga Aarti ki Raksha |
| 4 · Hampi | 6. Vijayanagara ke Khandahar (boss: Shilaasur) |
| 5 · Ellora | 7. Kailasa ki Gufayein |
| 6 · Tamas-lok | 8. Damodar ke Neeche (boss: Tamasur) |

## Controls

| Action | Keyboard & mouse | Gamepad |
| --- | --- | --- |
| Move | WASD | Left stick |
| Jump | Space | A / Cross |
| Dodge | Left Shift | B / Circle |
| Light attack | Left click | X / Square |
| Heavy attack | Right click | Y / Triangle |
| Climb / interact | F | RB |
| Switch weapon | Q / E | D-pad left / right |
| Aim (Dhanush) | Left Ctrl (hold) | LT (hold) |
| Pause | Esc | Start |

## Running from source

1. Install **Unity 6 LTS** via Unity Hub (Linux or Windows).
2. Clone with Git LFS: `git lfs install && git clone https://github.com/Kanak234/rishi.git`
3. Open the folder in Unity Hub → **Add project from disk**.
4. Menu **Tools ▸ RISHI ▸ Build All Scenes**, open `Assets/_Rishi/Scenes/MainMenu.unity`, press **Play**.
5. Release builds: **Tools ▸ RISHI ▸ Release ▸ Build Linux + Windows** (output in `Builds/`).

## Tech

Unity 6 LTS · URP · C# · Unity Input System · Git LFS

## Author

**Kanak Prabhakar** — [kanakprabhakar.online](https://kanakprabhakar.online) · YouTube: Cooding with Kanak

© 2026 Kanak Prabhakar. All rights reserved.
