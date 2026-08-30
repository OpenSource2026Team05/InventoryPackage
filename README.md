# Unity Inventory System

A reusable inventory system for Unity projects.

This package provides a simple slot-based inventory system with a ready-to-use UI and an optional 3D item preview.

The system is designed to remain independent from project-specific player controllers, pickup systems, input systems, puzzle systems, and interaction logic.

Developers can connect their own gameplay systems through the provided public methods and Unity Inspector events.


## Purpose

The purpose of this package is to provide a lightweight and reusable inventory system that can be integrated into different Unity projects with minimal project-specific dependencies.

The package provides:

- 8-slot inventory management
- Item registration and removal
- Slot selection
- Equipped-item state tracking
- Inventory UI
- Item name and description display
- Optional 3D item preview
- Public methods for external pickup and input systems
- Optional equip event for external player or equipment systems


## Package Structure

```text
com.cheeseandthemoon.inventory/
│
├── package.json
├── README.md
├── LICENSE
├── CHANGELOG.md
│
├── Runtime/
    │
    ├── Core/
    │   ├── Object_Grabbable.cs
    │   └── InventoryData.cs
    │
    ├── UI/
    │   ├── InventoryUIManager.cs
    │   ├── InventoryUIEffect.cs
    │   ├── BringData.cs
    │   ├── InventoryDisplayData.cs
    │   └── InventoryMeshPartData.cs
    │
    ├── Prefabs/
    │   └── InventorySystem.prefab
    │
    ├── Art/
    │   └── UI image assets
    │
    └── CheeseAndTheMoon.Inventory.asmdef

```

`Samples~` is optional and may be added later.


## Script Overview

| Script | Role |
| --- | --- |
| `Object_Grabbable` | Stores the basic information required by an inventory item, such as its name and description. |
| `InventoryData` | Core inventory logic. Manages slots, adding and removing items, selected slots, and equipped-item state. |
| `InventoryUIManager` | Connects `InventoryData` to the provided inventory UI and exposes methods used by external input or gameplay systems. |
| `InventoryUIEffect` | Updates slot visuals, selected-item information, and the optional 3D preview. |
| `BringData` | Collects item information and mesh data used by the inventory UI and 3D preview. |
| `InventoryDisplayData` | Stores read-only display information for the currently selected item. |
| `InventoryMeshPartData` | Stores mesh, material, and transform information used by the optional 3D preview. |


## Core and UI

The package is divided into two main parts.

### Core

The Core contains the minimum logic required to operate the inventory.

```text
Object_Grabbable
InventoryData
```

The Core does not depend on:

- Player controllers
- Pickup systems
- Input systems
- Hand Pivot systems
- Audio systems
- Puzzle systems
- Scene-specific gameplay code


### Inventory UI

The UI layer provides the default inventory interface included with this package.

```text
InventoryUIManager
InventoryUIEffect
BringData
InventoryDisplayData
InventoryMeshPartData
InventorySystem.prefab
UI image assets
```

The provided UI supports slot display, selected-item information, and an optional 3D mesh preview.


## Adding Items

This package does not include a specific pickup implementation.

The developer's own interaction system should call `InventoryData.TryAdd()` when an item is successfully picked up.

Example:

```csharp
public void Pickup(Object_Grabbable item)
{
    if (inventoryData.TryAdd(item, out int slotIndex))
    {
        Debug.Log($"Item added to slot {slotIndex}");
    }
}
```

The pickup system may be implemented using raycasts, triggers, mouse clicks, player interaction scripts, or any other gameplay system.


## Input Integration

This package does not require a specific Unity Input System configuration.

Input should be connected to the public methods provided by `InventoryUIManager`.

Available methods include:

```csharp
ToggleInventory();
OpenInventory();
CloseInventory();
SelectSlot(int slotIndex);
ConfirmSelectedItem();
CycleSelectedItem(int direction);
```

For example, a project's own input script can call:

```csharp
inventoryUIManager.ToggleInventory();
```

This keeps the package independent from project-specific keyboard, mouse, gamepad, or mobile input settings.


## Equipment Integration

The package tracks which inventory slot is currently equipped, but it does not control a player character or Hand Pivot directly.

`InventoryUIManager` provides an optional `On Equip Requested` event.

Projects that require visible equipped items can connect their own equipment or player-hand system to this event.

For example:

```text
InventoryUIManager
    On Equip Requested
            ↓
Player Equipment System
            ↓
Player Hand / Weapon Slot / Custom Equipment
```

Projects that do not require an equipment system can leave this event unassigned.


## 3D Preview

The included UI supports an optional 3D preview of the selected inventory item.

The preview is generated using the Mesh and Material information of the selected item.

The preview does not require a Player or Hand Pivot reference.

Projects that do not require the 3D preview may leave the preview references unassigned.


## Code Comments and Extension Points

The source code contains English comments and Inspector tooltips describing the responsibility of each component.

Important integration points are also marked in the source code.

Examples include:

- `InventoryData.TryAdd()` for external pickup systems
- `InventoryData.RemoveAt()` for item removal
- `InventoryUIManager` public methods for external input systems
- `On Equip Requested` for project-specific equipment systems
- Optional preview references in `InventoryUIEffect`

Game-specific behavior should be implemented outside this package.


## Prefab

The package includes a preconfigured `InventorySystem.prefab`.

The prefab contains the Inventory Core and the default Inventory UI.

After adding the prefab to a scene, developers can configure UI references and optional preview settings through the Inspector.


## Dependencies

The Inventory Core only requires Unity.

The provided UI uses TextMeshPro and Unity UI.


## License

See the `LICENSE` file for license information.
