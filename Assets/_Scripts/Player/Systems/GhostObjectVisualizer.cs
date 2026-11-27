using System.Collections.Generic;
using UnityEngine;

public class GhostObjectVisualizer : MonoBehaviour
{
    [Header("Ghost Object System")]
    [SerializeField, Tooltip("Objects that become transparent when controlling Shrodri")]
    private List<GameObject> _ghostObjects = new List<GameObject>();

    [SerializeField, Tooltip("Standard transparent teal/greenish-blue Unity material")]
    private Material _ghostMaterial;

    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Awake()
    {
        CacheOriginalMaterials();
    }

    private void OnEnable()
    {
        CharacterSwapManager.OnControlModeChanged += HandleControlModeChanged;
    }

    private void OnDisable()
    {
        CharacterSwapManager.OnControlModeChanged -= HandleControlModeChanged;
    }

    private void CacheOriginalMaterials()
    {
        _originalMaterials.Clear();

        foreach (GameObject ghostObj in _ghostObjects)
        {
            if (ghostObj == null) continue;

            Renderer[] renderers = ghostObj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in renderers)
            {
                if (!_originalMaterials.ContainsKey(rend))
                {
                    _originalMaterials.Add(rend, rend.sharedMaterials);
                }
            }
        }
    }

    private void HandleControlModeChanged(ControlMode mode)
    {
        if (mode == ControlMode.Shrodri)
        {
            ApplyGhostMaterial();
        }
        else
        {
            RestoreOriginalMaterials();
        }
    }

    private void ApplyGhostMaterial()
    {
        if (_ghostMaterial == null) return;

        foreach (var kvp in _originalMaterials)
        {
            Renderer rend = kvp.Key;
            if (rend == null) continue;

            int slotCount = kvp.Value.Length;
            Material[] ghostMats = new Material[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                ghostMats[i] = _ghostMaterial;
            }

            rend.sharedMaterials = ghostMats;
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in _originalMaterials)
        {
            Renderer rend = kvp.Key;
            if (rend == null) continue;

            rend.sharedMaterials = kvp.Value;
        }
    }
}