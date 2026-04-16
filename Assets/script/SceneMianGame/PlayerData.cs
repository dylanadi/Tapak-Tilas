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
    // 🔥 TUNGGU DATA PHOTON MASUK
    // =============================
    IEnumerator WaitForData()
    {
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("KarakterPilihan"))
        {
            Debug.Log("Menunggu data karakter...");
            yield return null;
        }

        SetupData();
    }

    // =============================
    // SET DATA PLAYER
    // =============================
    void SetupData()
    {
        if (!photonView.IsMine) return; // ✅ penting banget

        playerName = PhotonNetwork.NickName;
        actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("KarakterPilihan"))
        {
            characterID = (int)PhotonNetwork.LocalPlayer.CustomProperties["KarakterPilihan"];
            Debug.Log("Karakter berhasil di-set: " + characterID);
        }
        else
        {
            Debug.LogError("Karakter belum dipilih!");
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
