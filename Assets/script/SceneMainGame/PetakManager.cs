using UnityEngine;
using System.Linq;

public class PetakManager : MonoBehaviour
{
    public static PetakManager Instance;

    void Awake()
    {
        // Setup Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 🔥 Menerima ID Node Parkir dari pion yang mendarat
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

        // Kalau nodeID ini tidak terdaftar di objek map manapun (berarti petak jalan biasa), skip
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
                if (PanoramaBook.Instance != null)
                {
                    PanoramaBook.Instance.TambahKepingan(1); // 1 = Ketapang
                    PanoramaBook.Instance.GoToPage(1); // Buka halaman Ketapang
                    if (!PanoramaBook.Instance.mainPanel.activeSelf)
                        PanoramaBook.Instance.TogglePanel(); // Munculkan pop-up UI
                }
                else Debug.LogWarning("PanoramaBook tidak ditemukan di Scene!");
                break;

            case TipeObjek.Ijen:
                Debug.Log("<color=green>🌋 [Panorama]</color> Mendapat 1 Panorama Ijen!");
                if (PanoramaBook.Instance != null)
                {
                    PanoramaBook.Instance.TambahKepingan(2); // 2 = Ijen
                    PanoramaBook.Instance.GoToPage(2);
                    if (!PanoramaBook.Instance.mainPanel.activeSelf)
                        PanoramaBook.Instance.TogglePanel();
                }
                else Debug.LogWarning("PanoramaBook tidak ditemukan di Scene!");
                break;

            case TipeObjek.PulauMerah:
                Debug.Log("<color=green>🏖️ [Panorama]</color> Mendapat 1 Panorama Pulau Merah!");
                if (PanoramaBook.Instance != null)
                {
                    PanoramaBook.Instance.TambahKepingan(3); // 3 = Pulau Merah
                    PanoramaBook.Instance.GoToPage(3);
                    if (!PanoramaBook.Instance.mainPanel.activeSelf)
                        PanoramaBook.Instance.TogglePanel();
                }
                else Debug.LogWarning("PanoramaBook tidak ditemukan di Scene!");
                break;

            case TipeObjek.Rajinan:
                if (UIRajinanManager.Instance != null)
                    UIRajinanManager.Instance.OpenPanel();
                else
                    Debug.LogWarning("UIRajinanManager tidak ditemukan di Scene!");
                break;

            // 🔥 UPDATE BARU: BUKA TOKO PASAR
            case TipeObjek.Pasar:
                Debug.Log("<color=orange>🏪 [Pasar]</color> Membuka toko material premium!");
                if (UIPasarManager.Instance != null)
                {
                    UIPasarManager.Instance.BukaPasar();
                }
                else
                {
                    Debug.LogWarning("UIPasarManager tidak ditemukan di Scene!");
                }
                break;

            case TipeObjek.Hujan:
                string[] opsiHujan = { "Jas Hujan", "Payung Bambu", "Sepatu Boots", "Air Berkah" };
                string hasilHujan = opsiHujan[Random.Range(0, opsiHujan.Length)];
                Debug.Log($"<color=blue>🌧️ [Hujan]</color> Player mendapat item random: <b>{hasilHujan}</b>");
                break;

            case TipeObjek.Kharisma:
                int acakKharisma = Random.Range(2, 5);
                Debug.Log($"<color=yellow>✨ [Kharisma]</color> Player mendapat +{acakKharisma} Kharisma! (Belum tersimpan)");
                break;

            case TipeObjek.Warisan:
                int acakKoin = Random.Range(5, 8);
                Debug.Log($"<color=gold>💰 [Warisan]</color> Player mendapat +{acakKoin} Koin! (Belum tersimpan)");
                break;
        }
    }
}