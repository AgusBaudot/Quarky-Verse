using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles rendering, pooling, and event management for Quantum Ghosts.
/// Keeps transitions decoupled from visual ghost logic and ensures SRP batching.
/// </summary>
[RequireComponent(typeof(SuperpositionController), typeof(Renderer))]
public class QuantumGhostManager : MonoBehaviour
{
    [SerializeField] private float _ghostAlpha = 0.4f;
    [SerializeField] private Transform _ghostParent;

    private Renderer _renderer;
    private SuperpositionController _controller;
    private MaterialPropertyBlock _mpb;

    // Uses a custom comparer to prevent floating-point precision misses
    private readonly Dictionary<Vector3, GameObject> _ghosts = new(new Vector3EpsilonComparer());

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _controller = GetComponent<SuperpositionController>();
        _mpb = new MaterialPropertyBlock();

        _controller.OnCollapse += HideAllGhosts;
        _controller.OnRestore += ShowAllGhosts;
        _controller.OnQuantumDeactivated += HideAllGhosts;
        _controller.OnQuantumActivated += ShowAllGhosts;
    }

    private void OnDestroy()
    {
        if (_controller != null)
        {
            _controller.OnCollapse -= HideAllGhosts;
            _controller.OnRestore -= ShowAllGhosts;
            _controller.OnQuantumDeactivated -= HideAllGhosts;
            _controller.OnQuantumActivated -= ShowAllGhosts;
        }
    }

    /// <summary>
    /// Pre-warms ghosts at the given offsets.
    /// </summary>
    public void InitializeGhosts(List<Vector3> offsets, Vector3 originPosition)
    {
        EnsureInitialized();
        
        foreach (var offset in offsets)
        {
            Vector3 pos = originPosition + offset;
            EnsureGhostExists(pos, transform.rotation, transform.localScale);
        }
    }

    public void EnsureGhostExists(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        EnsureInitialized();
        
        if (!_ghosts.ContainsKey(pos))
        {
            _ghosts[pos] = BuildGhost(pos, rot, scale);
        }
    }

    public void ShowGhostAt(Vector3 position)
    {
        if (_ghosts.TryGetValue(position, out var ghost))
            ghost.SetActive(true);
    }

    public void HideGhostAt(Vector3 position)
    {
        if (_ghosts.TryGetValue(position, out var ghost))
            ghost.SetActive(false);
    }

    private GameObject BuildGhost(Vector3 worldPos, Quaternion rot, Vector3 scale)
    {
        var ghost = new GameObject("QuantumGhost");
        var mf = ghost.AddComponent<MeshFilter>();
        var mr = ghost.AddComponent<MeshRenderer>();
        
        var myMf = GetComponent<MeshFilter>();
        if (myMf != null) mf.mesh = myMf.sharedMesh;
        
        // Share the material to prevent memory leaks; use PropertyBlock for unique alpha
        mr.sharedMaterial = _renderer.sharedMaterial;
        
        ghost.transform.position = worldPos;
        ghost.transform.rotation = rot;
        ghost.transform.localScale = scale;
        
        SetAlpha(mr, _ghostAlpha);
        
        if (_ghostParent != null)
            ghost.transform.SetParent(_ghostParent, true);
        else
            ghost.transform.SetParent(transform.parent, true);
        
        return ghost;
    }

    private void SetAlpha(Renderer r, float alpha)
    {
        r.GetPropertyBlock(_mpb);
        Color c = r.sharedMaterial.HasProperty("_BaseColor") ? r.sharedMaterial.GetColor("_BaseColor") : Color.white;
        c.a = alpha;
        _mpb.SetColor("_BaseColor", c);
        r.SetPropertyBlock(_mpb);
    }

    private void HideAllGhosts()
    {
        foreach (var ghost in _ghosts.Values)
            ghost.SetActive(false);
    }

    private void ShowAllGhosts()
    {
        foreach (var ghost in _ghosts.Values)
            ghost.SetActive(true);
    }
    
    private void EnsureInitialized()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }
    }
}

/// <summary>
/// Prevents Dictionary TryGetValue failures due to tiny floating-point inaccuracies.
/// </summary>
public class Vector3EpsilonComparer : IEqualityComparer<Vector3>
{
    public bool Equals(Vector3 x, Vector3 y) => Vector3.SqrMagnitude(x - y) < 0.0001f;
    
    public int GetHashCode(Vector3 obj) => 
        Mathf.RoundToInt(obj.x * 10f) ^ Mathf.RoundToInt(obj.y * 10f) ^ Mathf.RoundToInt(obj.z * 10f);
}