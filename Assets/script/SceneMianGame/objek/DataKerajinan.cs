using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DataMaterial
{
    public string namaMaterial;
    public Sprite iconMaterial;
}

[System.Serializable]
public class KebutuhanBahan
{
    [Tooltip("Isi dengan ID (urutan) material dari Database Material")]
    public int idMaterial;
    public int jumlahButuh;
}

[System.Serializable]
public class DataKartu
{
    public string namaKartu;
    public Sprite gambarKartu;
    public int kharismaReward;

    [Header("Resep Bahan")]
    public List<KebutuhanBahan> listBahan;
}