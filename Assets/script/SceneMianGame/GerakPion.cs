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

    [Header("Indicator")]
    public GameObject indikatorMilik;

    private List<Vector3> pathPoints = new List<Vector3>();
    private bool sedangGerak = false;

    // 🔥 DIUBAH JADI PUBLIC agar bisa dicek dari luar (Anti Pindah Kursi)
    public StopNode currentNode;

    private PlayerData playerData;

    void Start()
    {
        playerData = GetComponent<PlayerData>();

        if (jalur == null) jalur = FindObjectOfType<LineRenderer>();
        if (indikatorMilik != null) indikatorMilik.SetActive(photonView.IsMine);

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

    // Kompatibilitas untuk script lama yang cuma ngirim 1 Node
    public void MoveToNode(StopNode targetTunggal)
    {
        MoveToNode(targetTunggal, targetTunggal);
    }

    // Menerima Titik Garis & Titik Parkir
    public void MoveToNode(StopNode nodeGaris, StopNode nodeParkir)
    {
        if (!photonView.IsMine) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsMyTurn(playerData)) return;
        if (sedangGerak) return;
        if (nodeParkir.IsFull()) return;

        photonView.RPC("RPC_GerakKeNode", RpcTarget.AllViaServer, nodeGaris.nodeID, nodeParkir.nodeID);
    }

    [PunRPC]
    void RPC_GerakKeNode(int garisID, int parkirID)
    {
        StopNode garis = FindObjectsOfType<StopNode>().FirstOrDefault(n => n.nodeID == garisID);
        StopNode parkir = FindObjectsOfType<StopNode>().FirstOrDefault(n => n.nodeID == parkirID);

        if (garis != null && parkir != null)
        {
            StopAllCoroutines();
            StartCoroutine(JalanKeNode(garis, parkir));
        }
    }

    IEnumerator JalanKeNode(StopNode nodeGaris, StopNode nodeParkir)
    {
        sedangGerak = true;
        AmbilJalur();

        if (currentNode != null) currentNode.RemovePlayer(gameObject);

        // Cari index garis tempat pion mulai dan tempat pion keluar
        int startIndex = CariIndexTerdekat(transform.position);

        Vector3 targetPosGaris = nodeGaris.transform.position;
        targetPosGaris.y += yOffset;
        int exitIndex = CariIndexTerdekat(targetPosGaris);

        int step = (exitIndex > startIndex) ? 1 : -1;

        // 1. NAIK KE GARIS PINK DULU
        Vector3 titikMasukGaris = pathPoints[startIndex];
        while (Vector3.Distance(transform.position, titikMasukGaris) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, titikMasukGaris, speed * Time.deltaTime);
            UpdateRotasi(titikMasukGaris);
            yield return null;
        }

        // 2. NYUSUR GARIS PINK SAMPAI TITIK NODE GARIS
        for (int i = startIndex; i != exitIndex; i += step)
        {
            int nextIndex = Mathf.Clamp(i + step, 0, pathPoints.Count - 1);
            Vector3 tujuan = pathPoints[nextIndex];

            while (Vector3.Distance(transform.position, tujuan) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, tujuan, speed * Time.deltaTime);
                UpdateRotasi(tujuan);
                yield return null;
            }
        }

        // 3. PASTIKAN BERHENTI TEPAT DI NODE GARIS (Pintu Keluar)
        while (Vector3.Distance(transform.position, targetPosGaris) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosGaris, speed * Time.deltaTime);
            UpdateRotasi(targetPosGaris);
            yield return null;
        }

        // 4. BARU DIA BELOK MENUJU PARKIRAN NODE
        Vector3 targetPosParkir = nodeParkir.transform.position;
        targetPosParkir.y += yOffset;

        while (Vector3.Distance(transform.position, targetPosParkir) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosParkir, speed * Time.deltaTime);
            UpdateRotasi(targetPosParkir);
            yield return null;
        }

        // SAMPAI TUJUAN AKHIR
        transform.position = targetPosParkir;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        currentNode = nodeParkir;
        currentNode.AddPlayer(gameObject);

        if (playerData != null) playerData.currentNodeIndex = nodeParkir.nodeID;

        HandleNodeEvent(nodeParkir);

        if (photonView.IsMine && GameManager.Instance != null) GameManager.Instance.NextTurn();

        sedangGerak = false;
    }

    void UpdateRotasi(Vector3 targetPos)
    {
        Vector3 arah = targetPos - transform.position;
        arah.y = 0;
        if (arah.magnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(arah);
            float goyang = Mathf.Sin(Time.time * goyangSpeed) * goyangAmount;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, rot.eulerAngles.y, goyang), Time.deltaTime * rotasiSpeed);
        }
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

    void HandleNodeEvent(StopNode node) { /* Event biasa */ }

    // ==========================================
    // 🔥 PEMBERSIH OTOMATIS SAAT DISCONNECT
    // ==========================================
    void OnDestroy()
    {
        if (currentNode != null)
        {
            currentNode.RemovePlayer(gameObject);
            Debug.Log($"[LALY-System] Pion hancur/keluar. Petak Node {currentNode.nodeID} kembali KOSONG!");
        }
    }
}