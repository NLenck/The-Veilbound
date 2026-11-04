# Prototype validation - 23 October 2026

Unity 6000.3.9f1 compiles the Windows x64 player. The project uses procedural 3D assets and built-in rendering.

## Logic checks

- Baseline/dilated resources and atomic spending.
- Building upgrades consume timber and stone, leaving iron for crafting.
- Specialist workshop gates, independent gear slots, optional cape removal and cloth production.
- Old suit/weapon saves migrate to slot ownership and workshop access without an automatic cape.
- New resources and equipped slots survive JSON roundtrip.
- Card slot limits, lock persistence, banishes and finite budgets.
- Upgrade checks exercise even-level acquisition, full-slot filtering, distinct offers, off-cycle lock deferral and exact rarity retention. Base odds, normalisation, monotonic lower-tier reduction and Legendary growth are checked through one million luck. Deterministic 100,000-roll samples at 0/100/500 luck agree with their theoretical probabilities within 0.6 percentage points.
- Built-player checks confirm rarity changes actual damage and health, and stronger guard upgrades respect the 56% reduction cap.
- Swept movement stops at props, slides and stays inside the boundary.
- Shared route maps escape a U-shaped obstacle cluster without penetrating props. Connections are clearance-checked and diagonals cannot cut blocked corners. This test caught and corrected a premature-turning stall.

## Built-player checks

- The player body remains one closed connected surface, with normalised skin weights and valid animated poses.
- Armour adds independent renderers over the same body mesh. Cloth, leather and plate can mix across slots. A cape can be crafted and removed independently.
- All ordinary variants and bosses enter at reachable perimeter positions.
- Player movement stops inside the fog's inner edge, independently of the enemy spawn boundary. Checks cover 24 movement directions and an actual player dodge.
- Monster portraits cover the fat emissive-bellied volatile, large back-crystal brute and lean grey-red lobber. Geometry and emission checks verify their distinctive features.
- Spawned health, speed and contact damage match the documented ratios.
- Quick ghouls move at 140% of normal speed, retaining half health/damage. Their claw hands rest at ground level and move through an alternating four-limbed stride. A dedicated portrait captures their crouched silhouette.
- Actual XP shard values are checked for every ordinary variant: normal/ranged/volatile 2.5, quick ghoul 1.75, brute 4. Duplicate reward calls do not generate extra shards.
- Standard husks survive an initial upgraded sword hit.
- Volatile proximity triggers a fuse and reduces speed to 80%. Killing one starts a fresh stationary three-second fuse. The corpse stays still until it explodes and damages both player and nearby enemies.
- Lob projectiles arc while a fixed warning circle remains visible. Leaving avoids the impact; remaining inside takes damage.
- Virtual-controller independent move/aim and dodge, equipped sword effects, clock pausing, save roundtrip, boss thresholds, victory and defeat still function.

## Visual and performance review

Screenshots cover the equipment panel, specialist workshops, raised armour, mixed materials, base body, new enemy examples, a lob and its landing warning, and volumetric fog at the arena edge. Character review views use explicitly baked poses. Combat examples are staged near the player after asserting their actual perimeter spawn locations.

The smoke run also records a short 110-enemy performance sample near the fog boundary in the player log. This is a hardware-specific sanity check, not a full performance certification. Boss timer checks advance the clock directly rather than playing a full fifteen-minute run.

Evidence: `logic-checks.txt`, `Builds/Windows/Smoke/PASS.txt` and images in that directory. Smoke tests use an isolated save and do not advance normal player progression. Full-screen screenshots need a visible game window on this computer.

## Further playtesting

Physical-controller feel, a full-length expedition, balance and progression costs, varied layouts, long sessions and crowded-wave performance remain hands-on work. Route maps describe the static arena; dynamic obstacles would require updating them. Armour fit can need further refinement at extreme poses. Cloth and leather production currently abstract raw-fibre/hide logistics, matching the panel-based city scope.


## Created-model integration — 28 October 2026

The live project and `Builds/Integrated` build now use the created male/female 52-bone players, twenty material/gender plate appearances and all twenty-one generated enemies. Editor and built-game tests pass equipment/body-mask restoration, finite skinning, direct-skeleton locomotion, two-hand grips, appearances/saves, thumbnails, biome spawns, bosses/combat and a 110-enemy crowd sample. Existing character roster/save regressions also pass. Enemy procedural rigs and extreme combat/cloth poses remain provisional. See `ModelIntegration/INTEGRATION_REPORT.md` and its JSON/log evidence. The old Windows build and pre-integration scripts/settings are preserved.
