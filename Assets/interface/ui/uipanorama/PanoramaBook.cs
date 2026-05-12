using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanoramaBook : MonoBehaviour
{
    private bool isMoving;
    public Image[] gambarUI; // Array Image di Canvas (index 0-3)
    public Image[] gambarAnim;

    [Header("Sprites Konfigurasi")]
    public Sprite[] panorama1; // Isi dengan 4 gambar untuk Hal 1
    public Sprite[] panorama2; // Isi dengan 4 gambar untuk Hal 2
    public Sprite[] panorama3; // Isi dengan 4 gambar untuk Hal 3

    public GameObject[] pageAnim;
    public int currentPage = 1;
    public int target;
    public GameObject[] pageBtn;

    void Start()
    {
        // Set tampilan awal saat start
        UpdateSprites(currentPage);
    }

    public void NextPage()
    {
        if (!isMoving && currentPage < 3)
        {
            currentPage++;
            StartCoroutine(TurnPage(1));
        }
    }

    public void GoToPage(int targetPage)
    {
        if(currentPage != targetPage)
        {
            target = targetPage;
            if(currentPage > targetPage)
            {
                PrevPage();
            } else if(currentPage < target)
            {
                NextPage();
            }
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

    public IEnumerator TurnPage(int dir)
    {
        yield return new WaitForSeconds(0.1f);
        isMoving = true;
        UpdateSprites(currentPage);
        // Pilih animator berdasarkan arah (0 = Prev, 1 = Next)
        int animIndex = (dir == 1) ? 0 : 1;
        string animName = (dir == 1) ? "TurnRight" : "TurnLeft";

        Animator anim = pageAnim[animIndex].GetComponent<Animator>();
        Canvas canvas = pageAnim[animIndex].GetComponent<Canvas>();

        canvas.sortingOrder = 2;
        anim.Play(animName);

        // Tunggu animasi selesai (sesuaikan dengan durasi animasi kamu)
        yield return new WaitForSeconds(0.49f);

        // Update gambar SETELAH/SAAT animasi berjalan agar pergantian halus

        canvas.sortingOrder = 1;
        isMoving = false;
        if(currentPage != target)
        {
            Debug.Log("aaa" + target);
            GoToPage(target);
        }
        
    }

    // Fungsi khusus untuk mengganti sprite berdasarkan halaman
    void UpdateSprites(int page)
    {
        Sprite[] selectedPanorama;

        // Tentukan array mana yang dipakai
        switch (page)
        {
            case 1: selectedPanorama = panorama1; break;
            case 2: selectedPanorama = panorama2; break;
            case 3: selectedPanorama = panorama3; break;
            default: return;
        }

        // Masukkan sprite ke UI (Pastikan isi array panorama selalu ada 4)
        for (int i = 0; i < gambarUI.Length; i++)
        {
            if (i < selectedPanorama.Length)
            {
                gambarUI[i].sprite = selectedPanorama[i];
            }
        }
    }

    public IEnumerator UpdateSprites2(int page, int dir)
    {
       
        Sprite[] selectedPanorama;


        // Tentukan array mana yang dipakai
        switch (page)
        {
            case 1: selectedPanorama = panorama1; break;
            case 2: selectedPanorama = panorama2; break;
            case 3: selectedPanorama = panorama3; break;
            default: yield break;
        }

        if (dir == 1)
        {
             pageBtn[0].SetActive(false);
            gambarAnim[0].sprite = selectedPanorama[3];
            gambarAnim[1].sprite = selectedPanorama[2];
            gambarAnim[0].rectTransform.localScale = new Vector3(1, 1, 1);
            gambarAnim[1].rectTransform.localScale = new Vector3(1, 1, 1);
        }
        else if (dir == 0)
        {
            pageBtn[1].SetActive(true);
            gambarAnim[3].rectTransform.localScale = new Vector3(-1, 1, 1);
            gambarAnim[2].rectTransform.localScale = new Vector3(-1, 1, 1);
            gambarAnim[3].sprite = selectedPanorama[0];
            gambarAnim[2].sprite = selectedPanorama[1];
        }

        yield return new WaitForSeconds(0.25f);

        gambarAnim[0].rectTransform.localScale = new Vector3(-1, 1, 1);
        gambarAnim[1].rectTransform.localScale = new Vector3(-1, 1, 1);
        gambarAnim[2].rectTransform.localScale = new Vector3(1, 1, 1);
        gambarAnim[3].rectTransform.localScale = new Vector3(1, 1, 1);

        // Masukkan sprite ke UI (Pastikan isi array panorama selalu ada 4)
        for (int i = 0; i < gambarAnim.Length; i++)
        {
            if (i < selectedPanorama.Length)
            {
                gambarAnim[i].sprite = selectedPanorama[i];
            }
        }
        pageBtn[1].SetActive(false);
        pageBtn[0].SetActive(true);
    }
}






/**
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanoramaBook : MonoBehaviour
{
    private bool isMoving;
    public Image[] gambarUI; 
    public Image[] gambarAnim;

    [Header("Sprites Konfigurasi")]
    public Sprite[] panorama1; 
    public Sprite[] panorama2; 
    public Sprite[] panorama3; 

    public GameObject[] pageAnim;
    public int currentPage = 1;
    public int target; // Sekarang digunakan sebagai penampung target halaman

    void Start()
    {
        target = currentPage;
        UpdateSprites(currentPage);
    }

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
        target = targetPage;
        if (!isMoving)
        {
            if (currentPage < target)
            {
                NextPage();
            }
            else if (currentPage > target)
            {
                PrevPage();
            }
        }
    }

    public IEnumerator TurnPage(int dir)
    {
        isMoving = true;

        // 1. Siapkan gambar di lembaran animasi dulu sebelum bergerak
        // Kita panggil UpdateSprites2 di sini
        yield return StartCoroutine(UpdateSprites2(currentPage, dir));

        // 2. Tentukan animator (0 = Next, 1 = Prev)
        int animIndex = (dir == 1) ? 0 : 1;
        string animName = (dir == 1) ? "TurnRight" : "TurnLeft";

        Animator anim = pageAnim[animIndex].GetComponent<Animator>();
        Canvas canvas = pageAnim[animIndex].GetComponent<Canvas>();

        canvas.sortingOrder = 10;
        anim.Play(animName, 0, 0f);

        // 3. Tunggu sampai pertengahan animasi (saat lembaran tegak lurus)
        // Baru ganti Background UI di belakangnya agar terlihat mulus
        yield return new WaitForSeconds(0.25f); 
        UpdateSprites(currentPage);

        // 4. Tunggu sampai animasi benar-benar selesai
        yield return new WaitForSeconds(0.25f);

        canvas.sortingOrder = 1;
        isMoving = false;

        // 5. Logika GoToPage: Jika belum sampai target, lanjut pindah halaman
        if (currentPage != target)
        {
            GoToPage(target);
        }
    }

    // Tetap ada sesuai permintaan, hanya diperbaiki logika scale-nya
    void UpdateSprites(int page)
    {
        Sprite[] selectedPanorama = GetSelectedArray(page);
        if (selectedPanorama == null) return;

        for (int i = 0; i < gambarUI.Length; i++)
        {
            if (i < selectedPanorama.Length)
            {
                gambarUI[i].sprite = selectedPanorama[i];
            }
        }
    }

    // Tetap ada sesuai permintaan, menyesuaikan tampilan lembaran yang sedang terbang
    public IEnumerator UpdateSprites2(int page, int dir)
    {
        Sprite[] selectedPanorama = GetSelectedArray(page);
        if (selectedPanorama == null) yield break;

        // Reset scale standar
        for (int i = 0; i < gambarAnim.Length; i++) 
            gambarAnim[i].rectTransform.localScale = Vector3.one;

        if (dir == 1) // Ke Kanan (Next)
        {
            // Lembaran yang terbang mengambil gambar dari halaman sebelumnya (sisi belakang)
            // dan halaman baru (sisi depan). Sesuaikan index sesuai desain prefab kamu.
            gambarAnim[0].sprite = selectedPanorama[0]; 
            gambarAnim[1].sprite = selectedPanorama[1];
        }
        else if (dir == 0) // Ke Kiri (Prev)
        {
            gambarAnim[2].sprite = selectedPanorama[2];
            gambarAnim[3].sprite = selectedPanorama[3];
        }

        yield return null; 
    }

    // Fungsi pembantu agar kode lebih bersih
    private Sprite[] GetSelectedArray(int page)
    {
        switch (page)
        {
            case 1: return panorama1;
            case 2: return panorama2;
            case 3: return panorama3;
            default: return null;
        }
    }
}
*/
