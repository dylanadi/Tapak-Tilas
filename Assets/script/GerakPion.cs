using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GerakPion : MonoBehaviour
{
    [Header("Referensi Jalur")]
    public LineRenderer jalurLine;

    [Header("Pengaturan Gerak")]
    public float kecepatan = 5f;
    public float rotasiSpeed = 10f;

    [Header("Data Pemberhentian (Otomatis)")]
    public List<int> posPemberhentian = new List<int>();

    [Header("Efek Goyang (Waddle)")]
    public float goyangSpeed = 12f;
    public float goyangAmount = 10f;

    private int indeksSekarang = 0;
    private bool sedangGerak = false;
    private Vector3[] titikJalur;

    void Start()
    {
        UpdateJalurData();
    }

    public void UpdateJalurData()
    {
        if (jalurLine != null && jalurLine.positionCount > 0)
        {
            titikJalur = new Vector3[jalurLine.positionCount];
            jalurLine.GetPositions(titikJalur);
            transform.position = titikJalur[0];
        }
    }

    void Update()
    {
        // Tekan Spasi untuk mulai atau lanjut dari POS
        if (Input.GetKeyDown(KeyCode.Space) && !sedangGerak && titikJalur != null)
        {
            StartCoroutine(JalanKePosBerikutnya());
        }
    }

    IEnumerator JalanKePosBerikutnya()
    {
        sedangGerak = true;

        while (indeksSekarang < titikJalur.Length - 1)
        {
            indeksSekarang++;
            Vector3 tujuan = titikJalur[indeksSekarang];

            // Gerak antar titik
            while (Vector3.Distance(transform.position, tujuan) > 0.02f)
            {
                // Move
                transform.position = Vector3.MoveTowards(transform.position, tujuan, kecepatan * Time.deltaTime);

                // Rotation + Goyang
                Vector3 arah = (tujuan - transform.position).normalized;
                if (arah != Vector3.zero)
                {
                    Quaternion rotasiTujuan = Quaternion.LookRotation(arah);
                    float goyang = Mathf.Sin(Time.time * goyangSpeed) * goyangAmount;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.Euler(0, rotasiTujuan.eulerAngles.y, goyang),
                        Time.deltaTime * rotasiSpeed);
                }
                yield return null;
            }

            // Cek apakah titik ini adalah POS (Klik Kiri)
            if (posPemberhentian.Contains(indeksSekarang))
            {
                Debug.Log("Berhenti di POS. Tekan Spasi lagi untuk lanjut!");
                // Tegakkan badan pion saat berhenti
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
                sedangGerak = false;
                yield break; // Keluar dari coroutine, tunggu input spasi lagi
            }
        }

        sedangGerak = false;
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        Debug.Log("Sampai di akhir jalur.");
    }

    // Visualisasi Titik POS di Scene View (Bola Merah)
    void OnDrawGizmos()
    {
        if (jalurLine == null) return;

        // Update titik jika sedang menggambar di editor
        Vector3[] tempPoints = new Vector3[jalurLine.positionCount];
        jalurLine.GetPositions(tempPoints);

        Gizmos.color = Color.red;
        foreach (int idx in posPemberhentian)
        {
            if (idx < tempPoints.Length)
            {
                Gizmos.DrawSphere(tempPoints[idx], 0.4f);
            }
        }
    }
}