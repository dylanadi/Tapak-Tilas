using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class UIRajinanManager : MonoBehaviour
{
    // 🔥 Singleton: Biar bisa dipanggil dari script Petak tanpa perlu tarik-tarik di Inspector
    public static UIRajinanManager Instance;

    [Header("Panel & UI")]
    public GameObject panelRajinan;
    public Button btnCreate;
    public Button btnRefresh;
    public Text refreshCostText;
    public Text infoMaterialDibutuhkan;

    [Header("Gacha Kartu")]
    public Transform containerAtas;
    public Transform containerBawah;
    public GameObject prefabKartu;

    [Header("Settings Refresh")]
    public int currentRefreshCost = 1;

    private List<UICardItem> kartuTerpasang = new List<UICardItem>();
    private UICardItem kartuTerpilih;

    void Awake()
    {
        // Setup Instance pas game mulai
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pastikan panel tertutup saat game baru mulai
        if (panelRajinan != null) panelRajinan.SetActive(false);
    }

    // ==========================================
    // 🛠️ FUNGSI BUKA / TUTUP PANEL
    // ==========================================
    public void OpenPanel()
    {
        if (panelRajinan != null) panelRajinan.SetActive(true);
        if (btnCreate != null) btnCreate.interactable = false;
        if (infoMaterialDibutuhkan != null) infoMaterialDibutuhkan.text = "Pilih kartu untuk melihat bahan...";

        // 🔥 Buka tas otomatis dan sembunyikan tombol close (X) tas
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

    public void ClosePanel()
    {
        if (panelRajinan != null) panelRajinan.SetActive(false);

        // 🔥 Tutup tas otomatis dan munculkan lagi tombol close (X) tas
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
    // 🔥 LOGIKA UTAMA: GACHA 3+3
    // ==========================================
    public void GenerateCards()
    {
        // Hapus kartu-kartu lama yang ada di layar
        foreach (var k in kartuTerpasang) Destroy(k.gameObject);
        kartuTerpasang.Clear();

        // Keamanan biar nggak error kalau databasenya lupa dipasang di GameManager
        if (KerajinanManager.Instance == null || KerajinanManager.Instance.dataBase == null)
        {
            Debug.LogError("[UI Rajinan] Database Kerajinan belum terpasang di GameManager!");
            return;
        }

        var db = KerajinanManager.Instance.dataBase.databaseKartu;

        // 1. Filter kartu yang "Mampu Beli" (Material Cukup)
        List<DataKartu> listMampu = db.Where(k => CekApakahMaterialCukup(k)).ToList();

        // 2. Acak & ambil maksimal 3 kartu untuk baris ATAS
        List<DataKartu> atas = listMampu.OrderBy(x => Random.value).Take(3).ToList();

        // 3. Sisa kartu (selain yang di atas), acak & ambil maksimal 3 untuk baris BAWAH
        List<DataKartu> bawah = db.Except(atas).OrderBy(x => Random.value).Take(3).ToList();

        // Spawn ke UI (layar)
        foreach (var d in atas) SpawnCard(d, true, containerAtas);
        foreach (var d in bawah) SpawnCard(d, CekApakahMaterialCukup(d), containerBawah);
    }

    void SpawnCard(DataKartu data, bool bisaBeli, Transform parent)
    {
        GameObject go = Instantiate(prefabKartu, parent);
        UICardItem item = go.GetComponent<UICardItem>();
        item.Setup(data, bisaBeli, this);
        kartuTerpasang.Add(item);
    }

    // ==========================================
    // 🛠️ LOGIKA EKONOMI & REFRESH
    // ==========================================
    public void RefreshButton()
    {
        // Nantinya tambahkan cek koin player di sini: if(PlayerCoin >= currentRefreshCost) { ...
        currentRefreshCost *= 2;
        GenerateCards();
        UpdateRefreshUI();
    }

    void UpdateRefreshUI()
    {
        if (refreshCostText != null) refreshCostText.text = "= " + currentRefreshCost.ToString();
    }

    // ==========================================
    // 🛠️ INTERAKSI KARTU & CRAFTING
    // ==========================================
    public void SelectCard(UICardItem item)
    {
        // Matikan outline oranye di semua kartu
        foreach (var k in kartuTerpasang) k.SetSelected(false);

        // Nyalakan outline oranye di kartu yang dipilih
        kartuTerpilih = item;
        kartuTerpilih.SetSelected(true);

        // Nyalakan/Matikan tombol Create (Keranjang) sesuai status bisa beli
        if (btnCreate != null) btnCreate.interactable = item.bisaBeli;

        // Tampilkan teks daftar bahan yang dibutuhkan
        string textBahan = "Bahan Dibutuhkan:\n";
        foreach (var b in item.data.listBahan)
        {
            var dataMat = KerajinanManager.Instance.GetMaterialByID(b.idMaterial);
            if (dataMat != null) textBahan += $"- {dataMat.namaMaterial}: {b.jumlahButuh}\n";
        }
        if (infoMaterialDibutuhkan != null) infoMaterialDibutuhkan.text = textBahan;
    }

    public bool CekApakahMaterialCukup(DataKartu kartu)
    {
        // ⚠️ LOGIKA TAS: Nantinya kamu ganti pakai pengecekan tas sungguhan
        // foreach(var butuh in kartu.listBahan) { if(TotalMaterialDiTas < butuh.jumlahButuh) return false; }
        return true;
    }

    public void CreateAct()
    {
        if (kartuTerpilih != null)
        {
            Debug.Log("🔥 Berhasil Membuat: " + kartuTerpilih.data.namaKartu);

            // Nantinya tambahkan logika kurangi material di tas & tambah kharisma di sini

            ClosePanel();
        }
    }
}