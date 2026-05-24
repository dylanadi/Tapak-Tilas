#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq; // 🔥 TAMBAHAN UTAMA: Biar bisa pakai fungsi FirstOrDefault()

public class DatabaseToCSVExporter : MonoBehaviour
{
    // 🔥 Menambahkan menu tombol klik di bagian atas Unity Editor
    [MenuItem("Tools/Export Database Kerajinan ke CSV")]
    public static void ExportDatabaseKeCSV()
    {
        string namaFile = "BukuIndukKerajinan.asset";
        
        // 🔥 TRIK BARU: .FirstOrDefault() bikin hasilnya langsung string tunggal, 
        // jadi kita TIDAK PERLU pakai kurung siku lagi!
        string fileDitemukan = Directory.GetFiles(Application.dataPath, namaFile, SearchOption.AllDirectories).FirstOrDefault();

        if (string.IsNullOrEmpty(fileDitemukan))
        {
            Debug.LogError("[Backup] Gagal! File database bernama 'BukuIndukKerajinan' tidak ditemukan di folder Assets. Pastikan nama filenya sama persis!");
            return;
        }

        // Karena 'fileDitemukan' sudah pasti string tunggal, fungsi .Replace() di bawah ini aman 100%
        string pathLokal = "Assets" + fileDitemukan.Replace(Application.dataPath, "").Replace('\\', '/');
        DatabaseKerajinan db = AssetDatabase.LoadAssetAtPath<DatabaseKerajinan>(pathLokal);

        if (db == null)
        {
            Debug.LogError("[Backup] File ditemukan tapi gagal dibaca sebagai DatabaseKerajinan.");
            return;
        }

        // ==========================================
        // 🛠️ PROSES BACKUP 1: DATA MATERIAL
        // ==========================================
        StringBuilder csvMaterial = new StringBuilder();
        csvMaterial.AppendLine("ID_Material,Nama_Material"); 

        for (int i = 0; i < db.databaseMaterial.Count; i++)
        {
            csvMaterial.AppendLine($"{i},{db.databaseMaterial[i].namaMaterial}");
        }
        
        string pathMaterialCSV = Application.dataPath + "/Backup_Database_Material.csv";
        File.WriteAllText(pathMaterialCSV, csvMaterial.ToString());

        // ==========================================
        // 🛠️ PROSES BACKUP 2: DATA KARTU
        // ==========================================
        StringBuilder csvKartu = new StringBuilder();
        csvKartu.AppendLine("Nama_Kartu,Kharisma_Reward,Resep_Bahan(ID_Bahan:Jumlah)"); 

        foreach (var kartu in db.databaseKartu)
        {
            string stringResep = "";
            foreach (var bahan in kartu.listBahan)
            {
                stringResep += $"{bahan.idMaterial}:{bahan.jumlahButuh}|";
            }
            
            if (stringResep.EndsWith("|")) 
                stringResep = stringResep.Substring(0, stringResep.Length - 1);

            csvKartu.AppendLine($"{kartu.namaKartu},{kartu.kharismaReward},\"{stringResep}\"");
        }

        string pathKartuCSV = Application.dataPath + "/Backup_Database_Kartuu1.csv";
        File.WriteAllText(pathKartuCSV, csvKartu.ToString());

        // Refresh Unity Editor
        AssetDatabase.Refresh();

        Debug.Log($"<color=yellow>🔥 BACKUP SUKSES!</color>\nFile disimpan di folder Assets utama:\n1. Backup_Database_Material.csv\n2. Backup_Database_Kartu.csv");
    }
}
#endif