using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanoramaBook : MonoBehaviour
{
    [Header("UI Panel Utama")]
    [Tooltip("Tarik objek pembungkus paling luar dari Panorama ke sini")]
    public GameObject mainPanel;

    [Header("Setting Animasi Pop-Up")]
    [Tooltip("Durasi animasi pop up (detik)")]
    public float popUpDuration = 0.3f;
    private bool isAnimatingPopUp = false; // Mencegah spam klik saat animasi jalan

    [Header("Referensi Objek UI")]
    public Image[] gambarUI; // Array Image di Canvas (index 0-3)
    public Image[] gambarAnim;
    public GameObject[] pageAnim;
    public GameObject[] pageBtn;

    [Header("Sprites Panorama (Isi 4 per Halaman)")]
    public Sprite[] panorama1;
    public Sprite[] panorama2;
    public Sprite[] panorama3;

    [Header("Status Kepingan (0 = Hilang, 4 = Full)")]
    public int jumlahBukaPan1 = 0;
    public int jumlahBukaPan2 = 0;
    public int jumlahBukaPan3 = 0;

    [Header("Navigasi Halaman")]
    public int currentPage = 1;
    public int target;
    private bool isMoving;

    void Start()
    {
        // 1. Ambil data untuk halaman awal (Halaman 1)
        Sprite[] selectedPanorama = panorama1;
        int jumlahTerbuka = DapatJumlahBuka(currentPage);

        // 2. Bersihkan/Update gambar dasar
        UpdateSprites(currentPage);

        // 3. Bersihkan juga gambar di lembaran kertas animasinya
        FixfixSprite(selectedPanorama, jumlahTerbuka);

        // 4. Set skala awal jadi 0 dan matikan panel
        if (mainPanel != null)
        {
            mainPanel.transform.localScale = Vector3.zero;
            mainPanel.SetActive(false);
        }
    }

    // ==========================================
    // 🔥 FUNGSI BUKA/TUTUP PANEL DENGAN POP-UP
    // ==========================================
    public void TogglePanel()
    {
        if (mainPanel == null)
        {
            Debug.LogWarning("[PanoramaBook] mainPanel belum diisi di Inspector!");
            return;
        }

        // Kalau lagi di tengah-tengah animasi buka/tutup, cegah klik ganda
        if (isAnimatingPopUp) return;

        bool isSaatIniNyala = mainPanel.activeSelf;

        if (!isSaatIniNyala)
        {
            // PROSES BUKA (Nyala lalu membesar)
            GoToPage(1); // Opsional: Reset ke hal 1 tiap dibuka
            mainPanel.SetActive(true);
            StartCoroutine(AnimasiPopUp(Vector3.zero, Vector3.one, true));
        }
        else
        {
            // PROSES TUTUP (Mengecil lalu mati)
            StartCoroutine(AnimasiPopUp(Vector3.one, Vector3.zero, false));
        }
    }

    // Coroutine untuk efek zoom-in / zoom-out (Ease-Out)
    private IEnumerator AnimasiPopUp(Vector3 startScale, Vector3 targetScale, bool isOpening)
    {
        isAnimatingPopUp = true;
        float time = 0;

        while (time < popUpDuration)
        {
            time += Time.deltaTime;
            float t = time / popUpDuration;

            // Rumus Ease-Out halus
            t = 1f - Mathf.Pow(1f - t, 3f);

            mainPanel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        // Pastikan skala pas di akhir animasi
        mainPanel.transform.localScale = targetScale;

        // Kalau proses nutup, setelah mengecil langsung matikan objeknya
        if (!isOpening)
        {
            mainPanel.SetActive(false);
        }

        isAnimatingPopUp = false;
    }

    // ==========================================
    // 🔥 FUNGSI TAMBAH KEPINGAN PUZZLE
    // ==========================================
    public void TambahKepingan(int panoramaID)
    {
        if (panoramaID == 1 && jumlahBukaPan1 < 4) jumlahBukaPan1++;
        else if (panoramaID == 2 && jumlahBukaPan2 < 4) jumlahBukaPan2++;
        else if (panoramaID == 3 && jumlahBukaPan3 < 4) jumlahBukaPan3++;

        Debug.Log($"[Panorama] Kepingan ditambah! Panorama {panoramaID} sekarang punya {DapatJumlahBuka(panoramaID)}/4 kepingan.");

        if (currentPage == panoramaID && !isMoving)
        {
            UpdateSprites(currentPage);
        }
    }

    private int DapatJumlahBuka(int page)
    {
        switch (page)
        {
            case 1: return jumlahBukaPan1;
            case 2: return jumlahBukaPan2;
            case 3: return jumlahBukaPan3;
            default: return 0;
        }
    }

    // ==========================================
    // 📖 LOGIKA BALIK HALAMAN
    // ==========================================
    public void NextPage()
    {
        if (!isMoving && currentPage < 3)
        {
            currentPage++;
            StartCoroutine(TurnPage(1));
        }
    }

    public void PrevPage()
    {
        if (!isMoving && currentPage > 1)
        {
            currentPage--;
            StartCoroutine(TurnPage(0));
        }
    }

    public void GoToPage(int targetPage)
    {
        if (currentPage != targetPage)
        {
            target = targetPage;
            if (currentPage > targetPage)
            {
                PrevPage();
            }
            else if (currentPage < target)
            {
                NextPage();
            }
        }
    }

    public IEnumerator TurnPage(int dir)
    {
        yield return new WaitForSeconds(0.001f);
        isMoving = true;
        UpdateSprites(currentPage);

        int animIndex = (dir == 1) ? 0 : 1;
        string animName = (dir == 1) ? "TurnRight" : "TurnLeft";
        string animName2 = (dir == 1) ? "Idle1" : "Idle2";

        Animator anim = pageAnim[animIndex].GetComponent<Animator>();
        Canvas canvas = pageAnim[animIndex].GetComponent<Canvas>();

        canvas.sortingOrder = 2;
        anim.Play(animName);

        yield return new WaitForSeconds(0.5f);

        canvas.sortingOrder = 1;
        anim.Play(animName2);
        isMoving = false;

        if (currentPage != target)
        {
            GoToPage(target);
        }
    }

    // ==========================================
    // 🖼️ LOGIKA RENDER GAMBAR & ALPHANYA
    // ==========================================
    void UpdateSprites(int page)
    {
        Sprite[] selectedPanorama;
        switch (page)
        {
            case 1: selectedPanorama = panorama1; break;
            case 2: selectedPanorama = panorama2; break;
            case 3: selectedPanorama = panorama3; break;
            default: return;
        }

        int jumlahTerbuka = DapatJumlahBuka(page);

        for (int i = 0; i < gambarUI.Length; i++)
        {
            if (i < selectedPanorama.Length)
            {
                gambarUI[i].sprite = selectedPanorama[i];

                Color warna = gambarUI[i].color;
                warna.a = (i < jumlahTerbuka) ? 1f : 0f;
                gambarUI[i].color = warna;
            }
        }
    }

    public IEnumerator UpdateSprites2(int page, int dir)
    {
        Sprite[] selectedPanorama;
        switch (page)
        {
            case 1: selectedPanorama = panorama1; break;
            case 2: selectedPanorama = panorama2; break;
            case 3: selectedPanorama = panorama3; break;
            default: yield break;
        }

        int jumlahTerbuka = DapatJumlahBuka(page);

        if (dir == 1)
        {
            pageBtn[0].SetActive(false);

            SetSpriteAndAlpha(gambarAnim[0], selectedPanorama[3], 3, jumlahTerbuka);
            SetSpriteAndAlpha(gambarAnim[1], selectedPanorama[2], 2, jumlahTerbuka);

            gambarAnim[0].rectTransform.localScale = new Vector3(1, 1, 1);
            gambarAnim[1].rectTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (dir == 0)
        {
            pageBtn[1].SetActive(true);

            gambarAnim[3].rectTransform.localScale = new Vector3(-1, 1, 1);
            gambarAnim[2].rectTransform.localScale = new Vector3(-1, 1, 1);

            SetSpriteAndAlpha(gambarAnim[3], selectedPanorama[0], 0, jumlahTerbuka);
            SetSpriteAndAlpha(gambarAnim[2], selectedPanorama[1], 1, jumlahTerbuka);
        }
        yield return new WaitForSeconds(0.25f);

        FixfixSprite(selectedPanorama, jumlahTerbuka);
    }

    public void FixBtn()
    {
        pageBtn[1].SetActive(false);
    }

    public void FixfixSprite(Sprite[] selectedPanorama, int jumlahTerbuka)
    {
        gambarAnim[0].rectTransform.localScale = new Vector3(-1, 1, 1);
        gambarAnim[1].rectTransform.localScale = new Vector3(-1, 1, 1);
        gambarAnim[2].rectTransform.localScale = new Vector3(1, 1, 1);
        gambarAnim[3].rectTransform.localScale = new Vector3(1, 1, 1);

        for (int i = 0; i < gambarAnim.Length; i++)
        {
            if (i < selectedPanorama.Length)
            {
                SetSpriteAndAlpha(gambarAnim[i], selectedPanorama[i], i, jumlahTerbuka);
            }
        }
        pageBtn[1].SetActive(false);
        pageBtn[0].SetActive(true);
    }

    public void FixSprite() { }

    private void SetSpriteAndAlpha(Image img, Sprite spr, int indexKepingan, int batasTerbuka)
    {
        img.sprite = spr;
        Color warna = img.color;
        warna.a = (indexKepingan < batasTerbuka) ? 1f : 0f;
        img.color = warna;
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