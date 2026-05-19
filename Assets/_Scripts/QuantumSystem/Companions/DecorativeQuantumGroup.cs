using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorativeQuantumGroup : MonoBehaviour
{
    [Tooltip("All Decorative quantum objects that swap positions with each other.")] [SerializeField]
    private List<SuperpositionController> _members = new();

    [Tooltip("Seconds between each position swap.")] [SerializeField]
    private float _interval = 2f;

    private Vector3[] _homePositions;
    private Vector3[] _targetPositions;
    private WaitForSeconds _wait;

    private void Start()
    {
        _wait = new WaitForSeconds(_interval);
        
        if (_members.Count < 2)
        {
            Debug.LogWarning($"{name}: DecorativeQuantumGroup needs at least 2 members to swap.", this);
            return;
        }

        _homePositions = new Vector3[_members.Count];
        _targetPositions = new Vector3[_members.Count];

        for (int i = 0; i < _members.Count; i++)
        {
            _homePositions[i] = _members[i].transform.position;
        }

        StartCoroutine(SwapLoop());
    }

    private IEnumerator SwapLoop()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return _wait;

            bool readyToSwap = true;
            foreach (var member in _members)
            {
                if (!member.IsVisuallyQuantum)
                {
                    readyToSwap = false;
                    break;
                }
            }

            if (!readyToSwap)
                continue;

            Swap();
        }
    }

    private void Swap()
    {
        Array.Copy(_homePositions, _targetPositions, _homePositions.Length);

        // 1. Standard Fisher-Yates Shuffle
        for (int i = _targetPositions.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (_targetPositions[i], _targetPositions[j]) = (_targetPositions[j], _targetPositions[i]);
        }

        // 2. Deterministic Derangement Pass
        // Guarantees no object stays in its current spot with zero infinite-loop risk.
        for (int i = 0; i < _members.Count; i++)
        {
            // If the newly assigned position is essentially its current position...
            if ((_members[i].transform.position - _targetPositions[i]).sqrMagnitude < 0.01f)
            {
                // ...force a swap with the next position in the array.
                int next = (i + 1) % _members.Count;
                (_targetPositions[i], _targetPositions[next]) = (_targetPositions[next], _targetPositions[i]);
            }
        }

        // 3. Apply the guaranteed-new positions
        for (int i = 0; i < _members.Count; i++)
        {
            _members[i].transform.position = _targetPositions[i];
        }
    }
}