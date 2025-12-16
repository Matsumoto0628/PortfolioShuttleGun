using System;
using UnityEngine;
using UniRx;

public abstract class Gun : MonoBehaviour, IGun
{
    protected Vector2 recoil;
    public Vector2 Recoil => recoil;
    protected IReactiveProperty<int> ammo = new ReactiveProperty<int>();
    public IReadOnlyReactiveProperty<int> Ammo => ammo;
    protected int maxAmmo;
    public int MaxAmmo => maxAmmo;
    private const float RAY_DISTANCE = 30f;

    public void Fire(Transform aim, Transform muzzle)
    {
        if (!CanFire())
            return;

        Vector3 direction = (aim.position - muzzle.position).normalized;
        RaycastHit[] hits = Physics.RaycastAll(muzzle.position, direction, RAY_DISTANCE);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        Impact(hits);
        Apply();
        Flash();
        PlaySE();
    }

    public abstract void Reload();
    protected abstract void Apply();
    protected abstract void PlaySE();
    protected abstract void Flash();
    protected abstract void Impact(RaycastHit[] hits);
    protected abstract bool CanFire();
}