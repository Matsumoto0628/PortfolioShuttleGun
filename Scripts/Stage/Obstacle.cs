using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Obstacle : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject brokenPrefab;
    [SerializeField] private float particleSize = 1f;
    private MeshRenderer mesh;
    private Collider selfCollider;
    private AnimationCurve ease = AnimationCurve.EaseInOut(
        timeStart: 0,
        valueStart: 0,
        timeEnd: 1f,
        valueEnd: 1f
    );
    private Vector3 initPos;

    protected int hp;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        selfCollider = GetComponent<Collider>();
        initPos = transform.position;
        Initialize();
    }

    protected abstract void Initialize();

    public void TakeDamage(int value)
    {
        hp -= value;
        Shake(1f).Forget();
        if (hp <= 0)
        {
            Explosion();
        }
    }

    protected virtual void Explosion()
    {
        mesh.enabled = false;
        selfCollider.enabled = false;
        GameObject broken = Instantiate(brokenPrefab, transform.position, Quaternion.identity);
        broken.transform.localScale *= particleSize;
        Destroy(broken, 3f);
    }

    private async UniTask Shake(float multiplier, float dur = 0.1f)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        while (elapsedTime < dur)
        {
            await UniTask.WaitUntil(() => Time.timeScale == 1f);
            elapsedTime += Time.deltaTime;
            float strength = ease.Evaluate(elapsedTime / dur);
            transform.position = startPos + UnityEngine.Random.insideUnitSphere * strength * multiplier * 0.1f;
            await UniTask.Yield();
        }

        transform.position = initPos;
    }
}