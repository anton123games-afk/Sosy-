using UnityEngine;

public class HitCircle : MonoBehaviour
{
    [Header("Timing Settings")]
    public float approachTime = 2f;
    public float hitTime;

    private float spawnTime;
    private SpriteRenderer circleRenderer;
    private bool wasClicked = false;

    void Start()
    {
        spawnTime = Time.time;
        circleRenderer = GetComponent<SpriteRenderer>();

        // Применяем текущий скин
        ApplyCurrentSkin();

        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void Update()
    {
        float timeSinceSpawn = Time.time - spawnTime;

        // Круг увеличивается
        float scale = 0.5f + (timeSinceSpawn / approachTime) * 1.5f;
        transform.localScale = Vector3.one * scale;

        // Круг становится прозрачнее
        if (circleRenderer != null)
        {
            Color color = circleRenderer.color;
            color.a = 1f - (timeSinceSpawn / approachTime);
            circleRenderer.color = color;
        }

        // Удаляем если время вышло
        if (timeSinceSpawn > approachTime && !wasClicked)
        {
            Miss();
        }
    }

    public void OnMouseDown()
    {
        if (!wasClicked)
        {
            Hit();
        }
    }

    void Hit()
    {
        wasClicked = true;

        float timeSinceSpawn = Time.time - spawnTime;
        float accuracy = Mathf.Abs(timeSinceSpawn - approachTime);

        int scoreValue = 0;
        Color hitColor = Color.white;

        if (accuracy < 0.1f)
        {
            scoreValue = 300;
            Debug.Log("PERFECT! +300");
            hitColor = Color.yellow;
        }
        else if (accuracy < 0.2f)
        {
            scoreValue = 100;
            Debug.Log("GOOD! +100");
            hitColor = Color.green;
        }
        else
        {
            scoreValue = 50;
            Debug.Log("OK! +50");
            hitColor = Color.blue;
        }

        // Меняем цвет при попадании
        if (circleRenderer != null)
        {
            circleRenderer.color = hitColor;
        }

        // Добавляем очки
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        // Увеличиваем круг при попадании
        transform.localScale = Vector3.one * 1.5f;

        // Удаляем через 0.1 секунды
        Destroy(gameObject, 0.1f);
    }

    void Miss()
    {
        Debug.Log("MISS!");
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddMiss();
        }
        Destroy(gameObject);
    }

    // Метод для обычных скинов
    public void ApplySkin(SkinManager.Skin skin)
    {
        if (circleRenderer != null)
        {
            if (skin.hitCircle != null)
            {
                circleRenderer.sprite = skin.hitCircle;
            }
            circleRenderer.color = skin.hitCircleColor;
        }
    }

    // Метод для osu скинов
    public void ApplyOsuSkin(SkinManager.OsuSkin skin)
    {
        if (circleRenderer != null && skin.hitCircle != null)
        {
            circleRenderer.sprite = skin.hitCircle;
            circleRenderer.color = Color.white;
        }
    }

    // Применяем текущий скин
    void ApplyCurrentSkin()
    {
        if (SkinManager.Instance != null)
        {
            // Сначала пробуем применить osu скин
            if (SkinManager.Instance.currentOsuSkin != null)
            {
                ApplyOsuSkin(SkinManager.Instance.currentOsuSkin);
            }
            // Если нет osu скина, применяем обычный
            else if (SkinManager.Instance.currentSkin != null)
            {
                ApplySkin(SkinManager.Instance.currentSkin);
            }
            else
            {
                // Если скинов нет - используем красный по умолчанию
                circleRenderer.color = Color.red;
            }
        }
        else
        {
            circleRenderer.color = Color.red;
        }
    }
}