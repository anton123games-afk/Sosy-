using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkinChangerUI : MonoBehaviour
{
    public Button redSkinButton;
    public Button blueSkinButton;
    public Button purpleSkinButton;
    
    void Start()
    {
        // Назначаем кнопкам функции
        if (redSkinButton != null)
            redSkinButton.onClick.AddListener(() => SetSkin("Red Skin"));
            
        if (blueSkinButton != null)
            blueSkinButton.onClick.AddListener(() => SetSkin("Blue Skin"));
            
        if (purpleSkinButton != null)
            purpleSkinButton.onClick.AddListener(() => SetSkin("Purple Skin"));
    }
    
    void SetSkin(string skinName)
    {
        if (SkinManager.Instance != null)
        {
            SkinManager.Instance.SetSkin(skinName);
        }
    }
}