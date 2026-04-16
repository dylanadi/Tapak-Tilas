using UnityEngine;
using Photon.Pun;

public class PlayerIndicator : MonoBehaviour
{
    public GameObject indicator;
    private PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (indicator != null)
        {
            indicator.SetActive(pv.IsMine);
        }
    }

    void Update()
    {
        if (indicator != null && indicator.activeSelf)
        {
            indicator.transform.Rotate(0, 100 * Time.deltaTime, 0);
        }
    }
}
