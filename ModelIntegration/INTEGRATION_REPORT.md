# Models integrated into The Veilbound

28 October 2026 (Australia/Sydney).

The live Unity project now uses the created male/female Veilborne characters, all ten plate materials on both rigs, and all twenty Town/Shrine enemy models plus the Forest Wolf. **No paid API calls or Tripo credits were used.**

## Launch

Run [TheVeilbound.exe](../Builds/Integrated/TheVeilbound.exe) with its data folder and DLLs beside it. The prior `Builds/Windows` build remains intact. In Unity, use the existing `Assets/Veilbound/Scenes/HollowCourt.unity` scene.

Choose **Male appearance** or **Female appearance** on the character selection screen, or choose the appearance while creating a character. Appearance is saved per character. Existing characters default to male until changed; resources, workshop levels, item ownership, rolls and expedition progression are preserved.

## Player and equipment

The new 52-bone skeletons are used directly with the supplied Generic idle/walk/run clips. Collision-resolved velocity drives movement blending, with continuous stride phases as speed changes. Root motion is off. Additive arm IK reaches the existing greatsword grips, and the existing swing/cast gesture calls remain connected. The legacy character system is retained as a fallback if the new catalog is unavailable.

Nine equipment slots remain independently equippable. Existing item IDs choose the approved Tin/Copper mesh families and their ten material palettes. Capes use the chest material palette, falling back to the weapon material when there is no metal chest piece; cape ownership, crafting costs and the ability to unequip it are unchanged. **Veilcrystal is blood-red metal with black cape and black belt cloth.** Cloth/leather items remain functional using fitted shared geometry with matte material treatment. No new cloth/leather designs were generated.

The body mask and reversible waist tuck are connected to actual equipment state. All removed armour restores 30,352 male / 27,084 female body triangles. Matched body vertices retain the original bone weights and idle pose (within 0.04 mm in the sampled comparison). Review captures bake the current skin pose, avoiding a previous-frame GPU skinning artifact during immediate screenshots.

The compatibility facade maps the new joints for existing inventory portraits, slot anchors, thumbnail cameras and combat callers. Character saves now carry appearance independently of the settlement and preserve old saves without requiring a reset.

## Enemies and biome mapping

Town uses Husk/Shrieker as standard variants, Lobber/Conduit as ranged variants, Brute/Bulwark as heavy variants, Ghoul/Impaler/Ravager/Wolf as fast variants and Burster as the volatile. The wolf is a Town wildlife variant because there is currently no Forest landscape. Town bosses use Brute, Conduit and Bulwark.

Shrine uses Acolyte/Blade Sentinel as standard variants, Cinder Priest/Lamenter as ranged variants, Shrine Guardian as heavy, Gargoyle/Ash Wraith as fast and Ritual Orb as volatile. Bosses use Shrine Guardian, Veil Angel and High Priest. Existing timings, health, damage, rewards, pathfinding, radius rules, projectiles and warning/fuse mechanics are preserved; boss display names match the new models.

All twenty-one enemy source triangle counts match the production manifest. UVs and colour textures are retained. New local 17-joint rigs and inexpensive procedural biped/quadruped/floating motion make the models usable without paid rigging. Red-dominant crystal texture regions supply restrained emission, with the existing hit and fuse property-block effects preserved. Enemies share their saved mesh/material assets. Textures are capped at 1024 pixels for these crowd assets; source textures and production files are unchanged.

| Model | Triangles | Local motion |
|---|---:|---|
| Town/Husk | 2762 | biped |
| Town/Lobber | 3902 | biped |
| Town/Brute | 6310 | biped |
| Town/Ghoul | 3185 | quadruped |
| Town/Burster | 4351 | biped |
| Town/Shrieker | 3074 | biped |
| Town/Bulwark | 6456 | biped |
| Town/Impaler | 4520 | biped |
| Town/Ravager | 4562 | biped |
| Town/Conduit | 6219 | biped |
| Shrine/Acolyte | 3337 | biped |
| Shrine/Cinder_Priest | 4464 | biped |
| Shrine/Ash_Wraith | 4460 | floating |
| Shrine/Shrine_Guardian | 6931 | biped |
| Shrine/Blade_Sentinel | 6429 | biped |
| Shrine/Ritual_Orb | 3836 | floating |
| Shrine/Lamenter | 4578 | floating |
| Shrine/Gargoyle | 6702 | quadruped |
| Shrine/Veil_Angel | 10087 | floating |
| Shrine/High_Priest | 8049 | biped |
| Forest/Wolf | 3047 | quadruped |

## Verified in the game

- Unity 6000.3.9f1 compiled and built `Builds/Integrated/TheVeilbound.exe`; existing economy/progression logic checks passed.
- Editor validation passed for two player rigs, twenty armour prefabs and twenty-one enemy meshes. Player foot-bone movement, two-handed grip reach, finite skinned vertices and full body restoration were checked.
- The built game passed all twenty gender/material appearances, all nine active single-material slots, mixed cloth/leather equipment, helmet removal and body restoration. Equipment portraits and thumbnails rendered using the new rigs.
- All twenty-one enemies retained their triangle counts and animated with finite, bounded geometry. Actual Town/Shrine edge spawns, biome selection, boss death/combat and crowded waves passed.
- Female appearance survived a character/settlement save round trip. The existing character creation, migration, interrupted-write recovery and independent character-switching regression passed with the new models.
- A two-second 110-enemy sample measured approximately **60.1 FPS**, near the game's 60 FPS cap on this machine. This is a short functional simulation sample, not a benchmark of 110 simultaneously visible meshes; many perimeter enemies may be outside the camera.
- All tests used isolated smoke/profile saves. No existing player character saves were rewritten by the test runs.

Evidence: `EDITOR_VALIDATION.json`, `RUNTIME_VALIDATION.json`, `RUNTIME_PASS.txt`, `logs/`, and `Builds/Integrated/Smoke/CharacterRoster/PASS.txt`. Actual Unity captures are under `screenshots/`; `INTEGRATED_MODELS_OVERVIEW.jpg` shows both player models and both maps.

## Animation limits

Enemy motion rigs are **local procedural prototypes**, not hand-authored final animation. Quadruped ground contacts, large weapons, wings, cloth and extreme combat poses still need an art/animation fitting pass. The player uses the supplied locomotion prototypes plus procedural grips/gestures; this integration does not claim a finished dodge/attack animation library or a full-run balance certification. All created models are present and usable in the game now.

## Source, backup and repeat

`INTEGRATED_MODELS.csv` lists all 43 character/armour/enemy entries and their source and Unity paths. Runtime loading is configured by `Assets/Veilbound/Resources/ProductionModels/Catalog.asset`. New source enemies live under `Assets/Veilbound/ProductionModels/Enemies`, with generated skinned meshes, materials and prefabs under `Assets/Veilbound/Resources/ProductionModels/Enemies`. Approved staged player/armour files are imported under `Assets/VeilbornePlayer` and `Assets/VeilboundPlateMasters`.

`backup_before_integration/` retains the previous scripts, scenes, editor code, package/project settings and README; `CHANGED_EXISTING_FILES.json` lists changed pre-existing files. Original reference art, raw generations, Blender masters, production exports and the previous executable were preserved. No new purchases, API jobs or credit expenditure occurred.

To rebuild the catalog/enemy rigs, run `Veilbound.Editor.ProductionModelImport.Run` in Unity batch mode; `Validate` checks them and `Build` produces the separate integrated Windows executable. Run that executable with `-batchmode -veilSmoke -productionModelsReview` for the isolated runtime suite, and `-characterRosterReview` with `-veilSmoke` for the character-save regression. The tested integration C# sources are copied into `pipeline/`.
