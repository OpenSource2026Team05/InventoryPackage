# Unity Inventory System

A reusable inventory system for Unity projects.

This package was designed to provide a simple inventory structure
that can be integrated with different player interaction systems.

The inventory system does not depend on a specific player controller,
pickup system, puzzle system, or game-specific interaction logic.

Developers can connect their own interaction logic to the inventory
through the provided public APIs and Unity Inspector references.


## Purpose

The purpose of this package is to provide a reusable inventory system
that can be integrated into different Unity projects with minimal
game-specific dependencies.

The package provides:

- Inventory item registration and removal
- Slot-based inventory management
- Item selection and equipped-item tracking
- Inventory UI
- Item information display
- 3D item preview
- Input System integration
- Optional Hand Pivot item detection


## Script Overview

| Script | Role |
| --- | --- |
| `InventoryData` | Stores and manages inventory slots, selected items, and equipped items. |
| `Object_Grabbable` | Defines the basic information required for an inventory item. |
| `InventoryUIManager` | Coordinates inventory data, UI, item selection, and equipped-item synchronization. |
| `InventoryUIEffect` | Updates inventory slots, selected-item information, and the 3D preview UI. |
| `InventoryInputBridge` | Connects Unity Input System actions to inventory controls. |
| `PerceiveObjectHandPivot` | Optionally detects an inventory item attached to a Hand Pivot. |
| `BringData` | Builds display information used by the inventory UI and 3D preview. |
| `InventoryDisplayData` | Stores item information used by the inventory UI. |
| `InventoryMeshPartData` | Stores mesh information required for the 3D item preview. |


## Integration

This package does not include a specific player pickup or interaction
implementation.

For example, classes such as `Player_Grab`, raycast interaction systems,
or custom player controllers should remain in the user's game project.

When an item is successfully picked up, the external interaction system
should register the item with `InventoryData`.

Example:

```csharp
public void Pickup(Object_Grabbable item)
{
    if (inventoryData.TryAdd(item, out int slotIndex))
    {
        Debug.Log($"Item added to slot {slotIndex}");
    }
}
