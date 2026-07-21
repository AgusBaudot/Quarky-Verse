using UnityEngine;

[RequireComponent(typeof(EntanglementNode))]
public class EntanglementInitiator : MonoBehaviour, IInteractable
{
    [SerializeField] private EntanglementEvent _eventToBroadcast = EntanglementEvent.Toggle;
    [SerializeField] private bool _canBeActivated = true;
    [SerializeField] private float _cooldown = 1f;

    private EntanglementNode _node;
    private float _lastActivationTime = -100f;

    private void Awake()
    {
        _node = GetComponent<EntanglementNode>();
    }

    public void OnInteract() 
    {
        if (!_canBeActivated) return;
        if (Time.time < _lastActivationTime + _cooldown) return;

        _lastActivationTime = Time.time;
        
        EntanglementNetwork.BroadcastEvent(_node.GroupID, _eventToBroadcast);
    }

    public void OnFocus()
    { }

    public void OnLoseFocus()
    { }
}