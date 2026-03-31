using UnityEngine;

public class KameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Sudut Kamera")]
    public float kemiringanKamera = 60f; // Sudut miring ke bawah

    [Header("Zoom Settings")]
    public float zoomMin = 5f;
    public float zoomMax = 40f;
    public float tinggiSekarang = 15f;

    [Header("Kontrol Geser")]
    public float panSensitivity = 1.2f;
    public float kecepatanFokus = 10f;

    private Vector3 offsetPan = Vector3.zero;
    private Vector3 lastTargetPos;

    void Start()
    {
        // Kunci rotasi agar selalu konsisten
        transform.rotation = Quaternion.Euler(kemiringanKamera, 0, 0);
        if (target) lastTargetPos = target.position;
    }

    void Update()
    {
        if (!target) return;

        // 1. ZOOM LOGIC
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        tinggiSekarang -= scroll * 12f;
        tinggiSekarang = Mathf.Clamp(tinggiSekarang, zoomMin, zoomMax);

        // Hitung persentase zoom untuk kontrol geser
        float zoomPercent = (tinggiSekarang - zoomMin) / (zoomMax - zoomMin);

        // 2. GESER MANUAL (Klik Kanan)
        if (Input.GetMouseButton(1))
        {
            float moveX = Input.GetAxis("Mouse X") * panSensitivity;
            float moveZ = Input.GetAxis("Mouse Y") * panSensitivity;
            // Di Zoom Out, geser terasa lebih luas
            offsetPan -= new Vector3(moveX, 0, moveZ) * (1f + zoomPercent * 2f);
        }

        // 3. AUTO-LOCK SAAT JALAN
        if (Vector3.Distance(target.position, lastTargetPos) > 0.01f)
        {
            // Reset geseran tangan dengan cepat saat pion bergerak
            offsetPan = Vector3.Lerp(offsetPan, Vector3.zero, Time.deltaTime * kecepatanFokus);
        }

        lastTargetPos = target.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- RUMUS MATEMATIKA AGAR PION DI TENGAH ---
        // Kita butuh jarak mundur (Z) yang pas berdasarkan tinggi (Y) dan sudut (Rotation)
        // Rumus: Z_offset = Tinggi / Tan(Sudut)
        float angleRad = kemiringanKamera * Mathf.Deg2Rad;
        float jarakMundurZ = tinggiSekarang / Mathf.Tan(angleRad);

        // Tentukan posisi dasar agar pion tepat di titik tengah layar
        Vector3 posisiDasar = new Vector3(target.position.x, tinggiSekarang, target.position.z - jarakMundurZ);

        // Tambahkan offset manual hasil geser tangan
        Vector3 posisiTujuan = posisiDasar + offsetPan;

        // Terapkan posisi (Gunakan Lerp cepat agar smooth tapi responsif)
        transform.position = Vector3.Lerp(transform.position, posisiTujuan, Time.deltaTime * kecepatanFokus);
    }
}