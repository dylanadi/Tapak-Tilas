using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerData : MonoBehaviourPun
{
    [Header("Info Player")]
    public string playerName;
    public int actorNumber;
    public int characterID;

    [Header("Game Data")]
    public int currentNodeIndex = -1;

    void Start()
    {
        StartCoroutine(WaitForData()); // ✅ panggil coroutine
    }

    // =============================
    // 🔥 TUNGGU DATA PHOTON MASUK DARI PEMILIK ASLI
    // =============================
    IEnumerator WaitForData()
    {
        // 1. Tunggu sampai karakter ini diakui punya pemilik di server
        while (photonView.Owner == null)
        {
            yield return null;
        }

        // 2. Tunggu sampai pemilik karakter ini punya data "KarakterPilihan"
        while (!photonView.Owner.CustomProperties.ContainsKey("KarakterPilihan"))
        {
            Debug.Log("Menunggu data karakter dari " + photonView.Owner.NickName + "...");
            yield return null;
        }

        SetupData();
    }

    // =============================
    // SET DATA PLAYER (SINKRON MULTIPLAYER)
    // =============================
    void SetupData()
    {
        // ❌ HAPUS: if (!photonView.IsMine) return;
        // Kita biarkan script jalan agar layar kita bisa nge-load data teman.

        // ✅ KUNCI MULTIPLAYER: Ambil data dari Pemilik (Owner), bukan LocalPlayer
        Player pemilik = photonView.Owner;

        playerName = pemilik.NickName;
        actorNumber = pemilik.ActorNumber;

        if (pemilik.CustomProperties.ContainsKey("KarakterPilihan"))
        {
            characterID = (int)pemilik.CustomProperties["KarakterPilihan"];
            Debug.Log($"[PlayerData] Karakter berhasil di-set! {playerName} memakai ID: {characterID}");
        }
        else
        {
            Debug.LogError($"[PlayerData] Karakter belum dipilih untuk {playerName}!");
        }
    }

    // =============================
    // HELPER
    // =============================
    public bool IsMine()
    {
        return photonView.IsMine;
    }
}