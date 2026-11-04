> Current version: **Mastered/MASTER_REPORT.md** — ten local colour variants, corrected masters and reversible body masks. The prototype notes below are historical.

# Veilbound plate armour production prototype

Start with ARMOUR_PROTOTYPE_REPORT.md. These are separable but visually failing fitting drafts, not finished game assets. Bulk generation is disabled. No API retries or paid rescue steps are automatic.

## Repeat locally without spending

Use Blender 5.2 and Python with Pillow. Scripts use explicit C:/The Veilbound/ArmourAssetProduction paths. Run prepare_pipeline.py (atlas/configs), inspect_rigs.py in Blender, inspect_prototype.py in Blender against the saved successful raw result, organise_prototype.py in Blender, then fit_prototype.py -- Male and -- Female in Blender. Run verify_exports.py in Blender. This rebuilds drafts from saved raw data and costs zero Tripo credits.

package_prototype.py creates the isolated UnityValidation project and source kit. Open only that validation project with Unity 6000.3.9f1. ArmourPrototypeReview.Run builds four draft prefabs and tests slot toggles plus actual animation. It creates Generated assets and should be run only with that disposable Generated folder absent; it never targets the live game's Generated or Assets directories. ArmourPrototypeReview.Export packages a passed technical test. finalise_prototype.py writes counts/report/ledger and copies validated staging files. Never treat a technical pass as visual acceptance.

The anatomy bands in organise_prototype.py are calibrated to this inspected two-set layout, not a universal five-set classifier. It fails if expected helmets/slots are absent or duplicated. For a different layout, inspect raw axes/components first, set explicit tier regions, then review the assignments. Never assign names to fused slots just to fill the manifest.

## Paid generation gate

run_prototype.ps1 defaults to a dry run. Its SubmitPrototype switch refuses to run when an attempt/result already exists. The present prototype has already been used and must not be resubmitted. Logs preserve result, progress, task ID, starting/remaining balance and validated payload. Group_A/Group_B_PROPOSAL_ONLY.json have submit=false and require a new user decision after fit/coverage review. Every CLI invocation sets TRIPO_CLIENT_APP=codex and TRIPO_DISTRIBUTION_CHANNEL=codex_plugin; credentials remain owned by the official CLI. Do not inspect/copy them.

## Unity review and future integration

The draft package adds ArmourPrototype and the required VeilbornePlayer review dependencies. It does not replace the live hero. Four Generated/*FITTING_DRAFT.prefab assets have nine armour slot renderers sharing one material; source left/right objects remain separate in Blender and in source FBXs. ModularPlateSlots.SetSlotEnabled uses the game's numeric nine-slot order. Rebind by exact bone names to the new 52-bone review rig; the old procedural 17-bone player is incompatible. Chest, belt and legs are weighted across relevant body bones. Cloth/cape uses spine/hips weights without simulation. Runtime body masks and correct complete leg/hand/boot coverage remain required before gameplay integration.

Renderer count can be reduced further by rebuilding a combined equipped mesh only when loadout changes. Prune unused bone palettes, build LODs after silhouette review, and budget cape geometry explicitly. Do not add real-time cloth simulation for every enemy by default.
