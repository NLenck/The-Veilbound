# Graveyard atmosphere update

Completed 28 October 2026. Run `Builds/Integrated/TheVeilbound.exe`.

Added six forged-iron braziers: four around the shrine approaches and two flanking the cathedral's north entrance. They have low-poly orange/gold flames, gentle scale/light flicker, compact warm light pools and solid stone feet. The six local point lights use no shadows. Their small ranges and limited number keep the lighting local; frame-rate performance was not benchmarked.

The shrine now has a broken stone halo behind its crystal. Two faded burgundy funeral banners on supported spars add identity to the cathedral and participate in the existing interior cutaway. These additions are decorative; the fires do not cause damage.

The grave plots, wide main lanes and combat floor remain clear. Brazier bases and shrine-frame feet have collision and were included in the existing large-body navigation checks. All four cathedral breaches and all 96 sampled perimeter spawns passed again. The scene is 16,358 static triangles and 46 mesh renderers, excluding characters, lights and the fog post-process. The previous pass was 14,846 triangles / 28 renderers.

Editor geometry/route checks, build logic and production asset validation passed. The actual rebuilt game passed enemy/boss entry, actual boss cathedral traversal, cutaway and random village/Graveyard switching checks. Actual-game previews are `gameplay.png`, `cathedral-gameplay.png` and `overview.png`. Evidence is in `validation.json`, `validation.log`, `build.log`, `runtime.log` and `PASS.txt`.

No Tripo requests or paid generation were used. Weapon stats, abilities, character saves, the village layout and arena bounds are unchanged by this pass. The Integrated executable was rebuilt; Builds/Windows remains unchanged.

Updated source snapshots are in `pipeline` and pre-change arena/validator sources are in `Backup`. Repeat validation and building with Unity 6000.3.9f1: `-batchmode -quit -projectPath "C:\The Veilbound" -executeMethod Veilbound.Editor.GraveyardValidation.Build -logFile <path>`. Run the player with `-batchmode -veilSmoke -graveyardReview -logFile <path>` for isolated gameplay checks and captures.
