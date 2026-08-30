using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// References and updates one inventory slot in the provided UI.
[Serializable]
public sealed class InventorySlotView
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private GameObject selectedFrame;

    [SerializeField]
    private GameObject equippedFrame;
    private UnityAction clickAction;

    public void Bind(
        int slotIndex,
        Action<int> onClicked)
    {
        Unbind();

        if (button == null ||
            onClicked == null)
        {
            return;
        }

        clickAction =
            () => onClicked(slotIndex);

        button.onClick.AddListener(
            clickAction
        );
    }

    public void Unbind()
    {
        if (button != null &&
            clickAction != null)
        {
            button.onClick.RemoveListener(
                clickAction
            );
        }

        clickAction = null;
    }

    public void Refresh(
        string itemName,
        bool selected,
        bool equipped)
    {
        if (itemNameText != null)
        {
            itemNameText.text =
                string.IsNullOrWhiteSpace(
                    itemName)
                    ? "—"
                    : itemName;
        }

        if (selectedFrame != null)
        {
            selectedFrame.SetActive(
                selected
            );
        }

        if (equippedFrame != null)
        {
            equippedFrame.SetActive(
                equipped
            );
        }
    }
}

/// Updates the inventory UI and the optional 3D item preview.
/// This component does not require a Player or HandPivot reference.
[DisallowMultipleComponent]
public sealed class InventoryUIEffect : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField]
    private GameObject inventoryRoot;

    [Header("Slots")]
    [Tooltip(
        "Assign slot views in inventory index order."
    )]
    [SerializeField]
    private InventorySlotView[] slots =
        new InventorySlotView[8];

    [Header("Selected Item")]
    [SerializeField]
    private TMP_Text selectedNameText;

    [SerializeField]
    private TMP_Text selectedDescriptionText;

    [Header("3D Preview (Optional)")]
    [Tooltip(
        "Root object enabled while a 3D item preview is visible."
    )]
    [SerializeField]
    private GameObject objectSelected;

    [Tooltip(
        "Parent transform used for generated 3D preview objects."
    )]
    [SerializeField]
    private Transform selectedRoot;

    [Header("Preview Transform")]
    [SerializeField]
    private Vector3 previewPosition =
        Vector3.zero;

    [SerializeField]
    private Vector3 previewRotation =
        new Vector3(
            15f,
            -25f,
            0f
        );

    [Min(0.0001f)]
    [SerializeField]
    private float previewTargetSize =
        0.3f;

    private GameObject currentPreviewObject;
    private Object_Grabbable currentPreviewSource;

    private void Awake()
    {
        SetPreviewActive(false);
    }

    private void OnDestroy()
    {
        UnbindSlots();
        HidePreview();
    }

    public void BindSlots(
        Action<int> onSlotClicked)
    {
        UnbindSlots();

        if (slots == null ||
            onSlotClicked == null)
        {
            return;
        }

        for (int i = 0;
             i < slots.Length;
             i++)
        {
            slots[i]?.Bind(
                i,
                onSlotClicked
            );
        }
    }

    public void SetOpen(bool open)
    {
        if (inventoryRoot != null)
        {
            inventoryRoot.SetActive(
                open
            );
        }

        if (!open)
        {
            HidePreview();
        }
    }

    public void Refresh(
        InventoryData inventoryData,
        InventoryDisplayData selectedDisplayData,
        bool open)
    {
        RefreshSlots(
            inventoryData
        );

        RefreshSelectedInformation(
            selectedDisplayData,
            open
        );
    }

    private void RefreshSlots(
        InventoryData inventoryData)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0;
             i < slots.Length;
             i++)
        {
            InventorySlotView slot =
                slots[i];

            if (slot == null)
            {
                continue;
            }

            slot.Refresh(
                inventoryData != null
                    ? inventoryData.GetObjectNameAt(i)
                    : "—",
                inventoryData != null &&
                    inventoryData.SelectedIndex == i,
                inventoryData != null &&
                    inventoryData.EquippedIndex == i
            );
        }
    }

    private void RefreshSelectedInformation(
        InventoryDisplayData selectedDisplayData,
        bool open)
    {
        if (selectedNameText != null)
        {
            selectedNameText.text =
                selectedDisplayData != null
                    ? selectedDisplayData.ItemName
                    : "—";
        }

        if (selectedDescriptionText != null)
        {
            selectedDescriptionText.text =
                selectedDisplayData != null
                    ? selectedDisplayData.Description
                    : string.Empty;
        }

        if (!open ||
            selectedDisplayData == null)
        {
            HidePreview();
            return;
        }

        ShowPreview(
            selectedDisplayData
        );
    }

    /// Generates a 3D preview from the selected item's mesh data.
    /// Leave objectSelected or selectedRoot unassigned to disable this feature.
    private void ShowPreview(
        InventoryDisplayData displayData)
    {
        if (displayData.SourceObject == null ||
            displayData.MeshParts == null ||
            displayData.MeshParts.Count == 0 ||
            objectSelected == null ||
            selectedRoot == null)
        {
            HidePreview();
            return;
        }

        if (currentPreviewObject != null &&
            currentPreviewSource ==
                displayData.SourceObject)
        {
            SetPreviewActive(true);
            return;
        }

        HidePreview();

        currentPreviewSource =
            displayData.SourceObject;

        currentPreviewObject =
            new GameObject(
                $"Preview_{displayData.ItemName}"
            );

        Transform previewTransform =
            currentPreviewObject.transform;

        previewTransform.SetParent(
            selectedRoot,
            false
        );

        previewTransform.localPosition =
            previewPosition;

        previewTransform.localRotation =
            Quaternion.Euler(
                previewRotation
            );

        GameObject geometryObject =
            new GameObject("Geometry");

        Transform geometryRoot =
            geometryObject.transform;

        geometryRoot.SetParent(
            previewTransform,
            false
        );

        Bounds combinedBounds = default;
        bool hasBounds = false;

        for (int i = 0;
             i < displayData.MeshParts.Count;
             i++)
        {
            InventoryMeshPartData meshPart =
                displayData.MeshParts[i];

            if (meshPart == null ||
                meshPart.Mesh == null)
            {
                continue;
            }

            Transform partTransform =
                CreatePreviewMeshPart(
                    meshPart,
                    i,
                    geometryRoot
                );

            EncapsulateMeshBounds(
                meshPart.Mesh.bounds,
                partTransform,
                previewTransform,
                ref combinedBounds,
                ref hasBounds
            );
        }

        if (!hasBounds)
        {
            HidePreview();
            return;
        }

        geometryRoot.localPosition =
            -combinedBounds.center;

        float largestSize =
            Mathf.Max(
                combinedBounds.size.x,
                combinedBounds.size.y,
                combinedBounds.size.z
            );

        if (largestSize > 0.0001f)
        {
            previewTransform.localScale =
                Vector3.one *
                (previewTargetSize /
                 largestSize);
        }

        SetPreviewActive(true);
    }

    private static Transform CreatePreviewMeshPart(
        InventoryMeshPartData meshPart,
        int partIndex,
        Transform geometryRoot)
    {
        GameObject partObject =
            new GameObject(
                $"MeshPart_{partIndex}"
            );

        Transform partTransform =
            partObject.transform;

        partTransform.SetParent(
            geometryRoot,
            false
        );

        partTransform.localPosition =
            meshPart.LocalPosition;

        partTransform.localRotation =
            meshPart.LocalRotation;

        partTransform.localScale =
            meshPart.LocalScale;

        MeshFilter meshFilter =
            partObject.AddComponent<MeshFilter>();

        meshFilter.sharedMesh =
            meshPart.Mesh;

        MeshRenderer meshRenderer =
            partObject.AddComponent<MeshRenderer>();

        meshRenderer.sharedMaterials =
            meshPart.Materials;

        meshRenderer.shadowCastingMode =
            ShadowCastingMode.Off;

        meshRenderer.receiveShadows =
            false;

        meshRenderer.lightProbeUsage =
            LightProbeUsage.Off;

        meshRenderer.reflectionProbeUsage =
            ReflectionProbeUsage.Off;

        return partTransform;
    }

    private static void EncapsulateMeshBounds(
        Bounds meshBounds,
        Transform partTransform,
        Transform previewTransform,
        ref Bounds combinedBounds,
        ref bool hasBounds)
    {
        Matrix4x4 partToPreviewMatrix =
            previewTransform.worldToLocalMatrix *
            partTransform.localToWorldMatrix;

        Vector3 min =
            meshBounds.min;

        Vector3 max =
            meshBounds.max;

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector3 localCorner =
                        new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z
                        );

                    Vector3 previewCorner =
                        partToPreviewMatrix
                            .MultiplyPoint3x4(
                                localCorner
                            );

                    if (!hasBounds)
                    {
                        combinedBounds =
                            new Bounds(
                                previewCorner,
                                Vector3.zero
                            );

                        hasBounds = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(
                            previewCorner
                        );
                    }
                }
            }
        }
    }

    private void HidePreview()
    {
        SetPreviewActive(false);

        if (currentPreviewObject != null)
        {
            if (Application.isPlaying)
            {
                Destroy(
                    currentPreviewObject
                );
            }
            else
            {
                DestroyImmediate(
                    currentPreviewObject
                );
            }
        }

        currentPreviewObject = null;
        currentPreviewSource = null;
    }

    private void SetPreviewActive(
        bool active)
    {
        if (objectSelected != null)
        {
            objectSelected.SetActive(
                active
            );
        }
    }

    private void UnbindSlots()
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0;
             i < slots.Length;
             i++)
        {
            slots[i]?.Unbind();
        }
    }
}