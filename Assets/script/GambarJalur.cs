#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineRenderer))]
public class GambarJalur : Editor
{
    private LineRenderer lineRenderer;
    private bool sedangMenggambar = false;

    void OnEnable() { lineRenderer = (LineRenderer)target; }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.Space();
        GUI.backgroundColor = sedangMenggambar ? Color.red : Color.green;
        if (GUILayout.Button(sedangMenggambar ? "MATIKAN MODE GAMBAR" : "AKTIFKAN MODE GAMBAR"))
        {
            sedangMenggambar = !sedangMenggambar;
            lineRenderer.useWorldSpace = true;
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("Hapus Semua Jalur"))
        {
            if (EditorUtility.DisplayDialog("Hapus Jalur", "Yakin mau hapus semua?", "Ya", "Tidak"))
            {
                Undo.RecordObject(lineRenderer, "Hapus Jalur");
                lineRenderer.positionCount = 0;
                var pion = FindObjectOfType<GerakPion>();
                if (pion) {
                    pion.posPemberhentian.Clear();
                    EditorUtility.SetDirty(pion);
                }
            }
        }

        EditorGUILayout.HelpBox("CARA MENGGAMBAR:\n1. Klik Kanan: Tambah Jalur Biasa (Lancar)\n2. Klik Kiri: Tambah Titik POS (Pion akan Berhenti)", MessageType.Info);
    }

    void OnSceneGUI()
    {
        if (!sedangMenggambar) return;
        Event e = Event.current;

        // Cegah klik kiri menyeleksi objek lain saat menggambar
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        // MouseDown: 0 = Klik Kiri (POS), 1 = Klik Kanan (Jalur Biasa)
        if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1))
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Undo.RecordObject(lineRenderer, "Tambah Titik Jalur");
                int index = lineRenderer.positionCount;
                lineRenderer.positionCount = index + 1;
                
                // Titik diletakkan sedikit di atas lantai agar tidak z-fighting
                Vector3 pointPos = hit.point + Vector3.up * 0.1f;
                lineRenderer.SetPosition(index, pointPos);

                var pion = FindObjectOfType<GerakPion>();
                if (pion != null)
                {
                    Undo.RecordObject(pion, "Tambah POS");
                    // Jika klik kiri (0), masukkan ke daftar pemberhentian
                    if (e.button == 0)
                    {
                        pion.posPemberhentian.Add(index);
                    }
                    EditorUtility.SetDirty(pion);
                }

                e.Use(); // Gunakan event agar tidak bentrok dengan fungsi editor lain
            }
        }
    }
}
#endif