# Scene Notes

Scene Notes is a Unity editor tool for attaching portable annotation data to scene and prefab objects.

## Prerequisites

- Unity 6000.0 or newer.

## Installation

For local development, add this package folder from Unity Package Manager, or add a local path dependency to the consuming
project's `Packages/manifest.json`:

```json
"dev.unlockedstudios.scenenotes": "file:../SceneNotes/SceneNotesPackages/dev.unlockedstudios.scenenotes"
```

For Git installation from this repository, use Unity's package path syntax. The repository contains the Unity project in
a top-level `SceneNotes` folder, so keep that folder in the `path` value:

```json
"dev.unlockedstudios.scenenotes": "https://github.com/unlockedStudios/UnitySceneNotes.git?path=/SceneNotes/SceneNotesPackages/dev.unlockedstudios.scenenotes"
```

Use the same URL in Unity Package Manager's `Install package from git URL...` field.

After installation, add the component from `Add Component > Scene Notes > Scene Note`, or search for `Scene Note`.

## Notes

Scene Notes creates editable project settings at `Assets/SceneNotes/Settings/SceneNoteSettings.asset` in the consuming
project. Package files remain separate from project-owned settings.
