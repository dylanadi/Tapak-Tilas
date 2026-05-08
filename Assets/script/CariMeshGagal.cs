using UnityEngine;
using System.Collections.Generic;

public class CariMeshGagal : MonoBehaviour
{
    void Start()
    {
        MeshFilter[] semuaBentuk3D = FindObjectsOfType<MeshFilter>();

        // Pakai HashSet biar kalau ada path yang kembar, otomatis cuma dihitung 1
        HashSet<string> daftarPathUnik = new HashSet<string>();

        foreach (MeshFilter mf in semuaBentuk3D)
        {
            if (mf.sharedMesh != null && !mf.sharedMesh.isReadable)
            {
                // Ambil jalur lengkap objeknya di Hierarchy
                string pathLengkap = DapatkanPathLengkap(mf.transform);
                daftarPathUnik.Add(pathLengkap);
            }
        }

        // Tampilkan hasilnya dengan rapi di Console
        Debug.Log("<color=yellow>=== DAFTAR OBJEK YANG MESH-NYA TERGEMBOK ===</color>");

        foreach (string path in daftarPathUnik)
        {
            Debug.Log($"<color=orange>Lokasi Objek:</color> <b>{path}</b>");
        }

        Debug.Log($"<color=yellow>Pencarian Selesai! Ada {daftarPathUnik.Count} grup objek unik yang bermasalah.</color>");
    }

    // Fungsi khusus buat ngurutin nama dari anak sampai ke induknya
    string DapatkanPathLengkap(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}