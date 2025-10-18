using UnityEngine;
using System.Collections;

public class OsuSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public AudioSource musicSource;

    private BeatmapData currentBeatmap;
    private int currentObjectIndex = 0;
    private float songStartTime;
    private bool isPlaying = false;
    private bool isPrepared = false;

    public void PrepareBeatmap(BeatmapData beatmap)
    {
        currentBeatmap = beatmap;
        currentObjectIndex = 0;
        isPrepared = true;

        Debug.Log($"Карта подготовлена: {beatmap.hitObjects.Count} нот");

        // Автоматически начинаем через 3 секунды
        Invoke("StartGame", 3f);
    }

    public void StartGame()
    {
        if (!isPrepared || currentBeatmap == null)
        {
            Debug.LogError("Карта не подготовлена!");
            return;
        }

        songStartTime = Time.time;

        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Музыка не загружена!");
        }

        isPlaying = true;

        Debug.Log("Игра началась!");
    }

    void Update()
    {
        if (!isPlaying || currentBeatmap == null) return;

        float currentMusicTime = Time.time - songStartTime;

        // Спавним ноты которые должны появиться
        while (currentObjectIndex < currentBeatmap.hitObjects.Count)
        {
            HitObjectData nextObj = currentBeatmap.hitObjects[currentObjectIndex];
            float spawnTime = nextObj.time - 2f; // За 2 секунды до удара

            if (currentMusicTime >= spawnTime)
            {
                SpawnHitObject(nextObj);
                currentObjectIndex++;
            }
            else
            {
                break;
            }
        }

        // Проверяем конец песни
        if (currentObjectIndex >= currentBeatmap.hitObjects.Count &&
            (musicSource == null || !musicSource.isPlaying))
        {
            EndGame();
        }
    }

    void SpawnHitObject(HitObjectData hitObj)
    {
        Vector3 position = ConvertOsuToUnityPosition(hitObj.x, hitObj.y);
        GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity);

        HitCircle circleScript = circle.GetComponent<HitCircle>();
        if (circleScript != null)
        {
            // Устанавливаем время удара
            circleScript.hitTime = hitObj.time;
            circleScript.approachTime = 2f;

            // Применяем osu скин если есть
            if (SkinManager.Instance != null && SkinManager.Instance.currentSkin != null)
            {
                circleScript.ApplyOsuSkin(SkinManager.Instance.currentSkin);
            }
        }
    }

    Vector3 ConvertOsuToUnityPosition(float osuX, float osuY)
    {
        float x = (osuX / 512f) * 10f - 5f;
        float y = (1f - osuY / 384f) * 7.5f - 3.75f;
        return new Vector3(x, y, 0);
    }

    void EndGame()
    {
        isPlaying = false;
        Debug.Log("Игра завершена!");

        if (ScoreManager.Instance != null)
        {
            int score = ScoreManager.Instance.score;
            int maxCombo = ScoreManager.Instance.maxCombo;
            Debug.Log($"Финальный счет: {score} | Макс. комбо: {maxCombo}x");
        }
    }

    public void StopGame()
    {
        isPlaying = false;
        if (musicSource != null)
        {
            musicSource.Stop();
        }
        Debug.Log("Игра остановлена");
    }
}