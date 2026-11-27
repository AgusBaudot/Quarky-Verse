using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeDuration = 1f;

    public IEnumerator FadeOut()
    {
        float timer = 0;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            Color color = _fadeImage.color;
            color.a = timer / _fadeDuration;
            _fadeImage.color = color;
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        float timer = 0;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            Color color = _fadeImage.color;
            color.a = 1f - (timer / _fadeDuration);
            _fadeImage.color = color;
            yield return null;
        }
    }
}