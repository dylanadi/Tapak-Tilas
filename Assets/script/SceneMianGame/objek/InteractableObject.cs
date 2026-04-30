using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Ketapang, Ijen, PulauMerah, Rajinan, Pasar, Hujan, Kharisma, Warisan }

    [Header("Titik Jalur Garis")]
    [Tooltip("ID Node yang posisinya TEPAT DI ATAS GARIS PINK (Pintu masuk/keluar)")]
    public int nodeGarisID;

    [Header("Daftar Petak Parkir")]
    [Tooltip("ID Node parkiran yang posisinya di luar/samping garis")]
    public int[] nodeParkirIDs;

    public ObjectType jenisObjek;

    [Header("Konten Popup")]
    [TextArea(3, 10)]
    public string infoText;

    [Header("Visual & Prefab")]
    public GameObject tanyaPrefab;
    public Vector3 tanyaOffset = new Vector3(0, 3.5f, 0);

    private GameObject currentTanya;
    private Outline outline;
    private bool isSelected = false;
    private StopNode chosenParkirNode;

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
    }

    void OnMouseDown()
    {
        // Blokir klik jika terhalang UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Cari Player Lokal
        PlayerData localPlayer = FindObjectsOfType<PlayerData>()
            .FirstOrDefault(p => p.photonView != null && p.photonView.IsMine);

        if (localPlayer == null) return;

        // 🔥 CEK ANTI PINDAH KURSI 🔥
        // Mengecek apakah player sudah berada di node halte atau salah satu parkiran di objek ini
        GerakPion pionLokal = localPlayer.GetComponent<GerakPion>();
        if (pionLokal != null && pionLokal.currentNode != null)
        {
            if (pionLokal.currentNode.nodeID == nodeGarisID || nodeParkirIDs.Contains(pionLokal.currentNode.nodeID))
            {
                Debug.LogWarning("[LALY-System] Kamu sudah berada di dalam objek ini!");
                if (PopupManager.Instance != null) PopupManager.Instance.ShowPopup("Kamu sudah berada di sini!");
                return; // Tolak kliknya!
            }
        }

        // Cari parkiran yang kosong
        chosenParkirNode = CariParkiranKosong();

        if (chosenParkirNode == null)
        {
            Debug.LogWarning($"<color=red>[LALY-System] Semua parkiran di {gameObject.name} sudah FULL!</color>");
            return;
        }

        // Cek giliran
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
                Debug.LogError("Node Garis tidak ditemukan! Pastikan ID-nya benar.");
            }
        }
    }

    void DeselectAllOtherObjects()
    {
        InteractableObject[] allObjects = FindObjectsOfType<InteractableObject>();
        foreach (var obj in allObjects) if (obj != this) obj.DeselectObject();
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