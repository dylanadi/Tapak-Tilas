#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineRenderer))]
public class GambarJalur : Editor
{
    private LineRenderer lineRenderer;
    private bool sedangMenggambar = false;

    private int selectedNodeID = 0;
    private int selectedCapacity = 1;
    private GameObject nodePrefab;

    void OnEnable()
    {
        lineRenderer = (LineRenderer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("NODE SETTINGS", EditorStyles.boldLabel);

        selectedNodeID = EditorGUILayout.IntField("Node ID", selectedNodeID);
        selectedCapacity = EditorGUILayout.IntField("Capacity", selectedCapacity);

        nodePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Node Prefab",
            nodePrefab,
            typeof(GameObject),
            false
        );

        GUI.backgroundColor = sedangMenggambar ? Color.red : Color.green;

        if (GUILayout.Button(sedangMenggambar ? "STOP DRAWING" : "START DRAWING"))
        {
            sedangMenggambar = !sedangMenggambar;
            lineRenderer.useWorldSpace = true;
        }

        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("CLEAR ALL"))
        {
            Undo.RecordObject(lineRenderer, "Clear Path");
            lineRenderer.positionCount = 0;
        }
    }

    void OnSceneGUI()
    {
        if (!sedangMenggambar) return;

        Event e = Event.current;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 pos = hit.point + Vector3.up * 0.1f;

                // 🔥 KLIK KANAN = JALUR BIASA
                if (e.button == 1)
                {
                    TambahJalur(pos);
                }

                // 🔥 KLIK KIRI = NODE
                if (e.button == 0)
                {
                    TambahJalur(pos);
                    BuatNode(pos);
                }

                e.Use();
            }
        }
    }

    void TambahJalur(Vector3 pos)
    {
        Undo.RecordObject(lineRenderer, "Tambah Titik");

        int index = lineRenderer.positionCount;
        lineRenderer.positionCount = index + 1;
        lineRenderer.SetPosition(index, pos);
    }

    void BuatNode(Vector3 pos)
    {
        if (nodePrefab == null)
        {
            Debug.LogWarning("Node Prefab belum diisi!");
            return;
        }

        GameObject node = (GameObject)PrefabUtility.InstantiatePrefab(nodePrefab);

        if (node != null)
        {
            Undo.RegisterCreatedObjectUndo(node, "Create Node");

            node.transform.position = pos;

            StopNode sn = node.GetComponent<StopNode>();

            if (sn != null)
            {
                sn.nodeID = selectedNodeID;
                sn.capacity = selectedCapacity;
            }

            node.name = "Node_" + selectedNodeID;
        }
    }
}
#endif
