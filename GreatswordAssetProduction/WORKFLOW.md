# Repeat the free local processing

The scripts currently target `C:\The Veilbound\GreatswordAssetProduction`. Review and change path constants before reusing them for another asset pack. Source references are read-only.

1. `prepare_local.py` inventories references and creates palette atlases. Preserve the original inventory when auditing this run.
2. Blender background mode: `--background --python pipeline/inspect_scene.py` inventories the saved raw result.
3. `--background --python pipeline/process_swords.py` classifies the ten complete native objects, repairs meshes, fits the socket, assigns materials, exports and renders. The source ordering is specific to this task; inspect any new generation before reusing it.
4. `--background --python pipeline/verify_exports.py` reimports all 20 exports. `--background --python pipeline/verify_completeness.py` checks complete components and winding and packs textures into the Blender file.
5. Copy the three C# sources to their existing new review locations and run Unity `-batchmode -quit -projectPath "C:\The Veilbound" -executeMethod GreatswordImport.Run -logFile <path>`. Graphics are required for preview renders. This rebuilds the new review assets only; inspect the script before adapting paths.
6. Run `prepare_standalone_validation.py`, then Unity on `UnityValidation` with `-batchmode -nographics -quit -executeMethod GreatswordStandaloneValidation.Run`. Require its pass marker/JSON, not just process launch.
7. `finalise_greatswords.py` verifies receipts, geometry, package and unchanged reference/runtime hashes, then writes reports and previews.

The successful paid request and dry-run plan are saved in logs. `run_prototype.ps1` has a persistent duplicate-spend guard. Do not remove the guard or rerun generation without new approval. Every future paid request needs a fresh balance/cost check. Local Blender cleanup, palette changes, previews and exports incur no Tripo credits.
