using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DatabaseKerajinan data; // Tarik asset ScriptableObject ke sini
    public int id; // ID / Index urutan kartu di databaseKartu

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mengirimkan ID kartu dan scriptable object ke TooltipSystem
        TooltipSystem.Instance.Show(id, data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance.Hide();
    }
}