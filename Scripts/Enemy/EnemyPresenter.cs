using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class EnemyPresenter : MonoBehaviour
{
    [SerializeField] private Transform canvas;
    [SerializeField] private Image healthUI;
    private Enemy enemy;

    private void Start()
    {
        enemy = GetComponent<Enemy>();

        enemy.Health
            .Subscribe(x =>
            {
                float ratio = (float)x / enemy.MaxHealth;
                healthUI.fillAmount = ratio;
            });
    }

    private void Update()
    {
        canvas.LookAt(Camera.main.transform);
    }
}