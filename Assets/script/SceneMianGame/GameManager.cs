using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Character Prefabs")]
    public GameObject[] characterPrefabs;

    [Header("Spawn Area")]
    public Transform spawnCenter;
    public float spawnRadius = 10f;
    public float minDistance = 2.5f;

    [Header("Turn System")]
    public TurnUI turnUI;
    private List<PlayerData> allPlayers = new List<PlayerData>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(WaitAndSpawn());
    }

    // =========================
    // 🔥 SPAWN SYSTEM
    // =========================
    IEnumerator WaitAndSpawn()
    {
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("KarakterPilihan"))
        {
            Debug.Log("Menunggu data karakter...");
            yield return null;
        }

        SpawnPlayer();

        // kasih delay biar semua player ke-spawn dulu
        yield return new WaitForSeconds(1f);

        UpdateTurn();
    }

    void SpawnPlayer()
    {
        int karakterID = (int)PhotonNetwork.LocalPlayer.CustomProperties["KarakterPilihan"];

        Vector3 spawnPos = GetSafeSpawnPosition();

        GameObject player = PhotonNetwork.Instantiate(
            characterPrefabs[karakterID].name,
            spawnPos,
            Quaternion.identity
        );

        Debug.Log("Spawn karakter ID: " + karakterID);
    }

    Vector3 GetSafeSpawnPosition()
    {
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;

        Vector2 rand = Random.insideUnitCircle * spawnRadius;
        float offset = actor * minDistance;

        Vector3 pos = new Vector3(
            spawnCenter.position.x + rand.x + offset,
            spawnCenter.position.y,
            spawnCenter.position.z + rand.y + offset
        );

        return pos;
    }

    // =========================
    // 🔥 TURN SYSTEM
    // =========================
    public void UpdateTurn()
    {
        // ambil semua player di scene
        allPlayers = FindObjectsOfType<PlayerData>()
            .OrderBy(p => p.currentNodeIndex)
            .ToList();

        // update UI
        if (turnUI != null)
        {
            turnUI.UpdateUI(allPlayers);
        }

        DebugTurn();
    }

    public PlayerData GetCurrentPlayer()
    {
        if (allPlayers.Count == 0) return null;

        // paling belakang = index 0
        return allPlayers[0];
    }

    void DebugTurn()
    {
        Debug.Log("=== TURN ORDER ===");

        for (int i = 0; i < allPlayers.Count; i++)
        {
            Debug.Log(i + " : " + allPlayers[i].name +
                " (Node: " + allPlayers[i].currentNodeIndex + ")");
        }
    }
}
