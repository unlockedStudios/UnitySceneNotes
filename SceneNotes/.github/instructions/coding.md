# C# Styleguide and Architecture

This document outlines the coding standards and architectural guidelines for C# development within our projects. Adhering to these conventions ensures code consistency, readability, and maintainability across the team.

When deciding how to structure, and write out code, prioritize this document over the copilot-instructions.md file. If this file does not specify or outline a giving condition or scenario, default to the copilot-instructions.md file for instructions.

## Method Extraction

- Extract private helper methods when they isolate meaningful behavior, remove real duplication, improve testability,
  or serve as required lifecycle, callback, interface, or public API boundaries.
- Do not extract one-line or very small private methods when they have only one caller and only wrap a simple condition,
  assignment, property selection, or direct method call.
- When a simple inline statement needs context, keep the statement in place and add a short "why" comment nearby.
  This is different from XML summary notes, which document public contracts, managers, and class responsibilities.

### Method Extraction Dos

- Do keep callback, lifecycle, interface, and public API methods even when they are small.
- Do extract a helper when the body has enough steps that a meaningful name reduces the caller's mental load.
- Do extract reused logic when multiple callers would otherwise duplicate the same condition or operation.
- Do keep simple guards, assignments, property selections, and direct calls inline when they have only one caller.
- Do use a short inline comment when the reason for a simple statement is not obvious from the code.

### Method Extraction Don'ts

- Do not create private helpers whose only job is to return one ternary, forward one call, or wrap one obvious guard.
- Do not move a statement out of its only caller just to make the caller read like pseudo-code.
- Do not hide required state changes behind tiny methods when reading the state change inline is clearer.
- Do not replace a helpful inline comment with a method name unless the extracted method owns real behavior.

Example:

```csharp
// Good: the simple selection stays near the call site, with context where it matters.
// Tan wolves use their own config; unknown summon types fall back to grey.
string poolTag = wolfType == CallThePackWolfSummonType.Tan ? _tanWolfPoolTag : _greyWolfPoolTag;
BaseFighter summonedWolf = _wolfPoolHandler.GetWolfFromPool(poolTag, prefab, _logService);

// Avoid: this helper has one caller and only hides a simple ternary.
private string GetPoolTag(CallThePackWolfSummonType wolfType)
{
    return wolfType == CallThePackWolfSummonType.Tan ? _tanWolfPoolTag : _greyWolfPoolTag;
}
```

## Naming Conventions

Use class and file suffixes to communicate a type's architectural role. A suffix should describe how other code
should understand the type, not just make the name sound official.

`Service` has a specific meaning in this project. Use it for broad system entry points, application or database
facades, dependency-registered contracts, or interface-backed APIs that act as the starting point for a feature.
Do not use `Service` for narrow feature-local helpers that only support one class or one skill.

### Dos

- Do use `Service` when the type is the main entry point into a system, data layer, internal application, or
  dependency-registered API.
- Do use `Handler` for local operational logic that performs a focused behavior for one feature, component, or skill.
- Do use `Adapter` when the type mainly wraps another API and translates the caller's needs into that API.
- Do use `Planner` for types that choose or calculate a future action without performing the action directly.
- Do use `Factory` for types that create or map objects.
- Do use `Resolver` for types that locate dependencies, references, or concrete implementations.
- Do use `Coordinator` only when the type orchestrates several collaborators and owns cross-step flow.

```csharp
public interface ICraftingService
{
    CraftedItem CraftRecipe(string recipeId);
}

public sealed class CraftingService : ICraftingService
{
    private readonly CraftingRecipeAccess _recipeAccess;

    public CraftedItem CraftRecipe(string recipeId)
    {
        CraftingRecipe recipe = _recipeAccess.GetRecipe(recipeId);
        return CreateCraftedItem(recipe);
    }
}
```

```csharp
internal sealed class TrapTriggerHandler
{
    public void ArmTrap(Trap trap)
    {
        trap.SetArmed(true);
    }
}
```

### Don'ts

- Don't add `Service` to a type only because it has helper methods or does useful work.
- Don't name a skill-local helper `Service` when it only prepares effects, pools objects, tracks temporary state, or
  performs one feature's internal cleanup.
- Don't use `Manager`, `Coordinator`, or `Controller` as a vague substitute when `Handler`, `Adapter`, or `Planner`
  describes the responsibility more clearly.
- Don't choose a suffix that claims a broader ownership boundary than the type actually has.

```csharp
// Avoid: this is a local helper, not a system entry point or data facade.
internal sealed class TrapParticleService
{
    public void PlayTrapParticles(Vector3 position)
    {
        // Particle setup only.
    }
}
```

## C# Argument Wrapping

- Prefer compact wrapping for method calls and constructors.
- Do not expand argument lists to one argument per line just because the call wraps.
- Put as many arguments on each line as reasonably fit, up to the project line-length limit.
- Target max line length: 120 characters.

Example:

```csharp
// Good: wrapped because the call is long, but arguments stay grouped where they fit.
EncounterResetPlan resetPlan = CreateResetPlan(
    isQuestResetEnabled: true, hasEncounterEngaged: true, isResetRunning: false,
    isAlphaAlive: IsAlive(_alphaWolf), isAlphaDead: IsAlphaDead(),
    activeLivingSummonCount: summons.Count, alphaStartPosition: _alphaStartPosition,
    alphaStartRotation: _alphaStartRotation, returnRadius: _summonReturnRadius,
    returnSpacing: _summonReturnSpacing);

// Avoid: one argument per line adds height without improving readability.
EncounterResetPlan resetPlan = CreateResetPlan(
    isQuestResetEnabled: true,
    hasEncounterEngaged: true,
    isResetRunning: false,
    isAlphaAlive: IsAlive(_alphaWolf),
    isAlphaDead: IsAlphaDead(),
    activeLivingSummonCount: summons.Count,
    alphaStartPosition: _alphaStartPosition,
    alphaStartRotation: _alphaStartRotation,
    returnRadius: _summonReturnRadius,
    returnSpacing: _summonReturnSpacing);
```

## Inspector Validation

Do not use `OnValidate()` as an automatic safety net for serialized fields when an Inspector attribute can express
the same rule. Inspector-facing constraints should be visible where the field is declared, especially when the value
only needs a minimum, maximum, normalized range, required reference, or custom authoring check.

### Dos

- Do use Odin Inspector attributes for authoring constraints when Odin is available.
- Do use Unity Inspector attributes when they are enough for the constraint.
- Do set clear defaults at declaration time, or in `Reset()` when the value is an editor default for newly added
  components.
- Do reserve `OnValidate()` for real editor synchronization, such as migrating legacy serialized data, refreshing
  editor previews, or updating derived editor-only cached values.
- Do wrap `OnValidate()` in `#if UNITY_EDITOR` when it calls editor-only APIs.

```csharp
using Sirenix.OdinInspector;
using UnityEngine;

public class ExampleSkillConfig : MonoBehaviour
{
    [SerializeField, MinValue(1)]
    private int _summonCount = 3;

    [SerializeField, PropertyRange(0f, 1f)]
    private float _normalizedBlendWeight = 0.5f;

    [SerializeField, Required]
    private GameObject _effectPrefab;
}
```

### Don'ts

- Don't add `OnValidate()` methods that only clamp serialized values, assign fallback defaults, or duplicate Odin
  Inspector attributes.
- Don't hide simple authoring constraints away from the field declaration.
- Don't add defensive Inspector repair code "just in case" when the same intent can be shown with attributes.

```csharp
using UnityEngine;

public class ExampleSkillConfig : MonoBehaviour
{
    [SerializeField] private int _summonCount = 3;
    [SerializeField] private float _normalizedBlendWeight = 0.5f;

    private void OnValidate()
    {
        _summonCount = Mathf.Max(1, _summonCount);
        _normalizedBlendWeight = Mathf.Clamp01(_normalizedBlendWeight);
    }
}
```

## Dependency Injection

- ✅ Always use dependency injection to obtain service instances rather than directly instantiating them or using static accessors. This promotes loose coupling and makes unit testing easier. For example, instead of using `SceneLoadManager.Instance`, inject the `ISceneLoadManager` interface into your class constructor or use a service locator pattern like `GODependencyRegistrar.GetService<ISceneLoadManager>()`.
- ⚠️ When pulling services from the GODependencyRegistrar, there should be an interface defined for the service being pulled. Avoid pulling concrete implementatimons directly.
- ✅ Resolve dependencies in the intended setup point first (`Awake()`, `Start()`, constructor, or `Initialize(...)`) and cache the reference for normal use.
- ❌ Do not repeatedly call `GODependencyRegistrar.GetService(...)` or `TryGetService(...)` inside regular runtime methods just because the dependency is being used again.
- ✅ Before adding any fallback dependency lookup, confirm that the dependency is not already being resolved during setup. Avoid duplicate resolution logic.
- ⚠️ Only add a late fallback lookup when there is a real lifecycle edge case that makes setup-time resolution unreliable, and document why that exception is needed near the code.

Example:

```csharp
private ISceneLoadManager _sceneLoader;

    void Start()
    {
        _sceneLoader = GODependencyRegistrar.GetService<ISceneLoadManager>();
    }
```

## Referencing the Player

- ✅ When a script needs a reference to the player, pull it from the `GODependencyRegistrar` using the `IPlayerCharacter` interface.
- ✅ Prefer `_player = GODependencyRegistrar.GetService<IPlayerCharacter>();` instead of searching the scene or using another static access path.
- ✅ If the script is not a `MonoBehaviour`, pull the player in the constructor.
- ✅ If the script has an `Initialize()` method, pull the player there.
- ✅ If the script is a `MonoBehaviour`, and there is no `Initialize()` method, pull the player in `Start()`.

Examples:

#### If Non-Monobehaviour Example:
```csharp
private IPlayerCharacter _player;

public SomeController()
{
    _player = GODependencyRegistrar.GetService<IPlayerCharacter>();
}
```

#### Monobehaviour Examples:
```csharp
private void Start()
{
    _player = GODependencyRegistrar.GetService<IPlayerCharacter>();
}
```


```csharp
public void Initialize()
{
    _player = GODependencyRegistrar.GetService<IPlayerCharacter>();
}
```

## Logging

- ✅ When logging errors, prioritize using the ILogCoreService over Unity's built-in Debug.Log methods. This ensures that all logs are centralized and can be managed more effectively. For example, instead of using `Debug.LogError("An error occurred")`, use `_logService.LogError("An error occurred")` after injecting the `ILogCoreService` into your class.
- ✅ When calling the logCoreService, pull from the GODependencyRegistrar using the void Start() Method. This should give it enough time to initialize in the scene, before its called from another script.
- ✅ Trust the cached `ILogCoreService` after it is resolved in `Awake()`, `Start()`, a constructor, or `Initialize(...)`. Call `_logService.LogInfo(...)`, `_logService.LogWarning(...)`, or `_logService.LogError(...)` directly from normal runtime methods.
- ❌ Do not add private wrapper methods that check `_logService != null` and fall back to `Debug.Log(...)` when the service is already resolved during setup. Only add a logging fallback for a documented lifecycle edge case where setup-time dependency resolution is genuinely unreliable.

Example:

```csharp
private ILogCoreService _logCoreService;

    void Start()
    {
        _logCoreService = GODependencyRegistrar.GetService<ILogCoreService>();
    }
    void SomeMethod()
    {
        _logCoreService.LogError("An error occurred");
    }
```

## Character Resource

- ℹ️ When dealing with a character resource, such as Health, Mana, or Rage, You will find a reference to this in the `BaseCharacter.cs` script, usually implemented as a field variable for `Health`, or `Resource`.
- ℹ️ When dealing damage, or receiving Health to an NPC, it must be a script that contains or derives from the `BaseCharacter` script. Otherwise, some scripts already implement the damage, or resource manipulation needed.

Example References:

```csharp
public CharacterResource Health { get; private set; }
public CharacterResource Resource { get; private set; }
```

## Writing Summary Notes
- Keep XML summary notes focused on class, manager, interface, and public contract documentation. For one-line
  implementation context inside a method, prefer a short inline "why" comment near the statement instead.
- ℹ️ When writing a new Manager, that Manager should contain notes on top of it, explaining the purpose of the manager, and what its used for.
- ℹ️ When describing these notes, give simple case scenarios that may be used for this manager as well.
- ℹ️ Managers are typically used as a middle layer between the gameCode, and the 'service' layer that will be used in code. Service layers job is to pull data, and save data for the manager, so that may need to be outlined in the notes as well.
- Make the comments realistic for you to use, whenever you need to compile the code based and get context on the code based, to try and save you processing time, and token use.
- ℹ️ If the manager owns some kind of collections/state, then add an explicit detail to the remarks, such as "What collections/state the manager owns". If we updated this, please update the note too.
- ℹ️ Every manager will most likely have an interface implementation as well. the interface implementation will include more comments for the Method being implemented. Read the interface files to get explicit details and purpose of how the document is being used.
- ℹ️ Every interface method should include XML summary notes. Add parameter and return tags when the method accepts inputs or returns a value. Interface summaries are the contract-level explanation that callers should be able to read without opening the implementation.

Example:
```csharp
/// <summary>
///  This outlines the details of someManager.
///  And how else SomeManager might be used in code.
/// </summary>
/// <remarks>
/// - If condition X is met, some manager will do Y.
/// - When condition A is met, some manager will try to do condition B.
/// - When someManager is called, it will also save data, and act as the middle man between the manager, and the 'Service' layer for saving data.
/// </remarks>
public SomeManager {

}
```

## Test-Seam Design

Testability should come from the same seams the runtime design needs. Do not introduce static helper or static runtime
state classes only to make tests easier, especially in Unity gameplay or Behavior Designer code where static state can
leak across scenes, play sessions, and tests.

### Dos

- Do test public or stable behavior through the real component, prefab, shared variable, event, or controller boundary when practical.
- Do use existing Unity and Behavior Designer APIs, such as `BehaviorTree.GetVariable(...)`, when tests need to inspect shared variables.
- Do remove or skip low-value tests when the only way to keep them is to add an artificial production abstraction.
- Do add small pure helpers only when they also clarify real runtime behavior and belong to the owning type.

### Don'ts

- Don't add static state just to expose private implementation details to tests.
- Don't change shared runtime code only because a unit test would be easier to write.
- Don't introduce a new global helper when an integration-style test with the actual component would better represent the feature.

## Data Access

- ℹ️ All of the data for the game can be found in several different scripts, for each system. Architecturally speaking, most Monobehavior scripts pull data from a system in the game. So if a component needs Inventory Data, it will pull this inventory data from `InventoryCoreServiceComponent.cs`.
- ✅ When the 'game-level' first Loads, the data is loaded from Bangsheez Database. We access that data through our DataService layer found here: ../\_GameFiles/Scripts/DataServices/
- ℹ️ DataService is similar to a service wrapper for accessing Data specific to the database. All other Game-Components call the service layer in order to pull data.
- ℹ️ All of our DataService layers can be accessed using our DependencyRegistrar, and can be used to fetch data from there. Most of the time however, This data should exists at a Wrapper Level for a specific Manager, ServiceComponent, or Handler. Many of those components can be found here, and can be identified by its name below, but this is not the full list:
  - InventoryCoreServiceComponent.cs
  - QuestServiceComponent.cs
  - QuestNPCHandler.cs
  - LoadingScreenComponent.cs
  - FactionManager.cs
  - SceneDependencyResolver.cs
  - EventsManager.cs
  - HPBarBossManager.cs
- ℹ️ DataService layer speaks directly to the DataAccess layer. and DataAccess layer only contains code interacting with BG Database (https://www.bansheegz.com/BGDatabase/).
- ℹ️ If we interact with the DataService, keep in mind that all of the instanced Fetched are Transient. Data is consistent, but the class instances are transient copies.

Example:

```csharp
//Component Level - Highest level that will use the data once its retrieved.
//This
public class BaseNPC
{
    protected void HandleNPCData()
    {
        // Get the associated NPC Data.
        InitializeDependencies();
        ContractNPCDetails npcDetails = _npcService.GetAllNPCDetails(_entityRef);
        _logService.LogInfo($"Initializing NPC Details for ({npcDetails.Name}, {npcDetails.EntityID})");
        NpcID = npcDetails.EntityID;
        // Map data to its correlating Sheet Data.
        SheetHandler.LoadInfo(npcDetails.BaseStats, npcDetails.StatsPerLevel, npcDetails.SheetModifiers);
    }

    private void InitializeDependencies()
    {
        if (InterfaceHelper.IsNull(_npcService))
            _npcService = GODependencyRegistrar.GetService<IDBNPCService>();
        if (_logService == null)
            _logService = GODependencyRegistrar.GetService<ILogCoreService>();
    }
}

//service Layer - a layer Deep to Get Data.
public class NPCService
{
    protected NPCAccess _npcAccess;

    public NPCService()
    {
        _npcAccess = new NPCAccess();
    }

    //Get all of the NPC Data from
    public ContractNPCDetails GetAllNPCDetails(string entityRef)
    {
        return _npcAccess.GetAllNPCDetails(entityRef);
    }
}

//access layer. this is used to interact with BG Database to interact with the data we need.
public class NPCAccess
{
    public ContractNPCDetails GetAllNPCDetails(string entityRefID)
    {
        E_NPC npc = E_NPC.FindEntity(x => x.EntityRef.ToUpper() == entityRefID.ToUpper());
        if(npc == null)
        {
            Debug.LogError($"No NPC found with reference tag: {entityRefID}");
            return null;
        }

        return NPCModelFactory.MapNPCDetails(npc);
    }
}
```

## Coroutines and/or Timers

- ℹ️ Use coroutines as a last resort.
- ✅ Use either Hooch.Timer, or the built in Unity Update() method.
- ✅ When using Hooch.Timer, Follow the example below as a template for how to implement it.
- ℹ️ Its possible that an external class may 'start' the timer. if that happens, make sure it has a `public` accessor.
- ℹ️ When you need to perform logic on every tick, you can make the timer aware of it by implementing a FixedUpdate example, using the Timer.IsRunning value. (Example below).

Example 1: Using Timer.
```csharp
public class TestClass 
{
    private Timer _someTimer;

    //how long should we wait before executing the timer.
    private float _timerDelay 2.0f;

    private void Awake()
    {
        _someTimer = new Timer(_timerDelay, OnTimerFinished);

        //Start the timer.
        _someTimer.Start();
    }
    
    // This will display when the _timerDelay finishes. Not During.
    private void OnTimerFinished() {
        //code logic goes here...
        //this is execute after 2 seconds (_timerDelay) finish.
    }
}
```

Example 2: Using Timer.
```csharp
public class TestClass 
{
    protected Timer RegenTimer;


    //how long should we wait before executing the timer.
    private float _timerDelay 2.0f;

    private void Awake()
    {
        RegenTimer = new Timer(1f, HandleRegens, new TimerMemoryManagementContainer(TimerMemoryManagementType.ClearOnObjectNullOrSceneUnload, gameObject));
            RegenTimer.Start();
    }

    // This will trigger after 1.0f seconds have passed.
    protected virtual void HandleRegens()
    {
        //This will restart the RegenTimer over, and over again, whenever the time limit is hit.
        RegenTimer.Start();
    }
}
```

Example 3: using Update method instead.
```csharp
public class TestClass : MonoBehaviour
{
    private bool _runTimer = false;
    private float _curTimer;

    //The max allowed time that must be reached for the _curTimer.
    private float _maxTimer = 3.0f;

    private void Awake()
    {
        _curTimer = 0.0f;
    }

    //potentially invoked from an external class, or a derived class.
    public void StartRunTimer() {
        _runTimer = true;
    }

    private void Update()
    {
        RunTimer();
    }

    private void RunTimer() {
        if(!_runTimer) return;
        
        if (_curTimer <= _maxTimer) {
            _curTimer += TIme.deltaTime;
            return;
        }

        MyMethod();
        //Prevent method from executing a second time.
        _runTimer = false;
    }

    private void MyMethod() {
        //code logic goes here...
    }
}
```

Example 4: Using Hooch.Timer on every tick..
```csharp
public class TestClass 
{
    protected Timer _myTimer;

    //how long should we wait before executing the timer.
    private float _timerDelay 2.0f;

    private void Awake()
    {
        _myTimer = new Timer(1f, PerformAction, new TimerMemoryManagementContainer(TimerMemoryManagementType.ClearOnObjectNullOrSceneUnload, gameObject));
            _myTimer.Start();
    }

    private void FixedUpdate()
    {
        //executes every frame that the timer is still running.
        if (_myTimer.IsRunning)
        {
            //while _myTimer is running...
            //perform tick logic here..
        }
    }

    // This will trigger after 1.0f seconds have passed.
    protected virtual void PerformAction()
    {
        //logic for the action to be performed when time finishes.
    }
}
```
