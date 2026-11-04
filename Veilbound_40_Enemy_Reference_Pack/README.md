# Veilbound 3D References — Original Red-and-Ashen Edition

This package contains **40 individually generated low-poly character reference sheets** from the Veilbound design conversation, grouped into 4 biomes. These are **2D concept images**, not `.blend`, `.glb`, `.fbx`, or game-ready assets. Files are copied without visual edits.

The artistically approved low-poly references are the primary source of truth, not the later purple test variants or biome overview posters.

## Rules for Codex / Tripo

- **Only use the source files under `references/`.** Red and ashen Veil corruption throughout; avoid purple.
- Image-to-3D reconstruction is approximate. Front/side/back images may be conceptually inconsistent; preserve the main silhouette and design features over literal reconstruction of every detail.
- Do not hallucinate shrine creatures in the town, or add new props/weapons/appendages not shown.
- Town Ghoul is a **distorted HUMAN-like humanoid running on all fours**, not a canine or wolf.
- Shrine Gargoyle **replaces** Stone Colossus/Golem; freeze under player observation, extremely fast when unobserved (this is a Unity gameplay behavior, not geometry).
- Basic species such as wolf and cougar need no decorative abilities or added weapons.
- Do not automatically spend API credits on all 40 until a sample has been reviewed. Print model generation and processing cost estimate, configure a budget and stop after initial tests.
- Save generated meshes outside `references/` and do not overwrite reference sheets.

## Suggested output layout

```
references/Town,Forest,Shrine,Cave/<Unit>.png
models_raw/Town,Forest,Shrine,Cave/<Unit>.glb
models_blender/Town,Forest,Shrine,Cave/<Unit>.blend
models_unity/Town,Forest,Shrine,Cave/<Unit>.fbx
previews/Town,Forest,Shrine,Cave/<Unit>.png
manifest.csv
```

Tripo Codex integration documentation: https://developers.tripo3d.ai/en/docs/codex-plugin
Tripo Developer API and Studio use separate balances. Developer console: https://developers.tripo3d.ai/
