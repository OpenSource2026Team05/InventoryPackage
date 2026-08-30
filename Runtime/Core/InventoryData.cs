using System;
using UnityEngine;

/// Stores and manages the core inventory state.
/// This class does not handle player interaction, UI input, or HandPivot behavior.
[DisallowMultipleComponent]
public sealed class InventoryData : MonoBehaviour
{
    private const int Capacity = 8;

    [Header("Slots")]
    [Tooltip("The inventory currently uses eight fixed slots.")]
    [SerializeField]
    private Object_Grabbable[] slots =
        new Object_Grabbable[Capacity];

    [Header("Runtime State")]
    [SerializeField]
    private int selectedIndex = -1;

    [SerializeField]
    private int equippedIndex = -1;

    public event Action Changed;
    public event Action InventoryFull;

    public int SlotCount => Capacity;
    public int SelectedIndex => selectedIndex;
    public int EquippedIndex => equippedIndex;

    public Object_Grabbable SelectedObject => GetObjectAt(selectedIndex);

    public Object_Grabbable EquippedObject => GetObjectAt(equippedIndex);

    private void Awake()
    {
        EnsureSlots();
        ValidateIndices();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureSlots();
        ValidateIndices();
    }
#endif

    /// Returns the item stored at the given slot index.
    public Object_Grabbable GetObjectAt(int slotIndex)
    {
        if (!IsValidIndex(slotIndex))
        {
            return null;
        }

        return slots[slotIndex];
    }

    /// Returns the display name of the item stored at the given slot index.
    public string GetObjectNameAt(int slotIndex)
    {
        Object_Grabbable item =
            GetObjectAt(slotIndex);

        return item != null
            ? item.ObjectName
            : "—";
    }


    /// Finds the slot index of the given item instance.
    public int FindIndexBySource(
        Object_Grabbable sourceObject)
    {
        if (sourceObject == null)
        {
            return -1;
        }

        for (int i = 0; i < Capacity; i++)
        {
            if (slots[i] == sourceObject)
            {
                return i;
            }
        }

        return -1;
    }


    /// Adds an item to the first empty slot.
    /// Call this method from the user's pickup or interaction system.
    public bool TryAdd(Object_Grabbable sourceObject, out int addedIndex)
    {
        addedIndex = -1;

        if (sourceObject == null)
        {
            return false;
        }

        int existingIndex =
            FindIndexBySource(sourceObject);

        if (existingIndex >= 0)
        {
            addedIndex = existingIndex;
            return true;
        }

        for (int i = 0; i < Capacity; i++)
        {
            if (slots[i] != null)
            {
                continue;
            }

            slots[i] = sourceObject;
            addedIndex = i;

            Changed?.Invoke();
            return true;
        }

        InventoryFull?.Invoke();
        return false;
    }


    /// Selects a slot for UI display.
    /// Empty slots may also be selected.
    public void Select(int slotIndex)
    {
        if (!IsValidIndex(slotIndex) ||
            selectedIndex == slotIndex)
        {
            return;
        }

        selectedIndex = slotIndex;
        Changed?.Invoke();
    }


    /// Sets the equipped slot.
    /// Use -1 to clear the equipped state.
    /// Actual hand or character equipment is handled by external game code.
    public bool SetEquipped(int slotIndex)
    {
        if (slotIndex == -1)
        {
            if (equippedIndex == -1)
            {
                return true;
            }

            equippedIndex = -1;
            Changed?.Invoke();
            return true;
        }

        if (!IsValidIndex(slotIndex) ||
            slots[slotIndex] == null)
        {
            return false;
        }

        if (equippedIndex == slotIndex)
        {
            return true;
        }

        equippedIndex = slotIndex;
        Changed?.Invoke();
        return true;
    }

    /// Selects and equips the same occupied slot.
    public bool SetSelectedAndEquipped(
        int slotIndex)
    {
        if (!IsValidIndex(slotIndex) ||
            slots[slotIndex] == null)
        {
            return false;
        }

        bool changed =
            selectedIndex != slotIndex ||
            equippedIndex != slotIndex;

        selectedIndex = slotIndex;
        equippedIndex = slotIndex;

        if (changed)
        {
            Changed?.Invoke();
        }

        return true;
    }

    /// Removes the item stored at the given slot index.
    public bool RemoveAt(int slotIndex)
    {
        if (!IsValidIndex(slotIndex) ||
            slots[slotIndex] == null)
        {
            return false;
        }

        slots[slotIndex] = null;

        if (selectedIndex == slotIndex)
        {
            selectedIndex = -1;
        }

        if (equippedIndex == slotIndex)
        {
            equippedIndex = -1;
        }

        Changed?.Invoke();
        return true;
    }

    /// Removes the given item instance from the inventory.
    public bool RemoveBySource(
        Object_Grabbable sourceObject)
    {
        int index =
            FindIndexBySource(sourceObject);

        return index >= 0 &&
               RemoveAt(index);
    }

    /// Clears all inventory slots and runtime selection state.
    public void Clear()
    {
        Array.Clear(slots,0, slots.Length);

        selectedIndex = -1;
        equippedIndex = -1;

        Changed?.Invoke();
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < Capacity;
    }

    private void EnsureSlots()
    {
        if (slots != null && slots.Length == Capacity)
        {
            return;
        }

        Object_Grabbable[] resized = new Object_Grabbable[Capacity];

        if (slots != null)
        {
            Array.Copy(slots,resized,Mathf.Min(slots.Length,Capacity));
        }

        slots = resized;
    }

    private void ValidateIndices()
    {
        if (!IsValidIndex(selectedIndex))
        {
            selectedIndex = -1;
        }

        if (!IsValidIndex(equippedIndex) || (equippedIndex >= 0 && slots[equippedIndex] == null))
        {
            equippedIndex = -1;
        }
    }
}