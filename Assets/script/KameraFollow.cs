using UnityEngine;

public class KameraFollow : MonoBehaviour
{
    [Header("Fokus Target")]
    [Tooltip("Tarik objek TitikTengah map ke sini sebagai cadangan jika player belum spawn")]
    public Transform targetSelection;

    [Header("Posisi & Sudut")]
    public float kemiringanKamera = 45f;
    public float rotasiHorizontal = 45f;

    [Header("Zoom Settings")]
    public float zoomMin = 5f;
    public float zoomMax = 40f;
    public float tinggiSekarang = 15f;
    [Tooltip("Kamera akan otomatis Lerp ke angka ini saat karakter berjalan")]
    public float tinggiSaatJalan = 13.6f;

    [Header("Kontrol Geser (Manual)")]
    public float panSensitivity = 1.0f;

    [Header("Transisi Fokus (Auto-Lock)")]
    public float kecepatanFokus = 10f;

    private Vector3 offsetPan = Vector3.zero;
    private Vector3 lastTargetPos;
    private Transform targetOtomatis;

    void Start()
    {
        // Langsung cari target pas awal game
        UpdateTarget();
        if (targetOtomatis) lastTargetPos = targetOtomatis.position;
    }

    void Update()
    {
        // Cek terus siapa yang lagi dapet giliran (biar ganti giliran kamera otomatis pindah)
        UpdateTarget();

        if (!targetOtomatis) return;

        // 1. INPUT ZOOM MANUAL
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            tinggiSekarang -= scroll * 12f;
        }

        // 2. LOGIKA AUTO-ZOOM & AUTO-FOLLOW (PAS JALAN)
        if (Vector3.Distance(targetOtomatis.position, lastTargetPos) > 0.01f)
        {
            // 🔥 Lerp ketinggian ke 13.6 secara halus saat bergerak
            tinggiSekarang = Mathf.Lerp(tinggiSekarang, tinggiSaatJalan, Time.deltaTime * kecepatanFokus);

            // 🔥 Reset geseran manual agar kamera kembali mengunci ke tengah karakter
            offsetPan = Vector3.Lerp(offsetPan, Vector3.zero, Time.deltaTime * kecepatanFokus);
        }

        tinggiSekarang = Mathf.Clamp(tinggiSekarang, zoomMin, zoomMax);
        float zoomPercent = (tinggiSekarang - zoomMin) / (zoomMax - zoomMin);

        // 3. INPUT GESER MANUAL (Klik Kanan)
        if (Input.GetMouseButton(1))
        {
            float moveX = Input.GetAxis("Mouse X") * panSensitivity;
            float moveZ = Input.GetAxis("Mouse Y") * panSensitivity;

            Vector3 inputMouse = new Vector3(moveX, 0, moveZ);
            Vector3 inputDisesuaikan = Quaternion.Euler(0, rotasiHorizontal, 0) * inputMouse;

            offsetPan -= inputDisesuaikan * (1f + zoomPercent * 2f);
        }

        lastTargetPos = targetOtomatis.position;
    }

    void LateUpdate()
    {
        if (!targetOtomatis) return;

        // Kunci Rotasi
        transform.rotation = Quaternion.Euler(kemiringanKamera, rotasiHorizontal, 0);

        // Hitung jarak mundur kamera berdasarkan tinggi
        float angleRad = kemiringanKamera * Mathf.Deg2Rad;
        float jarakMundurHorizontal = tinggiSekarang / Mathf.Tan(angleRad);

        Vector3 arahMundurDiagonal = transform.rotation * Vector3.back;
        arahMundurDiagonal.y = 0;
        Vector3 posisiOffsetMundur = arahMundurDiagonal.normalized * jarakMundurHorizontal;

        // Hitung posisi akhir
        Vector3 posisiDasar = targetOtomatis.position + posisiOffsetMundur + (Vector3.up * tinggiSekarang);
        Vector3 posisiTujuan = posisiDasar + offsetPan;

        // Gerakkan kamera secara halus
        transform.position = Vector3.Lerp(transform.position, posisiTujuan, Time.deltaTime * kecepatanFokus);
    }

    void UpdateTarget()
    {
        // 1. Coba cari siapa player yang dapet giliran sekarang
        if (GameManager.Instance != null)
        {
            PlayerData currentPlayer = GameManager.Instance.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                targetOtomatis = currentPlayer.transform;
                return; // Berhasil dapet player, keluar dari fungsi
            }
        }

        // 2. Kalau player belum ada (loading/spawn), fokus ke Titik Tengah map
        if (targetSelection != null)
        {
            targetOtomatis = targetSelection;
        }
    }
}