# Starter gear and matte equipment update

Completed 28 October 2026. Run `Builds/Integrated/TheVeilbound.exe` for the updated playable build. `Builds/Windows` is unchanged.

- All ten plate materials and the shared ten-sword review material use smoothness-map scale 0.45 instead of 1. This reduces highlights without changing tier colours, cloth colours, metallic masks or crystal emission. Runtime legacy sword tint smoothness is 0.20 (Veilcrystal 0.24). Updated import scripts preserve the finish on regeneration.
- Updated both armour Blender masters, 20 tier GLBs, sword Blender source and 20 sword GLB/FBX exports. The new Unity review sword package also contains the reduced-shine finish. Historical preview images and reports describe the prior prototype; use the new Unity images here for the current finish.
- A locally modelled wooden two-handed practice sword uses a single dry-wood material, no metallic reflectivity and 0.12 smoothness. It is 340 Blender triangles / 304 triangles after Unity's FBX import. The origin, +Z blade axis, 1.72 m tip and two-hand markers match the existing weapon sockets. Editable Blender source, GLB and FBX are in this folder.
- Fresh characters start with no equipped armour or cape, no free metal equipment and no free Blacksmith upgrade. Their original tattered base clothing remains. They own the wooden greatsword, which appears in the bag and can be re-equipped after a metal upgrade. It retains the prior starter's 18 base damage; wood mass is 1.8 kg. Metal-tier stats, crafting and recipes are unchanged.
- Wooden weapon state uses an explicit saved equipped-weapon sentinel and an owned inventory entry; it is not a new metal tier. Existing saved characters and gear are preserved. The initializer runs only for fresh characters; legacy whole-suit/weapon migration remains supported.
- Both male and female native rigs passed attachment, sampled movement/swing, no-armour masks and isolated on-disk save/reload checks in the actual rebuilt player. Editor checks also verified metal upgrade/re-equipping wood and legacy equipment preservation. All 20 sword exports passed geometry/UV/material round-trip checks after the finish update. Unity logic, production validation and Integrated build passed.
- An initial batch-mode roster test reached its screenshot stage but that screen-capture API returned null in batch mode. It was stopped and replaced with a dedicated graphics-independent runtime check, which passed. Real character saves were not used for these checks.

No Tripo requests or paid generation were made: **0 additional credits spent**. The last verified Developer API balance from the sword generation remains 220; it was not queried again for this local-only task.

Preview: `STARTER_PREVIEW.jpg`. Current sword finish: `SWORDS_MATTE_PREVIEW.jpg`. Actual character-with-armour previews were refreshed under `GreatswordAssetProduction/previews`. Validation receipts and build/runtime logs are in this folder. Before-change scripts and master Blender files are in `Backup`.

The ten generated metal swords remain a review library; their live weapon-factory activation was not part of this finish/starter update. The new wooden starter sword is connected to both held and sweep weapon paths in the playable game.
