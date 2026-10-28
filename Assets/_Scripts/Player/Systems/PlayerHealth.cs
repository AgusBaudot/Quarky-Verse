
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;

    private int _currentHealth;
    private bool _isInvulnerable;
    private bool _isDead;

    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 1.5f;
    [SerializeField] private float _blinkInterval = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float _knockbackForce = 4f;
    [SerializeField] private float _knockbackDuration = 0.2f;

    [Header("Visual")]
    [SerializeField] private GameObject _visualModel;

    private Coroutine _invulnerabilityCoroutine;
    private Coroutine _knockbackCoroutine;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsInvulnerable => _isInvulnerable;
    public bool IsDead => _isDead;
    [Header("Respawn")]
    [SerializeField] private CheckpointManager _checkpointManager;
    [SerializeField] private float _respawnDelay = 1f;

    private OdysseyPlayerController _playerController;
    private Coroutine _respawnCoroutine;

    private void Awake()
    {
        _playerController = GetComponent<OdysseyPlayerController>();
        if (_checkpointManager == null) _checkpointManager = FindObjectOfType<CheckpointManager>();
        Instance = this;
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position - transform.forward);
    }

    public void TakeDamage(int damage, Vector3 sourcePosition)
    {
        if (_isDead || _isInvulnerable) return;
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        Debug.Log("Player recibió daño. HP: " + _currentHealth);
        if (_currentHealth <= 0)
        {
            Die();
            return;
        }
        ApplyKnockback(sourcePosition);
        _invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
    }

    private void ApplyKnockback(Vector3 sourcePosition)
    {
        CharacterController[] controllers = FindObjectsOfType<CharacterController>();
        CharacterController activeController = null;
        foreach (CharacterController controller in controllers)
        {
            if (controller.enabled && controller.gameObject.activeInHierarchy)
            {
                activeController = controller;
                break;
            }
        }

        if (activeController == null) return;

        Vector3 direction = activeController.transform.position - sourcePosition;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = -activeController.transform.forward;
        direction.Normalize();
        if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = StartCoroutine(KnockbackRoutine(activeController, direction));
    }

    private IEnumerator KnockbackRoutine(CharacterController controller,Vector3 direction)
    {
        float elapsedTime = 0f;

        while (elapsedTime < _knockbackDuration)
        {
            if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy) break;
            float step = _knockbackForce * Time.deltaTime;
            controller.Move(direction * step);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _knockbackCoroutine = null;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        _isInvulnerable = true;
        float elapsedTime = 0f;
        while (elapsedTime < _invulnerabilityDuration)
        {
            if (_visualModel != null) _visualModel.SetActive(false);
            yield return new WaitForSeconds(_blinkInterval);
            if (_visualModel != null) _visualModel.SetActive(true);
            yield return new WaitForSeconds(_blinkInterval);
            elapsedTime += _blinkInterval * 2f;
        }
        if (_visualModel != null) _visualModel.SetActive(true);
        _isInvulnerable = false;
        _invulnerabilityCoroutine = null;
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;
        _isInvulnerable = false;
        if (_invulnerabilityCoroutine != null)
        {
            StopCoroutine(_invulnerabilityCoroutine);
            _invulnerabilityCoroutine = null;
        }
        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }
        if (_playerController != null) _playerController.enabled = false;
        if (_visualModel != null) _visualModel.SetActive(false);
        Debug.Log("Player ha muerto.");
        _respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(_respawnDelay);
        if (_checkpointManager != null) _checkpointManager.RespawnPlayer();
        else Debug.LogError("PlayerHealth: no se encontró CheckpointManager.");
        _respawnCoroutine = null;
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
    }

    public void ResetHealth()
    {
        _currentHealth = _maxHealth;
        _isDead = false;
        _isInvulnerable = false;
        if (_invulnerabilityCoroutine != null)
        {
            StopCoroutine(_invulnerabilityCoroutine);
            _invulnerabilityCoroutine = null;
        }
        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }
        if (_visualModel != null) _visualModel.SetActive(true);
        if (_playerController != null)
        {
            _playerController.ResetMovementState();
            _playerController.enabled = true;
        }
    }
}
