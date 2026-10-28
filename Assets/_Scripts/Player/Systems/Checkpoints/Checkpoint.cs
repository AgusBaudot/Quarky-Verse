using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private CheckpointManager _checkpointManager;
    [SerializeField] private string _playerTag = "Player";

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (_checkpointManager == null) _checkpointManager = FindObjectOfType<CheckpointManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        if (_checkpointManager == null)
        {
            Debug.LogWarning("No se encontró CheckpointManager.", this);
            return;
        }
        _checkpointManager.SetCheckpoint(transform);
    }
}