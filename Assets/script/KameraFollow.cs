using UnityEngine;
using UnityEngine.EventSystems;

public class KameraFollow : MonoBehaviour
{
    public static KameraFollow Instance;

    [Header("Fokus Target")]
    [Tooltip("Tarik objek TitikTengah map ke sini sebagai cadangan jika player belum spawn")]
    public Transform targetSelection;

    [Header("Posisi & Sudut")]
    public float kemiringanKamera = 45f;
    public float rotasiHorizontal = 45f;

    [Header("Zoom Settings")]
    public float zoomMin = 5f;
    public float zoomMax = 50f;
    public float tinggiSekarang = 15f;
    [Tooltip("Kamera akan otomatis Lerp ke angka ini saat karakter berjalan")]
    public float tinggiSaatJalan = 13.6f;

    [Header("Kontrol Geser (Manual)")]
    public float panSensitivity = 1.0f;

    [Header("Transisi Fokus (Auto-Lock)")]
    public float kecepatanFokus = 10f;
    public bool kameraAktif;

    private Vector3 offsetPan = Vector3.zero;
    private Vector3 lastTargetPos;
    private Transform targetOtomatis;

    // 🔥 TAMBAHAN: Variabel untuk Mode Event (UI Pasar)
    private bool sedangModeEvent = false;
    private Quaternion rotasiSebelumModeEvent;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateTarget();
        if (targetOtomatis) lastTargetPos = targetOtomatis.position;
        kameraAktif = true;
    }

    void Update()
    {
        // 🔥 JIKA DALAM MODE PASAR/EVENT, HENTIKAN SEMUA LOGIKA UPDATE KAMERA
        if (sedangModeEvent) return;

        UpdateTarget();

        if (!targetOtomatis) return;

        bool isMouseOverUI = false;
        if (EventSystem.current != null)
        {
            isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
        }

        if (kameraAktif)
        {
            if (!isMouseOverUI)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    tinggiSekarang -= scroll * 12f;
                }
            }

            if (Vector3.Distance(targetOtomatis.position, lastTargetPos) > 0.01f)
            {
                tinggiSekarang = Mathf.Lerp(tinggiSekarang, tinggiSaatJalan, Time.deltaTime * kecepatanFokus);
                offsetPan = Vector3.Lerp(offsetPan, Vector3.zero, Time.deltaTime * kecepatanFokus);
            }

            tinggiSekarang = Mathf.Clamp(tinggiSekarang, zoomMin, zoomMax);
            float zoomPercent = (tinggiSekarang - zoomMin) / (zoomMax - zoomMin);

            if (!isMouseOverUI)
            {
                if (Input.GetMouseButton(1))
                {
                    float moveX = Input.GetAxis("Mouse X") * panSensitivity;
                    float moveZ = Input.GetAxis("Mouse Y") * panSensitivity;

                    Vector3 inputMouse = new Vector3(moveX, 0, moveZ);
                    Vector3 inputDisesuaikan = Quaternion.Euler(0, rotasiHorizontal, 0) * inputMouse;

                    offsetPan -= inputDisesuaikan * (1f + zoomPercent * 2f);
                }
            }
        }

        lastTargetPos = targetOtomatis.position;
    }

    void LateUpdate()
    {
        // 🔥 JIKA DALAM MODE PASAR/EVENT, HENTIKAN KAMERA MENGIKUTI PLAYER
        if (sedangModeEvent) return;

        if (!targetOtomatis) return;

        transform.rotation = Quaternion.Euler(kemiringanKamera, rotasiHorizontal, 0);

        float angleRad = kemiringanKamera * Mathf.Deg2Rad;
        float jarakMundurHorizontal = tinggiSekarang / Mathf.Tan(angleRad);

        Vector3 arahMundurDiagonal = transform.rotation * Vector3.back;
        arahMundurDiagonal.y = 0;
        Vector3 posisiOffsetMundur = arahMundurDiagonal.normalized * jarakMundurHorizontal;

        Vector3 posisiDasar = targetOtomatis.position + posisiOffsetMundur + (Vector3.up * tinggiSekarang);
        Vector3 posisiTujuan = posisiDasar + offsetPan;

        transform.position = Vector3.Lerp(transform.position, posisiTujuan, Time.deltaTime * kecepatanFokus);
    }

    void UpdateTarget()
    {
        if (GameManager.Instance != null)
        {
            PlayerData currentPlayer = GameManager.Instance.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                targetOtomatis = currentPlayer.transform;
                return;
            }
        }

        if (targetSelection != null)
        {
            targetOtomatis = targetSelection;
        }
    }

    public void FokusKePosisi(Vector3 posisiTujuanFokus, float tinggiTarget)
    {
        if (targetOtomatis != null)
        {
            tinggiSekarang = tinggiTarget;
            Vector3 arahGeser = posisiTujuanFokus - targetOtomatis.position;
            arahGeser.y = 0;
            offsetPan = arahGeser;
        }
    }

    // ==========================================
    // 🔥 FUNGSI BARU KHUSUS EVENT TOKO/PASAR
    // ==========================================
    public void MasukModeKameraEvent(Transform titikKameraTujuan)
    {
        if (!sedangModeEvent)
        {
            // Simpan rotasi sebelum diubah
            rotasiSebelumModeEvent = transform.rotation;
        }

        sedangModeEvent = true;

        // Pindahkan kamera ke posisi target Empty GameObject
        transform.position = titikKameraTujuan.position;
        // Set rotasi jadi X:0, Y:0, Z:0 persis seperti maumu
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void KeluarModeKameraEvent()
    {
        sedangModeEvent = false;

        // Kembalikan ke rotasi awal (posisi akan otomatis kembali ke atas player lewat LateUpdate)
        transform.rotation = rotasiSebelumModeEvent;
    }
}