using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private OdysseyPlayerController _playerController;
    [SerializeField] private Animator _anim;
    
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");

    private void Start()
    {
        _playerController ??= GetComponent<OdysseyPlayerController>();
        _anim ??= GetComponent<Animator>();
    }

    private void Update()
    {
        _anim.SetFloat(_speedHash, _playerController.CurrentHorizontalVelocity.magnitude);
        _anim.SetBool(_isGroundedHash, _playerController.IsGrounded);
    }
}
