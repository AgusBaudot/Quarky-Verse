using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Singleton registry of all active SuperpositionControllers in the scene.
/// Replaces Physics.OverlapSphere in CollapseGlassesController, fully
/// decoupling object detection from the presence or state of a Collider.
/// Each SuperpositionController registers itself on Start and unregisters
/// on OnDestroy.
/// </summary>

// PROTOTYPE NOTE: Uses a simple List with linear distance check.
// In production, consider spatial partitioning if object count is large. 

public class QuantumRegistry : MonoBehaviour
{
    public static QuantumRegistry Instance { get; private set; }

    private readonly List<SuperpositionController> _controllers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(SuperpositionController c) => _controllers.Add(c);
    public void Unregister(SuperpositionController c) => _controllers.Remove(c);
    
    //Returns all controllers withing radius of origin, regardless of collider state
    public List<SuperpositionController> GetInRadius(Vector3 origin, float radius)
    {
        return _controllers.Where(c => c != null && (c.transform.position - origin).sqrMagnitude <= radius * radius).ToList();
    }
}