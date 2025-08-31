# GridRenderer Usage Guide

## Overview

The `GridRenderer` component provides minimal grid visualization for the Unity multiagent jewelry display system. It displays the logical state of the grid using Gizmos in the SceneView for debugging purposes.

## Features

- **Preview Mode**: Works without `GridService` using configurable dimensions
- **Gizmos Rendering**: Uses Unity Gizmos for SceneView visualization by default
- **Color-coded Cell Types**: Different colors for Empty, Wall, Shelf, Jewel, Zone, and RobotSpawn cells
- **Occupancy Visualization**: Shows dynamic occupants (Robot, Jewel, Zone, Reserved) with distinct symbols
- **No Circular Dependencies**: Safely consumes `GridService` via `ServiceRegistry` without creating it

## Usage

### Basic Setup

1. Add the `GridRenderer` component to any GameObject in your scene
2. Configure the preview settings in the Inspector if you want to test without `GridService`
3. The renderer will automatically connect to `GridService` if available, or use preview mode

### Preview Mode

When no `GridService` is registered:
- Uses configurable `_previewWidth`, `_previewHeight`, `_previewCellSize`, and `_previewOrigin`
- Displays a simple grid of empty cells with grid lines
- Useful for testing and initial scene setup

### With GridService

When `GridService` is available:
- Automatically displays the actual grid data
- Shows all cell types with appropriate colors
- Displays dynamic occupancy information
- Updates visualization as the grid state changes

### Color Configuration

The component provides Inspector fields to customize colors for:

**Cell Types:**
- Empty: Light gray (transparent)
- Wall: Brown (solid)
- Shelf: Tan (solid)
- Jewel: Gold (semi-transparent)
- Zone: Green (semi-transparent)
- RobotSpawn: Blue (semi-transparent)

**Occupants:**
- Robot: Red sphere
- Jewel: Yellow wire sphere
- Zone: Green wire cube
- Reserved: Magenta wire cube

### Inspector Settings

- **Preview Mode Settings**: Configure dimensions and appearance for preview mode
- **Rendering Settings**: Toggle grid, cell types, occupancy, and future tile mode
- **Colors**: Customize all visualization colors
- **Occupancy Colors**: Customize occupant visualization colors

## Integration Example

```csharp
// Example: Using GridRenderer with GridService
public class ExampleUsage : MonoBehaviour
{
    void Start()
    {
        // Create and register GridService
        var map = new GridMap(10, 8);
        var gridService = new GridService(map, Vector3.zero, 1.0f);
        ServiceRegistry.Register<GridService>(gridService);
        
        // GridRenderer will automatically connect to it
        // Add GridRenderer component to visualize the grid
        gameObject.AddComponent<GridRenderer>();
    }
}
```

## Demo

Use the `GridRendererDemo` component to see the GridRenderer in action:

1. Create an empty GameObject
2. Add the `GridRendererDemo` component
3. Configure demo settings in the Inspector
4. The demo will create a sample grid with all cell types and occupants
5. Add a `GridRenderer` component to the same or another GameObject to visualize it

## Context Menu Options

- **Reconnect to GridService**: Manually attempt to reconnect to GridService if it was registered after Start()

## Performance Notes

- Gizmos are only drawn in SceneView and when objects are selected
- No performance impact during gameplay
- Consider disabling `_showOccupancy` for very large grids if needed

## Troubleshooting

**Grid not visible:**
- Ensure the GameObject with GridRenderer is in the scene
- Check that `_showGrid` is enabled in the Inspector
- Verify you're looking at the SceneView (Gizmos don't appear in Game view)

**Preview mode instead of actual grid:**
- Ensure `GridService` is properly registered in `ServiceRegistry`
- Use the "Reconnect to GridService" context menu option
- Check console for connection messages

**Colors not as expected:**
- Adjust color settings in the Inspector
- Verify cell types are set correctly in your `GridMap`
- Check that `_showCellTypes` and `_showOccupancy` are enabled as needed