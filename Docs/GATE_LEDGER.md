# Gate ledger — MCP integration milestone

Game rebuild and release are not certified by this ledger.

| Gate | Status | Evidence / justification |
|---|---|---|
| G01 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G02 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G03 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G04 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G05 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G06 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G07 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G08 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G09 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G10 | PASS | MCP_TEST_RESULTS.json; LIVE_EDITOR_TEST_RESULTS.json; OPENCODE_MCP_CONNECTION.json; local integration only, not full game QA. |
| G11 | PASS | MCP_TEST_RESULTS.json; LIVE_EDITOR_TEST_RESULTS.json; OPENCODE_MCP_CONNECTION.json; local integration only, not full game QA. |
| G12 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G13 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G14 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G15 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G16 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G17 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G18 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G19 | NOT VERIFIED | Original assets preserved; full third-party license/legal audit required before redistribution, not a blocker to local bridge development. |
| G20 | NOT VERIFIED | Original assets preserved; full third-party license/legal audit required before redistribution, not a blocker to local bridge development. |
| G21 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |
| G22 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G23 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G24 | PASS | MCP_TEST_RESULTS.json; LIVE_EDITOR_TEST_RESULTS.json; OPENCODE_MCP_CONNECTION.json; local integration only, not full game QA. |
| G25 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G26 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G27 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G28 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G29 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G30 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G31 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G32 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G33 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G34 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G35 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G36 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G37 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G38 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G39 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G40 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G41 | NOT APPLICABLE | Local integration of an existing native game; no web, commerce or public deployment changes in this milestone. |
| G42 | PASS | MCP_TEST_RESULTS.json; LIVE_EDITOR_TEST_RESULTS.json; OPENCODE_MCP_CONNECTION.json; local integration only, not full game QA. |
| G43 | PASS | MCP_TEST_RESULTS.json; LIVE_EDITOR_TEST_RESULTS.json; OPENCODE_MCP_CONNECTION.json; local integration only, not full game QA. |
| G44 | NOT APPLICABLE | No release or public deployment in this local integration milestone; game release work remains pending. |
| G45 | NOT VERIFIED | Implementation / executable verification pending; no pass claimed. |
| G46 | NOT APPLICABLE | No release or public deployment in this local integration milestone; game release work remains pending. |
| G47 | PASS | MCP_TEST_RESULTS.json; LIVE_EDITOR_TEST_RESULTS.json; OPENCODE_MCP_CONNECTION.json; local integration only, not full game QA. |
| G48 | NOT APPLICABLE | No release or public deployment in this local integration milestone; game release work remains pending. |
| G49 | PASS | Docs/IMPLEMENTATION.md; original README and canon snapshots; requirements apply to integration milestone only. |

Next permitted action: implement and test local bridge. Live model and editor validation remain NOT VERIFIED until exercised.

## Current integration status
Unreal native editor initialized with NullRHI; live MCP inspection/spawn/move and original-actor protection passed. OpenCode project-scoped API reports connected. NVIDIA model ID verified using installed OpenCode model catalog. GUI rendering/full gameplay QA and full migration remain NOT VERIFIED. Codex configuration writes blocked by the host read-only policy; run the provided connect-codex-rishi.sh from a normal terminal and reload Codex. Source Unity files and canon hashes preserved.
