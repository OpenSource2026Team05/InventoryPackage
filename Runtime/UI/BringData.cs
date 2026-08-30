using System;
using System.Collections.Generic;
using UnityEngine;

/// Builds display data for the selected inventory item.
/// The mesh data is used only by the optional 3D preview.
[DisallowMultipleComponent]
public sealed class BringData : MonoBehaviour
{

    /// Builds item name, description, and mesh preview data
    /// from the item stored at the given slot index.
    public InventoryDisplayData BuildDisplayData( InventoryData inventoryData, int slotIndex)
    {
        if (inventoryData == null)
        {
            return null;
        }

        Object_Grabbable sourceObject = inventoryData.GetObjectAt( slotIndex );

        if (sourceObject == null)
        {
            return null;
        }

        return new InventoryDisplayData(sourceObject,CollectMeshParts(sourceObject.transform));
    }

    private static List<InventoryMeshPartData>
        CollectMeshParts(Transform sourceRoot)
    {
        List<InventoryMeshPartData> result = new List<InventoryMeshPartData>();

        if (sourceRoot == null)
        {
            return result;
        }

        CollectMeshFilters(sourceRoot, result);
        CollectSkinnedMeshes( sourceRoot,  result);
        return result;
    }

    private static void CollectMeshFilters(Transform sourceRoot,List<InventoryMeshPartData> result)
    {
        MeshFilter[] meshFilters = sourceRoot.GetComponentsInChildren <MeshFilter>(true);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                continue;
            }

            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            Material[] materials = meshRenderer != null ? meshRenderer.sharedMaterials : Array.Empty<Material>();
            AddMeshPart(sourceRoot,meshFilter.transform, meshFilter.sharedMesh, materials, result);
        }
    }

    private static void CollectSkinnedMeshes( Transform sourceRoot,List<InventoryMeshPartData> result)
    {
        SkinnedMeshRenderer[] renderers = sourceRoot.GetComponentsInChildren <SkinnedMeshRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];

            if (renderer == null || renderer.sharedMesh == null)
            {
                continue;
            }

            AddMeshPart(sourceRoot,renderer.transform,renderer.sharedMesh,renderer.sharedMaterials,result);
        }
    }

    private static void AddMeshPart(Transform sourceRoot,Transform partTransform,Mesh mesh, Material[] materials, List<InventoryMeshPartData> result)
    {
        Matrix4x4 relativeMatrix = sourceRoot.worldToLocalMatrix * partTransform.localToWorldMatrix;
        DecomposeMatrix( relativeMatrix,out Vector3 localPosition,out Quaternion localRotation,out Vector3 localScale);
        result.Add(new InventoryMeshPartData(mesh,materials,localPosition,localRotation,localScale));
    }

    private static void DecomposeMatrix(Matrix4x4 matrix,out Vector3 position,out Quaternion rotation,out Vector3 scale)
    {
        position =matrix.GetColumn(3);
        Vector3 right = matrix.GetColumn(0);
        Vector3 up = matrix.GetColumn(1);
        Vector3 forward = matrix.GetColumn(2);
        scale = new Vector3( right.magnitude, up.magnitude, forward.magnitude);

        if (scale.x > 0.0001f)
        {
            right /= scale.x;
        }

        if (scale.y > 0.0001f)
        {
            up /= scale.y;
        }

        if (scale.z > 0.0001f)
        {
            forward /= scale.z;
        }

        if (Vector3.Dot( Vector3.Cross(right, up), forward) < 0f)
        {
            scale.x *= -1f;
            right *= -1f;
        }

        if (forward.sqrMagnitude < 0.0001f || up.sqrMagnitude < 0.0001f)
        {
            rotation = Quaternion.identity;
            return;
        }

        rotation = Quaternion.LookRotation( forward, up);
    }
}