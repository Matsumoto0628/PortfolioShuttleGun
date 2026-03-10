using UnityEngine;

public class AniamtionEvent : MonoBehaviour
{
    private void ReloadStart()
    {
        SoundManager.Instance.PlaySE("reloadStart");
    }

    private void ReloadEnd()
    {
        SoundManager.Instance.PlaySE("reloadEnd");
    }

    private void Climbing()
    {
        SoundManager.Instance.PlaySE("climbing");
    }

    private void FootSteps()
    {
        SoundManager.Instance.PlaySE("footSteps");
    }
}