using UnityEngine;

public class KameraFollow : MonoBehaviour
{
    [Header("Fokus Target")]
    public Transform target;          // Ini untuk Pion (diisi otomatis nanti)
    public Transform targetSelection; // Tarik objek 'TitikTengah' ke sini di Inspector
    public bool sedangMemilih = true; // Centang ini agar kamera fokus ke karakter saat start

    [Header("Posisi & Sudut")]
    public float kemiringanKamera = 60f;
    public float rotasiHorizontal = 45f;

    [Header("Zoom Settings")]
    public float zoomMin = 5f;
    public float zoomMax = 40f;
    public float tinggiSekarang = 15f;

    [Header("Kontrol Geser (Manual)")]
    public float panSensitivity = 1.0f;

    [Header("Transisi Fokus (Auto-Lock)")]
    public float kecepatanFokus = 10f;

    private Vector3 offsetPan = Vector3.zero;
    private Vector3 lastTargetPos;

    void Start()
    {
        // Inisialisasi posisi awal agar tidak lompat kaget
        Transform targetAktif = sedangMemilih ? targetSelection : target;
        if (targetAktif) lastTargetPos = targetAktif.position;
    }

    void Update()
    {
        Transform targetAktif = sedangMemilih ? targetSelection : target;
        if (!targetAktif) return;

        // 1. INPUT ZOOM
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        tinggiSekarang -= scroll * 12f;
        tinggiSekarang = Mathf.Clamp(tinggiSekarang, zoomMin, zoomMax);

        float zoomPercent = (tinggiSekarang - zoomMin) / (zoomMax - zoomMin);

        // 2. INPUT GESER MANUAL (Klik Kanan)
        if (Input.GetMouseButton(1))
        {
            float moveX = Input.GetAxis("Mouse X") * panSensitivity;
            float moveZ = Input.GetAxis("Mouse Y") * panSensitivity;

            Vector3 inputMouse = new Vector3(moveX, 0, moveZ);
            Vector3 inputDisesuaikan = Quaternion.Euler(0, rotasiHorizontal, 0) * inputMouse;

            offsetPan -= inputDisesuaikan * (1f + zoomPercent * 2f);
        }

        // 3. AUTO-LOCK: Reset geseran jika target bergerak (untuk Pion)
        if (Vector3.Distance(targetAktif.position, lastTargetPos) > 0.01f)
        {
            offsetPan = Vector3.Lerp(offsetPan, Vector3.zero, Time.deltaTime * kecepatanFokus);
        }

        lastTargetPos = targetAktif.position;
    }

    void LateUpdate()
    {
        Transform targetAktif = sedangMemilih ? targetSelection : target;
        if (!targetAktif) return;

        // Kunci Rotasi Diagonal
        transform.rotation = Quaternion.Euler(kemiringanKamera, rotasiHorizontal, 0);

        // Rumus Trigonometri untuk Jarak Mundur
        float angleRad = kemiringanKamera * Mathf.Deg2Rad;
        float jarakMundurHorizontal = tinggiSekarang / Mathf.Tan(angleRad);

        // Hitung Arah Mundur Kamera
        Vector3 arahMundurDiagonal = transform.rotation * Vector3.back;
        arahMundurDiagonal.y = 0;
        Vector3 posisiOffsetMundur = arahMundurDiagonal.normalized * jarakMundurHorizontal;

        // Tentukan Posisi Akhir
        Vector3 posisiDasar = targetAktif.position + posisiOffsetMundur + (Vector3.up * tinggiSekarang);
        Vector3 posisiTujuan = posisiDasar + offsetPan;

        // Gerakkan Kamera dengan Halus
        transform.position = Vector3.Lerp(transform.position, posisiTujuan, Time.deltaTime * kecepatanFokus);
    }

    /// <summary>
    /// Panggil fungsi ini dari script lain untuk memindahkan kamera ke Pion.
    /// Contoh: KameraFollow.SelesaiMemilih(objekPion.transform);
    /// </summary>
    public void SelesaiMemilih(Transform pionBaru)
    {
        target = pionBaru;
        sedangMemilih = false;
        offsetPan = Vector3.zero; // Reset geseran agar langsung fokus ke pion
    }
}