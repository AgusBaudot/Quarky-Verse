using UnityEngine;

/// <summary>
/// Synchronizes player mechanics state to the visual Animator.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private OdysseyPlayerController _playerController;
    [SerializeField] private PlayerMeleeAttack _playerAttack;
    [SerializeField] private Animator _anim;
    
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _jumpHash = Animator.StringToHash("Jump");
    private readonly int _landHash = Animator.StringToHash("Land");
    private readonly int _punchHash = Animator.StringToHash("Punch");

    private void Awake()
    {
        _playerController ??= GetComponent<OdysseyPlayerController>();
        _playerAttack ??= GetComponent<PlayerMeleeAttack>();
        _anim ??= GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_playerController != null)
        {
            _playerController.OnJump += HandleJump;
            _playerController.OnLand += HandleLand;
        }

        if (_playerAttack != null)
        {
            _playerAttack.OnAttack += HandleAttack;
        }
    }

    private void OnDisable()
    {
        if (_playerController != null)
        {
            _playerController.OnJump -= HandleJump;
            _playerController.OnLand -= HandleLand;
        }
        
        if (_playerAttack != null)
        {
            _playerAttack.OnAttack -= HandleAttack;
        }
    }

    private void Update()
    {
        _anim.SetFloat(_speedHash, _playerController.CurrentHorizontalVelocity.magnitude);
        _anim.SetBool(_isGroundedHash, _playerController.IsGrounded);
    }

    private void HandleJump()
    {
        _anim.ResetTrigger(_landHash);
        _anim.SetTrigger(_jumpHash);
    }

    private void HandleLand()
    {
        _anim.SetTrigger(_landHash);
    }

    private void HandleAttack()
    {
        _anim.SetTrigger(_punchHash);
    }
}