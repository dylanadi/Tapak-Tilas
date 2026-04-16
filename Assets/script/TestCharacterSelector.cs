using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestCharacterSelector : MonoBehaviour
{
    [Header("UI References")]
    public Text txtNama;
    public Text txtDeskripsi;

    [Header("3D Models Data")]
    public List<GameObject> allCharacterPrefabs;
    public List<string> allNames;
    [TextArea] public List<string> allDescriptions;

    [Header("Spawn Positions")]
    public Transform posKiri;
    public Transform posTengah;

    [Header("Settings")]
    public float kecepatanSwap = 10f;

    private GameObject modelKiri, modelTengah;
    private int idKiri, idTengah;
    private QuickOutline currentHoverOutline;

    void Start()
    {
        // Untuk testing, kita munculkan karakter index 0 di tengah dan 1 di kiri
        idTengah = 0;
        idKiri = 1;

        MunculkanModelAwal();
    }

    void MunculkanModelAwal()
    {
        if (allCharacterPrefabs.Count < 2)
        {
            Debug.LogError("Dylan, isi dulu list All Character Prefabs-nya minimal 2!");
            return;
        }

        modelTengah = Instantiate(allCharacterPrefabs[idTengah], posTengah.position, posTengah.rotation);
        modelKiri = Instantiate(allCharacterPrefabs[idKiri], posKiri.position, posKiri.rotation);

        UpdateInfoUI();
    }

    void Update()
    {
        HandleHoverAndClick();
        HandleAnimations();
    }

    void HandleHoverAndClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Reset outline sebelumnya agar tidak nyala terus
        if (currentHoverOutline != null)
        {
            currentHoverOutline.enabled = false;
            currentHoverOutline = null;
        }

        // Deteksi apakah mouse di atas model 3D (Harus ada Collider!)
        if (Physics.Raycast(ray, out hit))
        {
            // Cari komponen QuickOutline di objek yang terkena raycast atau parent-nya
            QuickOutline outline = hit.transform.GetComponentInParent<QuickOutline>();

            if (outline != null)
            {
                outline.enabled = true;
                currentHoverOutline = outline;

                // Jika diklik kiri, cek apakah itu model yang di kiri untuk ditukar
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.root.gameObject == modelKiri)
                    {
                        TukarPosisi();
                    }
                }
            }
        }
    }

    void HandleAnimations()
    {
        if (modelTengah == null || modelKiri == null) return;

        // Gerak halus ke posisi masing-masing
        modelTengah.transform.position = Vector3.Lerp(modelTengah.transform.position, posTengah.position, Time.deltaTime * kecepatanSwap);
        modelKiri.transform.position = Vector3.Lerp(modelKiri.transform.position, posKiri.position, Time.deltaTime * kecepatanSwap);

        // Atur Kecerahan (1.0f = Terang, 0.3f = Gelap)
        SetBrightness(modelTengah, 1.0f);
        SetBrightness(modelKiri, 0.7f);
    }

    void TukarPosisi()
    {
        // Tukar referensi Object
        GameObject tempObj = modelTengah;
        modelTengah = modelKiri;
        modelKiri = tempObj;

        // Tukar ID Data
        int tempID = idTengah;
        idTengah = idKiri;
        idKiri = tempID;

        UpdateInfoUI();
    }

    void SetBrightness(GameObject obj, float factor)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                // Mengubah warna material menjadi lebih gelap/terang tanpa transparansi
                m.color = new Color(factor, factor, factor, 1.0f);
            }
        }
    }

    void UpdateInfoUI()
    {
        if (allNames.Count > idTengah) txtNama.text = allNames[idTengah];
        if (allDescriptions.Count > idTengah) txtDeskripsi.text = allDescriptions[idTengah];
    }
}