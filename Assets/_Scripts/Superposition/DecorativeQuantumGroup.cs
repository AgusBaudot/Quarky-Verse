using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorativeQuantumGroup : MonoBehaviour {
    [Tooltip("All Decorative quantum objects that swap positions with each other.")]
    [SerializeField] private List<SuperpositionController> _members = new();

    [Tooltip("Seconds between each position swap.")]
    [SerializeField] private float _interval = 2f;

    private Vector3[] _homePositions;
    private int _activeCount;

    private void Start()
    {
        if (_members.Count < 2)
        {
            Debug.LogWarning($"{name}: DecorativeQuantumGroup needs at least 2 members to swap.", this);
            return;
        }

        _homePositions = new Vector3[_members.Count];
        _activeCount   = _members.Count;

        for (int i = 0; i < _members.Count; i++)
        {
            _homePositions[i] = _members[i].transform.position;
            _members[i].OnQuantumDeactivated += HandleDeactivated;
            _members[i].OnQuantumActivated   += HandleActivated;
        }

        StartCoroutine(SwapLoop());
    }

    private void OnDestroy()
    {
        foreach (var member in _members)
        {
            if (member == null)
                continue;
                
            member.OnQuantumDeactivated -= HandleDeactivated;
            member.OnQuantumActivated   -= HandleActivated;
        }
    }

    private IEnumerator SwapLoop() {
        while (gameObject.activeInHierarchy) {
            yield return new WaitForSeconds(_interval);

            bool readyToSwap = true;
            foreach (var member in _members) {
                // Check your existing property! 
                // If any member is collapsed OR deactivated by the Planck bar, abort.
                if (!member.IsVisuallyQuantum) {
                    readyToSwap = false;
                    break; 
                }
            }

            if (!readyToSwap) continue;

            Swap();
        }
    }

    private void Swap() {
        var positions = new Vector3[_homePositions.Length];
        Array.Copy(_homePositions, positions, _homePositions.Length);

        // 1. Standard Fisher-Yates Shuffle
        for (int i = positions.Length - 1; i > 0; i--) {
            int j = UnityEngine.Random.Range(0, i + 1);
            (positions[i], positions[j]) = (positions[j], positions[i]);
        }

        // 2. Deterministic Derangement Pass
        // Guarantees no object stays in its current spot with zero infinite-loop risk.
        for (int i = 0; i < _members.Count; i++) {
            // If the newly assigned position is essentially its current position...
            if ((_members[i].transform.position - positions[i]).sqrMagnitude < 0.01f) {
                // ...force a swap with the next position in the array.
                int next = (i + 1) % _members.Count;
                (positions[i], positions[next]) = (positions[next], positions[i]);
            }
        }

        // 3. Apply the guaranteed-new positions
        for (int i = 0; i < _members.Count; i++) {
            _members[i].transform.position = positions[i];
        }
    }

    // _activeCount tracks how many members are currently quantum-active.
    // Subscribed once per member so each deactivation/activation adjusts by 1.
    private void HandleDeactivated() => _activeCount--;
    private void HandleActivated()   => _activeCount++;
}