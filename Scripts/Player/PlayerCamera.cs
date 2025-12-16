using DG.Tweening;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private float lookInputX;
    private float lookInputY;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;

    private float rotationX;
    private float rotationY;

    private float currentFOV;
    private float currentZTilt;
    private bool currentApproach;
    private bool currentWallRight;

    private Camera cam;

    private const float SENS_X = 10f;
    private const float SENS_Y = 10f;
    private const float SENS_MULTIPLIER = 0.25f;
    private const float MIN_ROT_X = -20f;
    private const float MAX_ROT_X = 45f;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentFOV = cam.fieldOfView;
        currentZTilt = transform.localRotation.z;
    }

    private void Update()
    {
        rotationY += lookInputX;

        rotationX -= lookInputY;
        rotationX = Mathf.Clamp(rotationX, MIN_ROT_X, MAX_ROT_X);

        camHolder.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    public void DoRot(float axisX, float axisY, bool isSlow)
    {
        if (isSlow)
        {
            lookInputX = axisX * Time.deltaTime * SENS_X * SENS_MULTIPLIER;
            lookInputY = axisY * Time.deltaTime * SENS_Y * SENS_MULTIPLIER;
        }
        else
        {
            lookInputX = axisX * Time.deltaTime * SENS_X;
            lookInputY = axisY * Time.deltaTime * SENS_Y;
        }
    }

    public void DoFov(float endValue)
    {
        if (currentFOV == endValue)
            return;

        cam.DOFieldOfView(endValue, 0.25f);
        currentFOV = endValue;
    }

    public void DoTilt(float zTilt)
    {
        if (currentZTilt == zTilt)
            return;

        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
        currentZTilt = zTilt;
    }

    public void DoAdjust(bool approach, bool wallRight = false)
    {
        if (currentApproach == approach && currentWallRight == wallRight)
            return;

        float ofstX = wallRight ? -0.5f : 0.5f;

        if (approach)
        {
            transform.DOLocalMove(new Vector3(ofstX, 0.75f, -5f), 0.25f);
        }
        else
        {
            transform.DOLocalMove(new Vector3(0, 1.5f, -5f), 0.25f);
        }

        currentApproach = approach;
        currentWallRight = wallRight;
    }
}
