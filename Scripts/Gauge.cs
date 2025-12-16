using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;

public class Gauge : MonoBehaviour
{
    [SerializeField] private Image fillUI;
    [SerializeField] private Image graceUI;
    [SerializeField] private Color fillColor;
    [SerializeField] private Color graceColor;
    private Material fillMat;
    private Material graceMat;
    private const float GRACE_DURATION = 1f;

    private void Start()
    {
        fillMat = Instantiate(fillUI.material);
        graceMat = Instantiate(graceUI.material);
        fillUI.material = fillMat;
        graceUI.material = graceMat;
        fillMat.SetColor("_Color", fillColor);
        graceMat.SetColor("_Color", graceColor);
    }

    public void Apply(float ratio)
    {
        fillMat.SetFloat("_FillAmount", ratio);
        GraceLerp(ratio).Forget();
    }
    
    private async UniTask GraceLerp(float next)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(GRACE_DURATION));

        float current = graceMat.GetFloat("_FillAmount");

        while (Mathf.Abs(current - next) > 0.001f)
        {
            current = Mathf.Lerp(current, next, Time.deltaTime);
            graceMat.SetFloat("_FillAmount", current);
            await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        graceMat.SetFloat("_FillAmount", next);
    }
}
