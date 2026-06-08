# Scene Notes Project

This repository contains the Unity project and the distributable Scene Notes Unity Package Manager package.

## Unity Package Manager URL

Install Scene Notes in another Unity project with this Git URL:

```text
https://github.com/unlockedStudios/UnitySceneNotes.git?path=/SceneNotes/SceneNotesPackages/dev.unlockedstudios.scenenotes
```

The `/SceneNotes/` prefix is required because the package folder lives inside the Unity project folder in this
repository.

## Package Location

The package source lives at:

```text
SceneNotesPackages/dev.unlockedstudios.scenenotes
```

## Runtime Build (Windows)

Create a Windows runtime/player build from command line:

```powershell
"C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe" \
	-batchmode -nographics -quit \
	-projectPath "d:\01 - Unity Projects\01 - Libraries\SceneNotes\SceneNotes" \
	-buildWindows64Player "d:\01 - Unity Projects\01 - Libraries\SceneNotes\SceneNotes\Builds\SceneNotesRuntime\SceneNotes.exe" \
	-logFile "d:\01 - Unity Projects\01 - Libraries\SceneNotes\SceneNotes\Builds\SceneNotesRuntime\build.log"
```

### Build Verification

- Confirm the file timestamps in `Builds/SceneNotesRuntime` were updated.
- Confirm `Builds/SceneNotesRuntime/build.log` contains:
	- `DisplayProgressNotification: Build Successful`
	- `Build Finished, Result: Success.`
	- `Exiting without the bug reporter. Application will terminate with return code 0`

### Troubleshooting Notes

- If command output returns before build status appears, check whether a Unity process is still running.
- Use the log file as the source of truth instead of shell output.
- Temporary licensing handshake lines can appear early in the log; if the build later ends with `Result: Success`, they were not fatal for that run.
