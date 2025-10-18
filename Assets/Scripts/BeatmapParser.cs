using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class BeatmapParser : MonoBehaviour
{
    public BeatmapData ParseOsuFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Файл карты не найден: " + filePath);
            return null;
        }

        BeatmapData beatmap = new BeatmapData();
        string[] allLines = File.ReadAllLines(filePath);

        bool inHitObjects = false;

        foreach (string line in allLines)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                continue;

            if (line.Trim() == "[HitObjects]")
            {
                inHitObjects = true;
                continue;
            }

            if (line.StartsWith("[") && line != "[HitObjects]")
            {
                inHitObjects = false;
                continue;
            }

            if (!inHitObjects)
            {
                ParseMetadata(line, beatmap);
            }

            if (inHitObjects)
            {
                ParseHitObjectData(line, beatmap);
            }
        }

        Debug.Log($"Загружена карта: {beatmap.title} - {beatmap.artist}");
        Debug.Log($"Найдено нот: {beatmap.hitObjects.Count}");

        return beatmap;
    }

    void ParseMetadata(string line, BeatmapData beatmap)
    {
        if (line.StartsWith("Title:"))
        {
            beatmap.title = ExtractValue(line);
        }
        else if (line.StartsWith("Artist:"))
        {
            beatmap.artist = ExtractValue(line);
        }
        else if (line.StartsWith("Version:"))
        {
            beatmap.difficulty = ExtractValue(line);
        }
        else if (line.StartsWith("AudioFilename:"))
        {
            beatmap.audioFile = ExtractValue(line);
        }
    }

    void ParseHitObjectData(string line, BeatmapData beatmap)
    {
        string[] parts = line.Split(',');

        if (parts.Length >= 4)
        {
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float time = float.Parse(parts[2]) / 1000f;
            int type = int.Parse(parts[3]);

            HitObjectData hitObj = new HitObjectData();
            hitObj.x = x;
            hitObj.y = y;
            hitObj.time = time;
            hitObj.type = type;

            if ((type & 1) == 1) // Circle
            {
                if (parts.Length > 4)
                    hitObj.soundType = int.Parse(parts[4]);

                beatmap.hitObjects.Add(hitObj);
            }
            else if ((type & 2) == 2 && parts.Length >= 8) // Slider
            {
                hitObj.sliderType = parts[5];
                hitObj.repeat = int.Parse(parts[6]);
                hitObj.pixelLength = float.Parse(parts[7]);
                beatmap.hitObjects.Add(hitObj);
            }
            else if ((type & 8) == 8 && parts.Length >= 5) // Spinner
            {
                hitObj.endTime = float.Parse(parts[5]) / 1000f;
                beatmap.hitObjects.Add(hitObj);
            }
        }
    }

    string ExtractValue(string line)
    {
        string[] parts = line.Split(':');
        if (parts.Length >= 2)
        {
            return parts[1].Trim();
        }
        return "";
    }

    // Старый метод для совместимости (если где-то используется)
    public BeatmapData ParseBeatmap(string filePath)
    {
        return ParseOsuFile(filePath);
    }
}