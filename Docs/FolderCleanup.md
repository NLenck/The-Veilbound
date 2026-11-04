# Folder cleanup — 5 November 2026

Removed 24 obsolete executable build directories, including the older Windows build, temporary review builds and repeated player backups. Removed file contents total 27,434,188,719 bytes (27.4 GB / 25.55 GiB).

Kept the current playable build at `Builds/Integrated/TheVeilbound.exe`. Its complete data folder and dependencies remain beside it. The earlier arm, material-thumbnail and relic-arrow fixes are included in this retained build.

Grouped 47 historical work folders under `_Archive/DevelopmentHistory`. Their reports, source checkpoints and other non-build contents were retained. Old full-build backups are now source/history checkpoints only: their obsolete executables and player data were removed. Old reports and manifests can mention original paths and files that were removed during this cleanup.

Preserved original model sources, Blender masters, rigs, export pipelines, reference packs, Unity project files and editor-tool dependencies. Unity's main Library cache was kept to avoid forcing a complete asset reimport. User save folders outside this project were not touched.

Verification: compared SHA-256 hashes for 3,884 retained files across Assets, Packages, ProjectSettings and the current executable build; every file matched its pre-cleanup contents. This was a filesystem-only cleanup, with no gameplay/source changes. README and these folder-management documents were updated separately.

See `folder-cleanup-plan.json` for the exact removed paths and archive mappings, and `folder-cleanup-result.json` for the result. The original root README is preserved at `_Archive/README-before-cleanup.md`.
