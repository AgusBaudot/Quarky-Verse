using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationSystem : MonoBehaviour
{
    public static SceneNavigationSystem Instance;
    private bool _isLoading;
    [SerializeField] private ScreenFader _fader;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;
        yield return _fader.FadeOut();
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }
        yield return _fader.FadeIn();
        _isLoading = false;
    }
}