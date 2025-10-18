using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SkinManager : MonoBehaviour
{
    [System.Serializable]
    public class Skin
    {
        public string skinName;
        public Sprite hitCircle;
        public Sprite approachCircle;
        public Color hitCircleColor = Color.white;
        public Color approachCircleColor = Color.white;
    }

    // ДОБАВЛЯЕМ КЛАСС OsuSkin
    [System.Serializable]
    public class OsuSkin
    {
        public string skinName;
        public string folderPath;

        // Основные элементы osu! скина
        public Sprite hitCircle;
        public Sprite approachCircle;
        public Sprite cursor;
        public Sprite cursorTrail;
        public Sprite followPoint;

        // Числа для комбо
        public Sprite[] comboNumbers = new Sprite[10];

        // Оценки
        public Sprite hit300;
        public Sprite hit100;
        public Sprite hit50;
        public Sprite hit0; // miss
    }

    public static SkinManager Instance;

    [Header("Available Skins")]
    public List<Skin> skins = new List<Skin>();
    public Skin currentSkin;

    [Header("Osu Skins")]
    public List<OsuSkin> availableOsuSkins = new List<OsuSkin>();
    public OsuSkin currentOsuSkin;

    void Awake()
    {
        Instance = this;
        CreateDefaultSkins();
        LoadOsuSkins();
    }

    void CreateDefaultSkins()
    {
        // Красный скин по умолчанию
        Skin redSkin = new Skin();
        redSkin.skinName = "Red Skin";
        redSkin.hitCircle = CreateCircleSprite(Color.red, 128);
        redSkin.approachCircle = CreateHollowSprite(Color.green, 128);
        redSkin.hitCircleColor = Color.red;
        redSkin.approachCircleColor = Color.green;
        skins.Add(redSkin);

        // Синий скин
        Skin blueSkin = new Skin();
        blueSkin.skinName = "Blue Skin";
        blueSkin.hitCircle = CreateCircleSprite(Color.blue, 128);
        blueSkin.approachCircle = CreateHollowSprite(Color.cyan, 128);
        blueSkin.hitCircleColor = Color.blue;
        blueSkin.approachCircleColor = Color.cyan;
        skins.Add(blueSkin);

        // Устанавливаем скин по умолчанию
        currentSkin = redSkin;

        Debug.Log("Создано простых скинов: " + skins.Count);
    }

    void LoadOsuSkins()
    {
        availableOsuSkins.Clear();

        // Создаем простой osu скин по умолчанию
        OsuSkin defaultOsuSkin = new OsuSkin();
        defaultOsuSkin.skinName = "Default Osu";
        defaultOsuSkin.hitCircle = CreateCircleSprite(Color.white, 128);
        defaultOsuSkin.approachCircle = CreateHollowSprite(Color.green, 128);
        defaultOsuSkin.cursor = CreateCursorSprite();

        availableOsuSkins.Add(defaultOsuSkin);
        currentOsuSkin = defaultOsuSkin;

        Debug.Log("Создан osu скин по умолчанию");
    }

    Sprite CreateCircleSprite(Color color, int size)
    {
        Texture2D tex = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                if (dist < size / 2 - 4)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateHollowSprite(Color color, int size)
    {
        Texture2D tex = new Texture2D(size, size);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                if (dist > size / 2 - 8 && dist < size / 2 - 2)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateCursorSprite()
    {
        Texture2D tex = new Texture2D(32, 32);

        // Простой курсор-крестик
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if (Mathf.Abs(x - 16) < 2 || Mathf.Abs(y - 16) < 2)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    public void SetSkin(string skinName)
    {
        Skin newSkin = skins.Find(s => s.skinName == skinName);
        if (newSkin != null)
        {
            currentSkin = newSkin;
            ApplySkinToAllCircles();
            Debug.Log("Установлен скин: " + skinName);
        }
    }

    public void SetOsuSkin(string skinName)
    {
        OsuSkin newSkin = availableOsuSkins.Find(s => s.skinName == skinName);
        if (newSkin != null)
        {
            currentOsuSkin = newSkin;
            ApplyOsuSkinToAllCircles();
            Debug.Log("Установлен osu скин: " + skinName);
        }
    }

    void ApplySkinToAllCircles()
    {
        HitCircle[] circles = FindObjectsByType<HitCircle>(FindObjectsSortMode.None);
        foreach (HitCircle circle in circles)
        {
            circle.ApplySkin(currentSkin);
        }
    }

    void ApplyOsuSkinToAllCircles()
    {
        HitCircle[] circles = FindObjectsByType<HitCircle>(FindObjectsSortMode.None);
        foreach (HitCircle circle in circles)
        {
            circle.ApplyOsuSkin(currentOsuSkin);
        }
    }

    public List<string> GetSkinNames()
    {
        List<string> names = new List<string>();
        foreach (Skin skin in skins)
        {
            names.Add(skin.skinName);
        }
        return names;
    }

    public List<string> GetOsuSkinNames()
    {
        List<string> names = new List<string>();
        foreach (OsuSkin skin in availableOsuSkins)
        {
            names.Add(skin.skinName);
        }
        return names;
    }
}