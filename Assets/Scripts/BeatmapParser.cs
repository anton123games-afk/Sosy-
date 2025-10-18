using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class BeatmapParser : MonoBehaviour
{
    public BeatmapData ParseBeatmap(string filePath)
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
            // Пропускаем пустые строки и комментарии
            if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                continue;

            // Начало секции хитобжектов
            if (line.Trim() == "[HitObjects]")
            {
                inHitObjects = true;
                continue;
            }

            // Конец секции (новая секция)
            if (line.StartsWith("[") && line != "[HitObjects]")
            {
                inHitObjects = false;
                continue;
            }

            // Читаем общую информацию
            if (!inHitObjects)
            {
                if (line.StartsWith("Title:"))
                {
                    beatmap.title = GetValue(line);
                }
                else if (line.StartsWith("Artist:"))
                {
                    beatmap.artist = GetValue(line);
                }
                else if (line.StartsWith("Version:"))
                {
                    beatmap.difficulty = GetValue(line);
                }
                else if (line.StartsWith("AudioFilename:"))
                {
                    beatmap.audioFile = GetValue(line);
                }
            }

            // Читаем хитобжекты
            if (inHitObjects)
            {
                ParseHitObjectLine(line, beatmap);
            }
        }

        Debug.Log($"Загружена карта: {beatmap.title} - {beatmap.artist}");
        Debug.Log($"Найдено нот: {beatmap.hitObjects.Count}");

        return beatmap;
    }

    void ParseHitObjectLine(string line, BeatmapData beatmap)
    {
        // Формат: x,y,time,type,hitSound,sliderParams,repeat,length
        string[] parts = line.Split(',');

        if (parts.Length >= 4)
        {
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float time = float.Parse(parts[2]) / 1000f; // конвертируем в секунды
            int type = int.Parse(parts[3]);

            HitObjectData hitObj = new HitObjectData();
            hitObj.x = x;
            hitObj.y = y;
            hitObj.time = time;
            hitObj.type = type;

            // Простые круги (бит 0 установлен)
            if ((type & 1) == 1)
            {
                if (parts.Length > 4)
                    hitObj.soundType = int.Parse(parts[4]);

                beatmap.hitObjects.Add(hitObj);
            }
            // Слайдеры (бит 1 установлен)
            else if ((type & 2) == 2 && parts.Length >= 8)
            {
                hitObj.sliderType = parts[5];
                hitObj.repeat = int.Parse(parts[6]);
                hitObj.pixelLength = float.Parse(parts[7]);
                beatmap.hitObjects.Add(hitObj);
            }
            // Спиннеры (бит 3 установлен)
            else if ((type & 8) == 8 && parts.Length >= 5)
            {
                hitObj.endTime = float.Parse(parts[5]) / 1000f;
                beatmap.hitObjects.Add(hitObj);
            }
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
}