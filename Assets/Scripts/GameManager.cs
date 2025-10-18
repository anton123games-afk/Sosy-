using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    void Awake()
    {
        Instance = this;
        Debug.Log("Игра запущена!");
    }
}