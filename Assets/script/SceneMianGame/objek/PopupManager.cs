using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("UI Reference")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;
    public Button closeButton;

    void Awake()
    {
        // 🔥 LOGIKA SINGLETON YANG BENER
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Aktifkan ini jika ingin popup terbawa ke scene lain
            Debug.Log("[PopupManager] Instance berhasil dibuat!");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Sembunyikan panel saat awal game
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        // Hubungkan tombol close secara otomatis jika belum diisi di Inspector
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    public void ShowPopup(string text)
    {
        if (popupPanel != null && popupText != null)
        {
            popupPanel.SetActive(true);
            popupText.text = text;
            Debug.Log("[PopupManager] Menampilkan popup dengan teks: " + text);
        }
        else
        {
            Debug.LogError("[PopupManager] Slot UI (Panel/Text) di Inspector masih kosong!");
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}