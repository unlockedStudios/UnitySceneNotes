# Interacting with our Data

We use BansheeGz Database to interact with runtime data in `Varadia-COY`.

Use this file for the project-specific architecture and maintenance rules. Use the official BGDatabase references for engine-specific details:

- [BGDatabase](https://www.bansheegz.com/BGDatabase/)
- [Extension Classes](https://www.bansheegz.com/BGDatabase/CodeGeneration/ExtensionClasses/)
- [MonoBehaviour Classes](https://www.bansheegz.com/BGDatabase/CodeGeneration/MonoBehaviourClasses/)
- [Behavior Integration](https://www.bansheegz.com/BGDatabase/ThirdParty/Behavior/)

# Architecture

Our runtime database flow is best understood as three practical layers:

- Manager / component / wrapper layer
- Service layer
- Access layer

The normal runtime path should be:

`Manager or Component -> Service Interface -> Service Implementation -> Access Layer -> BGDatabase`

In most cases, game code should call a service interface, not an access class directly.

## Registration Source

To verify which database interfaces are registered in runtime DI, check:

- `PreloadDependencyResolver.cs`

That file is the main source of truth for preload database registrations and transient service bindings.

## Rules

- Prefer resolving database services through `GODependencyRegistrar` by interface.
- Managers, handlers, wrappers, and components should usually call the service layer instead of the access layer.
- Services can call other services, but be careful not to create loops.
- Two services should not depend on each other in a circular way. If you find a service loop, call it out.
- Access-layer classes should contain BGDatabase-specific reads, writes, entity orchestration, and mapping logic.
- If an existing manager, wrapper, or handler already owns the database interaction for a feature, prefer extending that path instead of creating a second parallel entry point.
- Database services are usually registered as transients.

## Indirect Database Consumers

Not every gameplay system touches a database service directly.

Some systems consume database-backed state indirectly through save/load and initialization flows such as:

- `SaveCoreService.LoadCharacter(...)`
- `BaseCharacter.InitializeSheet(...)`
- `BaseCharacter.InitializeData(...)`
- `PlayerCharacter.InitializeSheet(...)`
- other manager initialization that replays saved character or world state after scene load

Treat those as database-backed flows too, even if the immediate caller is not resolving a DB service interface itself.

## Documentation Maintenance Rule

When a new system, feature, or request touches database-backed behavior, check whether the related database documentation is already mapped.

If it is missing or outdated, add or update:

- the service-layer references
- the access-layer references
- the shared access/core mapping when relevant
- the practical caller and scenario notes for indirect save/load-backed consumers

Not every system will map directly to the database, but some will still rely on database-backed contracts through later initialization. Document those after-the-fact flows too.

## Folder Notes

- The generated BGDatabase runtime surface lives in `Assets/_GameFiles/Scripts/DataAccess/CoreAccess.cs`.
- Hand-authored access-layer classes live under `Assets/_GameFiles/Scripts/DataAccess/Access/`.
- Service-layer classes live under `Assets/_GameFiles/Scripts/DataServices/`.
- Some access-only helpers do not have their own dedicated service wrapper. They are still part of the database area and should be documented accordingly.

## Code Details

Below is the normal example pattern for database reads. This example pulls NPC data from `BaseNPC.cs`. `BaseNPC` is a component attached to a Unity GameObject, but it still retrieves data through `IDBNPCService`, which then routes into `NPCAccess`.

### Example

```csharp
// Component level - the highest layer that consumes the returned data.
public class BaseNPC : MonoBehaviour
{
    protected void HandleNPCData()
    {
        InitializeDependencies();

        ContractNPCDetails npcDetails = _npcService.GetAllNPCDetails(_entityRef);
        _logService.LogInfo($"Initializing NPC Details for ({npcDetails.Name}, {npcDetails.EntityID})");
        NpcID = npcDetails.EntityID;
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

// Service layer - routes the request into the access layer.
public class NPCService
{
    protected NPCAccess _npcAccess;

    public NPCService()
    {
        _npcAccess = new NPCAccess();
    }

    public ContractNPCDetails GetAllNPCDetails(string entityRef)
    {
        return _npcAccess.GetAllNPCDetails(entityRef);
    }
}

// Access layer - talks directly to BGDatabase entities and mapping code.
public class NPCAccess
{
    public ContractNPCDetails GetAllNPCDetails(string entityRefID)
    {
        E_NPC npc = E_NPC.FindEntity(x => x.EntityRef.ToUpper() == entityRefID.ToUpper());

        if (npc == null)
        {
            Debug.LogError($"No NPC found with reference tag: {entityRefID}");
            return null;
        }

        return NPCModelFactory.MapNPCDetails(npc);
    }
}
```
