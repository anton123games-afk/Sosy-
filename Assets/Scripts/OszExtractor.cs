using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Collections;

public class OszExtractor : MonoBehaviour
{
    public string beatmapsFolder = "Beatmaps";
    public string importedFolder = "Imported";

    void Start()
    {
        // ������� ����� ���� �� ���
        CreateFolders();

        // ��������� ���������� �����
        StartCoroutine(MonitorBeatmapsFolder());
    }

    void CreateFolders()
    {
        string beatmapsPath = Path.Combine(Application.dataPath, beatmapsFolder);
        string importedPath = Path.Combine(Application.dataPath, importedFolder);

        if (!Directory.Exists(beatmapsPath))
            Directory.CreateDirectory(beatmapsPath);

        if (!Directory.Exists(importedPath))
            Directory.CreateDirectory(importedPath);

        Debug.Log("����� �������!");
    }

    IEnumerator MonitorBeatmapsFolder()
    {
        while (true)
        {
            CheckForNewOszFiles();
            yield return new WaitForSeconds(2f); // ��������� ������ 2 �������
        }
    }

    void CheckForNewOszFiles()
    {
        string beatmapsPath = Path.Combine(Application.dataPath, beatmapsFolder);
        string[] oszFiles = Directory.GetFiles(beatmapsPath, "*.osz");

        foreach (string oszFile in oszFiles)
        {
            Debug.Log($"������ ����� .osz ����: {Path.GetFileName(oszFile)}");
            ExtractOszFile(oszFile);
        }
    }

    void ExtractOszFile(string oszPath)
    {
        try
        {
            string fileName = Path.GetFileNameWithoutExtension(oszPath);
            string extractPath = Path.Combine(Application.dataPath, importedFolder, fileName);

            // ������� ����� ��� �����
            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            // ������������� .osz (��� ������ zip �����)
            using (ZipArchive archive = ZipFile.OpenRead(oszPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryPath = Path.Combine(extractPath, entry.Name);

                    // ��������� ����
                    entry.ExtractToFile(entryPath, true);
                    Debug.Log($"��������: {entry.Name}");
                }
            }

            // ���������� .osz ���� � �����
            string processedPath = Path.Combine(Application.dataPath, importedFolder, "processed", Path.GetFileName(oszPath));
            string processedDir = Path.GetDirectoryName(processedPath);
            if (!Directory.Exists(processedDir))
                Directory.CreateDirectory(processedDir);

            File.Move(oszPath, processedPath);

            Debug.Log($"����� {fileName} ������� �����������!");

            // ������������� ��������� �����
            GameObject.Find("BeatmapManager").GetComponent<BeatmapData>().ScanForNewMaps();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ����������: {e.Message}");
        }
    }
}