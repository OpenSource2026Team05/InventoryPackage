using System;
using UnityEngine;

/// Stores one mesh part used by the optional 3D inventory preview.
public sealed class InventoryMeshPartData
{
    public Mesh Mesh { get; }
    public Material[] Materials { get; }
    public Vector3 LocalPosition { get; }
    public Quaternion LocalRotation { get; }
    public Vector3 LocalScale { get; }

    public InventoryMeshPartData( Mesh mesh, Material[] materials, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        Mesh = mesh;

        Materials = materials ?? Array.Empty<Material>();

        LocalPosition = localPosition;
        LocalRotation = localRotation;
        LocalScale = localScale;
    }
}