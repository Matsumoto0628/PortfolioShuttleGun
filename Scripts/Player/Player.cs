using UnityEngine;
using UniRx;

public class Player : MonoBehaviour, IDamageable
{
    private const int MAX_HEALTH = 100;
    public int MaxHealth => MAX_HEALTH;
    private const int MAX_SHIELD = 100;
    public int MaxShield => MAX_SHIELD;
    private ReactiveProperty<int> health = new ReactiveProperty<int>(MAX_HEALTH);
    public IReadOnlyReactiveProperty<int> Health => health;
    private ReactiveProperty<int> shield = new ReactiveProperty<int>(MAX_SHIELD);
    public IReadOnlyReactiveProperty<int> Shield => shield;

    public void TakeDamage(int value)
    {
        if (shield.Value > 0)
        {
            int prev = shield.Value;
            int diff = value - shield.Value;
            shield.Value = Mathf.Clamp(shield.Value - value, 0, MAX_SHIELD);
            value = diff > 0 ? value - prev : 0;
        }
        health.Value = Mathf.Clamp(health.Value - value, 0, MAX_HEALTH);
    }
}