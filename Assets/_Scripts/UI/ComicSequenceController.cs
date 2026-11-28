using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ComicSequenceController : MonoBehaviour
{
    [Serializable]
    public class ComicPage
    {
        [Tooltip("Parent GameObject with a CanvasGroup holding the background and frames for this page.")]
        public CanvasGroup pageRoot;

        [Tooltip("Ordered list of frame Images to fade in one by one.")]
        public List<Image> frames = new List<Image>();
    }

    [Header("Pages")]
    [SerializeField] private List<ComicPage> _pages = new List<ComicPage>();
    [SerializeField] private bool _playOnStart = true;
    [SerializeField] private bool _disableOnComplete = true;

    [Header("Timing (Unscaled Seconds)")]
    [SerializeField] private float _frameFadeInDuration = 0.6f;
    [SerializeField] private float _delayBetweenFrames = 1.2f;
    [SerializeField] private float _endOfPageHoldDuration = 2.0f;
    [SerializeField] private float _pageFadeOutDuration = 0.5f;

    [Header("Events")]
    public UnityEvent OnComicStarted;
    public UnityEvent OnComicCompleted;

    private Coroutine _sequenceCoroutine;
    private bool _skipRequested;
    private bool _isPlaying;

    private void Start()
    {
        if (_playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            _skipRequested = true;
        }
    }

    public void Play()
    {
        if (_isPlaying || _pages.Count == 0) return;

        _sequenceCoroutine = StartCoroutine(RunComicSequence());
    }

    private IEnumerator RunComicSequence()
    {
        _isPlaying = true;
        Time.timeScale = 0f;
        OnComicStarted?.Invoke();

        PrepareAllPagesHidden();

        for (int p = 0; p < _pages.Count; p++)
        {
            ComicPage page = _pages[p];
            if (page.pageRoot == null) continue;

            page.pageRoot.gameObject.SetActive(true);
            page.pageRoot.alpha = 1f;
            SetFramesAlpha(page.frames, 0f);

            for (int f = 0; f < page.frames.Count; f++)
            {
                Image frame = page.frames[f];
                if (frame == null) continue;

                _skipRequested = false;
                yield return FadeImageAlpha(frame, 0f, 1f, _frameFadeInDuration, allowSkip: true);

                if (f < page.frames.Count - 1)
                {
                    _skipRequested = false;
                    yield return WaitUnscaled(_delayBetweenFrames, allowSkip: true);
                }
            }

            _skipRequested = false;
            yield return WaitUnscaled(_endOfPageHoldDuration, allowSkip: false);

            yield return FadeCanvasGroup(page.pageRoot, 1f, 0f, _pageFadeOutDuration);
            page.pageRoot.gameObject.SetActive(false);
        }

        FinishSequence();
    }

    private IEnumerator FadeImageAlpha(Image target, float from, float to, float duration, bool allowSkip)
    {
        if (duration <= 0f)
        {
            SetImageAlpha(target, to);
            yield break;
        }

        float elapsed = 0f;
        SetImageAlpha(target, from);

        while (elapsed < duration)
        {
            if (allowSkip && _skipRequested)
            {
                _skipRequested = false;
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetImageAlpha(target, Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetImageAlpha(target, to);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            group.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator WaitUnscaled(float duration, bool allowSkip)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (allowSkip && _skipRequested)
            {
                _skipRequested = false;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void PrepareAllPagesHidden()
    {
        foreach (ComicPage page in _pages)
        {
            if (page.pageRoot == null) continue;
            page.pageRoot.alpha = 0f;
            SetFramesAlpha(page.frames, 0f);
            page.pageRoot.gameObject.SetActive(false);
        }
    }

    private void SetFramesAlpha(List<Image> frames, float alpha)
    {
        foreach (Image frame in frames)
        {
            if (frame != null) SetImageAlpha(frame, alpha);
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }

    private void FinishSequence()
    {
        _isPlaying = false;
        Time.timeScale = 1f;
        OnComicCompleted?.Invoke();

        if (_disableOnComplete)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_isPlaying)
        {
            Time.timeScale = 1f;
        }
    }
}