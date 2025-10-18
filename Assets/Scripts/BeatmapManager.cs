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

        // ������������ ������ �����
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
            Debug.LogWarning("����� � ������� �� �������! ������...");
            Directory.CreateDirectory(importedPath);
            return;
        }

        // ���� ��� ����� � �������
        string[] mapFolders = Directory.GetDirectories(importedPath);

        foreach (string folder in mapFolders)
        {
            ScanFolderForBeatmaps(folder);
        }

        Debug.Log($"������� ����: {availableMaps.Count}");

        // ���������� ������ ���� � �������
        foreach (var map in availableMaps)
        {
            Debug.Log($"�����: {map.title} - {map.artist} ({map.difficulty})");
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
                else if (line.StartsWith("0,0,\"")) // ���
                {
                    try
                    {
                        string bgLine = line.Split('"')[1];
                        info.backgroundPath = Path.Combine(folderPath, bgLine);
                    }
                    catch
                    {
                        // ���������� ������ �������� ����
                    }
                }
            }

            return info;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������ ������ ����� {osuFilePath}: {e.Message}");
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
        Debug.Log($"��������� �����: {mapInfo.title} - {mapInfo.artist}");

        // ��������� ������
        StartCoroutine(LoadMusic(mapInfo.audioPath));

        // ��������� ��� ���� ����
        if (!string.IsNullOrEmpty(mapInfo.backgroundPath))
        {
            StartCoroutine(LoadBackground(mapInfo.backgroundPath));
        }

        // ������ � ��������� �����
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
            Debug.LogError("BeatmapParser ��� OsuSpawner �� ������!");
        }
    }

    System.Collections.IEnumerator LoadMusic(string audioPath)
    {
        if (File.Exists(audioPath))
        {
            Debug.Log($"��������� ������: {audioPath}");

            // ���������� UnityWebRequest ������ ����������� WWW
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
                        spawner.musicSource.Play();
                        Debug.Log("������ ������� ���������!");
                    }
                }
                else
                {
                    Debug.LogError("������ �������� ������: " + www.error);
                }
            }
        }
        else
        {
            Debug.LogError("���� ������ �� ������: " + audioPath);
        }
    }

    System.Collections.IEnumerator LoadBackground(string bgPath)
    {
        if (File.Exists(bgPath))
        {
            Debug.Log($"��������� ���: {bgPath}");

            string bgUrl = "file://" + bgPath;
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(bgUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                    spawner.background.sprite = Sprite.Create(texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));

                    // ������� ��� ������� ��� �� �����
                    GameObject bgObject = GameObject.Find("Background");
                    if (bgObject == null)
                    {
                        bgObject = new GameObject("Background");
                        SpriteRenderer sr = bgObject.AddComponent<SpriteRenderer>();
                        sr.sprite = spawner.background.sprite;
                    }
                    else
                    {
                        SpriteRenderer sr = bgObject.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.sprite = spawner.background.sprite;
                        }
                    }

                    Debug.Log("��� ��������!");
                }
                else
                {
                    Debug.LogError("������ �������� ����: " + www.error);
                }
            }
        }
        else
        {
            Debug.LogWarning("���� ���� �� ������: " + bgPath);
        }
    }

    // ��� UI - �������� ������ ����
    public List<BeatmapInfo> GetAvailableMaps()
    {
        return availableMaps;
    }

    // ����� ��� ��������� ������� �����
    public BeatmapInfo GetCurrentMap()
    {
        return currentMap;
    }

    // ����� ��� ������ ������������ ����
    [ContextMenu("��������������� �����")]
    public void RescanMaps()
    {
        ScanForNewMaps();
    }
}