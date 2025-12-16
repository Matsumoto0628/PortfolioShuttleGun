using UnityEngine;

public class AssultInfinity : Gun
{
    [SerializeField] private ParticleSystem flash;
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private GameObject impactBloodPrefab;

    private float fireCoolTimer;
    private const float FIRE_SPAN = 0.05f;
    private int ammo = MAX_AMMO;
    private const int MAX_AMMO = 24;

    private void Update()
    {
        fireCoolTimer += Time.deltaTime;
    }

    protected override void Apply()
    {
        fireCoolTimer = 0;
        recoil = new Vector2(Random.Range(-0.001f, 0.001f), Random.Range(0.0025f, 0.005f));
        ammo--;
    }

    public override void Reload()
    {
        ammo = MAX_AMMO;
    }

    protected override void Flash()
    {
        flash.Play();
    }

    protected override void Impact(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                GenerateImpact(impactBloodPrefab, hit);
                hit.collider.gameObject.GetComponent<Player>().TakeDamage(7);
                break;
            }

            if (hit.collider.CompareTag("Untagged"))
            {
                GenerateImpact(impactPrefab, hit);
                break;
            }
        }
    }

    protected override bool CanFire()
    {
        return fireCoolTimer >= FIRE_SPAN;
    }

    private void GenerateImpact(GameObject impactPrefab, RaycastHit hit)
    {
        GameObject impact = Instantiate(impactPrefab, hit.point + hit.normal * 0.1f, Quaternion.LookRotation(hit.normal));
        impact.transform.SetParent(hit.transform);
        Destroy(impact, 5f);
    }

    protected override void PlaySE()
    {
        SoundManager.Instance.PlaySE("assultFire");
    }
}