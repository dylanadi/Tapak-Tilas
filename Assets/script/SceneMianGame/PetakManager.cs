using UnityEngine;
using System.Linq;

public class PetakManager : MonoBehaviour
{
    public static PetakManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 🔥 SEKARANG MENERIMA NODE ID (Bukan ID Objek)
    public void EksekusiMekanikPetak(int idNodeInjakan)
    {
        if (MapDatabase.Instance == null)
        {
            Debug.LogError("[PetakManager] MapDatabase tidak ditemukan di Scene!");
            return;
        }

        // 🔎 LOGIKA PENCARIAN TERBALIK:
        // Mencari objek map yang array 'nodeParkirIDs'-nya mengandung idNodeInjakan saat ini
        DataObjekMap dataPetak = MapDatabase.Instance.semuaDataMap.Find(data =>
            data.nodeParkirIDs != null && data.nodeParkirIDs.Contains(idNodeInjakan)
        );

        // Kalau nodeID ini tidak terdaftar di objek map manapun (berarti petak jalan biasa), aman, langsung skip
        if (dataPetak == null)
        {
            Debug.Log($"<color=grey>[PetakManager]</color> Node {idNodeInjakan} adalah petak biasa/kosong.");
            return;
        }

        // Kalo ketemu, eksekusi mekaniknya!
        Debug.Log($"<color=cyan>[PetakManager]</color> MATCH! Node {idNodeInjakan} adalah bagian dari: <b>{dataPetak.namaObjek}</b> ({dataPetak.jenisObjek})");

        switch (dataPetak.jenisObjek)
        {
            case TipeObjek.Ketapang:
                Debug.Log("<color=green>🌴 [Panorama]</color> Mendapat 1 Panorama Ketapang!");
                // UIPanoramaManager.Instance.OpenPanel();
                break;

            case TipeObjek.Ijen:
                Debug.Log("<color=green>🌋 [Panorama]</color> Mendapat 1 Panorama Ijen!");
                // UIPanoramaManager.Instance.OpenPanel();
                break;

            case TipeObjek.PulauMerah:
                Debug.Log("<color=green>🏖️ [Panorama]</color> Mendapat 1 Panorama Pulau Merah!");
                // UIPanoramaManager.Instance.OpenPanel();
                break;

            case TipeObjek.Rajinan:
                if (UIRajinanManager.Instance != null) UIRajinanManager.Instance.OpenPanel();
                else Debug.LogWarning("UIRajinanManager tidak ditemukan!");
                break;

            case TipeObjek.Pasar:
                if (PopupManager.Instance != null) PopupManager.Instance.ShowPopup("Fitur Pasar Belum Tersedia!");
                else Debug.LogWarning("PopupManager tidak ditemukan!");
                break;

            case TipeObjek.Hujan:
                string[] opsiHujan = { "Jas Hujan", "Payung Bambu", "Sepatu Boots", "Air Berkah" };
                string hasilHujan = opsiHujan[Random.Range(0, opsiHujan.Length)];
                Debug.Log($"<color=blue>🌧️ [Hujan]</color> Player mendapat item random: <b>{hasilHujan}</b>");
                break;

            case TipeObjek.Kharisma:
                int acakKharisma = Random.Range(2, 5);
                Debug.Log($"<color=yellow>✨ [Kharisma]</color> Player mendapat +{acakKharisma} Kharisma!");
                break;

            case TipeObjek.Warisan:
                int acakKoin = Random.Range(5, 8);
                Debug.Log($"<color=gold>💰 [Warisan]</color> Player mendapat +{acakKoin} Koin!");
                break;
        }
    }
}