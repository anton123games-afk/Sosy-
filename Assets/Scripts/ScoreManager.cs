using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    
    [Header("Score Settings")]
    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;
    
    [Header("UI Elements")]
    public Text scoreText;
    public Text comboText;
    
    void Awake()
    {
        Instance = this;
        UpdateUI();
    }
    
    public void AddScore(int points)
    {
        score += points;
        combo++;
        
        if (combo > maxCombo)
            maxCombo = combo;
            
        UpdateUI();
        
        Debug.Log($"Очки: +{points} | Комбо: {combo}x | Всего: {score}");
    }
    
    public void AddMiss()
    {
        combo = 0;
        UpdateUI();
        Debug.Log("Промах! Комбо сброшено");
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
            
        if (comboText != null)
        {
            if (combo > 0)
                comboText.text = $"{combo}x COMBO!";
            else
                comboText.text = "";
        }
    }
    
    // Метод для сброса счета (если понадобится)
    public void ResetScore()
    {
        score = 0;
        combo = 0;
        maxCombo = 0;
        UpdateUI();
    }
}