using System.Collections.Generic;

public static class EntanglementNetwork
{
    private static readonly Dictionary<int, List<EntanglementNode>> _groups = new();

    public static void Register(EntanglementNode node)
    {
        if (!_groups.ContainsKey(node.GroupID))
        {
            _groups[node.GroupID] = new List<EntanglementNode>();
        }

        if (!_groups[node.GroupID].Contains(node))
        {
            _groups[node.GroupID].Add(node);
        }
    }
    
    public static void Unregister(EntanglementNode node)
    {
        if (_groups.ContainsKey(node.GroupID))
        {
            _groups[node.GroupID].Remove(node);
            
            if (_groups[node.GroupID].Count == 0)
            {
                _groups.Remove(node.GroupID);
            }
        }
    }
    
    public static void BroadcastEvent(int groupID, EntanglementEvent syncEvent)
    {
        if (!_groups.ContainsKey(groupID)) return;

        // Iterate backwards to safely handle potential mid-loop destruction
        for (int i = _groups[groupID].Count - 1; i >= 0; i--)
        {
            _groups[groupID][i].ReceiveQuantumSync(syncEvent);
        }
    }
}