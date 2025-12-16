using System;
using UnityEngine;

public class GunHolder : MonoBehaviour
{
    [SerializeField] private Transform neutral;
    [SerializeField] private Transform aiming;

    private Vector3 currentPos;
    private Quaternion currentRot;
    private Action onTransit;

    private const float LERP_MULTIPLIER = 10f;

    public void Transit(GunHoldState gunHoldState)
    {
        switch (gunHoldState)
        {
            case GunHoldState.NEUTRAL:
                currentPos = neutral.position;
                currentRot = neutral.rotation;
                break;

            case GunHoldState.AIMING:
                currentPos = aiming.position;
                currentRot = aiming.rotation;
                break;
        }

        transform.position = Vector3.Lerp(transform.position, currentPos, Time.deltaTime * LERP_MULTIPLIER);
        transform.rotation = Quaternion.Lerp(transform.rotation, currentRot, Time.deltaTime * LERP_MULTIPLIER);

        if (!IsComplete())
        {
            onTransit?.Invoke();
        }
    }

    private bool IsComplete()
    {
        float posDiff = Vector3.Distance(transform.position, currentPos);
        float rotDiff = Quaternion.Angle(transform.rotation, currentRot);

        return posDiff < 0.01f && rotDiff < 1f;
    }

    public void SetOnTransit(Action action)
    {
        onTransit = action;
    }
}