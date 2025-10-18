using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OsuSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public AudioSource musicSource;
    public Image background;

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

        Debug.Log($"����� ������������: {beatmap.hitObjects.Count} ���");

        Invoke("StartGame", 3f);
    }

    public void StartGame()
    {
        if (!isPrepared || currentBeatmap == null)
        {
            Debug.LogError("����� �� ������������!");
            return;
        }

        songStartTime = Time.time;

        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
        }

        isPlaying = true;
        Debug.Log("���� ��������!");
    }

    void Update()
    {
        if (!isPlaying || currentBeatmap == null) return;

        float currentMusicTime = Time.time - songStartTime;

        while (currentObjectIndex < currentBeatmap.hitObjects.Count)
        {
            HitObjectData nextObj = currentBeatmap.hitObjects[currentObjectIndex];
            float spawnTime = nextObj.time - 2f;

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
            circleScript.hitTime = hitObj.time;
            circleScript.approachTime = 2f;

            // ������� ��� ����� - ���� ������������� ���������� � HitCircle.Start()
            // HitCircle ��� ����� ����� ���� ��������� ����� ApplyCurrentSkin()
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
        Debug.Log("���� ���������!");
    }

    public void StopGame()
    {
        isPlaying = false;
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
}