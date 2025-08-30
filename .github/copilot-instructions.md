# Copilot Instructions - JoyeroMultiagente

## Project Overview

This is a Unity 3D project implementing a **multiagent jewelry display system** (Sistema Multiagente para Vitrina de Joyería). The system simulates micro-robots that collaborate autonomously to organize jewelry pieces by color in a display case.

### Core Problem
- Multiple micro-robots must classify and organize jewelry pieces initially placed randomly
- Each robot has omnidirectional mobility, proximity and pressure sensors, and color-based manipulation capabilities
- Goal: Transport all jewelry to appropriate color-categorized zones, optimizing time and movement efficiency
- The Unity implementation provides 3D visualization with animation, lighting, collision detection, and instance deployment

## Architecture & Key Components

### Agent System
- **`IAgent`**: Core interface for all simulation entities (robots, etc.)
  - Properties: `Id`, `Cell` (grid position)
  - Methods: `Tick()` (perception + action execution), `PlanNextAction()` (decision making)
- **`RobotController`**: Main robot implementation with state machine (Idle/Explore/GoToJewel/...)
- **`ZoneController`**: Defines delivery/target zones where robots deposit jewelry
- **`Jewel`**: Collectable items with color/type properties

### Grid & Pathfinding System
- **`GridCell`**: Pure logical unit with type (wall/empty/zone) and dynamic occupancy (robot/jewel/reserved)
- **`GridMap`**: Contains the complete grid structure
- **`GridService`**: Provides grid access and queries for the simulation
- **`PathfindingService`**: Implements A* pathfinding algorithm
- **`PathfindingComponent`**: Per-agent component that requests and consumes paths

### Core Systems
- **`SimulationManager`**: Main orchestrator managing simulation states (Init/Running/Paused/Finished)
- **`SpawnSystem`**: Handles spawning of robots and jewelry at simulation start
- **`RuleSystem`**: Defines global rules, win conditions, scoring, and validation
- **`MetricsLogger`**: Collects KPIs (movements, time, efficiency, jewelry collected/delivered)
- **`ServiceRegistry`**: Dependency injection container for services

### Configuration & Actions
- **`SimulationConfig`**, **`AgentConfig`**, **`SpawnConfig`**: ScriptableObjects for configuration
- **`IAction`**: Interface for atomic robot actions (move, pick, drop)

## Unity-Specific Guidelines

### MonoBehaviour Usage
- Use `MonoBehaviour` for components that need Unity lifecycle (Start, Update, etc.)
- Use pure C# classes for logic-only components (GridCell, PathfindingService, etc.)
- Prefer composition over inheritance for complex behaviors

### Serialization & Configuration
- Use `[SerializeField]` for private fields that need Inspector visibility
- Use ScriptableObjects for configuration data that designers should modify
- Use `[System.Serializable]` for data structures used in ScriptableObjects

### Performance Considerations
- Minimize allocations in Update loops - use object pooling for frequent spawning
- Cache component references in Start/Awake instead of using GetComponent repeatedly
- Use coroutines for multi-frame operations instead of blocking the main thread
- Consider using Jobs/Burst for heavy computational tasks if needed

## C# Coding Standards

### Documentation
- Use XML documentation comments (`///`) for all public members
- Include `<summary>`, `<param>`, `<returns>`, and `<remarks>` sections
- In `<remarks>`, document component interactions using `<see cref=""/>` tags
- Follow the existing pattern seen in the codebase

### Naming Conventions
- Classes: PascalCase (`RobotController`, `GridService`)
- Methods/Properties: PascalCase (`PlanNextAction`, `IsWalkableNow`)
- Fields: camelCase with `_` prefix for private (`_currentState`)
- Constants: PascalCase (`MaxRobotCount`)
- Enums: PascalCase for type and values (`CellType.Empty`)

### Code Organization
- Use `sealed` for classes not intended for inheritance
- Group related functionality in regions if files get large
- Use explicit access modifiers (`public`, `private`, `internal`)
- Prefer readonly fields where possible
- Use nullable reference types when appropriate

## Component Interaction Patterns

### Service Dependencies
- Services are registered in `ServiceRegistry` and injected via `GameBootstrap`
- Components request services through the registry, not direct references
- Use interfaces for service contracts (`IAgent`, `IAction`)

### Grid System Integration
- All position queries go through `GridService`
- Convert between world coordinates and grid cells using `GridService`
- Respect grid occupancy rules defined in `GridCell`

### Simulation Flow
1. `SimulationManager` coordinates overall state
2. Each tick: calls `IAgent.Tick()` on all agents
3. Agents use `PathfindingComponent` to navigate
4. Actions are validated by `RuleSystem`
5. Metrics are reported to `MetricsLogger`

### Event Communication
- Use Unity Events for loose coupling between components
- Agents report actions to `MetricsLogger` and `RuleSystem`
- Zones notify completion events for win condition checking

## Testing Approach

### QA Testing System
- Use `QATestingSystem` for runtime validation
- Add test methods following the pattern: `TestComponentName()`
- Use `Debug.Assert` for validation with descriptive messages
- Group related tests in the same method

### Unity Test Framework
- Create tests under `Assets/Tests/` if more comprehensive testing is needed
- Use `[Test]` for unit tests and `[UnityTest]` for integration tests
- Mock services using interfaces for isolated testing

## Implementation Guidelines

### State Management
- Use enums for robot states and state machines
- Keep state transitions explicit and well-documented
- Validate state changes in debug builds

### Error Handling
- Use defensive programming - validate inputs and state
- Provide meaningful error messages with context
- Use `Debug.LogError` for runtime errors, `Debug.LogWarning` for potential issues

### Memory Management
- Dispose of resources properly (implement IDisposable when needed)
- Use object pooling for frequently created/destroyed objects (jewelry, particles)
- Avoid creating unnecessary temporary objects in hot paths

## Domain-Specific Considerations

### Robot Behavior
- Robots should exhibit realistic constraints (movement speed, sensor range)
- Implement collision avoidance and coordination between robots
- Consider emergent behaviors from simple rules
- Robots must respect grid-based movement (no diagonal movement unless explicitly allowed)
- Implement realistic sensor limitations (limited vision range, occlusion)

### Jewelry Classification System
- Use consistent color representation throughout the system (RGB, HSV, or predefined color enums)
- Consider color-blind accessibility when designing the visual system
- Implement robust color detection algorithms that work under varying lighting
- Support multiple jewelry types beyond just color (size, material, shape)
- Handle edge cases where jewelry color is ambiguous

### Multi-Agent Coordination
- Implement deadlock detection and resolution for pathfinding
- Use reservation systems to prevent robots from planning conflicting paths
- Consider communication protocols between robots (direct, indirect via environment)
- Balance individual robot efficiency vs. global system optimization
- Implement task allocation algorithms (nearest-first, load balancing, etc.)

### Performance Optimization
- Target smooth 60 FPS even with multiple robots and jewelry pieces
- Use spatial partitioning for efficient neighbor queries
- Implement LOD (Level of Detail) for distant objects if needed
- Batch operations when possible (movement updates, collision checks)
- Profile regularly to identify bottlenecks in robot pathfinding and collision detection

## Common Patterns to Follow

### Initialization
```csharp
public class ComponentName : MonoBehaviour
{
    [SerializeField] private ComponentConfig _config;
    private ServiceType _service;
    
    private void Awake()
    {
        // Initialize component state
    }
    
    private void Start() 
    {
        // Get service dependencies
        _service = ServiceRegistry.Get<ServiceType>();
    }
}
```

### Service Implementation
```csharp
/// <summary>Service description</summary>
public sealed class ServiceName
{
    /// <summary>Method description</summary>
    public ReturnType MethodName(ParameterType parameter)
    {
        // Implementation
    }
}
```

## File Organization

### Script Directories
- `Assets/Scripts/Agents/` - Robot controllers, jewelry objects, zone controllers
- `Assets/Scripts/Core/` - Core simulation management and bootstrapping
- `Assets/Scripts/Grid/` - Grid system, cells, and spatial logic
- `Assets/Scripts/Systems/` - Pathfinding, spawning, rules, and QA testing
- `Assets/Scripts/Services/` - Validation and other utility services
- `Assets/Scripts/Config/` - ScriptableObject configurations
- `Assets/Scripts/Actions/` - Action interface definitions
- `Assets/Scripts/Debug/` - Debug and testing utilities

### Naming Conventions for New Files
- Controllers: `[EntityName]Controller.cs` (e.g., `RobotController.cs`)
- Services: `[Purpose]Service.cs` (e.g., `PathfindingService.cs`)
- Systems: `[Purpose]System.cs` (e.g., `SpawnSystem.cs`)
- Components: `[Purpose]Component.cs` (e.g., `PathfindingComponent.cs`)
- Interfaces: `I[Purpose].cs` (e.g., `IAgent.cs`)
- Configuration: `[Purpose]Config.cs` (e.g., `SimulationConfig.cs`)

## Unity Editor Integration

### Custom Inspectors
- Create custom inspectors for complex components to improve designer workflow
- Use `[Header("Section Name")]` to organize inspector fields
- Add `[Tooltip("Description")]` for designer guidance
- Use `[Range(min, max)]` for numeric parameters with constraints

### Scene Setup
- Use prefabs for reusable components (robots, jewelry, zones)
- Organize scene hierarchy logically (Managers, Environment, Agents, UI)
- Use empty GameObjects as organizational containers
- Set up proper layer assignments for collision detection

## Language-Specific Guidelines

### Spanish Language Support
- Comments and documentation should be in English for international collaboration
- Variable and method names should be in English
- UI text and designer-facing content can be in Spanish
- Use consistent terminology throughout (robot/agente, jewelry/joya, etc.)

### Performance Best Practices
- Use `Vector2Int` for grid coordinates to avoid floating-point precision issues
- Cache frequently accessed components in private fields
- Use coroutines for operations that span multiple frames
- Implement object pooling for jewelry and particle effects
- Use Unity's Job System for computationally intensive pathfinding if needed

## Git and Version Control

### Ignore Patterns
- The project already includes a comprehensive `.gitignore` for Unity projects
- Temporary files and Unity-generated content are properly excluded
- Binary assets should be managed with Git LFS if they become large

### Commit Guidelines
- Make atomic commits focused on single features or fixes
- Use descriptive commit messages in English
- Reference issue numbers when applicable
- Keep commits small and focused for easier review

Remember: This is a simulation system that balances realism with performance. Prioritize clear, maintainable code that can be easily extended and debugged. The system should be robust enough for research purposes while remaining accessible for educational use.