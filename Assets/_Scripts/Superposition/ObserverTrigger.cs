using UnityEngine;

public class ObserverTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SuperpositionController _targetObject;

    [Header("Condition")]
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private float _tolerance = 0.2f;

    [Header("Result")]
    [SerializeField] private GameObject _objectToActivate; //Puerta, Luz, etc.
    private bool _activated = false;
    
    void Update() 
    {
        if (_activated) return;
        if (_targetObject == null) return;
        //Solo si esta colapsado
        if (_targetObject.IsVisuallyQuantum) return;
        float dist = Vector3.Distance(_targetObject.transform.position,_targetPosition.position);
        if (dist <= _tolerance) Activate();
    }
    void Activate() 
    {
        if (_objectToActivate != null) _objectToActivate.SetActive(false); //ejemplo: abrir puerta
        Debug.Log("Observer puzzle ACTIVATED");
    }
}

