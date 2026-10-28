using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform _player;
    [SerializeField] private PlayerHealth _playerHealth;

    private CharacterController _characterController;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private Vector3 _checkpointPosition;
    private Quaternion _checkpointRotation;
    private bool _hasCheckpoint;

    [SerializeField] private CharacterSwapManager _characterSwapManager;
    private void Awake()
    {
        if (_characterSwapManager == null) _characterSwapManager = FindObjectOfType<CharacterSwapManager>();
        if (_playerHealth == null) _playerHealth = FindObjectOfType<PlayerHealth>();
        if (_player == null && _playerHealth != null) _player = _playerHealth.transform;

        if (_player == null)
        {
            Debug.LogError("CheckpointManager: asigná el jugador en el Inspector.", this);
            return;
        }
        _characterController = _player.GetComponent<CharacterController>();
        _startPosition = _player.position;
        _startRotation = _player.rotation;
        _checkpointPosition = _startPosition;
        _checkpointRotation = _startRotation;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null) return;
        _checkpointPosition = checkpoint.position;
        _checkpointRotation = checkpoint.rotation;
        _hasCheckpoint = true;
        Debug.Log("Checkpoint activado: " + checkpoint.name);
        Debug.Log($"Checkpoint guardado en {_checkpointPosition}", this);
    }

    public void RespawnPlayer()
    {
        if (_characterSwapManager != null) _characterSwapManager.ReturnToMainCharacter();
        if (_player == null) return;
        Vector3 spawnPosition = _hasCheckpoint ? _checkpointPosition : _startPosition;
        Quaternion spawnRotation = _hasCheckpoint ? _checkpointRotation : _startRotation;
        bool controllerWasEnabled = _characterController != null && _characterController.enabled;
        if (controllerWasEnabled) _characterController.enabled = false;
        _player.SetPositionAndRotation(spawnPosition, spawnRotation);
        if (controllerWasEnabled) _characterController.enabled = true;
        if (_playerHealth != null) _playerHealth.ResetHealth();
        Debug.Log($"Respawn: checkpoint activo={_hasCheckpoint}, jugador={_player.name}, destino={spawnPosition}",this);
    }
}