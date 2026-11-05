using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the player's basic grounded melee attack, managing input and active damage frames.
/// </summary>
[RequireComponent(typeof(OdysseyPlayerController))]
public class PlayerMeleeAttack : MonoBehaviour
{
    /// <summary>
    /// Event fired when a melee attack successfully initiates.
    /// </summary>
    public event Action OnAttack;

    [Header("Combat Settings")]
    [SerializeField, Tooltip("The damage dealer hitbox object to activate during the attack.")] 
    private GameObject _damageHitbox;
    
    [SerializeField, Tooltip("Delay in seconds before the hitbox becomes active.")] 
    private float _startupTime = 0.1f;
    
    [SerializeField, Tooltip("Duration in seconds the hitbox remains active.")] 
    private float _activeFramesDuration = 0.2f;
    
    [SerializeField, Tooltip("Cooldown in seconds before the player can attack again.")] 
    private float _attackCooldown = 0.5f;

    private OdysseyPlayerController _playerController;
    private bool _isAttacking;
    private float _lastAttackTime = -100f;

    private void Awake()
    {
        _playerController = GetComponent<OdysseyPlayerController>();
        if (_damageHitbox != null) 
        {
            _damageHitbox.SetActive(false);
        }
    }

    private void Update()
    {
        // 0 corresponds to Left Click.
        if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            StartCoroutine(AttackRoutine());
        }
    }

    /// <summary>
    /// Validates if the player is in a valid state to perform an attack.
    /// </summary>
    /// <returns>True if grounded and cooldown has elapsed.</returns>
    private bool CanAttack()
    {
        return !_isAttacking && _playerController.IsGrounded && Time.time >= _lastAttackTime + _attackCooldown;
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        OnAttack?.Invoke();

        yield return new WaitForSeconds(_startupTime);

        if (_damageHitbox != null) 
        {
            _damageHitbox.SetActive(true);
        }

        yield return new WaitForSeconds(_activeFramesDuration);

        if (_damageHitbox != null) 
        {
            _damageHitbox.SetActive(false);
        }

        _lastAttackTime = Time.time;
        _isAttacking = false;
    }
}