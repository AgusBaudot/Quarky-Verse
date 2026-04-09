using UnityEngine;

//Put some brief comment describing objective of class
public class ObserverTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SuperpositionController _targetObject;

    [Header("Condition")]
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private float _tolerance = 0.2f;

    [Header("Result")]
    [SerializeField] private GameObject _objectToActivate;

    private bool _activated = false;
    
    private void Update() 
    {
        if (_activated || _targetObject == null)
            return;

        //Solo si esta colapsado
        if (_targetObject.IsVisuallyQuantum)
            return;

        float dist = Vector3.Distance(_targetObject.transform.position,_targetPosition.position);
        if (dist <= _tolerance)
            Activate();
    }

    private void Activate() 
    {
        if (_objectToActivate != null)
            _objectToActivate.SetActive(false); //ejemplo: abrir puerta
        //What if the puzzle requires to make an object appear instead?
        Debug.Log("Observer puzzle ACTIVATED");
    }
}

