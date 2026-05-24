using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayBahan : MonoBehaviour
{
    public Image iconMaterial;
    public TextMeshProUGUI jumlahText;

    public void Setup(Sprite icon, int jumlah)
    {
        iconMaterial.sprite = icon;
        jumlahText.text = jumlah.ToString();
    }
}