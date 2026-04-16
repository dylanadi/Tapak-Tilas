using UnityEngine;
using Photon.Pun;

public class NodeSelector : MonoBehaviour
{
    private GerakPion player;

    void Start()
    {
        // 🔥 Cari player milik sendiri
        GerakPion[] semuaPlayer = FindObjectsOfType<GerakPion>();

        foreach (var p in semuaPlayer)
        {
            if (p.GetComponent<PhotonView>().IsMine)
            {
                player = p;
                break;
            }
        }

        if (player == null)
        {
            Debug.LogError("Player milik sendiri tidak ditemukan!");
        }
    }

    void Update()
    {
        if (player == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                StopNode node = hit.collider.GetComponent<StopNode>();

                if (node != null)
                {
                    player.MoveToNode(node);
                }
            }
        }
    }
}
