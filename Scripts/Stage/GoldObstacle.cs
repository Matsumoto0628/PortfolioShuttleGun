public class GoldObstacle : Obstacle
{
    private const int MAX_HP = 20;

    protected override void Initialize()
    {
        hp = MAX_HP;
    }
}