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

        // 🔥 FILTER ANTI 9 PLAYER + URUTAN SINKRON
        allPlayers = FindObjectsOfType<PlayerData>()
            .Where(p => p.photonView != null && p.photonView.Owner != null)
            .OrderBy(p => p.photonView.Owner.ActorNumber)
            .ToList();

        Debug.Log("[GM] Jumlah player ditemukan: " + allPlayers.Count);

        if (allPlayers.Count == 0)
        {
            Debug.LogError("[GM] ❌ Tidak ada PlayerData di scene!");
            return;
        }

        currentTurnIndex = 0;
        turnSystemReady = true;

        Debug.Log("[GM] ✅ TURN DIACAK (SINKRON)!");
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
    // 🔥 PINDAH GILIRAN (FITUR MULTIPLAYER)
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

        // Panggil fungsi pindah giliran di semua client yang terkoneksi
        photonView.RPC("RPC_NextTurn", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void RPC_NextTurn()
    {
        // Pastikan list belum kosong dan player tidak null
        if (allPlayers.Count > 0 && allPlayers[currentTurnIndex] != null)
        {
            PlayerData pemainSelesai = allPlayers[currentTurnIndex];
            Debug.Log("[GM] Player selesai: " + pemainSelesai.playerName);

            // pindah ke belakang
            allPlayers.RemoveAt(currentTurnIndex);
            allPlayers.Add(pemainSelesai);
        }

        currentTurnIndex = 0;

        Debug.Log("[GM] ✅ TURN BERPINDAH (SYNCED)");
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
            // Amankan debug kalau ada objek null pas disconnect
            if (allPlayers[i] != null)
            {
                string status = (i == 0) ? " <-- GILIRAN" : "";
                Debug.Log(i + " : " + allPlayers[i].playerName + status);
            }
        }
    }

    // ==========================================
    // 🔥 SISTEM ANTI-NYANGKUT & HOST MIGRATION
    // ==========================================

    // 1. TERPANGGIL KALAU KITA SENDIRI YANG PUTUS KONEKSI
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"[GM] Koneksi terputus! Penyebab: {cause}");
        // Memaksa balik ke menu utama biar game tidak freeze (Pastikan nama Scene menu sesuai)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // 2. TERPANGGIL KALAU ADA PLAYER LAIN YANG KELUAR/PUTUS
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning($"[GM] Player {otherPlayer.NickName} keluar dari room!");

        if (!turnSystemReady) return;

        // Kita jalankan Coroutine agar menunggu 1 frame.
        // Alasan: Kita memberi waktu pada Photon untuk menghancurkan (Destroy) objek milik player yang keluar tadi.
        StartCoroutine(CleanUpMissingPlayers());
    }

    IEnumerator CleanUpMissingPlayers()
    {
        yield return new WaitForEndOfFrame();

        // Cek apakah player yang sedang giliran (index 0) tiba-tiba jadi null/hancur
        bool isCurrentTurnMissing = (allPlayers.Count > 0 && (allPlayers[0] == null || allPlayers[0].photonView == null));

        // Hapus semua data yang null/hancur dari list turn
        int removedCount = allPlayers.RemoveAll(p => p == null || p.photonView == null);

        if (removedCount > 0)
        {
            Debug.Log($"[GM] {removedCount} player dihapus dari daftar giliran.");

            if (allPlayers.Count > 0)
            {
                if (isCurrentTurnMissing)
                {
                    Debug.Log("[GM] Giliran otomatis berlanjut ke player berikutnya karena player sebelumnya keluar.");
                }

                // Refresh UI agar giliran langsung update ke orang selanjutnya
                currentTurnIndex = 0;
                UpdateUI();
                DebugTurn();
            }
            else
            {
                Debug.LogWarning("[GM] Semua player telah keluar dari room.");
            }
        }
    }

    // 3. TERPANGGIL KALAU HOST LAMA KELUAR & TAHTA PINDAH KE HOST BARU
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.LogWarning($"[GM] Host lama keluar! Tahta Host sekarang dipegang oleh: {newMasterClient.NickName}");

        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            Debug.Log("[GM] KITA ADALAH HOST BARU! Mengambil alih kendali Room...");
        }
    }
}