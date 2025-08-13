# TheOne.Entities

Entity Manager for Unity

## Installation

### Option 1: Unity Scoped Registry (Recommended)

Add the following scoped registry to your project's `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "TheOne Studio",
      "url": "https://upm.the1studio.org/",
      "scopes": [
        "com.theone"
      ]
    }
  ],
  "dependencies": {
    "com.theone.entities": "1.1.0"
  }
}
```

### Option 2: Git URL

Add to Unity Package Manager:
```
https://github.com/The1Studio/TheOne.Entities.git
```

## Features

- Entity-Component system for Unity
- Lifecycle management for entities and components
- Controller support for complex entity behaviors
- Integration with popular DI frameworks

## Usage

### Basic Entity System

```csharp
using TheOne.Entities;

// Load entity prefabs into pool
entityManager.Load<Player>(count: 5); // Pre-instantiate 5 players
entityManager.Load<Enemy>(count: 10);

// Spawn an entity from pool
var player = entityManager.Spawn<Player>();
var enemy = entityManager.Spawn<Enemy>(position, rotation, parent);

// Spawn with parameters
var enemyConfig = new EnemyConfig { Health = 100, Damage = 10 };
var boss = entityManager.Spawn<Boss>(enemyConfig, position);

// Query active entities
var allEnemies = entityManager.Query<Enemy>();
foreach (var enemy in allEnemies)
{
    enemy.TakeDamage(10);
}

// Query entities with specific component
var healthComponents = entityManager.Query<HealthComponent>();
foreach (var health in healthComponents)
{
    health.Heal(20);
}

// Recycle entity back to pool
entityManager.Recycle(enemy);

// Recycle all entities of a type
entityManager.RecycleAll<Enemy>();

// Cleanup excess pooled instances
entityManager.Cleanup<Enemy>(retainCount: 5); // Keep only 5 in pool

// Completely unload entity type
entityManager.Unload<Enemy>();
```

### Entity with Parameters

```csharp
// Define entity with typed parameters - inherits from Entity<TParams>
public class Enemy : Entity<EnemyConfig>
{
    protected override void OnSpawn()
    {
        // Access typed Params property
        this.health = Params.Health;
        this.damage = Params.Damage;
    }
}

// Create with parameters
var enemyConfig = new EnemyConfig { Health = 100, Damage = 10 };
var enemy = entityManager.Spawn<Enemy>(enemyConfig);

// Real-world example: Using tuple parameters for multiple values
public class Level : Entity<(LevelModel Model, int Difficulty, float TimeLimit)>
{
    public LevelModel Model      => this.Params.Model;
    public int        Difficulty => this.Params.Difficulty;
    public float      TimeLimit  => this.Params.TimeLimit;
}

// Spawn with tuple
var level = entityManager.Spawn<Level>((model, 3, 120f));
```

### Async Entity Loading

```csharp
// Load entities asynchronously
#if THEONE_UNITASK
await entityManager.LoadAsync<Enemy>(count: 10);
await entityManager.LoadAsync("boss_prefab", count: 2);

// With progress reporting
var progress = new Progress<float>(p => Debug.Log($"Loading: {p:P}"));
await entityManager.LoadAsync<Player>(count: 5, progress: progress);
#else
// Coroutine version
StartCoroutine(entityManager.LoadAsync<Enemy>(count: 10, callback: () => 
{
    Debug.Log("Enemies loaded");
}));
#endif
```

### Components with Controllers

```csharp
using TheOne.Entities.Controller;

// Define a component with controller - inherits from Component<TController>
public class PlayerInput : Component<InputController>
{
    // Controller is automatically instantiated and managed
    public void HandleInput(Vector2 input)
    {
        Controller.ProcessInput(input);
    }
}

// Controller handles the logic
public class InputController : Controller<PlayerInput>
{
    protected override void OnSpawn()
    {
        // Called when component is spawned
        Debug.Log("Input controller ready");
    }
    
    public void ProcessInput(Vector2 input)
    {
        // Access Component property for the owning component
        Component.transform.Translate(input);
    }
    
    protected override void OnRecycle()
    {
        // Cleanup when recycled
    }
}
```

### Entity with Controller and Parameters

```csharp
using TheOne.Entities.Controller;

// Entity with both controller and parameters
public class Boss : Entity<BossConfig, BossController>
{
    protected override void OnSpawn()
    {
        // Access both Params and Controller
        health = Params.MaxHealth;
        Controller.StartBehavior();
    }
}

public class BossController : Controller<Boss>
{
    public void StartBehavior()
    {
        // Access Entity and Manager through protected properties
        var player = Manager.GetEntity<Player>();
        // Start boss AI
    }
}

## Architecture

### Folder Structure

```
TheOne.Entities/
├── Scripts/
│   ├── Entity.cs                      # Base entity class
│   ├── Component.cs                   # Base component class
│   ├── EntityManager.cs               # Entity lifecycle manager
│   ├── IEntity.cs                     # Entity interfaces
│   ├── IComponent.cs                  # Component interfaces
│   ├── IComponentLifecycle.cs         # Component lifecycle events
│   ├── IEntityManager.cs              # Entity manager interface
│   ├── Controller/                    # Controller pattern support
│   │   ├── Controller.cs
│   │   ├── IController.cs
│   │   └── IComponentWithController.cs
│   └── DI/                           # Dependency injection
│       ├── EntityManagerDI.cs
│       ├── EntityManagerVContainer.cs
│       └── EntityManagerZenject.cs
```

### Core Classes

#### Abstract Classes

##### `Component`
Base abstract class for all components:
- Extends `BetterMonoBehavior` from TheOne.Extensions
- Provides access to `Container`, `Manager`, and `Entity`
- Virtual lifecycle methods: `OnInstantiate()`, `OnSpawn()`, `OnRecycle()`, `OnCleanup()`

##### `Entity`
Base abstract class for entities without parameters:
- Extends `BaseEntity` (which extends `Component`)
- Use for simple entities that don't need initialization data

##### `Entity<TParams>`
Generic abstract class for entities with typed parameters:
- Access parameters through `Params` property
- Parameters are set before `OnSpawn()` is called

##### `Component<TController>` (in Controller namespace)
Abstract class for components with controllers:
- Automatically instantiates and manages the controller
- Controller accessible via `Controller` property
- Lifecycle methods are chained to controller

##### `Controller<TComponent>`
Abstract base for controller logic:
- Access component via `Component` property
- Access `Manager` and `Entity` through protected properties
- Provides helper methods for component access

##### `Entity<TController>` and `Entity<TParams, TController>` (in Controller namespace)
Entity classes with controller support:
- Combines entity, parameters, and controller patterns
- Controller is automatically managed

#### Interfaces

##### `IEntity`
Base interface for all entities:
- Extends `IComponent` for component-like behavior
- `IEntityWithParams` - For entities requiring initialization data
- `IEntityWithoutParams` - For simple entities

##### `IComponent`
Base interface for all components:
- Properties: `Container`, `Manager`, `Entity`
- Implements `IComponentLifecycle`

##### `IComponentLifecycle`
Component lifecycle events:
- `OnInstantiate()` - Called when pooled object is created
- `OnSpawn()` - Called when object is spawned from pool
- `OnRecycle()` - Called when object returns to pool
- `OnCleanup()` - Called when object is destroyed

##### `IEntityManager`
Entity management interface with pooling system:
- `Load<T>(count)` - Pre-instantiate entities into pool
- `Spawn<T>()` - Spawn entity from pool with position/rotation/parent
- `Spawn<T>(params)` - Spawn entity with initialization parameters
- `Query<T>()` - Query all active entities/components of type T
- `Recycle(entity)` - Return single entity to pool
- `RecycleAll<T>()` - Return all entities of type to pool
- `Cleanup<T>(retainCount)` - Destroy excess pooled instances
- `Unload<T>()` - Completely remove entity type from pool
- Events: `Instantiated`, `Spawned`, `Recycled`, `CleanedUp`

##### `IController` and `IComponentWithController`
Controller pattern interfaces:
- `IController` - Base controller interface
- `IComponentWithController` - Marks components that use controllers

### Design Patterns

- **Entity-Component System**: Composition over inheritance
- **Object Pooling**: Reuse components for performance
- **Controller Pattern**: Separate data from behavior
- **Lifecycle Management**: Consistent initialization/cleanup
- **Dependency Injection**: Flexible service integration

### Code Style & Conventions

- **Namespace**: All code under `TheOne.Entities` namespace
- **Null Safety**: Uses `#nullable enable` directive
- **Interfaces**: Prefixed with `I` (e.g., `IEntity`)
- **Generic Constraints**: Use where appropriate for type safety
- **Lifecycle Methods**: Follow Unity-like naming (OnEnable, OnDisable)
- **Component Access**: Provide both generic and type-based methods

### Query System

The `Query<T>()` method is powerful for finding and operating on active entities:

```csharp
// Query all entities of a specific type
var players = entityManager.Query<Player>();
foreach (var player in players)
{
    player.UpdateScore();
}

// Query all components across all entities
var allHealthComponents = entityManager.Query<HealthComponent>();
var totalHealth = allHealthComponents.Sum(h => h.CurrentHealth);

// Query and filter
var lowHealthEnemies = entityManager.Query<Enemy>()
    .Where(e => e.GetComponent<HealthComponent>().CurrentHealth < 20);
    
// Query components in controller
public class GameController : Controller<GameManager>
{
    protected override void OnSpawn()
    {
        // Find all active weapons
        var weapons = Manager.Query<Weapon>();
        
        // Find all entities with specific component
        var destructibles = Manager.Query<IDestructible>();
    }
}

// Combine with LINQ for complex queries
var bossesNearPlayer = entityManager.Query<Boss>()
    .Where(b => Vector3.Distance(b.transform.position, player.transform.position) < 50f)
    .OrderBy(b => b.GetComponent<HealthComponent>().CurrentHealth)
    .Take(3);
```

### Advanced Usage

#### Custom Component with Lifecycle

```csharp
public class NetworkComponent : Component
{
    private NetworkConnection connection;
    
    protected override void OnInstantiate()
    {
        // Called once when component is created in pool
        connection = new NetworkConnection();
    }
    
    protected override void OnSpawn()
    {
        // Called each time component is spawned from pool
        connection.Connect();
        connection.StartListening();
    }
    
    protected override void OnRecycle()
    {
        // Called when component returns to pool
        connection.StopListening();
        connection.Disconnect();
    }
    
    protected override void OnCleanup()
    {
        // Called when component is destroyed
        connection?.Dispose();
    }
}
```

#### Complete Entity Example

```csharp
// Entity with parameters
public class Weapon : Entity<WeaponConfig>
{
    private float damage;
    private float fireRate;
    
    protected override void OnSpawn()
    {
        // Access typed parameters
        damage = Params.Damage;
        fireRate = Params.FireRate;
        
        // Access EntityManager and Container
        var effects = Manager.Spawn<MuzzleFlash>();
        var audioManager = Container.Resolve<IAudioManager>();
    }
    
    public void Fire()
    {
        // Use the Recycle method from BaseEntity
        var bullet = Manager.Spawn<Bullet>();
        bullet.SetDamage(damage);
    }
    
    protected override void OnRecycle()
    {
        // Cleanup before returning to pool
        damage = 0;
        fireRate = 0;
    }
}
```

#### Entity Hierarchies

```csharp
public class Squad : Entity
{
    private List<Soldier> soldiers = new();
    
    public void AddSoldier(Soldier soldier)
    {
        soldiers.Add(soldier);
        soldier.transform.SetParent(this.transform);
    }
    
    public void Command(ICommand command)
    {
        foreach (var soldier in soldiers)
        {
            soldier.Execute(command);
        }
    }
}
```

#### Component Communication

```csharp
public class HealthComponent : Component
{
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;
    
    private float health = 100f;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        OnHealthChanged?.Invoke(health);
        
        if (health <= 0)
        {
            OnDeath?.Invoke();
            Entity.RemoveComponent(this);
        }
    }
}
```

## Common Patterns

### Custom Lifecycle Interfaces

```csharp
// Define custom lifecycle interfaces for game-specific behavior
public interface IPostSpawnListener
{
    void OnPostSpawn();
}

public interface IPauseable
{
    void Pause();
    void Resume();
}

public interface IHoldable
{
    void Hold();
    void Release();
}

// Use with components
public class TimeManager : Component, IPauseable, IHoldable
{
    void IPauseable.Pause() => this.timer.Pause();
    void IPauseable.Resume() => this.timer.Resume();
    void IHoldable.Hold() => this.timer.Hold();
    void IHoldable.Release() => this.timer.Release();
}

// Query and execute on all implementations
entityManager.Query<IPauseable>().ForEach(pauseable => pauseable.Pause());
entityManager.Query<IPostSpawnListener>().ForEach(listener => listener.OnPostSpawn());
```

### Nested Entity Spawning

```csharp
public class Block : Entity<BlockModel>
{
    protected override void OnInstantiate()
    {
        // Pre-load element prefabs during instantiation
        this.elementPrefabs.ForEach(prefab => this.Manager.Load(prefab));
    }
    
    protected override void OnSpawn()
    {
        // Spawn child elements within parent entity
        this.Elements = this.Model.Elements.Select(elementModel =>
            this.Manager.Spawn(
                prefab: this.elementPrefabs[elementModel.Type],
                @params: new ElementParams(elementModel, this),
                parent: this.elementContainer,
                spawnInWorldSpace: false
            )
        ).ToArray();
    }
    
    protected override void OnRecycle()
    {
        // Recycle all child elements
        this.Elements.ForEach(this.Manager.Recycle);
    }
    
    protected override void OnCleanup()
    {
        // Unload prefabs when entity type is being destroyed
        this.elementPrefabs.ForEach(prefab => this.Manager.Unload(prefab));
    }
}
```

### Service Integration with Entities

```csharp
public class BlockJamService : IEarlyLoadable, IAsyncEarlyLoadable
{
    private readonly IEntityManager entityManager;
    
    // Pre-load entities during service initialization
    async UniTask IAsyncEarlyLoadable.LoadAsync(IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await this.entityManager.LoadAsync<Level>(progress: progress, cancellationToken: cancellationToken);
    }
    
    // Query entities for game state
    public bool IsLoaded => this.entityManager.Query<Level>().Any();
    public int CurrentProgress => this.entityManager.Query<IProgressProvider>().Single().CurrentProgress;
    
    // Spawn with complex parameters
    public void LoadLevel(string levelId)
    {
        var level = this.entityManager.Spawn<Level>((
            model: GetLevelModel(levelId),
            difficulty: GetDifficulty(levelId),
            timeLimit: GetTimeLimit(levelId)
        ));
        
        // Execute post-spawn initialization
        this.entityManager.Query<IPostSpawnListener>()
            .ForEach(component => component.OnPostSpawn());
    }
}
```

## Performance Considerations

- Components are pooled to reduce allocations
- Entities use efficient component lookups
- Controllers update only when active
- Batch operations for multiple entities
- Lazy initialization where appropriate
- Use `Query<T>()` for efficient filtering and batch operations

## Best Practices

1. **Component Granularity**: Keep components focused on single responsibilities
2. **Data vs Logic**: Use controllers for complex logic, components for data
3. **Lifecycle Management**: Always implement cleanup in OnDestroy
4. **Entity Patterns**: Use factory patterns for complex entity creation
5. **Component Communication**: Use events/messages for loose coupling
6. **Performance**: Pool frequently created/destroyed entities
7. **Testing**: Mock EntityManager for unit tests