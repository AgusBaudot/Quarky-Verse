using UnityEngine;

public class FallSpawn : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CheckpointManager _checkpointManager;
    [SerializeField, Tooltip("Opcional: solo se usa si no hay CheckpointManager en la escena")]
    private Transform _fallbackSpawnPoint;

    [Header("Configuración")]
    [SerializeField, Tooltip("Tag que tiene el objeto del Jugador")]
    private string _playerTag = "Player";
    [SerializeField, Tooltip("Si está activo, al caer también restaura la vida al máximo")]
    private bool _resetHealthOnFall = true;

    private void Awake()
    {
        if (_checkpointManager == null)
            _checkpointManager = FindObjectOfType<CheckpointManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;

        if (_checkpointManager != null)
        {
            _checkpointManager.RespawnPlayer(_resetHealthOnFall);
        }
        else
        {
            TeleportToFallback(other.gameObject);
        }
    }

    private void TeleportToFallback(GameObject player)
    {
        if (_fallbackSpawnPoint == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(_fallbackSpawnPoint.position, _fallbackSpawnPoint.rotation);

        if (cc != null) cc.enabled = true;
    }
}