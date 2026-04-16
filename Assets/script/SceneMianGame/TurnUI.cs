using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

public class TurnUI : MonoBehaviour
{
    public GameObject playerItemPrefab;
    public Transform container;
    public List<Sprite> karakterIcons;

    public void UpdateUI(List<PlayerData> players)
    {
        // Hapus semua UI lama
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Buat ulang sesuai urutan
        foreach (var p in players)
        {
            GameObject item = Instantiate(playerItemPrefab, container);

            Image img = item.GetComponent<Image>();

            int id = (int)p.photonView.Owner.CustomProperties["KarakterPilihan"];
            img.sprite = karakterIcons[id];
        }
    }
}
