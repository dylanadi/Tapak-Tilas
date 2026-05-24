using UnityEngine;

public class KerajinanManager : MonoBehaviour
{
    public static KerajinanManager Instance;

    [Header("Data Base")]
    public DatabaseKerajinan dataBase;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public DataMaterial GetMaterialByID(int id)
    {
        if (dataBase != null && id >= 0 && id < dataBase.databaseMaterial.Count)
        {
            return dataBase.databaseMaterial[id];
        }
        Debug.LogError($"[Kerajinan] Material dengan ID {id} tidak ditemukan!");
        return null;
    }

    public DataKartu GetKartuByID(int id)
    {
        if (dataBase != null && id >= 0 && id < dataBase.databaseKartu.Count)
        {
            return dataBase.databaseKartu[id];
        }
        return null;
    }
}