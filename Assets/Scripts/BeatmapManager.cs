using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class BeatmapManager : MonoBehaviour
{
    public OsuSpawner spawner;
    public string importedFolder = "Imported";

    private List<BeatmapInfo> availableMaps = new List<BeatmapInfo>();
    private BeatmapInfo currentMap;

    [System.Serializable]
    public class BeatmapInfo
    {
        public string folderPath;
        public string mapPath;
        public string audioPath;
        public string backgroundPath;
        public string title;
        public string artist;
        public string difficulty;
    }

    void Start()
    {
        ScanForNewMaps();

        // Автозагрузка первой карты
        if (availableMaps.Count > 0)
        {
            LoadBeatmap(availableMaps[0]);
        }
    }

    public void ScanForNewMaps()
    {
        availableMaps.Clear();

        string importedPath = Path.Combine(Application.dataPath, importedFolder);

        if (!Directory.Exists(importedPath))
        {
            Debug.LogWarning("Папка с картами не найдена! Создаю...");
            Directory.CreateDirectory(importedPath);
            return;
        }

        // Ищем все папки с картами
        string[] mapFolders = Directory.GetDirectories(importedPath);

        foreach (string folder in mapFolders)
        {
            ScanFolderForBeatmaps(folder);
        }

        Debug.Log($"Найдено карт: {availableMaps.Count}");

        // Показываем список карт в консоли
        foreach (var map in availableMaps)
        {
            Debug.Log($"Карта: {map.title} - {map.artist} ({map.difficulty})");
        }
    }

    void ScanFolderForBeatmaps(string folderPath)
    {
        string[] osuFiles = Directory.GetFiles(folderPath, "*.osu");

        foreach (string osuFile in osuFiles)
        {
            BeatmapInfo mapInfo = ParseMapInfo(osuFile, folderPath);
            if (mapInfo != null)
            {
                availableMaps.Add(mapInfo);
            }
        }
    }

    BeatmapInfo ParseMapInfo(string osuFilePath, string folderPath)
    {
        try
        {
            BeatmapInfo info = new BeatmapInfo();
            info.folderPath = folderPath;
            info.mapPath = osuFilePath;

            string[] lines = File.ReadAllLines(osuFilePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("Title:"))
                    info.title = GetValue(line);
                else if (line.StartsWith("Artist:"))
                    info.artist = GetValue(line);
                else if (line.StartsWith("Version:"))
                    info.difficulty = GetValue(line);
                else if (line.StartsWith("AudioFilename:"))
                {
                    string audioFile = GetValue(line);
                    info.audioPath = Path.Combine(folderPath, audioFile);
                }
                else if (line.StartsWith("0,0,\"")) // Фон
                {
                    try
                    {
                        string bgLine = line.Split('"')[1];
                        info.backgroundPath = Path.Combine(folderPath, bgLine);
                    }
                    catch
                    {
                        // Игнорируем ошибки парсинга фона
                    }
                }
            }

            return info;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка чтения карты {osuFilePath}: {e.Message}");
            return null;
        }
    }

    string GetValue(string line)
    {
        string[] parts = line.Split(':');
        if (parts.Length >= 2)
        {
            return parts[1].Trim();
        }
        return "";
    }

    public void LoadBeatmap(BeatmapInfo mapInfo)
    {
        currentMap = mapInfo;
        Debug.Log($"Загружаем карту: {mapInfo.title} - {mapInfo.artist}");

        // Загружаем музыку
        StartCoroutine(LoadMusic(mapInfo.audioPath));

        // Загружаем фон если есть
        if (!string.IsNullOrEmpty(mapInfo.backgroundPath))
        {
            StartCoroutine(LoadBackground(mapInfo.backgroundPath));
        }

        // Парсим и загружаем карту
        BeatmapParser parser = GetComponent<BeatmapParser>();
        if (parser != null && spawner != null)
        {
            BeatmapData beatmap = parser.ParseOsuFile(mapInfo.mapPath);
            if (beatmap != null)
            {
                spawner.PrepareBeatmap(beatmap);
            }
        }
        else
        {
            Debug.LogError("BeatmapParser или OsuSpawner не найден!");
        }
    }

    System.Collections.IEnumerator LoadMusic(string audioPath)
    {
        if (File.Exists(audioPath))
        {
            Debug.Log($"Загружаем музыку: {audioPath}");

            // Используем UnityWebRequest вместо устаревшего WWW
            string audioUrl = "file://" + audioPath;
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(audioUrl, UnityEngine.AudioType.UNKNOWN))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    if (spawner != null && spawner.musicSource != null)
                    {
                        spawner.musicSource.clip = clip;
                        Debug.Log("Музыка успешно загружена!");
                    }
                }
                else
                {
                    Debug.LogError("Ошибка загрузки музыки: " + www.error);
                }
            }
        }
        else
        {
            Debug.LogError("Файл музыки не найден: " + audioPath);
        }
    }

    System.Collections.IEnumerator LoadBackground(string bgPath)
    {
        if (File.Exists(bgPath))
        {
            Debug.Log($"Загружаем фон: {bgPath}");

            string bgUrl = "file://" + bgPath;
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(bgUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                    Sprite bgSprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));

                    // Создаем или находим фон на сцене
                    GameObject bgObject = GameObject.Find("Background");
                    if (bgObject == null)
                    {
                        bgObject = new GameObject("Background");
                        SpriteRenderer sr = bgObject.AddComponent<SpriteRenderer>();
                        sr.sprite = bgSprite;
                        sr.sortingOrder = -10;

                        // Растягиваем на весь экран
                        bgObject.transform.localScale = new Vector3(10, 10, 1);
                    }
                    else
                    {
                        SpriteRenderer sr = bgObject.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = bgSprite;
                        }
                    }

                    Debug.Log("Фон загружен!");
                }
                else
                {
                    Debug.LogError("Ошибка загрузки фона: " + www.error);
                }
            }
        }
        else
        {
            Debug.LogWarning("Файл фона не найден: " + bgPath);
        }
    }

    // Для UI - получить список карт
    public List<BeatmapInfo> GetAvailableMaps()
    {
        return availableMaps;
    }

    // Метод для получения текущей карты
    public BeatmapInfo GetCurrentMap()
    {
        return currentMap;
    }

    // Метод для ручной перезагрузки карт
    [ContextMenu("Пересканировать карты")]
    public void RescanMaps()
    {
        ScanForNewMaps();
    }
}