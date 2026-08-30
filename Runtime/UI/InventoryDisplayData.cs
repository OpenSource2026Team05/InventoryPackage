using System.Collections.Generic;

/// Read-only data used to display a selected inventory item.
public sealed class InventoryDisplayData
{
    public string ItemName { get; }
    public string Description { get; }

    public Object_Grabbable SourceObject { get; }

    public IReadOnlyList<InventoryMeshPartData> MeshParts { get; }

    public InventoryDisplayData( Object_Grabbable sourceObject, List<InventoryMeshPartData> meshParts)
    {
        SourceObject = sourceObject;
        ItemName = sourceObject != null ? sourceObject.ObjectName : "Unknown";
        Description = sourceObject != null ? sourceObject.Description : string.Empty;
        MeshParts = meshParts ?? new List<InventoryMeshPartData>();
    }
}