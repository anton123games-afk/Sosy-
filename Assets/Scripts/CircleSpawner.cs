using UnityEngine;

public class CircleSpawner : MonoBehaviour
{
    public GameObject circlePrefab;
    public float spawnInterval = 2f;
    
    private float nextSpawnTime;
    
    void Start()
    {
        nextSpawnTime = Time.time + 1f;
    }
    
    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnCircle();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnCircle()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-4f, 4f),
            Random.Range(-3f, 3f),
            0
        );
        
        GameObject newCircle = Instantiate(circlePrefab, randomPos, Quaternion.identity);
        
        // Автоматически применяем текущий скин к новому кругу
        ApplySkinToCircle(newCircle);
    }
    
    void ApplySkinToCircle(GameObject circleObject)
    {
        if (SkinManager.Instance != null && SkinManager.Instance.currentSkin != null)
        {
            HitCircle hitCircle = circleObject.GetComponent<HitCircle>();
            if (hitCircle != null)
            {
                hitCircle.ApplySkin(SkinManager.Instance.currentSkin);
            }
        }
    }
}