using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class QB2GuideTrigger : MonoBehaviour
{
    [Header("Designer Settings")]
    [TextArea(3, 6)]
    [SerializeField] private string _guideText = "Write QB2's tip here...";

    [Header("References")]
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private TMP_Text _textDisplay;

    [Header("Detection")]
    [SerializeField] private string _playerTag = "Player";

    private Collider _triggerCollider;
    private readonly List<Collider> _trackedPlayers = new List<Collider>();

    /// <summary>
    /// The currently active player transform inside the zone, or null if none.
    /// Read by companion components (e.g., FaceTargetY).
    /// </summary>
    public Transform ActivePlayer { get; private set; }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider>();
        _triggerCollider.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        ApplyText();

        if (_visualRoot != null)
            _visualRoot.SetActive(false);
    }

    private void OnEnable()
    {
        CharacterSwapManager.OnControlModeChanged += HandleControlModeChanged;
    }

    private void OnDisable()
    {
        CharacterSwapManager.OnControlModeChanged -= HandleControlModeChanged;
    }

    private void OnValidate()
    {
        ApplyText();
    }

    private void HandleControlModeChanged(ControlMode newMode)
    {
        if (newMode == ControlMode.Quarky && _trackedPlayers.Count > 0)
            RefreshState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsValidPlayerCollider(other)) return;

        if (!_trackedPlayers.Contains(other))
            _trackedPlayers.Add(other);

        RefreshState();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_trackedPlayers.Remove(other))
            RefreshState();
    }

    private void Update()
    {
        if (_trackedPlayers.Count == 0) return;

        bool changed = false;

        for (int i = _trackedPlayers.Count - 1; i >= 0; i--)
        {
            Collider col = _trackedPlayers[i];
            if (!IsStillInside(col))
            {
                _trackedPlayers.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            RefreshState();
    }

    private bool IsValidPlayerCollider(Collider col)
    {
        if (col == null) return false;
        return col.CompareTag(_playerTag) || col.GetComponent<OdysseyPlayerController>() != null;
    }

    private bool IsStillInside(Collider col)
    {
        if (col == null || !col.enabled || !col.gameObject.activeInHierarchy)
            return false;

        return Physics.ComputePenetration(
            _triggerCollider, _triggerCollider.transform.position, _triggerCollider.transform.rotation,
            col, col.transform.position, col.transform.rotation,
            out _, out _
        );
    }

    private void RefreshState()
    {
        Transform nextTarget = null;
        for (int i = 0; i < _trackedPlayers.Count; i++)
        {
            Collider col = _trackedPlayers[i];
            if (col == null || !col.gameObject.activeInHierarchy) continue;

            var controller = col.GetComponent<OdysseyPlayerController>();
            if (controller != null && !controller.enabled) continue;

            nextTarget = col.transform;
            break;
        }

        ActivePlayer = nextTarget;

        bool shouldBeVisible = ActivePlayer != null;
        if (_visualRoot != null && _visualRoot.activeSelf != shouldBeVisible)
            _visualRoot.SetActive(shouldBeVisible);
    }

    private void ApplyText()
    {
        if (_textDisplay != null)
            _textDisplay.text = _guideText;
    }

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.8f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.8f);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
}