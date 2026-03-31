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
        // Ensure we start the game controlling the main character
        ActivateCharacter(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _isPlayingAsShodri = !_isPlayingAsShodri;
            ActivateCharacter(_isPlayingAsShodri);
        }
    }

    private void ActivateCharacter(bool playAsShodri)
    {
        if (playAsShodri)
        {
            // SWAPPING TO SHODRI
            
            // Disable Shodri's CharacterController briefly to allow direct transform manipulation
            CharacterController shodriCC = _shodriCharacter.GetComponent<CharacterController>();
            if (shodriCC) shodriCC.enabled = false;

            // Snap Shodri to the Main Player's exact position and rotation
            _shodriCharacter.transform.position = _mainCharacter.transform.position;
            _shodriCharacter.transform.rotation = _mainCharacter.transform.rotation;

            // Re-enable the CharacterController
            if (shodriCC) shodriCC.enabled = true;

            // Swap GameObjects
            _mainCharacter.enabled = false;
            _shodriCharacter.gameObject.SetActive(true);

            // Update cameras to follow Shodri
            UpdateCameras(_shodriCharacter.transform);
        }
        else
        {
            // SWAPPING TO MAIN PLAYER
            
            // Swap GameObjects (Main player naturally wakes up exactly where they were left)
            _shodriCharacter.gameObject.SetActive(false);
            _mainCharacter.enabled = true;

            // Update cameras to follow Main Player
            UpdateCameras(_mainCharacter.transform);
        }
    }

    private void UpdateCameras(Transform newTarget)
    {
        if (_thirdPersonCamera) _thirdPersonCamera.SetTarget(newTarget);
        if (_firstPersonCamera) _firstPersonCamera.SetTarget(newTarget);
    }
}