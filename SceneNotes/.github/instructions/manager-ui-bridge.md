# Preload Manager With Deferred UI Bridge

Use this document when creating or restructuring a feature where:

- a preload manager owns the runtime feature state
- a later-loaded UI controller binds to that manager through DI
- the UI lives under `Assets/_GameFiles/Scripts/UIControllers/<Feature>/`

This pattern is used when the feature state should persist independently from the specific scene UI that presents it.

## Core Goal

The manager should own the feature.
The UI controller should present and react to the feature.

Use this split when the feature needs persistent runtime state, events, or save/load coordination, but the actual UI appears later in a scene and should not become the source of truth.

Do not treat library-wrapper patterns such as inventory or quests as the default template here. Those remain case-by-case architecture decisions for later.

## Folder and Namespace Rules

### Manager-side code

For a new feature, prefer placing the manager-side system under its own folder:

- `Assets/_GameFiles/Scripts/<MyManager>/`

Prefer a feature namespace for the manager-side code, such as:

- `VoV.FastTravel`
- `VoV.MyFeature`
- `VoV.WorldMap`

Avoid defaulting a brand-new feature into a generic shared folder such as `Assets/_GameFiles/Scripts/Managers/` unless that shared area is intentionally the owner.

### UI-side code

UI controllers for that feature should live under:

- `Assets/_GameFiles/Scripts/UIControllers/<MyManager>/`

Keep the repo's current UI naming convention:

- prefix with `VE`
- suffix with `Controller`

Examples:

- `VEFastTravelMenuController`
- `VEWorldMapController`
- `VEMyFeatureController`

The UI folder should be feature-specific, but the default UI namespace should remain:

- `VoV.UIControllers`

Only introduce a more specific UI namespace if there is a strong reason to do so.

## Ownership Split

### The preload manager should own:

- feature state
- runtime collections/maps
- feature events
- save/load-facing lifecycle
- gameplay-facing actions

### The UI controller should own:

- `UIDocument` access
- visual element queries
- button and UI event wiring
- menu open/close presentation
- subscribing and unsubscribing to manager events

Do not move persistent feature state into the UI controller just because that controller is the visible entry point.

## Registration and DI

The manager should still be registered through:

- `Assets/_GameFiles/Scripts/Orchestrator/PreloadDependencyResolver.cs`

The later UI controller should resolve the manager by interface through `GODependencyRegistrar`.
Keep the interface in its own file, separate from the manager implementation file.
Do not define `IMyFeatureManager` inside the same class file as `MyFeatureManager` for a normal manager-driven feature.

Example registration shape:

```csharp
public class PreloadDependencyResolver : MonoBehaviour
{
    [SerializeField] private GameObject _dependencyObject;

    private void Awake()
    {
        IMyFeatureManager manager = _dependencyObject.GetComponent<IMyFeatureManager>();

        GODependencyRegistrar.Initialize();
        GODependencyRegistrar.RegisterSingleton<IMyFeatureManager, IMyFeatureManager>(manager);
    }
}
```

Preferred shape:

```csharp
private IMyFeatureManager _manager;

private void Start()
{
    _manager = GODependencyRegistrar.GetService<IMyFeatureManager>();
    _manager.WindowRequested += OnWindowRequested;
}

private void OnDisable()
{
    _manager.WindowRequested -= OnWindowRequested;
}
```

The UI should subscribe cleanly and unsubscribe cleanly. Treat subscription cleanup as part of the default pattern, not as optional polish.

## Lifecycle: `Initialize(...)` vs `InitializeData(...)`

This pattern still uses the same staged manager lifecycle described by save/load.

The canonical reference is:

- `SaveCoreService.InitializeCharacterData()`
- `SaveCoreService.InitializeSceneData()`

### `Initialize(...)`

Use this for early setup once the profile or character context is known.

Typical responsibilities:

- resolve feature services
- store character/profile identifiers
- clear and reset manager-owned runtime state
- prepare event-driven state the later UI will rely on

### `InitializeData(...)`

Use this when the manager must hydrate feature data after the scene and character state are ready.

Typical responsibilities:

- load the feature's discovered/unlocked/runtime data
- rebuild manager-owned collections
- invoke "data initialized" events
- make the feature ready for later UI listeners

The UI controller should assume the manager lifecycle exists and should bind to manager state/events instead of duplicating that lifecycle itself.

## Save / Load Integration Notes

If the manager participates in save/load, document:

- what is valid immediately after `Initialize(...)`
- what becomes valid only after `InitializeData(...)` or scene-stage hydration
- whether the UI may appear before or after the manager data is hydrated

This prevents the UI from becoming tightly coupled to save/load ordering assumptions.

## Example Implementation

Use a shape like this when introducing a new manager-driven UI feature:

```csharp
// IMyFeatureManager.cs
namespace VoV.MyFeature
{
    public interface IMyFeatureManager
    {
        event Action WindowRequested;
        event Action DataInitialized;

        void Initialize(string characterId);
        void InitializeData(string characterId);
        void OpenWindow();
    }
}
```

```csharp
// MyFeatureManager.cs
namespace VoV.MyFeature
{
    public class MyFeatureManager : MonoBehaviour, IMyFeatureManager
    {
        public event Action WindowRequested;
        public event Action DataInitialized;

        private string _characterId;
        private bool _isInitialized;

        public void Initialize(string characterId)
        {
            _characterId = characterId;
            _isInitialized = false;
        }

        public void InitializeData(string characterId)
        {
            _characterId = characterId;

            // Load feature state here.
            _isInitialized = true;
            DataInitialized?.Invoke();
        }

        public void OpenWindow()
        {
            WindowRequested?.Invoke();
        }
    }
}
```

```csharp
// VEMyFeatureController.cs
namespace VoV.UIControllers
{
    public class VEMyFeatureController : MonoBehaviour
    {
        private IMyFeatureManager _manager;
        private VisualElement _root;

        private void Awake()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
        }

        private void Start()
        {
            _manager = GODependencyRegistrar.GetService<IMyFeatureManager>();
            _manager.WindowRequested += OnWindowRequested;
            _manager.DataInitialized += OnDataInitialized;
        }

        private void OnDisable()
        {
            _manager.WindowRequested -= OnWindowRequested;
            _manager.DataInitialized -= OnDataInitialized;
        }

        private void OnWindowRequested()
        {
            _root.style.display = DisplayStyle.Flex;
        }

        private void OnDataInitialized()
        {
            // Rebuild or refresh UI from manager-owned data here.
        }
    }
}
```

## Relationship To `uitoolkit.md`

Load `uitoolkit.md` alongside this file when the task also touches:

- UXML
- USS
- `UIDocument`
- UI Toolkit controller structure
- subcomponent naming and UI file layout

This file explains the manager/UI ownership split.
`uitoolkit.md` explains the UI Toolkit-side file, naming, and styling rules.

## When To Load This Doc

Load this instruction when the task involves:

- building a preload manager that drives a later scene UI
- splitting a feature into manager-side state and UI-side presentation
- deciding where manager code vs `VE...Controller` code should live
- documenting or implementing `Initialize(...)` and later hydration for a manager-backed UI feature

Do not load this file for a headless manager that has no dedicated feature UI.
