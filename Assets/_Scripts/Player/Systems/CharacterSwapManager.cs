using UnityEngine;

public class CharacterSwapManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField, Tooltip("The main player's controller")] 
    private OdysseyPlayerController _mainCharacter;
    [SerializeField, Tooltip("Shodri's controller")] 
    private ShodriPlayerController _shodriCharacter;

    [Header("Cameras")]
    [SerializeField] private OdysseyThirdPersonCamera _thirdPersonCamera;
    [SerializeField] private OdysseyFirstPersonCamera _firstPersonCamera;

    private bool _isPlayingAsShodri = false;

    private void Start()
    {
        ActivateCharacter(false);
    }

    private void Update()
    {
        if (PlayerHealth.Instance != null && (PlayerHealth.Instance.IsDead || PlayerHealth.Instance.IsInvulnerable)) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _isPlayingAsShodri = !_isPlayingAsShodri;
            ActivateCharacter(_isPlayingAsShodri);
        }
    }
    public void ReturnToMainCharacter()
    {
        _isPlayingAsShodri = false;
        ActivateCharacter(false);
    }

    private void ActivateCharacter(bool playAsShodri)
    {
        if (playAsShodri)
        {
            _shodriCharacter.Teleport(_mainCharacter.transform.position, _mainCharacter.transform.rotation);
            _mainCharacter.enabled = false;
            _shodriCharacter.gameObject.SetActive(true);
            UpdateCameras(_shodriCharacter.transform);
        }
        else
        {
            _shodriCharacter.gameObject.SetActive(false);
            _mainCharacter.enabled = true;
            UpdateCameras(_mainCharacter.transform);
        }
    }
    private void UpdateCameras(Transform newTarget)
    {
        if (_thirdPersonCamera) _thirdPersonCamera.SetTarget(newTarget);
        if (_firstPersonCamera) _firstPersonCamera.SetTarget(newTarget);
    }
    public void SetControlsLocked(bool locked)
    {
        if (_mainCharacter != null) _mainCharacter.SetControlsLocked(locked);
        if (_shodriCharacter != null) _shodriCharacter.SetControlsLocked(locked);
    }
}