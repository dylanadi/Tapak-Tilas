using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanoramaBook : MonoBehaviour
{
    // =========================
    // SINGLETON
    // =========================
    public static PanoramaBook Instance;

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

    [Header("UI Panel Utama")]
    [Tooltip("Tarik objek pembungkus paling luar dari Panorama ke sini")]
    public GameObject mainPanel;

    [Header("Setting Animasi Pop-Up")]
    [Tooltip("Durasi animasi pop up (detik)")]
    public float popUpDuration = 0.3f;

    private bool isAnimatingPopUp = false;

    [Header("Referensi Objek UI")]
    public Image[] gambarUI;
    public Image[] gambarAnim;
    public GameObject[] pageAnim;
    public GameObject[] pageBtn;

    [Header("Sprites Panorama")]
    public Sprite[] panorama1;
    public Sprite[] panorama2;
    public Sprite[] panorama3;

    [Header("Status Kepingan")]
    [Range(0, 4)] public int jumlahBukaPan1 = 0;
    [Range(0, 4)] public int jumlahBukaPan2 = 0;
    [Range(0, 4)] public int jumlahBukaPan3 = 0;

    [Header("Navigasi")]
    public int currentPage = 1;

    private int targetPage = 1;
    private bool isMoving = false;

    [Header("Camera")]
    public KameraFollow kamera;

    // =========================
    // START
    // =========================
    void Start()
    {
        SetupCurrentPage();

        if (mainPanel != null)
        {
            mainPanel.transform.localScale = Vector3.zero;
            mainPanel.SetActive(false);
        }
    }

    // =========================
    // SETUP PAGE
    // =========================
    void SetupCurrentPage()
    {
        Sprite[] selectedPanorama = GetPanorama(currentPage);

        if (selectedPanorama == null)
            return;

        int jumlahTerbuka = DapatJumlahBuka(currentPage);

        UpdateSprites(currentPage);
        FixfixSprite(selectedPanorama, jumlahTerbuka);
    }

    // =========================
    // PANEL
    // =========================
    public void TogglePanel()
    {
        if (mainPanel == null)
        {
            Debug.LogWarning("[PanoramaBook] Main Panel belum diisi.");
            return;
        }

        if (isAnimatingPopUp)
            return;

        bool isOpen = mainPanel.activeSelf;

        if (!isOpen)
        {
            GoToPage(1);

            mainPanel.SetActive(true);

            StartCoroutine(
                AnimasiPopUp(
                    Vector3.zero,
                    Vector3.one,
                    true
                )
            );
        }
        else
        {
            StartCoroutine(
                AnimasiPopUp(
                    Vector3.one,
                    Vector3.zero,
                    false
                )
            );
        }
    }

    IEnumerator AnimasiPopUp(Vector3 startScale, Vector3 endScale, bool isOpening)
    {
        isAnimatingPopUp = true;

        float time = 0f;

        while (time < popUpDuration)
        {
            time += Time.deltaTime;

            float t = time / popUpDuration;

            // Ease Out Cubic
            t = 1f - Mathf.Pow(1f - t, 3f);

            mainPanel.transform.localScale =
                Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        mainPanel.transform.localScale = endScale;

        if (!isOpening)
        {
            mainPanel.SetActive(false);
        }

        isAnimatingPopUp = false;
    }

    // =========================
    // TAMBAH KEPINGAN
    // =========================
    public void TambahKepingan(int panoramaID)
    {
        switch (panoramaID)
        {
            case 1:
                if (jumlahBukaPan1 < 4)
                    jumlahBukaPan1++;
                break;

            case 2:
                if (jumlahBukaPan2 < 4)
                    jumlahBukaPan2++;
                break;

            case 3:
                if (jumlahBukaPan3 < 4)
                    jumlahBukaPan3++;
                break;
        }

        Debug.Log(
            $"[Panorama] Panorama {panoramaID} sekarang punya {DapatJumlahBuka(panoramaID)}/4 kepingan."
        );

        if (currentPage == panoramaID && !isMoving)
        {
            UpdateSprites(currentPage);
        }
    }

    int DapatJumlahBuka(int page)
    {
        switch (page)
        {
            case 1: return jumlahBukaPan1;
            case 2: return jumlahBukaPan2;
            case 3: return jumlahBukaPan3;
        }

        return 0;
    }

    // =========================
    // PAGE NAVIGATION
    // =========================
    public void NextPage()
    {
        if (isMoving)
            return;

        if (currentPage >= 3)
            return;

        currentPage++;

        StartCoroutine(TurnPage(1));
    }

    public void PrevPage()
    {
        if (isMoving)
            return;

        if (currentPage <= 1)
            return;

        currentPage--;

        StartCoroutine(TurnPage(0));
    }

    public void GoToPage(int page)
    {
        if (page < 1 || page > 3)
            return;

        targetPage = page;

        if (currentPage == targetPage)
            return;

        if (currentPage < targetPage)
        {
            NextPage();
        }
        else
        {
            PrevPage();
        }
    }

    IEnumerator TurnPage(int dir)
    {
        yield return new WaitForSeconds(0.001f);

        isMoving = true;

        int animIndex = (dir == 1) ? 0 : 1;

        string animName =
            (dir == 1) ? "TurnRight" : "TurnLeft";

        string idleName =
            (dir == 1) ? "Idle1" : "Idle2";

        Animator anim =
            pageAnim[animIndex].GetComponent<Animator>();

        Canvas canvas =
            pageAnim[animIndex].GetComponent<Canvas>();

        canvas.sortingOrder = 2;

        anim.Play(animName);

        // TUNGGU SETENGAH ANIMASI DULU
        yield return new WaitForSeconds(0.25f);

        // BARU GANTI SPRITE
        UpdateSprites(currentPage);

        yield return new WaitForSeconds(0.25f);

        canvas.sortingOrder = 1;

        anim.Play(idleName);

        isMoving = false;

        if (currentPage != targetPage)
        {
            GoToPage(targetPage);
        }
    }

    // =========================
    // UPDATE SPRITES
    // =========================
    void UpdateSprites(int page)
    {
        Sprite[] selectedPanorama = GetPanorama(page);

        if (selectedPanorama == null)
            return;

        int jumlahTerbuka = DapatJumlahBuka(page);

        for (int i = 0; i < gambarUI.Length; i++)
        {
            if (i >= selectedPanorama.Length)
                continue;

            gambarUI[i].sprite = selectedPanorama[i];

            Color warna = gambarUI[i].color;

            warna.a =
                (i < jumlahTerbuka)
                ? 1f
                : 0f;

            gambarUI[i].color = warna;
        }
    }

    public IEnumerator UpdateSprites2(int page, int dir)
    {
        Sprite[] selectedPanorama = GetPanorama(page);

        if (selectedPanorama == null)
            yield break;

        int jumlahTerbuka = DapatJumlahBuka(page);

        if (dir == 1)
        {
            pageBtn[0].SetActive(false);

            SetSpriteAndAlpha(
                gambarAnim[0],
                selectedPanorama[3],
                3,
                jumlahTerbuka
            );

            SetSpriteAndAlpha(
                gambarAnim[1],
                selectedPanorama[2],
                2,
                jumlahTerbuka
            );

            gambarAnim[0].rectTransform.localScale =
                new Vector3(1, 1, 1);

            gambarAnim[1].rectTransform.localScale =
                new Vector3(1, 1, 1);
        }
        else
        {
            pageBtn[1].SetActive(true);

            gambarAnim[3].rectTransform.localScale =
                new Vector3(-1, 1, 1);

            gambarAnim[2].rectTransform.localScale =
                new Vector3(-1, 1, 1);

            SetSpriteAndAlpha(
                gambarAnim[3],
                selectedPanorama[0],
                0,
                jumlahTerbuka
            );

            SetSpriteAndAlpha(
                gambarAnim[2],
                selectedPanorama[1],
                1,
                jumlahTerbuka
            );
        }

        yield return new WaitForSeconds(0.25f);

        FixfixSprite(selectedPanorama, jumlahTerbuka);
    }

    // =========================
    // FIX BUTTON
    // =========================
    public void FixBtn()
    {
        if (pageBtn.Length > 1)
        {
            pageBtn[1].SetActive(false);
        }
    }

    // =========================
    // FIX SPRITE
    // =========================
    public void FixfixSprite(
        Sprite[] selectedPanorama,
        int jumlahTerbuka
    )
    {
        gambarAnim[0].rectTransform.localScale =
            new Vector3(-1, 1, 1);

        gambarAnim[1].rectTransform.localScale =
            new Vector3(-1, 1, 1);

        gambarAnim[2].rectTransform.localScale =
            new Vector3(1, 1, 1);

        gambarAnim[3].rectTransform.localScale =
            new Vector3(1, 1, 1);

        for (int i = 0; i < gambarAnim.Length; i++)
        {
            if (i >= selectedPanorama.Length)
                continue;

            SetSpriteAndAlpha(
                gambarAnim[i],
                selectedPanorama[i],
                i,
                jumlahTerbuka
            );
        }

        if (pageBtn.Length > 1)
        {
            pageBtn[1].SetActive(false);
            pageBtn[0].SetActive(true);
        }
    }

    public void FixSprite() { }

    // =========================
    // SET SPRITE
    // =========================
    void SetSpriteAndAlpha(
        Image img,
        Sprite spr,
        int indexKepingan,
        int batasTerbuka
    )
    {
        if (img == null)
            return;

        img.sprite = spr;

        Color warna = img.color;

        warna.a =
            (indexKepingan < batasTerbuka)
            ? 1f
            : 0f;

        img.color = warna;
    }

    // =========================
    // GET PANORAMA
    // =========================
    Sprite[] GetPanorama(int page)
    {
        switch (page)
        {
            case 1: return panorama1;
            case 2: return panorama2;
            case 3: return panorama3;
        }

        return null;
    }

    // =========================
    // CAMERA
    // =========================
    public void DisableCamera()
    {
        if (kamera != null)
        {
            kamera.kameraAktif = false;
        }
    }

    public void EnableCamera()
    {
        if (kamera != null)
        {
            kamera.kameraAktif = true;
        }
    }
}