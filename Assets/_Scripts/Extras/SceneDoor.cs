using UnityEngine;

public class SceneDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private string _targetScene;

    public void OnInteract()
    {
        SceneNavigationSystem.Instance.LoadScene(_targetScene);
    }
    public void OnFocus() { }
    public void OnLoseFocus() { }
}