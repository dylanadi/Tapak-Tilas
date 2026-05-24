using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnUI : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject playerItemPrefab;
    public Transform container;
    public List<Sprite> karakterIcons;

    private List<GameObject> uiPool = new List<GameObject>();
    private string lastOrderFingerprint = "";

    public void UpdateUI(List<PlayerData> players)
    {
        // 🔍 DEBUG 1: Cek apakah fungsi ini terpanggil terus-menerus
        // Debug.Log("<color=cyan>TurnUI: UpdateUI dipanggil!</color>");

        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("TurnUI: List players kosong atau null!");
            return;
        }

        // 1. BUAT SIDIK JARI
        string currentOrder = "";
        foreach (var p in players)
        {
            if (p != null && p.photonView != null && p.photonView.Owner != null)
                currentOrder += p.photonView.Owner.ActorNumber + "|";
            else
                currentOrder += "loading|";
        }

        // 🔍 DEBUG 2: Bandingkan sidik jari
        if (currentOrder == lastOrderFingerprint)
        {
            // Jika kamu melihat log ini muncul ribuan kali, berarti UpdateUI dipanggil di Update()
            // tapi untungnya tertahan oleh sensor sidik jari ini.
            // Debug.Log("TurnUI: Urutan sama, skip update.");
            return;
        }

        // 🔍 DEBUG 3: Ini yang bahaya! Kalau log ini muncul terus, berarti sidik jarinya berubah-ubah
        Debug.Log($"<color=yellow>TurnUI: UPDATE EXECUTED!</color> Order Baru: {currentOrder} | Order Lama: {lastOrderFingerprint}");

        lastOrderFingerprint = currentOrder;

        // 3. LOGIKA OBJECT POOLING
        while (uiPool.Count < players.Count)
        {
            Debug.Log("<color=green>TurnUI: Menambah UI Item baru ke pool.</color>");
            GameObject newObj = Instantiate(playerItemPrefab, container);
            uiPool.Add(newObj);
        }

        // 4. SET DATA
        for (int i = 0; i < uiPool.Count; i++)
        {
            if (i < players.Count)
            {
                uiPool[i].SetActive(true);
                PlayerData p = players[i];

                Image img = uiPool[i].GetComponent<Image>();
                if (img != null && p != null)
                {
                    if (p.characterID >= 0 && p.characterID < karakterIcons.Count)
                        img.sprite = karakterIcons[p.characterID];

                    // Visual Highlight
                    if (i == 0)
                    {
                        uiPool[i].transform.localScale = Vector3.one * 1.2f;
                        img.color = Color.white;
                    }
                    else
                    {
                        uiPool[i].transform.localScale = Vector3.one;
                        img.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                    }
                }
            }
            else
            {
                uiPool[i].SetActive(false);
            }
        }
    }
}