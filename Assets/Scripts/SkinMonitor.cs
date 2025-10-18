using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SkinMonitor : MonoBehaviour
{
    [Header("Settings")]
    public string skinsFolder = "Skins";
    public float checkInterval = 3f;
    
    [Header("Debug")]
    public bool enableMonitoring = true;
    
    private SkinManager skinManager;
    private float lastCheckTime;
    private List<string> knownSkinFolders = new List<string>();
    
    void Start()
    {
        // Исправленная строка - используем новый метод
        skinManager = FindFirstObjectByType<SkinManager>();
        
        if (skinManager == null)
        {
            Debug.LogError("SkinManager не найден на сцене!");
            return;
        }
        
        // Создаем папку для скинов если её нет
        string skinsPath = Path.Combine(Application.dataPath, skinsFolder);
        if (!Directory.Exists(skinsPath))
        {
            Directory.CreateDirectory(skinsPath);
            Debug.Log("Создана папка для скинов: " + skinsPath);
        }
        
        // Начальное сканирование
        ScanForSkins();
        
        Debug.Log("Мониторинг скинов запущен. Кидайте .osk файлы в папку: " + skinsPath);
    }
    
    void Update()
    {
        if (!enableMonitoring || skinManager == null) return;
        
        // Проверяем изменения каждые checkInterval секунд
        if (Time.time - lastCheckTime > checkInterval)
        {
            ScanForSkins();
            lastCheckTime = Time.time;
        }
    }
    
    void ScanForSkins()
    {
        string skinsPath = Path.Combine(Application.dataPath, skinsFolder);
        
        if (!Directory.Exists(skinsPath)) return;
        
        // Ищем .osk файлы
        string[] oskFiles = Directory.GetFiles(skinsPath, "*.osk");
        foreach (string oskFile in oskFiles)
        {
            StartCoroutine(ProcessOskFile(oskFile));
        }
    }
    
    IEnumerator ProcessOskFile(string oskPath)
    {
        Debug.Log("Найден .osk файл: " + Path.GetFileName(oskPath));
        
        try
        {
            string fileName = Path.GetFileNameWithoutExtension(oskPath);
            string extractPath = Path.Combine(Application.dataPath, skinsFolder, fileName);
            
            // Создаем папку для распаковки
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }
            
            // Распаковываем ZIP архив (.osk это просто zip)
            System.IO.Compression.ZipFile.ExtractToDirectory(oskPath, extractPath, true);
            
            Debug.Log("Скин распакован: " + fileName);
            
            // Переименовываем обработанный .osk файл
            string processedPath = oskPath + ".processed";
            File.Move(oskPath, processedPath);
            
            Debug.Log("Скин успешно импортирован: " + fileName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Ошибка обработки .osk файла: " + e.Message);
        }
        
        yield return null;
    }
    
    // Метод для ручной проверки
    [ContextMenu("Проверить скины сейчас")]
    public void CheckSkinsNow()
    {
        ScanForSkins();
        Debug.Log("Ручная проверка скинов выполнена");
    }
}