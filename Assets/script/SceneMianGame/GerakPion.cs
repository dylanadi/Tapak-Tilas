using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

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
    public GameObject indikatorMilik; // 🔥 ICON DI ATAS KEPALA

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

        // 🔥 Aktifkan indikator kalau ini milik player sendiri
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
    // 🔥 MOVE KE NODE (SUDAH DI BATASI TURN + OWNERSHIP)
    // =========================
    public void MoveToNode(StopNode targetNode)
    {
        // ❌ Bukan punya player ini
        if (!photonView.IsMine)
        {
            Debug.Log("Bukan karakter kamu!");
            return;
        }

        // ❌ Bukan giliran
        if (!GameManager.Instance.IsMyTurn(playerData))
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

        StartCoroutine(JalanKeNode(targetNode));
    }

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
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    tujuan,
                    speed * Time.deltaTime
                );

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

        // 🔥 SAMPAI
        transform.position = targetPos;

        currentNode = target;
        currentNode.AddPlayer(gameObject);

        if (playerData != null)
        {
            playerData.currentNodeIndex = target.nodeID;
        }

        Debug.Log("Berhenti di node ID: " + target.nodeID);

        HandleNodeEvent(target);

        // 🔥 PINDAH GILIRAN
        GameManager.Instance.NextTurn();

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
        switch (node.nodeID)
        {
            case 0: Debug.Log("Heal Area"); break;
            case 1: Debug.Log("Shop Area"); break;
            case 2: Debug.Log("Event Area"); break;
            default: Debug.Log("Node biasa"); break;
        }
    }

    public bool IsMoving()
    {
        return sedangGerak;
    }
}
