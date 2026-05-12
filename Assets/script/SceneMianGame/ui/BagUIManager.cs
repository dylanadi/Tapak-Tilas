using UnityEngine;
using System.Collections;

public class BagUIManager : MonoBehaviour
{
    [Header("Referensi UI")]
    public RectTransform bagPanel;

    [Header("Setting Animasi")]
    public float durasiSlide = 0.4f;

    [Tooltip("Posisi X saat tas disembunyikan (misal: -800)")]
    public float posisiX_Tutup = -800f;

    [Tooltip("Posisi X saat tas dibuka (misal: 0 atau 50)")]
    public float posisiX_Buka = 0f;



    private bool isBagOpen = false;
    private Coroutine animasiCoroutine;

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

    public KameraFollow kamera;

    public void DisableCamera()
    {

        kamera.kameraAktif = false;

    }

    public void EnableCamera()
    {
        kamera.kameraAktif = true;
    }
}