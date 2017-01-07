using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour {

    [Header("HP")]

    public RectTransform HPSlider;
    public RectTransform healthPoint;
    private List<RectTransform> healthPoints;

    [Header("Water")]

    public Slider waterSlider;
    private Image waterSliderFill;

    [Header("Score")]

    public Text enemyCount;

    [Header("References")]

    public Damageable healthBar;
    public WaterFlask waterFlask;

    void Awake ()
    {
        InitWater();
        InitHealth();
    }

    void InitHealth()
    {
        healthPoints = new List<RectTransform>();
        int HP = healthBar.MaxHP;
        float width = HPSlider.rect.width / HP;
        for(int idx = 0; idx < HP; idx++)
        {
            RectTransform point = Instantiate(healthPoint);
            point.SetParent(HPSlider, false);
            point.offsetMin = Vector2.right * idx * width;
            point.offsetMax = Vector2.right * (idx + 1) * width;
            point.gameObject.SetActive(false);
            healthPoints.Add(point);
        }
    }

    void InitWater()
    {
        waterSlider.maxValue = waterFlask.MaxWater;
        waterSliderFill = waterSlider.transform.GetChild(1).GetComponentInChildren<Image>();
    }

    void UpdateEnemyCount()
    {
        enemyCount.text = GameController.GameManager.EnemyCount.ToString();
    }

    void Update ()
    {
        UpdateWater();
        UpdateHealth();
        UpdateEnemyCount();
    }

    void UpdateHealth ()
    {
        int HP = healthBar.CurrentHP;
        float width = HPSlider.rect.width / healthBar.MaxHP;
        for (int idx = 0; idx < healthPoints.Count; idx++)
        {
            healthPoints[idx].offsetMin = Vector2.right * idx * width;
            healthPoints[idx].offsetMax = Vector2.right * (idx + 1) * width;
            healthPoints[idx].gameObject.SetActive(idx < HP);
        }
    }

    void UpdateWater()
    {
        waterSlider.value = waterFlask.CurrentWater;
        waterSliderFill.color = waterFlask.Frozen ? new Color(0.3f, 0.85f, 0.95f) : new Color(0.05f, 0.45f, 1f);
    }
}
