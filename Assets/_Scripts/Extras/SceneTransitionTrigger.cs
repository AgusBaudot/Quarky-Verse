using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [SerializeField] private string _targetScene;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SceneNavigationSystem.Instance.LoadScene(_targetScene);
    }
}