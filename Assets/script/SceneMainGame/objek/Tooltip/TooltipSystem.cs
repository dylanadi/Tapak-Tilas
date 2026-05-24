using UnityEngine;
using TMPro;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance;

    [Header("Referensi UI Tooltip")]
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipRect;

    [Header("Sistem Instantiate Bahan")]
    [SerializeField] private GameObject prefabDisplayBahan;
    [SerializeField] private Transform bahanContainer;

    [Header("Pengaturan Jarak (Offset)")]
    [SerializeField] private float paddingX = 10f; // Atur di Inspector (Makin kecil makin dekat dengan kursor)
    [SerializeField] private float paddingY = 10f;

    // ... (Awake biarkan sama)
    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        // 1. Cek kursor mouse sedang ada di area mana (dalam persentase 0.0 sampai 1.0)
        float persentaseX = mousePos.x / Screen.width;
        float persentaseY = mousePos.y / Screen.height;

        // 2. Tentukan Pivot secara dinamis
        // Jika kursor di kanan layar (> 0.5), set pivot X ke 1 (Kanan). Jika di kiri, set ke 0 (Kiri).
        float pivotX = persentaseX > 0.5f ? 1f : 0f;

        // Jika kursor di atas layar (> 0.5), set pivot Y ke 1 (Atas). Jika di bawah, set ke 0 (Bawah).
        float pivotY = persentaseY > 0.5f ? 1f : 0f;

        // Terapkan pivot baru ke RectTransform
        tooltipRect.pivot = new Vector2(pivotX, pivotY);

        // 3. Atur arah Jarak/Padding agar tooltip tidak menutupi ujung kursor persis
        // Kalau pivot di kiri, geser ke kanan (+). Kalau pivot di kanan, geser mundur ke kiri (-)
        float offsetX = pivotX == 0f ? paddingX : -paddingX;
        float offsetY = pivotY == 0f ? paddingY : -paddingY;

        // 4. Aplikasikan posisi akhirnya
        tooltipRect.position = new Vector3(mousePos.x + offsetX, mousePos.y + offsetY, 0);
    }

    public void Show(int idKartu, DatabaseKerajinan db)
    {
        tooltip.SetActive(true);

        foreach (Transform child in bahanContainer)
        {
            Destroy(child.gameObject);
        }

        DataKartu kartuYangDipilih = db.databaseKartu[idKartu];void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        // 1. Cek kursor mouse sedang ada di area mana (dalam persentase 0.0 sampai 1.0)
        float persentaseX = mousePos.x / Screen.width;
        float persentaseY = mousePos.y / Screen.height;

        // 2. Tentukan Pivot secara dinamis
        // Jika kursor di kanan layar (> 0.5), set pivot X ke 1 (Kanan). Jika di kiri, set ke 0 (Kiri).
        float pivotX = persentaseX > 0.5f ? 1f : 0f;
        
        // Jika kursor di atas layar (> 0.5), set pivot Y ke 1 (Atas). Jika di bawah, set ke 0 (Bawah).
        float pivotY = persentaseY > 0.5f ? 1f : 0f;

        // Terapkan pivot baru ke RectTransform
        tooltipRect.pivot = new Vector2(pivotX, pivotY);

        // 3. Atur arah Jarak/Padding agar tooltip tidak menutupi ujung kursor persis
        // Kalau pivot di kiri, geser ke kanan (+). Kalau pivot di kanan, geser mundur ke kiri (-)
        float offsetX = pivotX == 0f ? paddingX : -paddingX;
        float offsetY = pivotY == 0f ? paddingY : -paddingY;

        // 4. Aplikasikan posisi akhirnya
        tooltipRect.position = new Vector3(mousePos.x + offsetX, mousePos.y + offsetY, 0);
    }
        //tooltipText.text = kartuYangDipilih.namaKartu;

        foreach (KebutuhanBahan bahan in kartuYangDipilih.listBahan)
        {
            GameObject objekBahan = Instantiate(prefabDisplayBahan, bahanContainer);
            DisplayBahan komponenDisplay = objekBahan.GetComponent<DisplayBahan>();
            Sprite icon = db.databaseMaterial[bahan.idMaterial].iconMaterial;
            komponenDisplay.Setup(icon, bahan.jumlahButuh);
        }

        // PRO-TIP: Paksa Unity untuk menghitung ulang ukuran Content Size Fitter detik ini juga.
        // Jika tidak, ukuran 'tooltipRect.rect.width' di fungsi Update akan terlambat 1 frame 
        // dan bisa menyebabkan tooltip sedikit berkedip/glitch saat pertama kali muncul di pojok layar.
        Canvas.ForceUpdateCanvases();
    }

    public void Hide()
    {
        tooltip.SetActive(false);
    }
}