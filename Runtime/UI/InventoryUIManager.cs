using System;
using UnityEngine;
using UnityEngine.Events;

/// Event invoked when the user requests an inventory item to be equipped.
/// Connect this event to the project's own hand or equipment system when needed.
[Serializable]
public sealed class GrabbableEvent :
    UnityEvent<Object_Grabbable>
{
}


/// Connects the core inventory state to the provided inventory UI.
/// Player movement, HandPivot, pickup, and game-specific input logic are intentionally excluded.
[DisallowMultipleComponent]
public sealed class InventoryUIManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField]
    private InventoryData inventoryData;

    [SerializeField]
    private BringData bringData;

    [SerializeField]
    private InventoryUIEffect uiEffect;

    [Header("Window")]
    [SerializeField]
    private bool startClosed = true;

    [Header("Optional Equip Request")]
    [Tooltip(
        "Optional callback for a project-specific hand or equipment system."
    )]
    [SerializeField]
    private GrabbableEvent onEquipRequested;

    private InventoryDisplayData selectedDisplayData;
    private bool isOpen;

    public bool IsOpen => isOpen;

    public InventoryDisplayData SelectedDisplayData =>
        selectedDisplayData;

    private void Awake()
    {
        isOpen = !startClosed;

        if (uiEffect != null)
        {
            uiEffect.BindSlots( SelectSlot);
            uiEffect.SetOpen(isOpen);
        }
    }

    private void OnEnable()
    {
        if (inventoryData != null)
        {
            inventoryData.Changed += HandleInventoryChanged;
        }
    }

    private void Start()
    {
        SyncSelectedDisplayData();
        RefreshView();
    }

    private void OnDisable()
    {
        if (inventoryData != null)
        {
            inventoryData.Changed -= HandleInventoryChanged;
        }
    }

    public void ToggleInventory()
    {
        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;

        if (uiEffect != null)
        {
            uiEffect.SetOpen(true);
        }

        SyncSelectedDisplayData();
        RefreshView();
    }

    public void CloseInventory()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        if (uiEffect != null)
        {
            uiEffect.SetOpen(false);
        }
    }


    /// Selects a slot and refreshes the selected item display.
    public void SelectSlot(int slotIndex)
    {
        if (inventoryData == null)
        {
            return;
        }

        inventoryData.Select(
            slotIndex
        );
    }


    /// Equips the selected item in the inventory state
    /// and notifies external game code through On Equip Requested.
    public void ConfirmSelectedItem()
    {
        if (inventoryData == null)
        {
            return;
        }

        int selectedIndex = inventoryData.SelectedIndex;

        Object_Grabbable selectedObject = inventoryData.GetObjectAt( selectedIndex );

        if (selectedObject == null)
        {
            return;
        }

        if (!inventoryData.SetEquipped(selectedIndex))
        {
            return;
        }

        onEquipRequested?.Invoke(
            selectedObject
        );
    }

    /// Moves the equipped selection to the next occupied slot.
    /// This method can be called by any external input system.
    public void CycleSelectedItem(int direction)
    {
        if (inventoryData == null || direction == 0)
        {
            return;
        }

        int step = direction > 0 ? 1 : -1;

        int startIndex = inventoryData.EquippedIndex;

        if (startIndex < 0)
        {
            startIndex = step > 0 ? -1 : inventoryData.SlotCount;
        }

        for (int i = 1; i <= inventoryData.SlotCount; i++)
        {
            int nextIndex =(startIndex + (step * i) + inventoryData.SlotCount) % inventoryData.SlotCount;
            Object_Grabbable item =inventoryData.GetObjectAt(nextIndex);
            if (item == null)
            {
                continue;
            }

            inventoryData.SetSelectedAndEquipped(nextIndex);
            onEquipRequested?.Invoke(item);
            return;
        }
    }

    private void HandleInventoryChanged()
    {
        SyncSelectedDisplayData();
        RefreshView();
    }

    private void SyncSelectedDisplayData()
    {
        if (inventoryData == null || bringData == null)
        {
            selectedDisplayData = null;
            return;
        }

        selectedDisplayData = bringData.BuildDisplayData(inventoryData,inventoryData.SelectedIndex);
    }

    private void RefreshView()
    {
        if (uiEffect == null)
        {
            return;
        }

        uiEffect.Refresh(inventoryData, selectedDisplayData, isOpen);
    }
}