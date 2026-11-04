# The Veilbound — playable prototype

Launch `Builds/Windows/TheVeilbound.exe`, keeping its data folder and DLLs beside it. To edit, add this project to Unity Hub with **Unity 6000.3.9f1**, open `Assets/Veilbound/Scenes/HollowCourt.unity`, and press Play.

## Start here

Build through **Settlement**, equip through **Equipment**, then enter a standard 15-minute expedition or the 3-minute trial. Bosses enter at one-third, two-thirds and the end of the timer. Kill all three to win. The trial currently awards the same rewards and tier unlocks, for faster iteration.

WASD/left stick moves, mouse/right stick aims, Space/controller south button or RB dodges, and Escape/Start pauses. Hold right mouse to retain facing while moving. Menus support mouse, arrows/Tab + Enter, and D-pad/stick + A. Attacks activate automatically.

## Level-ups, rarity and luck

New abilities are offered on even character levels (2, 4, 6, ...) while fewer than four ability slots are filled, including the starting sword. These rounds prioritise unowned abilities. If only two remain, the third card is a passive/owned upgrade. Odd levels, and every level after the slots fill, offer passives or upgrades to owned abilities. Rerolls keep the same round rules. If every remaining ability is banished, normal upgrade choices continue.

Every fresh card rolls its own rarity: **Common 50%, Uncommon 25%, Rare 15%, Epic 8%, Legendary 2%** at zero luck. Strength multipliers are **1 / 1.25 / 1.5 / 1.85 / 2.5**. Descriptions show the actual gain. New abilities of higher rarity start with bonus damage; owned ability upgrades and passive bonuses scale with rarity. Damage reduction remains capped at 56%.

The Fortune in ashes passive grants 10 luck at Common rarity. For luck L, let B = 1 + max(0,L)/100. Unnormalised weights are **0.50, 0.25, 0.15 B, 0.08 B², 0.02 B³**; divide each by their sum. Common and Uncommon both decrease as luck rises. Legendary keeps increasing; Rare and Epic can eventually decrease as Legendary takes over. Luck 100 gives approximately **32.7 / 16.3 / 19.6 / 20.9 / 10.5%**. These are tunable prototype values. Luck resets with the run, and affects future rolls, including rerolls. Locks preserve the exact rolled rarity and wait for an eligible round.

The choice screen displays luck, current odds and labelled rarity colours. Epic uses crimson to retain the game's red corruption palette.

## Settlement and crafting

All building construction/upgrades cost **timber and stone**. Timber Yard, Stone Quarry and Ironworks produce basic resources. **Weaver produces cloth**. Invest in any combination of workshops:

- **Blacksmith:** weapons and plate; each level also adds baseline weapon damage.
- **Tailor:** cloth equipment and optional capes.
- **Leatherworker:** leather production and leather equipment.

Each workshop level unlocks the corresponding crafting tier. Gear consumes iron, cloth or leather, with boss essence also required for tier two. Buildings do not consume iron for upgrades.

## Equipment

Nine independent slots: **helm, shoulders, chest, gloves, belt, bracers, legs, boots and cape**. Choose a material and slot, craft, cycle through owned pieces, or remove the item. Eight body slots support cloth, leather and plate at two tiers; capes are cloth. The cape starts unequipped. The base shirt/trousers/shoes are part of the underlying character.

Armour is separate geometry over the connected skinned body: hollow chest shells, shoulder plates, cuffs, gloves, leg guards and boots. It follows the skeleton instead of replacing the body's colour. Cloth grants experience gain; leather balances health/protection; plate adds more health/protection and some weight. Cloth colour applies to cloth equipment.

The greatsword remains a separate weapon slot. Reaping Circle and Oathblades use its equipped model. There are four automatic ability slots, including cleave, plus passives. Runs provide three rerolls, two banishes and one lock.

## Enemies

All ordinary enemies and bosses spawn through reachable locations at the **arena perimeter**.

| Enemy | Health | Speed | Contact damage |
|---|---:|---:|---:|
| Standard | 100% | 100% | 100% |
| Lobber | 180% | 80% | 100% |
| Brute | 200% | 55% | 150% |
| Quick ghoul | 50% | 140% | 50% |
| Volatile | 100% | 100%; 80% when triggered | 100% |

These are relative to a standard husk at the same time/tier. Quick ghouls crawl on all fours with alternating planted claw hands and hind feet. XP rewards are 100% for standard/lobber/volatile, 70% for quick ghouls and 160% for brutes (2.5 / 1.75 / 4 base XP respectively). Boss rewards are unchanged. Standard health starts at 72 and grows with time/tier. Lobbers throw arcing projectiles at a fixed area; its amber warning remains visible during the 1.4-second flight. Leaving the circle avoids the impact.

Volatile husks trigger within 4.8 metres and start a visible three-second fuse. Killing one stops it and starts a fresh three-second fuse at its death location. Its 3.4-metre blast damages the player and all living enemies, including bosses and other volatile husks. Baseline blast damage is 64 with tier scaling. Rewards are awarded only once.

Shared route maps use four body-clearance sizes and verified connections to route around prop clusters. Swept collision stops walking/dodging through props. Direct attacks/projectiles respect props; lobbed attacks travel over them.

The ground extends beneath depth-aware volumetric Veil fog. Dense charcoal and black storm clouds surround the playable boundary, with sparse red-orange embers. Walking and dodging stop at the inner cloud edge (30-metre boundary, including player clearance); enemies still emerge from the outer perimeter. Corruption crystals, abilities, equipment accents and UI highlights use red.

Volatile husks have swollen bodies and glowing bellies, brutes carry a large back crystal cluster, and lobbers are lean with grey-red skin. Crystal growth is the visible mark of the Veil on every variant.

## Time, saves and scope

One real minute equals one city hour in the hub. Expedition acceleration is `2 + 0.1 * (tier - 1)`. Level-up/manual pause, results and a closed application produce nothing. Death/retreat retain production and boss essence already earned. There is no offline catch-up.

Saves are in `%USERPROFILE%/AppData/LocalLow/Veilbound/The Veilbound Prototype/settlement-v1.json`, with a `.bak` copy. Old whole-suit unlocks migrate to individual pieces and sword unlocks retain blacksmith access. Old saves do not gain an automatic cape. New resources, workshop levels, item ownership and equipped slots persist. Interrupted expeditions do not resume.

Pressure, population, hunger, corruption and physical city placement remain outside this slice. Art and balance are provisional.

## Build and verification

Editor **Veilbound → Generate scene and build Windows** runs logic checks, prepares assets and builds the player. **Run logic checks** validates economy, equipment, migration, card rules and routing.

Run with `-veilSmoke` for isolated runtime tests using a separate save. Evidence is in `Builds/Windows/Smoke`, including `PASS.txt`. The game window must be visible for screenshots on this computer. Combat review shots stage examples near the player after verifying their real edge spawn positions. Character portraits use baked review poses.

See `Docs/Validation.md` for coverage. Physical-controller feel, full-run balance, varied layouts and crowded-wave performance still need hands-on testing.
