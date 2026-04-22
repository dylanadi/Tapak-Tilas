using UnityEngine;
using Photon.Pun;
using System.Linq;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Landmark, Warung, Misteri, Finish }

    [Header("Konfigurasi Node")]
    public int targetNodeID; 
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
        Debug.Log($"[GM-Interact] Klik pada Objek: {gameObject.name}");

        PlayerData localPlayer = FindObjectsOfType<PlayerData>()
            .FirstOrDefault(p => p.photonView != null && p.photonView.IsMine);

        if (localPlayer == null) return;

        if (GameManager.Instance != null && !GameManager.Instance.IsMyTurn(localPlayer))
        {
            Debug.LogWarning("[GM-Interact] Klik ditolak: Bukan giliranmu!");
            return;
        }

        if (!isSelected) SelectObject();
        else ExecuteMovement(localPlayer);
    }

    void SelectObject()
    {
        DeselectAllOtherObjects();
        isSelected = true;
        if (outline != null) outline.enabled = true;

        if (currentTanya == null && tanyaPrefab != null)
        {
            Vector3 spawnPos = transform.position + tanyaOffset;
            currentTanya = Instantiate(tanyaPrefab, spawnPos, Quaternion.identity, transform);
            
            // Tambahkan helper klik
            TandaTanyaInteraction ttInteraction = currentTanya.AddComponent<TandaTanyaInteraction>();
            ttInteraction.pesanPopup = infoText;
            
            Debug.Log("[GM-Interact] Tanda tanya berhasil muncul!");
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
        StopNode targetNode = FindObjectsOfType<StopNode>().FirstOrDefault(n => n.nodeID == targetNodeID);

        if (pion != null && targetNode != null)
        {
            pion.MoveToNode(targetNode);
            DeselectObject();
        }
    }

    void DeselectAllOtherObjects()
    {
        InteractableObject[] allObjects = FindObjectsOfType<InteractableObject>();
        foreach (var obj in allObjects) if (obj != this) obj.DeselectObject();
    }
}

// --- SCRIPT HELPER DENGAN LOG TAMBAHAN ---
public class TandaTanyaInteraction : MonoBehaviour
{
    public string pesanPopup;

    void OnMouseDown()
    {
        Debug.Log($"[GM-TandaTanya] KLIK TERDETEKSI pada Tanda Tanya!");

        if (PopupManager.Instance != null)
        {
            Debug.Log("[GM-TandaTanya] Memanggil PopupManager.ShowPopup...");
            PopupManager.Instance.ShowPopup(pesanPopup);
        }
        else
        {
            Debug.LogError("[GM-TandaTanya] PopupManager.Instance NULL! Pastikan ada script PopupManager di scene.");
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, 120f * Time.deltaTime);
    }
}