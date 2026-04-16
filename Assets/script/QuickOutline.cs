using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QuickOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.yellow;
    [Range(0f, 10f)]
    public float outlineWidth = 5f;

    // Pakai Material Swapper kalau kamu gapunya Shader khusus
    public Material hoverMaterial;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Renderer[] allRenderers;
    private bool isHovering = false;

    void Awake()
    {
        // Cari semua renderer (Orang + Alas)
        allRenderers = GetComponentsInChildren<Renderer>();

        // Simpan data material asli
        foreach (var r in allRenderers)
        {
            originalMaterials[r] = r.sharedMaterials;
        }
    }

    // Fungsi utama yang dipanggil oleh CharacterSelection.cs
    public bool enabled
    {
        get { return isHovering; }
        set
        {
            isHovering = value;
            ApplyHoverEffect(isHovering);
        }
    }

    void ApplyHoverEffect(bool state)
    {
        if (allRenderers == null) return;

        foreach (var r in allRenderers)
        {
            if (r == null) continue;

            if (state)
            {
                // Ganti semua material jadi material hover (Kuning)
                Material[] hoverMats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < hoverMats.Length; i++)
                {
                    hoverMats[i] = hoverMaterial;
                }
                r.materials = hoverMats;
            }
            else
            {
                // Balikin ke asal
                if (originalMaterials.ContainsKey(r))
                {
                    r.materials = originalMaterials[r];
                }
            }
        }
    }

    // Biar script CharacterSelection lama kamu nggak error, kita buatkan fungsi ini
    public void SetOutlineColor(Color c) => outlineColor = c;
}