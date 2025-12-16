using UnityEngine.AI;

public class StageObstacle : Obstacle
{
    private NavMeshObstacle navMeshObstacle;
    private const int MAX_HP = 20;

    protected override void Initialize()
    {
        hp = MAX_HP;
        navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    protected override void Explosion()
    {
        base.Explosion();
        navMeshObstacle.enabled = false;
        SoundManager.Instance.PlaySE("glass");
    }
}