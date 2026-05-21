using UnityEngine;
using UnityEngine.UI;

public class UICardItem : MonoBehaviour
{
    [Header("Referensi UI Kartu")]
    [Tooltip("Tarik komponen Image dari objek KartuBase ini ke sini")]
    public Image gambarUtamaKartu;

    [Tooltip("Tarik objek Image OutlineOranye ke sini")]
    public GameObject outlinePilih;

    // Data yang disimpan diam-diam (tidak muncul di Inspector)
    [HideInInspector] public DataKartu data;
    [HideInInspector] public bool bisaBeli;

    private UIRajinanManager manager;

    // Fungsi ini dipanggil dari UIRajinanManager saat nge-spawn kartu
    public void Setup(DataKartu dataBaru, bool statusBeli, UIRajinanManager managerAsli)
    {
        data = dataBaru;
        bisaBeli = statusBeli;
        manager = managerAsli;

        // Pasang gambar sprite kartu utuh dari database ke UI
        if (gambarUtamaKartu != null)
        {
            gambarUtamaKartu.sprite = data.gambarKartu;

            // Visual Feedback: Kalau material nggak cukup, kartunya otomatis digelapkan
            gambarUtamaKartu.color = bisaBeli ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        // Matikan outline oranye saat kartu baru pertama kali muncul
        SetSelected(false);
    }

    // Fungsi ini dipanggil saat komponen Button di kartu ini diklik
    public void OnClickCard()
    {
        if (manager != null)
        {
            manager.SelectCard(this);
        }
    }

    // Fungsi untuk menyalakan/mematikan garis pinggir oranye
    public void SetSelected(bool isSelected)
    {
        if (outlinePilih != null)
        {
            outlinePilih.SetActive(isSelected);
        }
    }
}