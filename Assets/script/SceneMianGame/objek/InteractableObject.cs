using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour
{
    [Header("ID Identitas Objek")]
    [Tooltip("Isi angka 1 - 14. Semua data (Tipe, Node) diambil otomatis dari MapDatabase!")]
    public int idObjek;

    [Header("Konten Popup")]
    [TextArea(3, 10)]
    public string infoText;

    [Header("Visual & Prefab")]
    public GameObject tanyaPrefab;
    public Vector3 tanyaOffset = new Vector3(0, 3.5f, 0);

    // 🔥 Data ini disembunyikan karena otomatis diisi dari MapDatabase
    [HideInInspector] public TipeObjek jenisObjek;
    [HideInInspector] public int nodeGarisID;
    [HideInInspector] public int[] nodeParkirIDs;

    private GameObject currentTanya;
    private Outline outline;
    private bool isSelected = false;
    private StopNode chosenParkirNode;
    private bool dataSudahDiambil = false;

    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineWidth = 5f;
            outline.OutlineColor = Color.yellow;
        }

        // Tunda ambil data sedikit biar MapDatabase selesai loading duluan
        Invoke("AmbilDataDariDatabase", 0.5f);
    }

    void AmbilDataDariDatabase()
    {
        if (MapDatabase.Instance != null)
        {
            DataObjekMap dataKu = MapDatabase.Instance.AmbilData(idObjek);
            if (dataKu != null)
            {
                // Ambil semua data penting dari Buku Induk
                jenisObjek = dataKu.jenisObjek;
                nodeGarisID = dataKu.nodeGarisID;
                nodeParkirIDs = dataKu.nodeParkirIDs;

                dataSudahDiambil = true;
                Debug.Log($"[MapDatabase] Objek {gameObject.name} (ID: {idObjek} - {jenisObjek}) berhasil mengambil data!");
            }
            else
            {
                Debug.LogError($"[MapDatabase] ❌ ID Objek {idObjek} tidak ditemukan di Buku Induk!");
            }
        }
    }

    void OnMouseDown()
    {
        // Cegah klik kalau data belum siap atau terhalang UI
        if (!dataSudahDiambil) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Cari Player Lokal
        PlayerData localPlayer = FindObjectsOfType<PlayerData>()
            .FirstOrDefault(p => p.photonView != null && p.photonView.IsMine);

        if (localPlayer == null) return;

        // 🔥 CEK ANTI PINDAH KURSI 🔥
        GerakPion pionLokal = localPlayer.GetComponent<GerakPion>();
        if (pionLokal != null && pionLokal.currentNode != null)
        {
            if (pionLokal.currentNode.nodeID == nodeGarisID || (nodeParkirIDs != null && nodeParkirIDs.Contains(pionLokal.currentNode.nodeID)))
            {
                Debug.LogWarning("[LALY-System] Kamu sudah berada di dalam objek ini!");
                if (PopupManager.Instance != null) PopupManager.Instance.ShowPopup("Kamu sudah berada di sini!");
                return; // Tolak kliknya
            }
        }

        // Cari parkiran kosong
        chosenParkirNode = CariParkiranKosong();

        if (chosenParkirNode == null)
        {
            Debug.LogWarning($"<color=red>[LALY-System] Semua parkiran di {gameObject.name} sudah FULL!</color>");
            return;
        }

        // Cek Giliran
        if (GameManager.Instance != null && !GameManager.Instance.IsMyTurn(localPlayer))
        {
            Debug.LogWarning("[LALY-System] Tunggu giliranmu!");
            return;
        }

        if (!isSelected) SelectObject();
        else ExecuteMovement(localPlayer);
    }

    StopNode CariParkiranKosong()
    {
        if (nodeParkirIDs == null || nodeParkirIDs.Length == 0) return null;

        StopNode[] allNodes = FindObjectsOfType<StopNode>();
        foreach (int id in nodeParkirIDs)
        {
            StopNode node = allNodes.FirstOrDefault(n => n.nodeID == id);
            if (node != null && !node.IsFull()) return node;
        }
        return null;
    }

    void SelectObject()
    {
        DeselectAllOtherObjects();
        isSelected = true;
        if (outline != null) outline.enabled = true;

        if (currentTanya == null && tanyaPrefab != null)
        {
            Vector3 spawnPos = transform.position + tanyaOffset;
            currentTanya = Instantiate(tanyaPrefab, spawnPos, Quaternion.identity);
            currentTanya.transform.localScale = Vector3.one;
            currentTanya.transform.SetParent(transform);

            TandaTanyaInteraction ttInteraction = currentTanya.AddComponent<TandaTanyaInteraction>();
            ttInteraction.pesanPopup = infoText;
        }
    }

    public void DeselectObject()
    {
        isSelected = false;
        if (outline != null) outline.enabled = false;
        if (currentTanya != null) Destroy(currentTanya);
    }

    void ExecuteMovement(PlayerData pData)
    {
        GerakPion pion = pData.GetComponent<GerakPion>();

        if (pion != null && chosenParkirNode != null)
        {
            StopNode nodeGaris = FindObjectsOfType<StopNode>().FirstOrDefault(n => n.nodeID == nodeGarisID);

            if (nodeGaris != null)
            {
                pion.MoveToNode(nodeGaris, chosenParkirNode);
                DeselectObject();
            }
            else
            {
                Debug.LogError($"Node Garis ID {nodeGarisID} tidak ditemukan! Pastikan ID-nya benar di MapDatabase.");
            }
        }
    }

    void DeselectAllOtherObjects()
    {
        InteractableObject[] allObjects = FindObjectsOfType<InteractableObject>();
        foreach (var obj in allObjects) if (obj != this) obj.DeselectObject();
    }

    // 🔥 FUNGSI TAMBAHAN UNTUK UI NANTINYA
    public void TerpilihDariUI()
    {
        if (!isSelected) SelectObject();
    }

    public StopNode[] DapatkanSemuaNodeParkir()
    {
        if (nodeParkirIDs == null || nodeParkirIDs.Length == 0) return new StopNode[0];

        StopNode[] allNodes = FindObjectsOfType<StopNode>();
        return allNodes.Where(n => nodeParkirIDs.Contains(n.nodeID)).ToArray();
    }
}

// --- Class Terpisah untuk Tanda Tanya ---
public class TandaTanyaInteraction : MonoBehaviour
{
    public string pesanPopup;

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (PopupManager.Instance != null) PopupManager.Instance.ShowPopup(pesanPopup);
    }

    void Update()
    {
        transform.Rotate(Vector3.up, 120f * Time.deltaTime);
    }
}