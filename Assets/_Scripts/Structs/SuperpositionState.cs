using UnityEngine;

/// <summary>
/// Shared data struct representing a single spatial configuration.
/// Holds position, rotation and scale. Used as both canonical (true)
/// state and the apparent (superposed) state across all quantum scripts.
/// Serializable so it shows in inspector.
/// </summary>

[System.Serializable]
public struct SuperpositionState
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
}