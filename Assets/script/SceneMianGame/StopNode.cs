using UnityEngine;
using System.Collections.Generic;

public class StopNode : MonoBehaviour
{
    public int nodeID;
    public int capacity = 1;

    public List<GameObject> playersInside = new List<GameObject>();

    public bool IsFull()
    {
        return playersInside.Count >= capacity;
    }

    public void AddPlayer(GameObject player)
    {
        if (!playersInside.Contains(player))
            playersInside.Add(player);
    }

    public void RemovePlayer(GameObject player)
    {
        if (playersInside.Contains(player))
            playersInside.Remove(player);
    }
}
