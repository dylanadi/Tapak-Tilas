using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun; // Pastikan baris ini ada di paling atas file

public class UIGenerator : MonoBehaviour
{
    [Header("Referensi Prefab UI")]
    [Tooltip("Masukkan Prefab BaseIcon ke sini")]
    public GameObject prefabIconUI;

    [Tooltip("Masukkan Prefab KotakMerah ke sini")]
    public GameObject prefabKotakMerah;

    void Start()
    {
        // Kasih jeda 1 detik biar MapDatabase dan Objek 3D siap duluan saat game mulai
        Invoke("GenerateUIOtomatis", 1f);
    }

    void GenerateUIOtomatis()
    {
        if (MapDatabase.Instance == null)
        {
            Debug.LogError("[UI] MapDatabase tidak ditemukan! Pastikan objek MapDatabase ada di Scene.");
            return;
        }

        // Cari semua objek 3D di map yang punya script InteractableObject (Pasar, Patung, dll)
        InteractableObject[] semuaObjek3D = FindObjectsOfType<InteractableObject>();

        // Looping sebanyak data yang ada di Buku Induk
        foreach (DataObjekMap data in MapDatabase.Instance.semuaDataMap)
        {
            // 1. Spawn Prefab Icon di dalam Container Bawah
            GameObject newIconObj = Instantiate(prefabIconUI, transform);
            newIconObj.name = "IconUI_" + data.namaObjek;

            // 2. Pasang Gambar Icon
            Transform gambarIconTransform = newIconObj.transform.Find("GambarIcon");
            if (gambarIconTransform != null)
            {
                Image iconImage = gambarIconTransform.GetComponent<Image>();
                if (iconImage != null && data.iconUI != null)
                {
                    iconImage.sprite = data.iconUI;
                }
            }
            else
            {
                Debug.LogWarning($"[UI] Objek 'GambarIcon' tidak ditemukan di dalam Prefab Icon {data.namaObjek}. Pastikan namanya persis!");
            }

            // 3. Spawn Kotak Merah ke dalam 'WadahKotak'
            Transform wadahKotak = newIconObj.transform.Find("WadahKotak");
            List<Image> indikatorKotak = new List<Image>();

            if (wadahKotak != null && data.nodeParkirIDs != null)
            {
                // Bikin kotak merah sebanyak jumlah petak parkir di database
                for (int i = 0; i < data.nodeParkirIDs.Length; i++)
                {
                    GameObject newKotak = Instantiate(prefabKotakMerah, wadahKotak);
                    indikatorKotak.Add(newKotak.GetComponent<Image>());
                }
            }

            // 4. Cari Objek 3D pasangannya (Yang ID Objek-nya sama dengan di Database)
            InteractableObject targetObjek = semuaObjek3D.FirstOrDefault(obj => obj.idObjek == data.idObjek);

            // 5. Tempelkan otak logika ke Icon ini
            if (targetObjek != null)
            {
                UIIconLogic iconLogic = newIconObj.AddComponent<UIIconLogic>();
                iconLogic.SetupIcon(targetObjek, indikatorKotak.ToArray());
            }
            else
            {
                Debug.LogWarning($"[UI] Objek 3D dengan ID {data.idObjek} ({data.namaObjek}) tidak ditemukan di Map! Icon tidak disambungkan.");
            }
        }

        Debug.Log("[UI] 14 Icon UI berhasil di-generate secara otomatis!");
    }
}

// ==========================================
// 🔥 SCRIPT OTOMATIS UNTUK LOGIKA TIAP ICON
// ==========================================
public class UIIconLogic : MonoBehaviour
{
    private InteractableObject targetObjek3D;
    private Image[] kotakIndikator;
    private StopNode[] nodeParkir;

    public void SetupIcon(InteractableObject objek3D, Image[] kotak)
    {
        targetObjek3D = objek3D;
        kotakIndikator = kotak;

        // Bikin tombol bisa diklik (Jika prefab belum ada Button, otomatis ditambahin)
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();

        btn.onClick.AddListener(OnClickIcon);

        // Ambil data node aslinya dari objek 3D
        if (targetObjek3D != null)
        {
            nodeParkir = targetObjek3D.DapatkanSemuaNodeParkir();
        }
    }

    void Update()
    {
        // 🔄 SISTEM CEK KOTAK MERAH (Otomatis transparan 60% atau Solid 100%)
        if (nodeParkir == null || kotakIndikator == null) return;

        for (int i = 0; i < kotakIndikator.Length; i++)
        {
            if (i < nodeParkir.Length && nodeParkir[i] != null)
            {
                Color warna = kotakIndikator[i].color;

                // Kalau ada orangnya opacity 1f (100%), kalau kosong opacity 0.6f (60%)
                warna.a = nodeParkir[i].IsFull() ? 1f : 0.6f;

                kotakIndikator[i].color = warna;
            }
        }
    }

    void OnClickIcon()
    {
        if (targetObjek3D == null) return;

        // 🔥 1. CEK APAKAH PLAYER SUDAH BERADA DI OBJEK INI
        // Cari player lokal kita sendiri
        PlayerData localPlayer = FindObjectsOfType<PlayerData>().FirstOrDefault(p => p.photonView != null && p.photonView.IsMine);
        
        if (localPlayer != null)
        {
            GerakPion pionLokal = localPlayer.GetComponent<GerakPion>();
            if (pionLokal != null && pionLokal.currentNode != null)
            {
                // Kalau ID node player SAMA dengan ID pintu masuk objek, ATAU ada di dalam parkirannya...
                if (pionLokal.currentNode.nodeID == targetObjek3D.nodeGarisID || 
                   (targetObjek3D.nodeParkirIDs != null && targetObjek3D.nodeParkirIDs.Contains(pionLokal.currentNode.nodeID)))
                {
                    if (PopupManager.Instance != null) 
                    {
                        PopupManager.Instance.ShowPopup("Kamu sudah di petak ini, silahkan pilih petak lainnya!");
                    }
                    Debug.Log($"[UI] Klik ditolak, Player sudah berada di {targetObjek3D.gameObject.name}.");
                    return; // Stop di sini, batalkan klik
                }
            }
        }

        // 🔥 2. CEK APAKAH SEMUA PARKIRAN SUDAH FULL
        bool parkiranFull = true; 
        
        if (nodeParkir != null && nodeParkir.Length > 0)
        {
            foreach (StopNode node in nodeParkir)
            {
                if (node != null && !node.IsFull()) 
                {
                    parkiranFull = false; // Ketemu 1 yang kosong!
                    break; 
                }
            }
        }

        // Kalau ternyata full beneran
        if (parkiranFull)
        {
            if (PopupManager.Instance != null) 
            {
                PopupManager.Instance.ShowPopup("Parkiran di objek ini sudah penuh!");
            }
            Debug.Log($"[UI] Klik ditolak, objek {targetObjek3D.gameObject.name} penuh.");
            return; // Stop di sini
        }

        // 🔥 3. KALAU SEMUA AMAN (Gak di tempat & Gak full), JALANKAN NAVIGASI
        Debug.Log($"[UI] Mengarahkan kamera ke {targetObjek3D.gameObject.name}");
        
        if (KameraFollow.Instance != null) 
        {
            KameraFollow.Instance.FokusKePosisi(targetObjek3D.transform.position);
        }
        
        targetObjek3D.TerpilihDariUI();
    }
}