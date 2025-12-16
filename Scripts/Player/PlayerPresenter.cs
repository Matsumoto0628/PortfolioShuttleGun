using UnityEngine;
using UniRx;

public class PlayerPresenter : MonoBehaviour
{
    [SerializeField] private Gauge healthGauge;
    [SerializeField] private Gauge[] shieldGauges;
    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();

        player.Health
            .Subscribe(x =>
            {
                float ratio = (float)x / player.MaxHealth;
                healthGauge.Apply(ratio);
            });

        player.Shield
            .Subscribe(x =>
            {
                float ratio = (float)x / player.MaxShield;
                int gaugeCount = player.MaxShield / 20;
                float perGauge = 1f / gaugeCount;

                for (int i = 0; i < gaugeCount; i++)
                {
                    float start = perGauge * i;
                    float end = perGauge * (i + 1);
                    float localFill = Mathf.InverseLerp(start, end, ratio);
                    shieldGauges[i].Apply(localFill);
                }
            });
    }
}