using UnityEngine;

public class WaveParticleObject : MonoBehaviour
{
    [Header("Entities")]
    [SerializeField] private Transform _playerTransform; // Quarky
    [SerializeField] private Transform _waveTransform;   // Shodri

    [Header("Controllers")]
    [SerializeField] private OdysseyPlayerController _playerController;
    [SerializeField] private ShodriPlayerController _waveController;

    [Header("Camera")]
    [SerializeField] private OdysseyFirstPersonCamera _firstPersonCamera;
    [SerializeField] private OdysseyThirdPersonCamera _thirdPersonCamera;

    [Header("Separation")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.Q;
    [SerializeField] private float _returnSpeed = 8f;
    [SerializeField] private float _maxDistance = 10f;

    private bool _isSeparated;
    private Vector3 _lastValidPosition;

    private void Start()
    {
        _lastValidPosition = _playerTransform.position;
        SetWaveActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            if (!_isSeparated) Separate();
            else TryReturn();
        }

        if (_isSeparated)
        {
            KeepPlayerStill();
            if (Vector3.Distance(_waveTransform.position, _playerTransform.position) > _maxDistance) ReturnWave();
        }
    }

    private void Separate()
    {
        _isSeparated = true;
        _lastValidPosition = _playerTransform.position;
        SetWaveActive(true);
        SetCameraTarget(_waveTransform);
    }

    private void TryReturn()
    {
        if (IsPositionClear(_playerTransform.position)) ReturnWave();
        else Debug.Log("No hay espacio para reunirse aquí.");
    }

    private void ReturnWave()
    {
        _waveTransform.position = Vector3.MoveTowards(_waveTransform.position,_playerTransform.position,_returnSpeed * Time.deltaTime);
        if (Vector3.Distance(_waveTransform.position, _playerTransform.position) < 0.2f)
        {
            _waveTransform.position = _playerTransform.position;
            _isSeparated = false;
            SetWaveActive(false);
            SetCameraTarget(_playerTransform);
        }
    }

    private bool IsPositionClear(Vector3 position)
    {
        CharacterController playerCollider = _playerTransform.GetComponent<CharacterController>();
        if (playerCollider == null) return true;
        return !Physics.CheckSphere(position + Vector3.up * (playerCollider.height * 0.5f),playerCollider.radius * 0.9f,~0,QueryTriggerInteraction.Ignore);
    }

    private void KeepPlayerStill()
    {
        // El controlador de Quarky se desactiva al separar.
    }

    private void SetWaveActive(bool active)
    {
        _playerController.enabled = !active;
        _waveController.enabled = active;
    }

    private void SetCameraTarget(Transform target)
    {
        if (_firstPersonCamera != null) _firstPersonCamera.SetTarget(target);
        if (_thirdPersonCamera != null) _thirdPersonCamera.SetTarget(target);
    }
}
