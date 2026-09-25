using System.Collections;
using UnityEngine;

public class CrusherMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _fallDistance = 5f;
    [SerializeField] private float _riseDuration = 2f;
    [SerializeField] private float _fallDuration = 0.15f;

    [Header("Timing")]
    [SerializeField] private float _topWaitTime = 1f;
    [SerializeField] private float _bottomWaitTime = 0.5f;

    private Vector3 _topPosition;
    private Vector3 _bottomPosition;

    private void Start()
    {
        _topPosition = transform.localPosition;
        _bottomPosition = _topPosition - transform.up * _fallDistance;
        StartCoroutine(CrusherRoutine());
    }

    private IEnumerator CrusherRoutine()
    {
        while (true)
        {
            // Esperar arriba
            yield return new WaitForSeconds(_topWaitTime);
            // Caer rápidamente
            yield return MoveToPosition(_bottomPosition, _fallDuration);
            // Permanecer abajo
            yield return new WaitForSeconds(_bottomWaitTime);
            // Subir lentamente
            yield return MoveToPosition(_topPosition, _riseDuration);
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            transform.localPosition = Vector3.Lerp(startPosition,targetPosition,progress);
            yield return null;
        }
        transform.localPosition = targetPosition;
    }
}
