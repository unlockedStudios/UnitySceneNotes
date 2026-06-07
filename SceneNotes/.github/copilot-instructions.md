# Github Copilot Instructions Flow
The `.github/instructions/` folder contains the canonical project guidance for this repository.
Each file is scoped to a specific domain or workflow.

Do not load every instruction file by default.
Load the small core set first, then load only the subsystem or workflow files that are relevant to the current task.
This keeps the working context focused and avoids pulling in unrelated architecture notes.

## File Structure
- `general.md`: General Unity C# coding standards and naming/style guidance.
- `coding.md`: Project-wide architecture, dependency injection, logging, data access, timers, and team-specific coding conventions.
- `database.md`: Instructions specific to interacting with BansheeGz Database and related data-access layers.
- `manager-headless.md`: Guidance for preload managers that own feature state and coordination without a dedicated feature UI. Covers folder layout, feature namespaces, DI registration, and staged initialization.
- `manager-ui-bridge.md`: Guidance for preload managers that coordinate with a later-loaded UI Toolkit controller. Covers manager/UI split, folder layout, VE controller naming, and staged initialization.
- `uitoolkit.md`: UI Toolkit architecture and workflow guidance for UXML, USS, UI controller structure, naming, and styling rules.
- `instruction-authoring.md`: Guidance for writing and formatting repository instruction files, including section structure, Dos/Don'ts formatting, summaries, examples, and instruction-document style rules.

## Default Loading Strategy

Always load and apply these core files for every task:

- `.github/instructions/coding.md`
- `.github/instructions/general.md`

Load these additional files only when the task actually touches that area:

- `.github/instructions/database.md`
  - Load for BansheeGz database access, data-access layers, data-service layers, persistence plumbing, or database-backed content workflows.
- `.github/instructions/manager-headless.md`
  - Load for creating or restructuring preload managers that do not own a dedicated feature UI.
  - Load when defining manager folder layout, feature namespaces, preload DI registration, or `Initialize(...)` / later data-hydration staging for a headless manager.
- `.github/instructions/manager-ui-bridge.md`
  - Load for creating or restructuring a preload manager plus a later scene/UI Toolkit controller pairing.
  - Load when a manager owns runtime state/events and a `VE...Controller` binds to that manager from `Assets/_GameFiles/Scripts/UIControllers/<Feature>/`.
- `.github/instructions/uitoolkit.md`
  - Load for UI Toolkit related functionality, including UXML, USS, VisualElement controllers, UI architecture, binding, layout, or styling.
- `.github/instructions/instruction-authoring.md`
  - Load for any request that creates or updates instruction files, changes instruction-file formatting or style,
    adds Dos/Don'ts sections, changes summary/detail guidance, updates examples in instruction docs, or defines
    where future documentation/style rules should live.

If a task spans multiple systems, load every relevant scoped file for those systems.
Do not load unrelated subsystem documents just because they exist.

## Priority Order

When multiple instruction files apply, prefer them in this order:

1. The most specific subsystem or workflow instruction for the files and feature being changed.
2. `instruction-authoring.md` when creating or updating repository instruction files.
3. `coding.md` for project-wide architecture, dependency injection, logging, and integration rules.
4. `general.md` for general Unity C# style and naming conventions.

## Practical Examples

- UI Toolkit task:
  - Load `coding.md`, `general.md`, and `uitoolkit.md`.
  - Do not load `character-sheet.md`, `character-skillsystem.md`, `level-manager.md`, or the manager docs unless the UI task directly touches those systems or patterns.
- Character Sheet save integration task:
  - Load `coding.md`, `general.md`, `character-sheet.md`, and `database.md` if database-backed persistence is involved.
- New Behavior Designer task that activates a skill:
  - Load `coding.md`, `general.md`, `opsive.md`, and `character-skillsystem.md`.
- Scene transition UI task:
  - Load `coding.md`, `general.md`, `uitoolkit.md`, and `level-manager.md`.
- New faction-like preload system with no dedicated feature UI:
  - Load `coding.md`, `general.md`, and `manager-headless.md`.
  - Also load a subsystem doc only if the new manager belongs to an existing documented subsystem.
- New fast-travel-like system where a preload manager drives a later `VE...Controller`:
  - Load `coding.md`, `general.md`, `manager-ui-bridge.md`, and `uitoolkit.md`.
  - Also load a subsystem doc if the feature already has one.
- Instruction formatting task:
  - Load `coding.md`, `general.md`, and `instruction-authoring.md`.
  - Also load a subsystem doc only if the instruction change is specific to that subsystem.
