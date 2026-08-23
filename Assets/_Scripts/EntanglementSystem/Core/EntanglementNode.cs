using System;
using UnityEngine;

public class EntanglementNode : MonoBehaviour
{
    [Tooltip("Objects sharing this ID will synchronize globally.")]
    public int GroupID = 0;
    
    public EntanglementState CurrentState { get; private set; } = EntanglementState.Idle;
    public EntanglementState PreviousState { get; private set; } = EntanglementState.Idle;

    public event Action<EntanglementEvent> OnQuantumSync;
    public event Action<Vector3> OnMirrorSync;

    private void OnEnable() => EntanglementNetwork.Register(this);
    private void OnDisable() => EntanglementNetwork.Unregister(this);

    public void ReceiveQuantumSync(EntanglementEvent syncEvent)
    {
        PreviousState = CurrentState;

        switch (syncEvent)
        {
            case EntanglementEvent.Activate:
                CurrentState = EntanglementState.Activated;
                break;
           
            case EntanglementEvent.Deactivate:
                CurrentState = EntanglementState.Idle;
                break;
            
            case EntanglementEvent.Toggle:
                CurrentState = CurrentState == EntanglementState.Idle ? EntanglementState.Activated : EntanglementState.Idle;
                break;
        }

        OnQuantumSync?.Invoke(syncEvent);
    }

    public void ReceiveMirrorMovement(Vector3 delta) => OnMirrorSync?.Invoke(delta);
}