using UnityEngine;
using System.Collections.Generic;

public class StopNode : MonoBehaviour
{
    [Header("Identitas Petak")]
    [Tooltip("ID urutan jalan untuk pergerakan player")]
    public int nodeID; 
    
    [Tooltip("Isi dengan ID Objek dari MapDatabase. Biarkan 0 jika ini petak biasa.")]
    public int idObjek; 

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