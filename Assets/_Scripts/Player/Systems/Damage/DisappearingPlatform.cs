using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _warningTime = 1.5f;
    [SerializeField] private float _respawnTime = 1f;

    [Header("Shake")]
    [SerializeField] private float _shakeAmount = 0.05f;
    [SerializeField] private float _shakeSpeed = 25f;

    private Vector3 _originalPosition;

    private Collider _collider;
    private Renderer _renderer;

    private bool _activated;

    private void Awake()
    {
        _originalPosition = transform.localPosition;

        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
    }

    private IEnumerator PlatformRoutine()
    {
        _activated = true;

        float elapsedTime = 0f;

        // La plataforma tiembla antes de desaparecer.
        while (elapsedTime < _warningTime)
        {
            elapsedTime += Time.deltaTime;
            float shakeX = Mathf.Sin(elapsedTime * _shakeSpeed) * _shakeAmount;
            float shakeZ = Mathf.Cos(elapsedTime * _shakeSpeed) * _shakeAmount;
            transform.localPosition = _originalPosition + new Vector3(shakeX, 0f, shakeZ);
            yield return null;
        }

        transform.localPosition = _originalPosition;

        // Desaparece.
        _collider.enabled = false;

        if (_renderer != null) _renderer.enabled = false;

        // Tiempo hasta reaparecer.
        yield return new WaitForSeconds(_respawnTime);

        // Reaparece.
        if (_renderer != null) _renderer.enabled = true;
        _collider.enabled = true;
        transform.localPosition = _originalPosition;
        _activated = false;
    }
    public void PlayerSteppedOn()
    {
        if (_activated) return;
        StartCoroutine(PlatformRoutine());
    }
}