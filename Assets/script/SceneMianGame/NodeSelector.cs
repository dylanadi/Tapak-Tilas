using UnityEngine;

public class NodeSelector : MonoBehaviour
{
    public GerakPion player;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
