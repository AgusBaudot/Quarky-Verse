using UnityEngine;

public class HazardDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerHealth.Instance == null) return;
        PlayerHealth.Instance.TakeDamage(1);
    }
}