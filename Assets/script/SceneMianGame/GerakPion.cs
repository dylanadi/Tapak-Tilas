using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerData))]
public class GerakPion : MonoBehaviour
{
    [Header("Referensi")]
    public LineRenderer jalur;

    [Header("Movement")]
    public float speed = 5f;
    public float rotasiSpeed = 10f;

    [Header("Waddle (Goyang)")]
    public float goyangSpeed = 6f;
    public float goyangAmount = 6f;

    [Header("Offset (Anti Nancep Tanah)")]
    public float yOffset = 0.5f;

    private List<Vector3> pathPoints = new List<Vector3>();
    private bool sedangGerak = false;

    private StopNode currentNode;
    private PlayerData playerData;

    void Start()
    {
        playerData = GetComponent<PlayerData>();

        if (jalur == null)
        {
            Debug.LogError("Jalur belum diisi!");
            return;
        }

        AmbilJalur();
    }

    // =========================
    // 🔥 AMBIL DATA JALUR
    // =========================
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
    // 🔥 MOVE KE NODE
    // =========================
    public void MoveToNode(StopNode targetNode)
    {
        if (sedangGerak) return;

        if (targetNode.IsFull())
        {
            Debug.Log("Node penuh!");
            return;
        }

        StartCoroutine(JalanKeNode(targetNode));
    }

    // =========================
    // 🔥 CORE GERAK
    // =========================
    IEnumerator JalanKeNode(StopNode target)
    {
        sedangGerak = true;

        if (currentNode != null)
            currentNode.RemovePlayer(gameObject);

        int startIndex = CariIndexTerdekat(transform.position);

        Vector3 targetPos = target.transform.position;
        targetPos.y += yOffset;

        int targetIndex = CariIndexTerdekat(targetPos);
        int step = (targetIndex > startIndex) ? 1 : -1;

        for (int i = startIndex; i != targetIndex; i += step)
        {
            Vector3 tujuan = pathPoints[i + step];

            while (Vector3.Distance(transform.position, tujuan) > 0.05f)
            {
                // 🔥 MOVE
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    tujuan,
                    speed * Time.deltaTime
                );

                // 🔥 ROTASI + GOYANG
                Vector3 arah = (tujuan - transform.position).normalized;

                if (arah != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(arah);

                    float goyang = Mathf.Sin(Time.time * goyangSpeed) * goyangAmount;

                    Quaternion finalRot = Quaternion.Euler(
                        0,
                        rot.eulerAngles.y,
                        goyang
                    );

                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        finalRot,
                        Time.deltaTime * rotasiSpeed
                    );
                }

                yield return null;
            }
        }

        // =========================
        // 🔥 SAMPAI DI NODE
        // =========================
        transform.position = targetPos;

        currentNode = target;
        currentNode.AddPlayer(gameObject);

        // 🔥 UPDATE NODE INDEX KE PLAYER DATA
        if (playerData != null)
        {
            playerData.currentNodeIndex = target.nodeID;
        }

        Debug.Log("Berhenti di node ID: " + target.nodeID);

        HandleNodeEvent(target);

        // 🔥 UPDATE TURN SYSTEM
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateTurn();
        }

        sedangGerak = false;
    }

    // =========================
    // 🔍 CARI TITIK TERDEKAT DI JALUR
    // =========================
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

    // =========================
    // 🎯 EVENT NODE
    // =========================
    void HandleNodeEvent(StopNode node)
    {
        switch (node.nodeID)
        {
            case 0:
                Debug.Log("Heal Area");
                break;

            case 1:
                Debug.Log("Shop Area");
                break;

            case 2:
                Debug.Log("Event Area");
                break;

            default:
                Debug.Log("Node biasa");
                break;
        }
    }

    // =========================
    // 🔒 CEK STATUS
    // =========================
    public bool IsMoving()
    {
        return sedangGerak;
    }
}


//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class GerakPion : MonoBehaviour
//{
//    [Header("Referensi")]
//    public LineRenderer jalur;

//    [Header("Movement")]
//    public float speed = 5f;
//    public float rotasiSpeed = 10f;

//    [Header("Setup Posisi (Anti-Mendelep)")]
//    [Tooltip("Tambahkan nilai ini jika kaki pion tenggelam ke tanah")]
//    public float yOffset = 0.5f; // 🔥 SESUAIKAN NILAI INI DI INSPECTOR

//    private List<Vector3> pathPoints = new List<Vector3>();
//    private bool sedangGerak = false;
//    private StopNode currentNode;

//    void Start()
//    {
//        AmbilJalur();
//    }

//    void AmbilJalur()
//    {
//        pathPoints.Clear();
//        for (int i = 0; i < jalur.positionCount; i++)
//        {
//            // Ambil posisi dari LineRenderer
//            Vector3 point = jalur.GetPosition(i);
//            // Tambahkan offset Y agar saat "tracing" jalur, pion tetap di atas
//            point.y += yOffset;
//            pathPoints.Add(point);
//        }
//    }

//    public void MoveToNode(StopNode targetNode)
//    {
//        if (sedangGerak) return;

//        if (targetNode.IsFull())
//        {
//            Debug.Log("Node penuh!");
//            return;
//        }

//        StartCoroutine(JalanKeNode(targetNode));
//    }

//    IEnumerator JalanKeNode(StopNode target)
//    {
//        sedangGerak = true;

//        if (currentNode != null)
//            currentNode.RemovePlayer(gameObject);

//        int startIndex = CariIndexTerdekat(transform.position);

//        // Target posisi node juga ditambah offset biar nggak nancep ke tanah di akhir
//        Vector3 targetPosWithOffset = target.transform.position;
//        targetPosWithOffset.y += yOffset;

//        int targetIndex = CariIndexTerdekat(targetPosWithOffset);
//        int step = (targetIndex > startIndex) ? 1 : -1;

//        for (int i = startIndex; i != targetIndex; i += step)
//        {
//            Vector3 tujuan = pathPoints[i + step];

//            while (Vector3.Distance(transform.position, tujuan) > 0.05f)
//            {
//                transform.position = Vector3.MoveTowards(
//                    transform.position,
//                    tujuan,
//                    speed * Time.deltaTime
//                );

//                Vector3 arah = (tujuan - transform.position).normalized;

//                if (arah != Vector3.zero)
//                {
//                    Quaternion rot = Quaternion.LookRotation(arah);
//                    float goyang = Mathf.Sin(Time.time * 10f) * 10f;

//                    Quaternion finalRot = Quaternion.Euler(
//                        0,
//                        rot.eulerAngles.y,
//                        goyang
//                    );

//                    transform.rotation = Quaternion.Slerp(
//                        transform.rotation,
//                        finalRot,
//                        Time.deltaTime * rotasiSpeed
//                    );
//                }
//                yield return null;
//            }
//        }

//        // --- FIX DI SINI ---
//        // Sampai di node akhir dengan tetap menjaga ketinggian offset
//        transform.position = targetPosWithOffset;

//        currentNode = target;
//        currentNode.AddPlayer(gameObject);

//        Debug.Log("Berhenti di node ID: " + target.nodeID);
//        HandleNodeEvent(target);

//        sedangGerak = false;
//    }

//    int CariIndexTerdekat(Vector3 posisi)
//    {
//        float jarakTerdekat = Mathf.Infinity;
//        int index = 0;

//        for (int i = 0; i < pathPoints.Count; i++)
//        {
//            float jarak = Vector3.Distance(posisi, pathPoints[i]);
//            if (jarak < jarakTerdekat)
//            {
//                jarakTerdekat = jarak;
//                index = i;
//            }
//        }
//        return index;
//    }

//    void HandleNodeEvent(StopNode node)
//    {
//        // ... (kode event kamu tetap sama)
//        switch (node.nodeID)
//        {
//            case 0: Debug.Log("Heal Area"); break;
//            case 1: Debug.Log("Shop Area"); break;
//            case 2: Debug.Log("Event Area"); break;
//            default: Debug.Log("Node biasa"); break;
//        }
//    }
//}