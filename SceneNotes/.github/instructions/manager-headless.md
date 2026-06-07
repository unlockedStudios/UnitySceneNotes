# Headless Preload Managers

Use this document when creating or restructuring a preload manager that does not own a dedicated feature UI.

These managers are long-lived runtime coordinators. They usually live on the preload object, are registered through DI, and are consumed by gameplay code, save/load code, and other managers.

## Core Goal

A headless preload manager should:

- own runtime state, events, and feature coordination for one system
- expose a feature-facing interface for DI consumers
- stay alive through preload instead of being scene-local
- avoid becoming a catch-all bucket for unrelated features

Do not treat this pattern as the same thing as a library-wrapper component such as inventory or quests. Wrapper patterns remain case-by-case architecture decisions and are not the default manager template for this repository.

## Folder and Namespace Rules

When creating a new manager system, prefer giving that system its own feature folder under:

- `Assets/_GameFiles/Scripts/<MyManager>/`

Examples of the desired direction:

- `Assets/_GameFiles/Scripts/Faction/`
- `Assets/_GameFiles/Scripts/LevelManager/`
- `Assets/_GameFiles/Scripts/MyManager/`

Avoid defaulting new systems into generic buckets such as:

- `Assets/_GameFiles/Scripts/Managers/`

unless you are intentionally extending an already-shared manager area that already lives there.

New manager systems should also prefer their own feature namespace, such as:

- `VoV.Faction`
- `VoV.LevelManager`
- `VoV.MyManager`

Prefer a feature namespace over broad catch-all namespaces like `VoV.Managers` when introducing a new standalone system.

## Registration and DI

The main source of truth for preload registrations is:

- `Assets/_GameFiles/Scripts/Orchestrator/PreloadDependencyResolver.cs`

Headless preload managers should usually follow this pattern:

1. The preload object contains the manager component.
2. The manager implements a feature-facing interface defined in its own separate interface file.
3. `PreloadDependencyResolver` pulls the interface from the preload object.
4. The interface is registered with `GODependencyRegistrar`.
5. Consumers resolve the interface instead of referencing the concrete class directly.

Keep the interface in its own file, separate from the manager implementation file.
Do not define `IMyManager` inside the same class file as `MyManager` for a normal manager system.

Example registration shape:

```csharp
public class PreloadDependencyResolver : MonoBehaviour
{
    [SerializeField] private GameObject _dependencyObject;

    private void Awake()
    {
        IMyManager manager = _dependencyObject.GetComponent<IMyManager>();

        GODependencyRegistrar.Initialize();
        GODependencyRegistrar.RegisterSingleton<IMyManager, IMyManager>(manager);
    }
}
```

Prefer interface-first DI such as:

```csharp
private IMyManager _manager;

private void Start()
{
    _manager = GODependencyRegistrar.GetService<IMyManager>();
}
```

Do not introduce direct concrete lookups if the manager is intended to be a shared preload service.

## Responsibilities

A headless preload manager may:

- own runtime collections or caches for its feature
- coordinate one feature's events and state transitions
- call other services by interface
- react to level or save/load lifecycle events
- expose feature actions to gameplay code
- encourage event-driven paths instead of consumer-side polling

A headless preload manager should not:

- own feature UI rendering logic
- hide unrelated systems behind one broad manager
- bypass DI and force consumers to depend on concrete scene objects
- rely on `Update()` polling as the default coordination path
- expose readiness or "can I do this?" checks as the primary way other systems discover state changes

## Event-Driven First

For headless managers in this repository, event-driven architecture is the default.
In most cases, do not give a manager its own `Update()`, `FixedUpdate()`, or `LateUpdate()` loop just to watch for state changes.

Prefer this pattern:

- another system performs the triggering action
- the manager updates its owned state once
- the manager raises an event immediately
- listeners react from that event

Avoid this pattern:

- another system keeps calling `if (manager.IsReady())`
- another system keeps checking `if (manager.CanDoThing())`
- another system polls a manager every frame waiting for a state transition

The goal is to avoid latency and hidden polling costs as systems grow.
Managers should push state changes outward with events instead of forcing listeners to keep asking whether something became possible yet.

If a feature truly needs frame-based work, prefer putting that logic in a dedicated runtime component, timer, or handler that owns that responsibility.
Do not make a preload manager's update loop the default answer for coordination.

## Event Examples From This Repository

Use simple `Action` or `Action<T>` events as the normal pattern.

Real examples already in the project:

- `Assets/_GameFiles/Scripts/Level Manager/LevelManager.cs`
  - `event Action<ContractLevel> BeforeLevelLoaded;`
  - `event Action<ContractLevel> AllLevelsLoaded;`
- `Assets/_GameFiles/Scripts/Fast Travel Waypoint/FastTravelManager.cs`
  - `event Action OnInitializedData;`
- `Assets/_GameFiles/Scripts/Managers/TalentManager.cs`
  - `event Action PointsUpdated;`
- `Assets/_GameFiles/Scripts/CharacterSheet/ExperienceHandler.cs`
  - `event Action<int> leveledUp;`
  - `event Action<int> addedExperience;`

Preferred event shapes:

```csharp
public event Action DataInitialized;
public event Action<int> ChargeCountChanged;
```

Usage shape:

```csharp
public interface IMyManager
{
    event Action DataInitialized;
    event Action<int> ChargeCountChanged;

    void Initialize(string characterId);
    void InitializeData(string characterId);
}
```

```csharp
public class MyManager : MonoBehaviour, IMyManager
{
    public event Action DataInitialized;
    public event Action<int> ChargeCountChanged;

    private int _chargeCount;

    public void Initialize(string characterId)
    {
        _chargeCount = 0;
    }

    public void InitializeData(string characterId)
    {
        _chargeCount = 3;
        DataInitialized?.Invoke();
        ChargeCountChanged?.Invoke(_chargeCount);
    }

    public void SetChargeCount(int chargeCount)
    {
        _chargeCount = chargeCount;
        ChargeCountChanged?.Invoke(_chargeCount);
    }
}
```

```csharp
public class MyConsumer : MonoBehaviour
{
    private IMyManager _manager;

    private void Start()
    {
        _manager = GODependencyRegistrar.GetService<IMyManager>();
        _manager.DataInitialized += OnDataInitialized;
        _manager.ChargeCountChanged += OnChargeCountChanged;
    }

    private void OnDisable()
    {
        _manager.DataInitialized -= OnDataInitialized;
        _manager.ChargeCountChanged -= OnChargeCountChanged;
    }

    private void OnDataInitialized()
    {
        // React immediately when the manager becomes ready.
    }

    private void OnChargeCountChanged(int chargeCount)
    {
        // React to the new value instead of polling for it.
    }
}
```

## Lifecycle: `Initialize(...)` vs `InitializeData(...)`

Many preload managers need more than DI registration. In this project, save/load often stages manager setup in two passes.

The canonical reference is:

- `SaveCoreService.InitializeCharacterData()`
- `SaveCoreService.InitializeSceneData()`

Use the split like this:

### `Initialize(...)`

Use this for early runtime setup that can happen once the save profile or character context is known.

Typical responsibilities:

- resolve service dependencies
- store the current character or profile id
- reset feature-owned runtime collections/state
- subscribe to persistent events
- prepare the manager for later hydration

### `InitializeData(...)`

Use this for later data hydration that depends on scene state, character state, or post-load ordering.

Typical responsibilities:

- load feature data after character data is ready
- hydrate runtime collections from service results
- invoke "data is ready" events for listeners
- perform scene-sensitive work that cannot happen during the earlier setup pass

If a manager only needs one stage, that is fine. Do not add `InitializeData(...)` unless the feature genuinely benefits from a second hydration pass.

## Save / Load Integration Notes

If the manager participates in save/load:

- document whether it is initialized during `InitializeCharacterData()`
- document whether it also needs later hydration during `InitializeSceneData()`
- be explicit about what state the manager owns before and after each stage

This matters because some managers are registered at preload time, but their feature data is not valid until the profile, character, and scene state have all been loaded.

## Example Implementation

Use a shape like this when introducing a new headless preload manager:

```csharp
// IMyManager.cs
namespace VoV.MyManager
{
    public interface IMyManager
    {
        event Action DataInitialized;
        event Action<int> ChargeCountChanged;

        void Initialize(string characterId);
        void InitializeData(string characterId);
    }
}
```

```csharp
// MyManager.cs
namespace VoV.MyManager
{
    /// <summary>
    /// Owns runtime state and coordination for one feature.
    /// This manager is intended to live on the preload object and be resolved through DI.
    /// </summary>
    /// <remarks>
    /// - Use Initialize(...) for dependency setup and runtime reset.
    /// - Use InitializeData(...) for later hydration after scene or character data is ready.
    /// - Keep persistent feature state here instead of spreading it across scene objects.
    /// </remarks>
    public class MyManager : MonoBehaviour, IMyManager
    {
        public event Action DataInitialized;
        public event Action<int> ChargeCountChanged;

        private ILogCoreService _logService;
        private string _characterId;
        private int _chargeCount;

        public void Initialize(string characterId)
        {
            _characterId = characterId;
            _logService = GODependencyRegistrar.GetService<ILogCoreService>();
            _chargeCount = 0;
        }

        public void InitializeData(string characterId)
        {
            _characterId = characterId;

            // Load or rebuild feature state here.
            _chargeCount = 3;
            DataInitialized?.Invoke();
            ChargeCountChanged?.Invoke(_chargeCount);
        }
    }
}
```

## When To Load This Doc

Load this instruction when the task involves:

- creating a new preload manager without a dedicated feature UI
- restructuring an existing manager into its own feature folder/namespace
- adding preload DI registration for a new manager
- defining manager lifecycle staging around `Initialize(...)` and later data hydration

Do not load this file just because the word "manager" appears in a task. Use it when the work is about the preload manager pattern itself.
