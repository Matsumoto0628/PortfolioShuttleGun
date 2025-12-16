using UnityEngine;
using UniRx;
using System;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject explosionPrefab;
    private const int MAX_HEALTH = 100;
    public int MaxHealth => MAX_HEALTH;
    private ReactiveProperty<int> health = new ReactiveProperty<int>(MAX_HEALTH);
    public IReadOnlyReactiveProperty<int> Health => health;
    private Subject<Unit> onDamage = new Subject<Unit>();
    public IObservable<Unit> OnDamage => onDamage;

    public void TakeDamage(int value)
    {
        onDamage.OnNext(Unit.Default);
        health.Value = Mathf.Clamp(health.Value - value, 0, MAX_HEALTH);

        if (health.Value <= 0)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            SoundManager.Instance.PlaySE("explosion");
            Destroy(explosion, 3f);
            Destroy(gameObject);
        }
    }

    private void Oestroy()
    {
        health?.Dispose();
        onDamage?.OnCompleted();
        onDamage?.Dispose();       
    }
}