using UnityEngine;

public class TandaTanya : MonoBehaviour
{
    private string pesan;

    public void Setup(string txt)
    {
        pesan = txt;
    }

    void OnMouseDown()
    {
        // Panggil popup di UI
        PopupManager.Instance.ShowPopup(pesan);
    }

    void Update()
    {
        // Animasi muter-muter biar keren
        transform.Rotate(Vector3.up, 100f * Time.deltaTime);
    }
}