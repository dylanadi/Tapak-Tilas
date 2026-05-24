using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class SlotPasar
{
    public Button tombol;
    public Image ikonMaterial;

    [HideInInspector] public int idMaterialSaatIni;
    [HideInInspector] public string namaMaterial;
    [HideInInspector] public int harga;
    [HideInInspector] public bool sudahTerbeli;
}

public class UIPasarManager : MonoBehaviour
{
    // ==========================================
    // SINGLETON
    // ==========================================
    public static UIPasarManager Instance;

    [Header("Setup Event Pasar")]
    public GameObject panelUtamaUI;
    public Button tombolClose;

    [Tooltip("Posisi kamera saat membuka pasar")]
    public Transform titikKameraPasar;

    [Header("Data Koin")]
    public int koinPlayer = 88;
    public TextMeshProUGUI textKoinUI;

    [Header("Material Premium")]
    public List<int> idMaterialPremium =
        new List<int>();

    [Header("UI")]
    public SlotPasar[] slotToko;
    public TextMeshProUGUI textInfoAtas;

    [Header("Animasi")]
    public float scaleNormal = 1f;
    public float scalePopUp = 1.15f;
    public float kecepatanAnimasi = 10f;

    private int slotTerpilih = -1;

    private bool pasarSedangBuka = false;

    private Coroutine coroutineAnimasi;

    // ==========================================
    // AWAKE
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
            return;
        }
    }

    // ==========================================
    // START
    // ==========================================
    void Start()
    {
        if (panelUtamaUI != null)
            panelUtamaUI.SetActive(false);

        if (tombolClose != null)
        {
            tombolClose.onClick.RemoveAllListeners();
            tombolClose.onClick.AddListener(TutupPasar);
        }

        UpdateUIKoin();

        Debug.Log("[Pasar] UIPasarManager aktif.");
    }

    // ==========================================
    // BUKA PASAR
    // ==========================================
    public void BukaPasar()
    {
        if (pasarSedangBuka)
            return;

        pasarSedangBuka = true;

        Debug.Log("[Pasar] Membuka pasar.");

        if (panelUtamaUI != null)
            panelUtamaUI.SetActive(true);

        if (
            KameraFollow.Instance != null &&
            titikKameraPasar != null
        )
        {
            KameraFollow.Instance
                .MasukModeKameraEvent(
                    titikKameraPasar
                );
        }

        slotTerpilih = -1;

        ResetUkuranSemuaSlot();

        UpdateUIKoin();

        if (textInfoAtas != null)
        {
            textInfoAtas.text =
                "Pilih barang yang ingin dibeli.";
        }

        AcakBarangPasar();
    }

    // ==========================================
    // TUTUP PASAR
    // ==========================================
    public void TutupPasar()
    {
        if (!pasarSedangBuka)
            return;

        pasarSedangBuka = false;

        Debug.Log("[Pasar] Menutup pasar.");

        if (panelUtamaUI != null)
            panelUtamaUI.SetActive(false);

        if (KameraFollow.Instance != null)
        {
            KameraFollow.Instance
                .KeluarModeKameraEvent();
        }
    }

    // ==========================================
    // ACAK BARANG
    // ==========================================
    private void AcakBarangPasar()
    {
        if (slotToko == null || slotToko.Length == 0)
        {
            Debug.LogError(
                "[Pasar] Slot toko kosong!"
            );
            return;
        }

        if (KerajinanManager.Instance == null)
        {
            Debug.LogError(
                "[Pasar] KerajinanManager tidak ditemukan!"
            );
            return;
        }

        if (
            idMaterialPremium.Count <
            slotToko.Length
        )
        {
            Debug.LogError(
                "[Pasar] Material premium kurang!"
            );
            return;
        }

        List<int> poolID =
            new List<int>(idMaterialPremium);

        for (int i = 0; i < slotToko.Length; i++)
        {
            if (slotToko[i] == null)
                continue;

            int randomIndex =
                Random.Range(0, poolID.Count);

            int idTerpilih =
                poolID[randomIndex];

            poolID.RemoveAt(randomIndex);

            DataMaterial dataMat =
                KerajinanManager.Instance
                .GetMaterialByID(idTerpilih);

            if (dataMat == null)
            {
                Debug.LogWarning(
                    $"[Pasar] Material ID {idTerpilih} tidak ditemukan."
                );

                continue;
            }

            slotToko[i].idMaterialSaatIni =
                idTerpilih;

            slotToko[i].namaMaterial =
                dataMat.namaMaterial;

            slotToko[i].harga =
                Random.Range(15, 35);

            slotToko[i].sudahTerbeli =
                false;

            if (slotToko[i].ikonMaterial != null)
            {
                slotToko[i].ikonMaterial.sprite =
                    dataMat.iconMaterial;

                slotToko[i].ikonMaterial.color =
                    Color.white;
            }

            if (slotToko[i].tombol != null)
            {
                slotToko[i].tombol.interactable =
                    true;

                int indexSlot = i;

                slotToko[i]
                    .tombol
                    .onClick
                    .RemoveAllListeners();

                slotToko[i]
                    .tombol
                    .onClick
                    .AddListener(() =>
                    {
                        OnSlotDiklik(indexSlot);
                    });
            }
        }
    }

    // ==========================================
    // SLOT DIKLIK
    // ==========================================
    public void OnSlotDiklik(int index)
    {
        if (
            index < 0 ||
            index >= slotToko.Length
        )
            return;

        if (slotToko[index].sudahTerbeli)
            return;

        // Klik kedua = beli
        if (slotTerpilih == index)
        {
            ProsesBeli(index);
        }
        else
        {
            slotTerpilih = index;

            UpdateInfoTeks(index);

            if (coroutineAnimasi != null)
            {
                StopCoroutine(coroutineAnimasi);
            }

            coroutineAnimasi =
                StartCoroutine(
                    AnimasiMembesar(index)
                );
        }
    }

    // ==========================================
    // BELI
    // ==========================================
    void ProsesBeli(int index)
    {
        int hargaBarang =
            slotToko[index].harga;

        if (koinPlayer >= hargaBarang)
        {
            koinPlayer -= hargaBarang;

            UpdateUIKoin();

            slotToko[index].sudahTerbeli =
                true;

            if (slotToko[index].tombol != null)
            {
                slotToko[index]
                    .tombol
                    .interactable = false;
            }

            if (slotToko[index].ikonMaterial != null)
            {
                slotToko[index]
                    .ikonMaterial
                    .color =
                        new Color(
                            0.3f,
                            0.3f,
                            0.3f,
                            1f
                        );
            }

            if (textInfoAtas != null)
            {
                textInfoAtas.text =
                    $"<color=green>Berhasil membeli {slotToko[index].namaMaterial}!</color>";
            }

            Debug.Log(
                $"[Pasar] Membeli material ID {slotToko[index].idMaterialSaatIni}"
            );

            slotTerpilih = -1;

            ResetUkuranSemuaSlot();
        }
        else
        {
            if (textInfoAtas != null)
            {
                textInfoAtas.text =
                    "<color=red>Koin tidak cukup!</color>";
            }

            StartCoroutine(
                AnimasiGagalBeli(index)
            );
        }
    }

    // ==========================================
    // UPDATE INFO
    // ==========================================
    void UpdateInfoTeks(int index)
    {
        string nama =
            slotToko[index].namaMaterial;

        int harga =
            slotToko[index].harga;

        int idMaterial =
            slotToko[index].idMaterialSaatIni;

        string infoCustom =
            "Bahan premium pilihan khas Banyuwangi.";

        switch (idMaterial)
        {
            case 1:
                infoCustom =
                    "Bahan langka kualitas nomor satu.";
                break;

            case 4:
                infoCustom =
                    "Sangat dicari para pengrajin.";
                break;

            case 7:
                infoCustom =
                    "Kuat dan punya corak indah.";
                break;
        }

        if (textInfoAtas != null)
        {
            textInfoAtas.text =
                $"<b>{nama}</b>\n" +
                $"<size=18>{infoCustom}</size>\n" +
                $"Harga: <color=yellow>{harga} Koin</color>\n" +
                $"<size=14><i>(Klik sekali lagi untuk beli)</i></size>";
        }
    }

    // ==========================================
    // UPDATE UI KOIN
    // ==========================================
    void UpdateUIKoin()
    {
        if (textKoinUI != null)
        {
            textKoinUI.text =
                $"{koinPlayer}";
        }
    }

    // ==========================================
    // ANIMASI MEMBESAR
    // ==========================================
    IEnumerator AnimasiMembesar(int indexTarget)
    {
        float t = 0f;

        Vector3[] startScales =
            new Vector3[slotToko.Length];

        for (int i = 0; i < slotToko.Length; i++)
        {
            if (slotToko[i].tombol != null)
            {
                startScales[i] =
                    slotToko[i]
                    .tombol
                    .transform
                    .localScale;
            }
        }

        Vector3 scaleBesar =
            Vector3.one * scalePopUp;

        Vector3 scaleBiasa =
            Vector3.one * scaleNormal;

        while (t < 1f)
        {
            t +=
                Time.deltaTime *
                kecepatanAnimasi;

            for (int i = 0; i < slotToko.Length; i++)
            {
                if (slotToko[i].tombol == null)
                    continue;

                Vector3 targetScale =
                    (i == indexTarget)
                    ? scaleBesar
                    : scaleBiasa;

                slotToko[i]
                    .tombol
                    .transform
                    .localScale =
                        Vector3.Lerp(
                            startScales[i],
                            targetScale,
                            t
                        );
            }

            yield return null;
        }
    }

    // ==========================================
    // ANIMASI GAGAL BELI
    // ==========================================
    IEnumerator AnimasiGagalBeli(int index)
    {
        if (slotToko[index].tombol == null)
            yield break;

        slotToko[index]
            .tombol
            .transform
            .localScale =
                Vector3.one * scaleNormal;

        yield return new WaitForSeconds(0.1f);

        slotToko[index]
            .tombol
            .transform
            .localScale =
                Vector3.one * scalePopUp;
    }

    // ==========================================
    // RESET SCALE
    // ==========================================
    void ResetUkuranSemuaSlot()
    {
        if (slotToko == null)
            return;

        for (int i = 0; i < slotToko.Length; i++)
        {
            if (slotToko[i].tombol != null)
            {
                slotToko[i]
                    .tombol
                    .transform
                    .localScale =
                        Vector3.one * scaleNormal;
            }
        }
    }
}