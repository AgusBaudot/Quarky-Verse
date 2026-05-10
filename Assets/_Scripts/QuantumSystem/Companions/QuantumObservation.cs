using System;
using UnityEngine;

/// QuantumObservation.cs
/// ─────────────────────────────────────────────────────
/// Serializable struct pairing a designer-defined offset with the
/// GameObject consequence that activates when the quantum object
/// is collapsed at that position. Used by ObserverTrigger and
/// read by InstantTransition as its source of offsets.
/// ─────────────────────────────────────────────────────

[Serializable]
public struct QuantumObservation
{
    public Vector3     offset;
    public GameObject  consequence;
}