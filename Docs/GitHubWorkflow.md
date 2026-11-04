# GitHub workflow

The main repository is https://github.com/NLenck/The-Veilbound.

## Open the project

1. Install Git and Git LFS, then run `git lfs install`.
2. Clone this repository normally (avoid downloading GitHub's source ZIP for development).
3. Run `git lfs pull` if LFS downloads were skipped.
4. Open the repository root in Unity Hub using Unity **6000.3.9f1**.
5. Open `Assets/Veilbound/Scenes/HollowCourt.unity`.

Unity regenerates Library and local editor state. Assets, their .meta files, Packages and ProjectSettings are versioned. Editor serialization uses text. Keep .meta files with their assets when moving or renaming files.

## Save changes

Commit related source and asset changes together. Git LFS tracks binary art/audio and the large generated Unity asset files listed in `.gitattributes`; ordinary scenes, prefabs, materials and metadata remain regular Git text. Run `git status` and review the staged changes before committing. Push both Git history and the associated LFS objects using a normal `git push`.

Do not commit Unity caches, executable builds, old backup folders, local saves or credentials. The .gitignore applies to the main project and nested validation projects. Historical snapshots and duplicate unitypackage/staging exports remain local; current authoring sources, models and pipeline scripts are included.

## Playable releases

Build the game, then ZIP the entire `Builds/Integrated` folder, including its data directory and DLLs. Upload that ZIP as a GitHub Release attachment associated with a version tag. Do not commit executable builds to the source repository.

Original art authoring scripts currently use some `C:/The Veilbound` paths. The normal Unity project opens from another clone location; rerunning those external art pipelines may require updating their paths or placing the clone at the original location.
