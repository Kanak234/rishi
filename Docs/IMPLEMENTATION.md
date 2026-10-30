# RISHI Unreal migration and MCP integration

Source: https://github.com/Kanak234/rishi at `48defa49243ced8d0a5823104ed00e4de7fc79b3`.

## Scope and requirements
Open the original RISHI repository in OpenCode, connect a local MCP server to a separate Unreal 5.8 project, and rebuild gameplay without changing original story, mission sequence, characters, endings, or Sohrai visual direction. Original Unity implementation stays intact as the reference. This integration does not claim a completed game port.

## Architecture (G05, G06, G49)
OpenCode/NVIDIA Nemotron → MCP stdio process (Python SDK 1.30) → project-local file queue → Unreal Python callback on editor main thread. No network listener or arbitrary Python execution tool. No separate model endpoint inside MCP: the model belongs to the OpenCode client. Native Unreal runtime is appropriate for the existing 3D action-adventure rendering, physics, and authoring workflow. A Chromium wrapper would not replace the required native game engine. G49 feasibility evaluated before implementation; use Unreal, not a browser runtime.

## Preservation
Docs/Canon contains byte-identical source snapshots and SHA-256 evidence. `Content/Story/missions.json` exports all eight original missions and every narration line without rewriting. Agents must run `Tools/UnrealMCP/check_canon.py` before and after work. These hashes detect drift; they are not OS access control. Third-party model use follows the user's chosen NVIDIA provider; this setup does not send the whole repository or copy API keys.

## Tasks
1. Preserve canon and original Unity sources.
2. Create Unreal project and main-thread editor bridge.
3. Implement MCP status, canon, actor inspection/creation/transforms, and explicit save tools.
4. Validate real MCP initialization, discovery, requests and OpenCode connection.
5. Validate live Unreal connection; report environment blockers accurately.
6. Rebuild movement/camera and Mission 1 vertical slice first, then combat, checkpoints, narrative, the remaining seven missions, bosses, settings, save/load and release QA. These gameplay tasks remain pending, not completed by creating a project shell.

## UX and safety rules
Open the Rishi project folder in OpenCode Desktop or launch CLI from repo root. Start Unreal using `Tools/open-unreal.sh`. Call `rishi_status`, then `rishi_canon`, then editor tools. Scene mutations require a live main-thread heartbeat; stale or expired requests are rejected rather than replayed. Only actors created by this bridge are transformable. Saving is explicit. No delete, shell, arbitrary code, credential, or Git push tool is exposed. Model auth remains in the user's existing OpenCode profile. Public hosting, commerce, website SEO and mobile web UI are out of scope.

## Sources
- https://opencode.ai/v2/docs/mcp-servers
- https://github.com/modelcontextprotocol/python-sdk
- Unreal Python plugin installed locally: Engine/Plugins/Experimental/PythonScriptPlugin

## Verified connection and Codex handoff
OpenCode model: `nvidia/nvidia/nemotron-3-ultra-550b-a55b`. Installed model catalog verified this exact ID. OpenCode v2 CLI mcp list without a location can report the home context; verify with `opencode api mcp.list --param "location[directory]=$PWD"` from the repository. That project-scoped API returned connected. Live Unreal 5.8.4 editor tools were tested with NullRHI (not rendered gameplay). Epic settings directory write test passed after directory access was granted, resolving the settings error for that process. Codex global/project config directories remain read-only to the available execution tools; the user-facing connect-codex script adds the same server via the official CLI. A restart/reload is needed to populate native tools.

## Settings-popup follow-up
The previous sandbox-launched editor session was stopped on 2026-10-25 after the user reported continuing settings dialogs. A successful write probe in a separate process did not grant an already-running sandbox editor access. The launcher now probes Epic settings before starting and exits with a clear terminal error on failure. Launch from the normal Ubuntu terminal for persistent desktop use. User screenshot and TOML inspection confirm Codex server registration; current chat native tool discovery has not yet loaded rishi_unreal.
