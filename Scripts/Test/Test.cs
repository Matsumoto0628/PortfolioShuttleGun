public class Test
{
    private bool IsBigger(int value)
    {
        for (int i = 0; i < 5; i++)
        {
            if (value % 3 == 0)
            {
                value += 10;
            }
            else
            {
                value += 5;
            }
        }

        return (value > 100);
    }
}