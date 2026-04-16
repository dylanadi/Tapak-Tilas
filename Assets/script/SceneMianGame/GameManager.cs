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
    private int currentTurnIndex = 0;

    private bool turnSystemReady = false;

    void Awake()
    {
        Instance = this;
        Debug.Log("[GM] Awake");
    }

    void Start()
    {
        Debug.Log("[GM] Start → mulai spawn system");
        StartCoroutine(WaitAndSpawn());
    }

    // =========================
    // 🔥 SPAWN SYSTEM
    // =========================
    IEnumerator WaitAndSpawn()
    {
        Debug.Log("[GM] Menunggu CustomProperties...");

        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("KarakterPilihan"))
        {
            yield return null;
        }

        Debug.Log("[GM] Data karakter ditemukan");

        SpawnPlayer();

        Debug.Log("[GM] Tunggu player lain spawn...");
        yield return new WaitForSeconds(2f);

        SetupTurnSystem();
    }

    void SpawnPlayer()
    {
        int karakterID = (int)PhotonNetwork.LocalPlayer.CustomProperties["KarakterPilihan"];

        Vector3 spawnPos = GetSafeSpawnPosition();

        Debug.Log("[GM] Spawn karakter ID: " + karakterID);

        PhotonNetwork.Instantiate(
            characterPrefabs[karakterID].name,
            spawnPos,
            Quaternion.identity
        );
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

        Debug.Log("[GM] Spawn position: " + pos);

        return pos;
    }

    // =========================
    // 🔥 SETUP TURN AWAL
    // =========================
    void SetupTurnSystem()
    {
        if (turnSystemReady)
        {
            Debug.LogWarning("[GM] Turn system sudah pernah setup!");
            return;
        }

        allPlayers = FindObjectsOfType<PlayerData>().ToList();

        Debug.Log("[GM] Jumlah player ditemukan: " + allPlayers.Count);

        if (allPlayers.Count == 0)
        {
            Debug.LogError("[GM] ❌ Tidak ada PlayerData di scene!");
            return;
        }

        // 🔥 RANDOM URUTAN
        allPlayers = allPlayers.OrderBy(x => Random.value).ToList();

        currentTurnIndex = 0;
        turnSystemReady = true;

        Debug.Log("[GM] ✅ TURN DIACAK!");
        DebugTurn();

        UpdateUI();
    }

    // =========================
    // 🔥 CEK GILIRAN
    // =========================
    public bool IsMyTurn(PlayerData player)
    {
        if (!turnSystemReady)
        {
            Debug.Log("[GM] Turn belum siap");
            return false;
        }

        if (allPlayers.Count == 0)
            return false;

        bool result = allPlayers[currentTurnIndex] == player;

        if (result)
            Debug.Log("[GM] ✅ INI GILIRANMU");
        else
            Debug.Log("[GM] ❌ BUKAN GILIRANMU");

        return result;
    }

    // =========================
    // 🔥 PINDAH GILIRAN
    // =========================
    public void NextTurn()
    {
        Debug.Log("[GM] 🔥 NEXT TURN DIPANGGIL | Frame: " + Time.frameCount);

        if (!turnSystemReady)
        {
            Debug.LogWarning("[GM] Turn system belum ready!");
            return;
        }

        if (allPlayers.Count == 0)
        {
            Debug.LogError("[GM] Player kosong!");
            return;
        }

        PlayerData pemainSelesai = allPlayers[currentTurnIndex];

        Debug.Log("[GM] Player selesai: " + pemainSelesai.playerName);

        // pindah ke belakang
        allPlayers.RemoveAt(currentTurnIndex);
        allPlayers.Add(pemainSelesai);

        currentTurnIndex = 0;

        Debug.Log("[GM] ✅ TURN BERPINDAH");
        DebugTurn();

        UpdateUI();
    }

    // =========================
    // 🔥 UI UPDATE
    // =========================
    void UpdateUI()
    {
        if (turnUI == null)
        {
            Debug.LogWarning("[GM] TurnUI belum di assign!");
            return;
        }

        Debug.Log("[GM] 🔄 Update UI");

        turnUI.UpdateUI(allPlayers);
    }

    // =========================
    // 🔥 GET PLAYER SEKARANG
    // =========================
    public PlayerData GetCurrentPlayer()
    {
        if (!turnSystemReady || allPlayers.Count == 0)
            return null;

        return allPlayers[currentTurnIndex];
    }

    // =========================
    // 🔥 DEBUG TURN
    // =========================
    void DebugTurn()
    {
        Debug.Log("=== TURN ORDER ===");

        for (int i = 0; i < allPlayers.Count; i++)
        {
            string status = (i == 0) ? " <-- GILIRAN" : "";
            Debug.Log(i + " : " + allPlayers[i].playerName + status);
        }
    }
}
