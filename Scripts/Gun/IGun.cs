using UnityEngine;
using UniRx;

public interface IGun
{
    public Vector2 Recoil { get; }
    public IReadOnlyReactiveProperty<int> Ammo { get; }
    public int MaxAmmo { get; }
    public void Fire(Transform aim, Transform muzzle);
    public abstract void Reload();
}