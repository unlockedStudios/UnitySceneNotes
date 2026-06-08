# Scene Notes

Scene Notes is a Unity editor tool for attaching portable annotation data to scene and prefab objects.

## Prerequisites

- Unity 6000.0 or newer.
- Odin Inspector installed in the consuming Unity project.

Odin Inspector is intentionally not bundled with this package. Install Odin in the project before adding Scene Notes so the
`Sirenix.OdinInspector` assembly references can resolve.

## Installation

For local development, add this package folder from Unity Package Manager, or add a local path dependency to the consuming
project's `Packages/manifest.json`:

```json
"dev.unlockedstudios.scenenotes": "file:../SceneNotes/SceneNotesPackages/dev.unlockedstudios.scenenotes"
```

For Git installation from this repository, use Unity's package path syntax:

```json
"dev.unlockedstudios.scenenotes": "https://github.com/UnlockedStudios/SceneNotes.git?path=/SceneNotesPackages/dev.unlockedstudios.scenenotes#v1.0.0"
```

## Notes

Scene Notes creates editable project settings at `Assets/SceneNotes/Settings/SceneNoteSettings.asset` in the consuming
project. Package files remain separate from project-owned settings.
