using UnityEngine;
using System.Collections.Generic;

public class SkinManager : MonoBehaviour
{
    [System.Serializable]
    public class Skin
    {
        public string skinName;
        public Sprite hitCircle;
        public Color hitCircleColor = Color.white;
    }

    public static SkinManager Instance;
    
    [Header("Available Skins")]
    public List<Skin> skins = new List<Skin>();
    public Skin currentSkin;
    
    void Awake()
    {
        Instance = this;
        CreateDefaultSkins();
    }
    
    void CreateDefaultSkins()
    {
        // Красный скин по умолчанию
        Skin redSkin = new Skin();
        redSkin.skinName = "Red Skin";
        redSkin.hitCircle = CreateCircleSprite(Color.red, 128);
        redSkin.hitCircleColor = Color.red;
        skins.Add(redSkin);
        
        // Синий скин
        Skin blueSkin = new Skin();
        blueSkin.skinName = "Blue Skin";
        blueSkin.hitCircle = CreateCircleSprite(Color.blue, 128);
        blueSkin.hitCircleColor = Color.blue;
        skins.Add(blueSkin);
        
        // Зеленый скин
        Skin greenSkin = new Skin();
        greenSkin.skinName = "Green Skin";
        greenSkin.hitCircle = CreateCircleSprite(Color.green, 128);
        greenSkin.hitCircleColor = Color.green;
        skins.Add(greenSkin);
        
        // Желтый скин
        Skin yellowSkin = new Skin();
        yellowSkin.skinName = "Yellow Skin";
        yellowSkin.hitCircle = CreateCircleSprite(Color.yellow, 128);
        yellowSkin.hitCircleColor = Color.yellow;
        skins.Add(yellowSkin);
        
        // Устанавливаем скин по умолчанию
        currentSkin = redSkin;
        
        Debug.Log("Создано скинов: " + skins.Count);
    }
    
    Sprite CreateCircleSprite(Color color, int size)
    {
        Texture2D tex = new Texture2D(size, size);
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size/2, size/2));
                if (dist < size/2 - 4)
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
    
    void ApplySkinToAllCircles()
    {
        // Применяем скин ко всем кругам на сцене
        HitCircle[] circles = FindObjectsByType<HitCircle>(FindObjectsSortMode.None);
        foreach (HitCircle circle in circles)
        {
            circle.ApplySkin(currentSkin);
        }
        Debug.Log("Скин применен к " + circles.Length + " кругам");
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
}