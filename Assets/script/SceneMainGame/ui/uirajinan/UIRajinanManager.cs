using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UIRajinanManager : MonoBehaviour
{
    // ==========================================
    // 🔥 SINGLETON
    // ==========================================
    public static UIRajinanManager Instance;

    // ==========================================
    // 🎨 PANEL & UI
    // ==========================================
    [Header("Panel & UI")]
    public GameObject panelRajinan;

    public Button btnCreate;
    public Button btnRefresh;

    public TMP_Text refreshCostText;
    public TMP_Text infoMaterialDibutuhkan;

    // ==========================================
    // 🃏 CONTAINER KARTU
    // ==========================================
    [Header("Gacha Kartu")]
    public Transform containerAtas;
    public Transform containerBawah;

    public GameObject prefabKartu;

    // ==========================================
    // 💰 REFRESH SETTINGS
    // ==========================================
    [Header("Settings Refresh")]
    public int currentRefreshCost = 1;

    // ==========================================
    // 📦 RUNTIME
    // ==========================================
    private List<UICardItem> kartuTerpasang = new List<UICardItem>();

    private UICardItem kartuTerpilih;

    // ==========================================
    // 🚀 UNITY
    // ==========================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (panelRajinan != null)
        {
            panelRajinan.SetActive(false);
        }

        if (btnCreate != null)
        {
            btnCreate.interactable = false;
        }

        UpdateRefreshUI();
    }

    // ==========================================
    // 📂 OPEN PANEL
    // ==========================================
    public void OpenPanel()
    {
        if (panelRajinan != null)
        {
            panelRajinan.SetActive(true);
        }

        if (btnCreate != null)
        {
            btnCreate.interactable = false;
        }

        if (infoMaterialDibutuhkan != null)
        {
            infoMaterialDibutuhkan.text = "Pilih kartu untuk melihat bahan...";
        }

        // ==========================================
        // 🎒 AUTO OPEN BAG
        // ==========================================
        if (BagUIManager.Instance != null)
        {
            if (!BagUIManager.Instance.isBagOpen)
            {
                BagUIManager.Instance.ToggleBag();
            }

            if (BagUIManager.Instance.btnClose != null)
            {
                BagUIManager.Instance.btnClose.SetActive(false);
            }
        }

        GenerateCards();

        UpdateRefreshUI();
    }

    // ==========================================
    // ❌ CLOSE PANEL
    // ==========================================
    public void ClosePanel()
    {
        if (panelRajinan != null)
        {
            panelRajinan.SetActive(false);
        }

        // ==========================================
        // 🎒 AUTO CLOSE BAG
        // ==========================================
        if (BagUIManager.Instance != null)
        {
            if (BagUIManager.Instance.btnClose != null)
            {
                BagUIManager.Instance.btnClose.SetActive(true);
            }

            if (BagUIManager.Instance.isBagOpen)
            {
                BagUIManager.Instance.ToggleBag();
            }
        }
    }

    // ==========================================
    // 🎰 GENERATE CARD
    // ==========================================
    public void GenerateCards()
    {
        // ==========================================
        // 🧹 CLEAR KARTU LAMA
        // ==========================================
        foreach (UICardItem item in kartuTerpasang)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        kartuTerpasang.Clear();

        // ==========================================
        // ⚠️ VALIDASI DATABASE
        // ==========================================
        if (KerajinanManager.Instance == null)
        {
            Debug.LogError("[UIRajinanManager] KerajinanManager Instance NULL!");
            return;
        }

        if (KerajinanManager.Instance.dataBase == null)
        {
            Debug.LogError("[UIRajinanManager] Database belum dipasang!");
            return;
        }

        List<DataKartu> semuaKartu =
            KerajinanManager.Instance.dataBase.databaseKartu;

        // ==========================================
        // ✅ FILTER YANG BISA DIBELI
        // ==========================================
        List<DataKartu> listMampu =
            semuaKartu
            .Where(k => CekApakahMaterialCukup(k))
            .OrderBy(x => Random.value)
            .ToList();

        // ==========================================
        // 🔥 BARIS ATAS
        // ==========================================
        List<DataKartu> kartuAtas =
            listMampu
            .Take(3)
            .ToList();

        // ==========================================
        // 🔥 BARIS BAWAH
        // ==========================================
        List<DataKartu> kartuBawah =
            semuaKartu
            .Except(kartuAtas)
            .OrderBy(x => Random.value)
            .Take(3)
            .ToList();

        // ==========================================
        // 🃏 SPAWN KARTU ATAS
        // ==========================================
        foreach (DataKartu data in kartuAtas)
        {
            SpawnCard(
                data,
                true,
                containerAtas
            );
        }

        // ==========================================
        // 🃏 SPAWN KARTU BAWAH
        // ==========================================
        foreach (DataKartu data in kartuBawah)
        {
            SpawnCard(
                data,
                CekApakahMaterialCukup(data),
                containerBawah
            );
        }
    }

    // ==========================================
    // 🃏 SPAWN SINGLE CARD
    // ==========================================
    void SpawnCard(
        DataKartu data,
        bool bisaBeli,
        Transform parent
    )
    {
        if (prefabKartu == null)
        {
            Debug.LogError("[UIRajinanManager] Prefab kartu kosong!");
            return;
        }

        GameObject obj =
            Instantiate(
                prefabKartu,
                parent
            );

        UICardItem item =
            obj.GetComponent<UICardItem>();

        if (item == null)
        {
            Debug.LogError("[UIRajinanManager] UICardItem tidak ditemukan!");
            return;
        }

        item.Setup(
            data,
            bisaBeli,
            this
        );

        kartuTerpasang.Add(item);
    }

    // ==========================================
    // 🔄 REFRESH
    // ==========================================
    public void RefreshButton()
    {
        // TODO:
        // Tambahkan pengecekan coin player

        currentRefreshCost *= 2;

        GenerateCards();

        UpdateRefreshUI();
    }

    // ==========================================
    // 💰 UPDATE REFRESH TEXT
    // ==========================================
    void UpdateRefreshUI()
    {
        if (refreshCostText != null)
        {
            refreshCostText.text =
                "= " + currentRefreshCost.ToString();
        }
    }

    // ==========================================
    // 🎯 SELECT CARD
    // ==========================================
    public void SelectCard(UICardItem item)
    {
        // ==========================================
        // ❌ MATIKAN SEMUA SELECTION
        // ==========================================
        foreach (UICardItem k in kartuTerpasang)
        {
            if (k != null)
            {
                k.SetSelected(false);
            }
        }

        // ==========================================
        // ✅ PILIH KARTU BARU
        // ==========================================
        kartuTerpilih = item;

        kartuTerpilih.SetSelected(true);

        // ==========================================
        // 🛒 BUTTON CREATE
        // ==========================================
        if (btnCreate != null)
        {
            btnCreate.interactable =
                item.bisaBeli;
        }

        // ==========================================
        // 📜 TAMPILKAN BAHAN
        // ==========================================
        string textBahan =
            "Bahan Dibutuhkan:\n";

        foreach (var bahan in item.data.listBahan)
        {
            DataMaterial material =
                KerajinanManager.Instance
                .GetMaterialByID(
                    bahan.idMaterial
                );

            if (material != null)
            {
                textBahan +=
                    "- "
                    + material.namaMaterial
                    + " : "
                    + bahan.jumlahButuh
                    + "\n";
            }
        }

        if (infoMaterialDibutuhkan != null)
        {
            infoMaterialDibutuhkan.text =
                textBahan;
        }
    }

    // ==========================================
    // 🎒 CEK MATERIAL
    // ==========================================
    public bool CekApakahMaterialCukup(
        DataKartu kartu
    )
    {
        // ==========================================
        // ⚠️ SEMENTARA TRUE DULU
        // ==========================================

        /*
        Contoh logika nanti:

        foreach(var butuh in kartu.listBahan)
        {
            int total = InventoryManager.Instance
                .GetJumlahMaterial(butuh.idMaterial);

            if(total < butuh.jumlahButuh)
            {
                return false;
            }
        }
        */

        return true;
    }

    // ==========================================
    // 🔨 CREATE / CRAFT
    // ==========================================
    public void CreateAct()
    {
        if (kartuTerpilih == null)
        {
            Debug.LogWarning("Belum memilih kartu!");
            return;
        }

        Debug.Log(
            "🔥 Berhasil Membuat: "
            + kartuTerpilih.data.namaKartu
        );

        // ==========================================
        // TODO:
        // - Kurangi material
        // - Tambahkan item hasil
        // - Tambahkan efek
        // ==========================================

        ClosePanel();
    }
}