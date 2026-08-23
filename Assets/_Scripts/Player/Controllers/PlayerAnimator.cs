using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private OdysseyPlayerController _playerController;
    [SerializeField] private Animator _anim;
    
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _jumpHash = Animator.StringToHash("Jump");
    private readonly int _landHash = Animator.StringToHash("Land");

    private void Awake()
    {
        _playerController ??= GetComponent<OdysseyPlayerController>();
        _anim ??= GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_playerController == null) 
            return;
        
        _playerController.OnJump += HandleJump;
        _playerController.OnLand += HandleLand;
    }

    private void OnDisable()
    {
        if (_playerController == null) 
            return;
        
        _playerController.OnJump -= HandleJump;
        _playerController.OnLand -= HandleLand;
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
}