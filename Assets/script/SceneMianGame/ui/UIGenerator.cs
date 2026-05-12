using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

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

        // Looping sebanyak data yang ada di Buku Induk (MapDatabase)
        foreach (DataObjekMap data in MapDatabase.Instance.semuaDataMap)
        {
            // 1. Spawn Prefab Icon di dalam Container Bawah (Transform script ini)
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

            // 3. Spawn Kotak Merah ke dalam 'WadahKotak'
            Transform wadahKotak = newIconObj.transform.Find("WadahKotak");
            List<Image> indikatorKotak = new List<Image>();

            if (wadahKotak != null && data.nodeParkirIDs != null)
            {
                for (int i = 0; i < data.nodeParkirIDs.Length; i++)
                {
                    GameObject newKotak = Instantiate(prefabKotakMerah, wadahKotak);
                    indikatorKotak.Add(newKotak.GetComponent<Image>());
                }
            }

            // 4. Cari Objek 3D pasangannya
            InteractableObject targetObjek = semuaObjek3D.FirstOrDefault(obj => obj.idObjek == data.idObjek);

            // 5. Tempelkan otak logika ke Icon ini
            if (targetObjek != null)
            {
                UIIconLogic iconLogic = newIconObj.AddComponent<UIIconLogic>();

                // 🔥 UPDATE: Sekarang kita mengirimkan data.tinggiKamera ke dalam fungsi SetupIcon!
                iconLogic.SetupIcon(targetObjek, indikatorKotak.ToArray(), data.tinggiKamera);
            }
        }

        Debug.Log("[UI] Semua Icon UI berhasil di-generate secara otomatis!");
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

    // 🔥 TAMBAHAN: Variabel untuk menyimpan tinggi kamera khusus objek ini
    private float tinggiKameraObjek;

    // 🔥 UPDATE: Parameter fungsi ditambah 'float tinggi'
    public void SetupIcon(InteractableObject objek3D, Image[] kotak, float tinggi)
    {
        targetObjek3D = objek3D;
        kotakIndikator = kotak;
        tinggiKameraObjek = tinggi; // Simpan angkanya ke variabel di atas

        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();

        btn.onClick.AddListener(OnClickIcon);

        if (targetObjek3D != null)
        {
            nodeParkir = targetObjek3D.DapatkanSemuaNodeParkir();
        }
    }

    void Update()
    {
        // 🔄 SISTEM UPDATE INDIKATOR (Transparansi kotak merah)
        if (nodeParkir == null || kotakIndikator == null) return;

        for (int i = 0; i < kotakIndikator.Length; i++)
        {
            if (i < nodeParkir.Length && nodeParkir[i] != null)
            {
                Color warna = kotakIndikator[i].color;
                // Full = 100% Opacity, Kosong = 60% Opacity
                warna.a = nodeParkir[i].IsFull() ? 1f : 0.6f;
                kotakIndikator[i].color = warna;
            }
        }
    }

    void OnClickIcon()
    {
        if (targetObjek3D == null) return;

        // 🔥 1. CEK GILIRAN (Turn-Based System)
        if (GameManager.Instance != null)
        {
            PlayerData pemainSekarang = GameManager.Instance.GetCurrentPlayer();
            if (pemainSekarang == null || (pemainSekarang.photonView != null && !pemainSekarang.photonView.IsMine))
            {
                if (PopupManager.Instance != null)
                {
                    PopupManager.Instance.ShowPopup("Bukan giliranmu! Sabar nunggu ya.");
                }
                return;
            }
        }

        // 🔥 2. CEK APAKAH SUDAH BERADA DI LOKASI INI
        PlayerData localPlayer = FindObjectsOfType<PlayerData>().FirstOrDefault(p => p.photonView != null && p.photonView.IsMine);
        if (localPlayer != null)
        {
            GerakPion pionLokal = localPlayer.GetComponent<GerakPion>();
            if (pionLokal != null && pionLokal.currentNode != null)
            {
                if (pionLokal.currentNode.nodeID == targetObjek3D.nodeGarisID ||
                   (targetObjek3D.nodeParkirIDs != null && targetObjek3D.nodeParkirIDs.Contains(pionLokal.currentNode.nodeID)))
                {
                    if (PopupManager.Instance != null)
                    {
                        PopupManager.Instance.ShowPopup("Kamu sudah di petak ini, silahkan pilih petak lainnya!");
                    }
                    return;
                }
            }
        }

        // 🔥 3. CEK KAPASITAS PARKIR (Full Check)
        bool parkiranFull = true;
        if (nodeParkir != null && nodeParkir.Length > 0)
        {
            foreach (StopNode node in nodeParkir)
            {
                if (node != null && !node.IsFull())
                {
                    parkiranFull = false;
                    break;
                }
            }
        }

        if (parkiranFull)
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowPopup("Parkiran di objek ini sudah penuh!");
            }
            return;
        }

        // 🔥 4. JALANKAN NAVIGASI & FOKUS KAMERA
        if (KameraFollow.Instance != null)
        {
            // 🔥 UPDATE: Mengirimkan dua data sekaligus (Posisi Target & Tinggi Target)
            KameraFollow.Instance.FokusKePosisi(targetObjek3D.transform.position, tinggiKameraObjek);
        }

        targetObjek3D.TerpilihDariUI();
    }
}