using UnityEngine;
using System.Collections.Generic;

public class StopNode : MonoBehaviour
{
    public int nodeID;
    public InteractableObject.ObjectType jenisNode;
    
    [Header("Kapasitas Petak")]
    [Tooltip("Berapa banyak player yang boleh berdiri di sini?")]
    public int capacity = 1;

    private List<GameObject> daftarPlayer = new List<GameObject>();

    public bool IsFull()
    {
        return daftarPlayer.Count >= capacity;
    }

    public void AddPlayer(GameObject player)
    {
        if (!daftarPlayer.Contains(player))
        {
            daftarPlayer.Add(player);
            Debug.Log($"[StopNode {nodeID}] {player.name} masuk. Isi: {daftarPlayer.Count}/{capacity}");
        }
    }

    public void RemovePlayer(GameObject player)
    {
        if (daftarPlayer.Contains(player))
        {
            daftarPlayer.Remove(player);
            Debug.Log($"[StopNode {nodeID}] {player.name} keluar. Isi: {daftarPlayer.Count}/{capacity}");
        }
    }
}