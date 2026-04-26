using UnityEngine;
using Photon.Pun;
using System.Linq;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType 
    { 
        Ketapang, 
        Ijen, 
        PulauMerah, 
        Rajinan, 
        Pasar, 
        Hujan, 
        Kharisma, 
        Warisan 
    }

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
            
            // 🔥 FIX SCALE: Spawn tanpa parent dulu biar scale gak rusak
            currentTanya = Instantiate(tanyaPrefab, spawnPos, Quaternion.identity);
            
            // 🔥 Paksa scale balik ke ukuran asli (1,1,1)
            currentTanya.transform.localScale = Vector3.one;
            
            // 🔥 Baru masukkan ke dalam parent (objek map)
            currentTanya.transform.SetParent(transform);
            
            TandaTanyaInteraction ttInteraction = currentTanya.AddComponent<TandaTanyaInteraction>();
            ttInteraction.pesanPopup = infoText;
            
            Debug.Log("[GM-Interact] Tanda tanya muncul dengan skala normal!");
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

public class TandaTanyaInteraction : MonoBehaviour
{
    public string pesanPopup;

    void OnMouseDown()
    {
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.ShowPopup(pesanPopup);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, 120f * Time.deltaTime);
    }
}