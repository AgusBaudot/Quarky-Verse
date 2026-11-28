using System;
using UnityEngine;

public enum ControlMode
{
    Quarky,
    Shrodri
}

public class CharacterSwapManager : MonoBehaviour
{
    public static event Action<ControlMode> OnControlModeChanged;

    public ControlMode CurrentMode { get; private set; } = ControlMode.Quarky;
    public bool IsSeparated => CurrentMode == ControlMode.Shrodri;
    public Vector3 LastValidPosition { get; private set; }

    [Header("Input")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.Q;

    [Header("Characters")]
    [SerializeField, Tooltip("Quarky (Particle) - Main character controller")]
    private OdysseyPlayerController _quarkyCharacter;
    [SerializeField, Tooltip("Shrodri (Wave) - Ghost character controller")]
    private OdysseyPlayerController _shrodriCharacter;

    [Header("Quarky-Exclusive Systems")]
    [SerializeField, Tooltip("Disabled while controlling Shrodri")]
    private CollapseGlassesController _collapseGlasses;

    [Header("Cameras")]
    [SerializeField] private OdysseyThirdPersonCamera _thirdPersonCamera;
    [SerializeField] private OdysseyFirstPersonCamera _firstPersonCamera;

    [Header("Collision & Recombination Safety")]
    [SerializeField, Tooltip("Layer name for obstacles Shrodri can pass through")]
    private string _ghostLayerName = "Ghost";
    [SerializeField, Tooltip("Solid layers that block recombination if overlapping Quarky")]
    private LayerMask _invalidRecombineMask;

    private void Awake()
    {
        ConfigureGhostLayerCollision();
    }

    private void Start()
    {
        LastValidPosition = _quarkyCharacter.transform.position;
        ApplyControlMode(ControlMode.Quarky);
    }

    private void Update()
    {
        if (!IsSeparated && _quarkyCharacter.IsGrounded)
        {
            LastValidPosition = _quarkyCharacter.transform.position;
        }

        if (Input.GetKeyDown(_toggleKey))
        {
            SwitchControl();
        }
    }

    public void SwitchControl()
    {
        if (CurrentMode == ControlMode.Quarky)
        {
            ApplyControlMode(ControlMode.Shrodri);
        }
        else
        {
            if (!CanRecombine())
            {
                Debug.LogWarning("[CharacterSwapManager] Cannot recombine: Quarky's position is obstructed.");
                return;
            }

            ApplyControlMode(ControlMode.Quarky);
        }
    }

    private void ApplyControlMode(ControlMode newMode)
    {
        CurrentMode = newMode;
        bool isShrodri = newMode == ControlMode.Shrodri;

        if (isShrodri)
        {
            LastValidPosition = _quarkyCharacter.transform.position;

            _quarkyCharacter.OnControlLost();
            _quarkyCharacter.enabled = false;

            if (_collapseGlasses != null) _collapseGlasses.enabled = false;

            _shrodriCharacter.gameObject.SetActive(true);
            _shrodriCharacter.Teleport(_quarkyCharacter.transform.position, _quarkyCharacter.transform.rotation);
            _shrodriCharacter.enabled = true;

            UpdateCameras(_shrodriCharacter.transform);
        }
        else
        {
            _shrodriCharacter.OnControlLost();
            _shrodriCharacter.enabled = false;
            _shrodriCharacter.gameObject.SetActive(false);

            _quarkyCharacter.enabled = true;
            if (_collapseGlasses != null) _collapseGlasses.enabled = true;

            UpdateCameras(_quarkyCharacter.transform);
        }

        OnControlModeChanged?.Invoke(CurrentMode);
    }

    private bool CanRecombine()
    {
        CharacterController cc = _quarkyCharacter.Controller;
        if (cc == null) return true;

        Vector3 center = _quarkyCharacter.transform.TransformPoint(cc.center);
        float halfHeight = Mathf.Max(0f, (cc.height * 0.5f) - cc.radius);
        Vector3 bottom = center - Vector3.up * halfHeight;
        Vector3 top = center + Vector3.up * halfHeight;

        float checkRadius = Mathf.Max(0.01f, cc.radius - 0.05f);

        return !Physics.CheckCapsule(bottom, top, checkRadius, _invalidRecombineMask, QueryTriggerInteraction.Ignore);
    }

    private void ConfigureGhostLayerCollision()
    {
        int ghostLayer = LayerMask.NameToLayer(_ghostLayerName);
        if (ghostLayer == -1)
        {
            Debug.LogWarning($"[CharacterSwapManager] Layer '{_ghostLayerName}' not found. Create it in Tags & Layers.");
            return;
        }

        int shrodriLayer = _shrodriCharacter.gameObject.layer;
        if (shrodriLayer == _quarkyCharacter.gameObject.layer)
        {
            Debug.LogWarning("[CharacterSwapManager] Quarky and Shrodri share the same Layer! Assign Shrodri to a separate layer (e.g., 'Shrodri') so only Shrodri ignores the Ghost layer.");
            return;
        }

        Physics.IgnoreLayerCollision(shrodriLayer, ghostLayer, true);
    }

    private void UpdateCameras(Transform newTarget)
    {
        if (_thirdPersonCamera != null) _thirdPersonCamera.SetTarget(newTarget);
        if (_firstPersonCamera != null) _firstPersonCamera.SetTarget(newTarget);
    }
}