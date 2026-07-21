using UnityEngine;

[RequireComponent(typeof(EntanglementNode))]
public class EntanglementDoor : MonoBehaviour
{
    [SerializeField] private Vector3 _openOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float _speed = 5f;

    private EntanglementNode _node;
    private Vector3 _closedPosition;
    private Vector3 _targetPosition;

    private void Awake()
    {
        _node = GetComponent<EntanglementNode>();
        _closedPosition = transform.position;
        _targetPosition = _closedPosition;
    }

    private void OnEnable() => _node.OnQuantumSync += HandleSync;
    private void OnDisable() => _node.OnQuantumSync -= HandleSync;

    private void HandleSync(EntanglementEvent syncEvent)
    {
        if (syncEvent == EntanglementEvent.Activate || syncEvent == EntanglementEvent.Toggle && _node.CurrentState == EntanglementState.Activated)
        {
            _targetPosition = _closedPosition + _openOffset;
        }
        
        else if (syncEvent == EntanglementEvent.Deactivate || syncEvent == EntanglementEvent.Toggle && _node.CurrentState == EntanglementState.Idle)
        {
            _targetPosition = _closedPosition;
        }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _speed);
    }
}