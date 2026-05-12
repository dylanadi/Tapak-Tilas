using UnityEngine;
using System.Collections.Generic;

// 🔥 ENUM TIPE OBJEK JADI PUSAT DI SINI
public enum TipeObjek { Ketapang, Ijen, PulauMerah, Rajinan, Pasar, Hujan, Kharisma, Warisan }

[System.Serializable]
public class DataObjekMap
{
    [Header("Identitas")]
    public int idObjek;              // ID Objek (1 - 14)
    public string namaObjek;         // Cuma label Inspector biar gampang dibaca
    public TipeObjek jenisObjek;     // Tipe Objek Aslinya

    [Header("Data Node")]
    public int nodeGarisID;          // ID Pintu Garis (Halte)
    public int[] nodeParkirIDs;      // ID Petak Parkir

    [Header("Setting Kamera")]
    public float tinggiKamera = 15f;

    [Header("Data UI (Untuk Nanti)")]
    public Sprite iconUI;            // Gambar icon disiapin aja dulu
}

public class MapDatabase : MonoBehaviour
{
    public static MapDatabase Instance;

    [Header("Buku Induk Data Map")]
    public List<DataObjekMap> semuaDataMap = new List<DataObjekMap>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Cari data berdasarkan ID Objek (Biasa dipakai Objek 3D & UI)
    public DataObjekMap AmbilData(int id)
    {
        return semuaDataMap.Find(data => data.idObjek == id);
    }

    // Cari data berdasarkan Tipe Objek (Berguna untuk script event/efek nantinya)
    public DataObjekMap AmbilDataBerdasarkanTipe(TipeObjek tipe)
    {
        return semuaDataMap.Find(data => data.jenisObjek == tipe);
    }
}