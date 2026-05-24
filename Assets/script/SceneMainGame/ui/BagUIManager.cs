using UnityEngine;
using System.Collections;

public class BagUIManager : MonoBehaviour
{
    // 🔥 TAMBAHAN 1: Singleton (Biar UIRajinanManager bisa buka tas otomatis)
    public static BagUIManager Instance;

    [Header("Referensi UI")]
    public RectTransform bagPanel;
    public GameObject btnClose; // 🔥 TAMBAHAN 2: Slot untuk narik tombol silang (X) tas

    [Header("Setting Animasi")]
    public float durasiSlide = 0.4f;

    [Tooltip("Posisi X saat tas disembunyikan (misal: -800)")]
    public float posisiX_Tutup = -800f;

    [Tooltip("Posisi X saat tas dibuka (misal: 0 atau 50)")]
    public float posisiX_Buka = 0f;

    [HideInInspector]
    public bool isBagOpen = false; // 🔥 Diubah jadi public biar script Rajinan bisa ngecek status tasnya

    private Coroutine animasiCoroutine;

    void Awake()
    {
        // Setup Instance pas game mulai
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Pastikan tas tertutup di awal game
        Vector2 posisiAwal = bagPanel.anchoredPosition;
        bagPanel.anchoredPosition = new Vector2(posisiX_Tutup, posisiAwal.y);
    }

    // Panggil fungsi ini di Tombol Tas (buka) & Tombol X (tutup)
    public void ToggleBag()
    {
        isBagOpen = !isBagOpen;

        if (animasiCoroutine != null)
        {
            StopCoroutine(animasiCoroutine);
        }

        animasiCoroutine = StartCoroutine(AnimasiSlidePanel(isBagOpen));
    }

    IEnumerator AnimasiSlidePanel(bool buka)
    {
        float time = 0;
        Vector2 posAwal = bagPanel.anchoredPosition;

        // Tentukan tujuan X berdasarkan status buka/tutup
        float targetX = buka ? posisiX_Buka : posisiX_Tutup;
        Vector2 posTujuan = new Vector2(targetX, posAwal.y);

        while (time < durasiSlide)
        {
            time += Time.deltaTime;
            float t = time / durasiSlide;

            // Rumus ease-out (biar gesernya mulus pas mau berhenti)
            t = 1f - Mathf.Pow(1f - t, 3f);

            bagPanel.anchoredPosition = Vector2.Lerp(posAwal, posTujuan, t);
            yield return null;
        }

        bagPanel.anchoredPosition = posTujuan;
    }

    // (Opsional) Kamera manual disable/enable milikmu tetap dibiarkan aman di sini
    public KameraFollow kamera;

    public void DisableCamera()
    {
        if (kamera != null) kamera.kameraAktif = false;
    }

    public void EnableCamera()
    {
        if (kamera != null) kamera.kameraAktif = true;
    }
}