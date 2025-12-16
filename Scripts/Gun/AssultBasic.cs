using UnityEngine;

public class AssultBasic : Gun
{
    [SerializeField] private ParticleSystem flash;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private GameObject impactFirePrefab;

    private float fireCoolTimer;
    private const float FIRE_SPAN = 0.05f;
    
    private void Start()
    {
        maxAmmo = 24;
        ammo.Value = maxAmmo;
    }

    private void Update()
    {
        fireCoolTimer += Time.deltaTime;
    }

    protected override void Apply()
    {
        fireCoolTimer = 0;
        recoil = new Vector2(Random.Range(-0.001f, 0.001f), Random.Range(0.0025f, 0.005f));
        ammo.Value--;
    }

    public override void Reload()
    {
        ammo.Value = maxAmmo;
    }

    protected override void Flash()
    {
        flash.Play();
    }

    protected override void Impact(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                GenerateImpact(impactPrefab, hit);
                hit.collider.gameObject.GetComponent<Obstacle>().TakeDamage(1);
                break;
            }

            if (hit.collider.CompareTag("Untagged"))
            {
                GenerateImpact(impactPrefab, hit);
                break;
            }

            if (hit.collider.CompareTag("Enemy"))
            {
                GenerateImpact(impactFirePrefab, hit);
                hit.collider.gameObject.GetComponent<Enemy>().TakeDamage(7);
                SoundManager.Instance.PlaySE("assultHit", 10);
                break;
            }
        }
    }

    protected override bool CanFire()
    {
        return fireCoolTimer >= FIRE_SPAN && ammo.Value >= 1;
    }

    private void GenerateImpact(GameObject impactPrefab, RaycastHit hit)
    {
        GameObject impact = Instantiate(impactPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
        impact.transform.SetParent(hit.transform);
        Destroy(impact, 5f);
    }

    protected override void PlaySE()
    {
        SoundManager.Instance.PlaySE("assultFire", 10);
    }
}