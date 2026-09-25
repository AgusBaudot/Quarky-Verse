using System.Collections;
using UnityEngine;

public class MovingSpikes : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _extendDistance = 1f;
    [SerializeField] private float _moveDuration = 0.3f;

    [Header("Timing")]
    [SerializeField] private float _extendedTime = 0.7f;
    [SerializeField] private float _retractedTime = 0.7f;

    private Vector3 _retractedPosition;
    private Vector3 _extendedPosition;

    private void Start()
    {
        _retractedPosition = transform.localPosition;
        _extendedPosition = _retractedPosition + transform.up * _extendDistance;

        StartCoroutine(SpikeRoutine());
    }

    private IEnumerator SpikeRoutine()
    {
        while (true)
        {
            // Esperar mientras los pinchos están retraídos
            yield return new WaitForSeconds(_retractedTime);

            // Salir
            yield return MoveToPosition(_extendedPosition);

            // Permanecer fuera
            yield return new WaitForSeconds(_extendedTime);

            // Entrar
            yield return MoveToPosition(_retractedPosition);
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _moveDuration;
            transform.localPosition = Vector3.Lerp(startPosition,targetPosition,progress);
            yield return null;
        }

        transform.localPosition = targetPosition;
    }
}
