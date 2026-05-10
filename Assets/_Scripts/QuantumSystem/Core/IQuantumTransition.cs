using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Contract every quantum transition behaviour must implement.
/// Each SuperpositionController needs one Mono that implements this.
/// </summary>

public interface IQuantumTransition
{
    /// <summary>
    /// Decides where the object appears next.
    /// </summary>
    /// <param name="current">Current state of object</param>
    /// <returns>Returns the next apparent state.</returns>
    SuperpositionState PickNextState(SuperpositionState current);
    
    /// <summary>
    /// Decided how it gets there (runs as coroutine). Performs the visual transition from one state to another.
    /// Must yield at least once - use yield break if the effect is instant.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="isCancelled"></param>
    /// <returns></returns>
    IEnumerator Execute(Transform target, SuperpositionState from, SuperpositionState to, Func<bool> isCancelled);
}