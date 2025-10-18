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
        CreateFolders();
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

        Debug.Log("Папки для карт созданы!");
    }

    IEnumerator MonitorBeatmapsFolder()
    {
        while (true)
        {
            CheckForNewOszFiles();
            yield return new WaitForSeconds(3f); // Проверяем каждые 3 секунды
        }
    }

    void CheckForNewOszFiles()
    {
        string beatmapsPath = Path.Combine(Application.dataPath, beatmapsFolder);
        string[] oszFiles = Directory.GetFiles(beatmapsPath, "*.osz");

        foreach (string oszFile in oszFiles)
        {
            Debug.Log($"Найден новый .osz файл: {Path.GetFileName(oszFile)}");
            ExtractOszFile(oszFile);
        }
    }

    void ExtractOszFile(string oszPath)
    {
        try
        {
            string fileName = Path.GetFileNameWithoutExtension(oszPath);
            string extractPath = Path.Combine(Application.dataPath, importedFolder, fileName);

            if (!Directory.Exists(extractPath))
                Directory.CreateDirectory(extractPath);

            // Распаковываем архив
            using (ZipArchive archive = ZipFile.OpenRead(oszPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryPath = Path.Combine(extractPath, entry.Name);
                    entry.ExtractToFile(entryPath, true);
                }
            }

            Debug.Log($"Карта {fileName} успешно распакована!");

            // Перемещаем обработанный файл
            string processedDir = Path.Combine(Application.dataPath, importedFolder, "processed");
            if (!Directory.Exists(processedDir))
                Directory.CreateDirectory(processedDir);

            string processedPath = Path.Combine(processedDir, Path.GetFileName(oszPath));
            File.Move(oszPath, processedPath);

            // Уведомляем в консоль
            Debug.Log($"Карта {fileName} готова к использованию! Перезапусти игру или нажми 'Rescan Maps' в BeatmapManager.");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка распаковки: {e.Message}");
        }
    }

    [ContextMenu("Открыть папку Beatmaps")]
    public void OpenBeatmapsFolder()
    {
        string beatmapsPath = Path.Combine(Application.dataPath, beatmapsFolder);
        if (Directory.Exists(beatmapsPath))
        {
            Application.OpenURL("file://" + beatmapsPath);
        }
        else
        {
            Debug.LogWarning("Папка Beatmaps не найдена!");
        }
    }

    [ContextMenu("Проверить карты сейчас")]
    public void CheckNow()
    {
        CheckForNewOszFiles();
    }
}