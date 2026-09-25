using UnityEngine;

public class PlatformSensor : MonoBehaviour
{
    private DisappearingPlatform _platform;

    private void Awake()
    {
        _platform = GetComponentInParent<DisappearingPlatform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_platform != null) _platform.PlayerSteppedOn();
    }
}
