using UnityEngine;

/// Basic item component used by the Inventory System.
/// This component does not depend on any specific player, grab, audio, scene, or puzzle system.
[DisallowMultipleComponent]
public sealed class Object_Grabbable : MonoBehaviour
{
    [Header("Item Information")]

    [Tooltip("The item name displayed in the inventory.")]
    public string objectName;

    [Tooltip("The item description displayed in the inventory.")]
    [TextArea(3, 8)]
    public string description;
    /// Gets the display name of the item.
    /// If objectName is empty, the GameObject name is used instead.
    public string ObjectName => string.IsNullOrWhiteSpace(objectName) ? gameObject.name : objectName;

    /// Gets the display description of the item.
    public string Description =>
        description ?? string.Empty;
}