using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 3;

    private int _currentHealth;
    private bool _isInvulnerable;
    private bool _isDead;

    [Header("Invulnerability")]
    [SerializeField] private float _invulnerabilityDuration = 1.5f;
    [SerializeField] private float _blinkInterval = 0.1f;

    [Header("Visual")]
    [SerializeField] private GameObject _visualModel;

    private Coroutine _invulnerabilityCoroutine;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsInvulnerable => _isInvulnerable;
    public bool IsDead => _isDead;

    public static PlayerHealth Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        // No recibir daño si ya está muerto
        if (_isDead) return;

        // No recibir daño durante la invulnerabilidad
        if (_isInvulnerable) return;

        _currentHealth -= damage;
        // Evitar valores negativos
        _currentHealth = Mathf.Max(_currentHealth, 0);
        Debug.Log("Player recibió daño. HP: " + _currentHealth);

        // Comprobar muerte
        if (_currentHealth <= 0)
        {
            Die();
            return;
        }

        // Comenzar invulnerabilidad
        _invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
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
        _isDead = true;
        Debug.Log("Player ha muerto.");

        // Más adelante conectaremos aquí:
        // - Animación de muerte
        // - Sonido
        // - Respawn
        // - Checkpoint
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
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
        if (_visualModel != null) _visualModel.SetActive(true);
    }
}
