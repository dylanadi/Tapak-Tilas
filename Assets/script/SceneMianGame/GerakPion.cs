using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;

[RequireComponent(typeof(PlayerData))]
public class GerakPion : MonoBehaviourPun
{
    [Header("Referensi")]
    public LineRenderer jalur;

    [Header("Movement")]
    public float speed = 5f;
    public float rotasiSpeed = 10f;

    [Header("Waddle (Goyang)")]
    public float goyangSpeed = 6f;
    public float goyangAmount = 6f;

    [Header("Offset")]
    public float yOffset = 0.5f;

    [Header("Indicator (Punya Player Sendiri)")]
    public GameObject indikatorMilik;

    private List<Vector3> pathPoints = new List<Vector3>();
    private bool sedangGerak = false;

    private StopNode currentNode;
    private PlayerData playerData;

    void Start()
    {
        playerData = GetComponent<PlayerData>();

        // 🔥 FIX MULTIPLAYER: Saat prefab di-spawn lewat Photon, referensi Inspector sering hilang.
        if (jalur == null)
        {
            jalur = FindObjectOfType<LineRenderer>();
            if (jalur == null)
            {
                Debug.LogError("[GerakPion] Jalur (LineRenderer) tidak ditemukan di Scene!");
                return;
            }
        }

        if (indikatorMilik != null)
        {
            indikatorMilik.SetActive(photonView.IsMine);
        }

        AmbilJalur();
    }

    void AmbilJalur()
    {
        pathPoints.Clear();

        for (int i = 0; i < jalur.positionCount; i++)
        {
            Vector3 point = jalur.GetPosition(i);
            point.y += yOffset;
            pathPoints.Add(point);
        }
    }

    // =========================
    // 🔥 PERINTAH JALAN (DIPANGGIL LOKAL OLEH PEMILIK)
    // =========================
    public void MoveToNode(StopNode targetNode)
    {
        if (!photonView.IsMine)
        {
            Debug.Log("Bukan karakter kamu!");
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.IsMyTurn(playerData))
        {
            Debug.Log("Bukan giliran kamu!");
            return;
        }

        if (sedangGerak) return;

        if (targetNode.IsFull())
        {
            Debug.Log("Node penuh!");
            return;
        }

        // Kita JANGAN langsung jalan. Kita suruh Photon kirim pesan ke SEMUA ORANG
        photonView.RPC("RPC_GerakKeNode", RpcTarget.AllViaServer, targetNode.nodeID);
    }

    // =========================
    // 🔥 RPC (DIJALANKAN DI SEMUA LAPTOP)
    // =========================
    [PunRPC]
    void RPC_GerakKeNode(int targetNodeID)
    {
        StopNode target = FindObjectsOfType<StopNode>().FirstOrDefault(n => n.nodeID == targetNodeID);

        if (target != null)
        {
            // Pastikan coroutine lama stop dulu biar gak bentrok
            StopAllCoroutines();
            StartCoroutine(JalanKeNode(target));
        }
        else
        {
            Debug.LogError($"[GerakPion] Node dengan ID {targetNodeID} tidak ditemukan di scene!");
        }
    }

    IEnumerator JalanKeNode(StopNode target)
    {
        sedangGerak = true;

        // Refresh jalur jaga-jaga kalau ada perubahan
        AmbilJalur();

        if (currentNode != null)
            currentNode.RemovePlayer(gameObject);

        int startIndex = CariIndexTerdekat(transform.position);

        Vector3 targetPos = target.transform.position;
        targetPos.y += yOffset;

        int targetIndex = CariIndexTerdekat(targetPos);
        int step = (targetIndex > startIndex) ? 1 : -1;

        // =========================================================
        // 🔥 1. JALAN KE TITIK GARIS TERDEKAT DULU (MENCARI JALUR)
        // =========================================================
        Vector3 titikMasukGaris = pathPoints[startIndex];
        while (Vector3.Distance(transform.position, titikMasukGaris) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, titikMasukGaris, speed * Time.deltaTime);

            Vector3 arah = titikMasukGaris - transform.position;
            arah.y = 0;
            if (arah.magnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(arah);
                float goyang = Mathf.Sin(Time.time * goyangSpeed) * goyangAmount;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, rot.eulerAngles.y, goyang), Time.deltaTime * rotasiSpeed);
            }
            yield return null;
        }

        // =========================================================
        // 🔥 2. MENYUSURI GARIS (KODE ASLI KAMU)
        // =========================================================
        for (int i = startIndex; i != targetIndex; i += step)
        {
            // Pengaman index biar gak error
            int nextIndex = Mathf.Clamp(i + step, 0, pathPoints.Count - 1);
            Vector3 tujuan = pathPoints[nextIndex];

            while (Vector3.Distance(transform.position, tujuan) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, tujuan, speed * Time.deltaTime);

                Vector3 arah = tujuan - transform.position;
                arah.y = 0; // Fix biar badan gak nunduk

                if (arah.magnitude > 0.01f)
                {
                    Quaternion rot = Quaternion.LookRotation(arah);
                    float goyang = Mathf.Sin(Time.time * goyangSpeed) * goyangAmount;
                    Quaternion finalRot = Quaternion.Euler(0, rot.eulerAngles.y, goyang);

                    transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotasiSpeed);
                }

                yield return null;
            }
        }

        // =========================================================
        // 🔥 3. JALAN DARI UJUNG GARIS MENUJU TITIK TENGAH NODE
        // =========================================================
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            Vector3 arahAkhir = targetPos - transform.position;
            arahAkhir.y = 0;
            if (arahAkhir.magnitude > 0.01f)
            {
                Quaternion rot = Quaternion.LookRotation(arahAkhir);
                float goyang = Mathf.Sin(Time.time * goyangSpeed) * goyangAmount;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, rot.eulerAngles.y, goyang), Time.deltaTime * rotasiSpeed);
            }
            yield return null;
        }

        // 🔥 SAMPAI DI TUJUAN PERSIS
        transform.position = targetPos;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); // Berdiri tegak

        currentNode = target;
        currentNode.AddPlayer(gameObject);

        if (playerData != null)
        {
            playerData.currentNodeIndex = target.nodeID;
        }

        Debug.Log("Berhenti di node ID: " + target.nodeID);
        HandleNodeEvent(target);

        // 🔥 FIX NULL REFERENCE & DOUBLE TURN ERROR:
        if (photonView.IsMine)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NextTurn();
            }
            else
            {
                Debug.LogError("[GerakPion] NullReference: GameManager.Instance tidak ditemukan!");
            }
        }

        sedangGerak = false;
    }

    int CariIndexTerdekat(Vector3 posisi)
    {
        float jarakTerdekat = Mathf.Infinity;
        int index = 0;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            float jarak = Vector3.Distance(posisi, pathPoints[i]);

            if (jarak < jarakTerdekat)
            {
                jarakTerdekat = jarak;
                index = i;
            }
        }
        return index;
    }

    void HandleNodeEvent(StopNode node)
    {
        // Cek jenis objek yang ada di Node tersebut
        switch (node.jenisNode)
        {
            case InteractableObject.ObjectType.Ketapang:
                Debug.Log("Menjalankan Fungsi Panorama Ketapang...");
                // Panggil fungsi khusus Ketapang di sini
                break;

            case InteractableObject.ObjectType.Ijen:
                Debug.Log("Menjalankan Fungsi Panorama Ijen...");
                // Misal: Munculkan popup info kawah ijen
                break;

            case InteractableObject.ObjectType.Pasar:
                Debug.Log("Menjalankan Fungsi Pasar...");
                // Misal: Buka UI Toko Jajanan BWI
                break;

            case InteractableObject.ObjectType.Hujan:
                Debug.Log("Menjalankan Fungsi Hujan...");
                // Misal: Kurangi movement player berikutnya
                break;

            // Tambahkan case lainnya sesuai list 8 kategori kamu
            default:
                Debug.Log("Berhenti di Node biasa.");
                break;
        }
    }

    public bool IsMoving()
    {
        return sedangGerak;
    }
}