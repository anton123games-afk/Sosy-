using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BeatmapData
{
    public string title;
    public string artist;
    public string difficulty;
    public string audioFile;
    public List<HitObjectData> hitObjects = new List<HitObjectData>();
}

[System.Serializable]
public class HitObjectData
{
    public float x;
    public float y;
    public float time;
    public int type;
    public int soundType;

    // Для слайдеров
    public string sliderType;
    public int repeat;
    public float pixelLength;

    // Для спиннеров
    public float endTime;
}