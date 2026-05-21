using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DatabaseKerajinanUtama", menuName = "Database Game/Database Kerajinan")]
public class DatabaseKerajinan : ScriptableObject
{
    [Header("Buku Induk Material")]
    [Tooltip("Urutan di sini (0, 1, 2, dst) otomatis menjadi ID Material!")]
    public List<DataMaterial> databaseMaterial = new List<DataMaterial>();

    [Header("Buku Induk Kartu Kerajinan")]
    public List<DataKartu> databaseKartu = new List<DataKartu>();
}