# Veilbound / Tripo Codex First Run

This is an unattended-*safe first batch*, not authorization to spend indefinitely. Use the attached 40 image reference assets under `references/`.

## Objective
Build an image-reference-to-low-poly-3D pipeline for Unity enemy assets using Tripo 3D (official Codex plugin, or Tripo CLI as fallback) and Blender Python for inspection / optional normalization. All 40 enemies should be inventoried, but generate **only 3 test models** until reviewed: `Town/Husk.png`, `Forest/Wolf.png`, and `Shrine/Gargoyle.png`.

## Constraints
- The supplied images are the visual source of truth; do not remix or redesign creatures. The overall Veil corruption is red, faceted, and ashen. Purple recolor tests are not authoritative.
- Input images are concept sheets with different views. Use the hero view as primary input, cropping if appropriate without replacing the artwork. Use multi-view input only if a correctly matched front/side/back set can be isolated; do not invent views.
- The Shrine Gargoyle replaces the earlier Stone Colossus. Its freeze-when-observed behavior is for later Unity scripting, not Tripo geometry generation.
- Preserve visual silhouettes and weapons, especially Town Ghoul's *humanoid on all fours* shape (do not turn it into a dog).
- Prefer clearly intentional faceted silhouettes, a compact texture/material palette, and modest topology; do not burn credits on textures, rigging, animation, or high-resolution retopology yet.
- Do not modify/overwrite `references/`; write `models_raw/`, `previews/`, and a manifest of successful and failed tasks.
- No silent retries, auto top-ups, subscription purchases, API-key disclosures, or auto-approval of broad system permissions. Pause when human authorization is needed.
- Check available Tripo developer API credits and document per-job actual credit costs. **Hard total spend cap: 300 developer API credits (approximately US$3)** for this first batch; never exceed it. Check available credits and pricing before starting; if the estimated work exceeds the cap, stop first.

## Deliverables
1. `manifest.csv` tracking all 40 original references, job status, Tripo job IDs, costs, outputs, and review notes.
2. Exactly three test `.glb` meshes in biome folders under `models_raw/`, provided service access works.
3. Three readable renders/previews under `previews/`, plus optional orbit screenshots or Blender validation (mesh count, poly count, bounds, material slots, source name).
4. `FIRST_BATCH_REPORT.md`: how the pipeline was set up, tasks attempted, issues encountered, credit totals and remaining, qualitative silhouette comparison, and specific changes to make before bulk generation.
5. Do not start the remaining 37 models before a new explicit approval.

## Instructions to begin
Inspect `README.md`, `manifest.csv`, and the individual images under `references/`. Verify the Tripo 3D Codex plugin is enabled; if unavailable, use Tripo CLI from the official documentation. Check login and API balance. If browser/device authentication is required, show the URL and one-time code for the user and pause. Then complete the three test models within the budget and review constraints.

Docs: https://developers.tripo3d.ai/en/docs/codex-plugin
Docs CLI fallback: https://developers.tripo3d.ai/en/docs/cli
