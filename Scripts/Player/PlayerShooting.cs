using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private PlayerCamera playerCam;
    [SerializeField] private Animator animator;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private TwoBoneIKConstraint rigHandL;
    [SerializeField] private Transform rigAim;
    [SerializeField] private Transform aim;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Gun currentGun;
    [SerializeField] private GunHolder gunHolder;
    
    private PlayerMovement pm;
    private PlayerInput playerInput;

    private bool fireInput;
    private bool aimInput;
    public bool IsAim => aimInput;
    private bool isReload;
    private Vector3 rigAimInitPos;
    private Vector3 aimInitPos;
    private Vector2 recoil;
    private CancellationTokenSource reloadCts;
    public Gun CurrentGun => currentGun;

    private const float INIT_AIM_Y = -5f;
    private const float MAX_RECOIL_Y = 1f;
    private const float MIN_RECOIL_Y = -2f;
    private const float RELOAD_DURATION = 2f;

    private void Awake()
    {
        pm = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();
        rigAimInitPos = rigAim.localPosition;
        aimInitPos = aim.localPosition;
        gunHolder.SetOnTransit(() => {
            rigBuilder.Build();
            rigBuilder.Evaluate(Time.deltaTime);
        });
    }

    private void OnEnable()
    {
        if (playerInput == null)
            return;

        playerInput.actions["Look"].performed += OnLook;
        playerInput.actions["Look"].canceled += OnLook;
        playerInput.actions["Fire"].started += OnFire;
        playerInput.actions["Fire"].canceled += OnFire;
        playerInput.actions["Aim"].started += OnAim;
        playerInput.actions["Aim"].canceled += OnAim;
        playerInput.actions["Reload"].started += OnReload;
        playerInput.actions["Reload"].canceled += OnReload;
    }

    private void OnDisable()
    {
        if (playerInput == null)
            return;

        playerInput.actions["Look"].performed -= OnLook;
        playerInput.actions["Look"].canceled -= OnLook;
        playerInput.actions["Fire"].started -= OnFire;
        playerInput.actions["Fire"].canceled -= OnFire;
        playerInput.actions["Aim"].started -= OnAim;
        playerInput.actions["Aim"].canceled -= OnAim;
        playerInput.actions["Reload"].started -= OnReload;
        playerInput.actions["Reload"].canceled -= OnReload;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.action.name != "Look")
            return;

        Vector2 axis = context.ReadValue<Vector2>();

        playerCam.DoRot(axis.x, axis.y, aimInput);
            
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        if (context.action.name != "Fire")
            return;

        switch (context.phase)
        {
            case InputActionPhase.Started:
                fireInput = true;
                ReloadCancel();
                break;

            case InputActionPhase.Canceled:
                fireInput = false;
                break;
        }
    }

    private void OnAim(InputAction.CallbackContext context)
    {
        if (context.action.name != "Aim")
            return;

        switch (context.phase)
        {
            case InputActionPhase.Started:
                aimInput = true;
                ReloadCancel();
                break;

            case InputActionPhase.Canceled:
                aimInput = false;
                break;
        }
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (context.action.name != "Reload")
            return;

        switch (context.phase)
        {
            case InputActionPhase.Started:
                aimInput = false;
                fireInput = false;
                Reload().Forget();
                break;
        }
    }

    void Update()
    {
        if (CanFire())
        {
            currentGun.Fire(aim, muzzle);
            float recoilMultiplier = aimInput ? 0.5f : 2f;
            ApplyRecoil(currentGun.Recoil.x * recoilMultiplier, currentGun.Recoil.y * recoilMultiplier);
        }
        else
            recoil = Vector2.zero;

        if (pm.IsClimbState())
        {
            ReloadCancel();
        }

        if (IsShootingBusy())
        {
            gunHolder.Transit(GunHoldState.AIMING);
        }
        else
        {
            gunHolder.Transit(GunHoldState.NEUTRAL);
        }

        AimHandler(rigAim, rigAimInitPos, INIT_AIM_Y);
        AimHandler(aim, aimInitPos, aimInitPos.y);
        playerCam.transform.LookAt(aim);
        AnimationHandler();
        CameraEffect();
    }

    private void AnimationHandler()
    {
        animator.SetBool("isFire", fireInput);
        animator.SetBool("isReload", isReload);

        if (aimInput || fireInput)
            Constraint(1f);
        else
            Constraint(0);

        if (isReload)
            rigHandL.weight = 0;
        else
            rigHandL.weight = 1;

        if (pm.IsClimbState())
            rigBuilder.layers[1].rig.weight = 0;
        else
            rigBuilder.layers[1].rig.weight = 1;
    }

    private void Constraint(float next)
    {
        const float STEP = 5f;
        float current = rigBuilder.layers[0].rig.weight;
        current = Mathf.Lerp(current, next, Time.deltaTime * STEP);
        rigBuilder.layers[0].rig.weight = current;
    }

    private void AimHandler(Transform curret, Vector3 initPos, float initAimY)
    {
        const float STEP = 5f;
        Vector3 next;
        if (fireInput)
        {
            next = new Vector3(initPos.x + recoil.x, initAimY + recoil.y, initPos.z);
        }
        else
        {
            next = initPos;
        }
        curret.localPosition = Vector3.Lerp(curret.localPosition, next, Time.deltaTime * STEP);
    }

    private void CameraEffect()
    {
        if (aimInput)
        {
            playerCam.DoFov(40f);
            playerCam.DoTilt(0);
            playerCam.DoAdjust(true, pm.IsWallRight);
        }
    }

    private bool CanFire()
    {
        return fireInput && animator.GetLayerWeight(1) >= 0.9f;
    }

    private void ApplyRecoil(float x, float y)
    {
        recoil = new Vector2(recoil.x + x, Mathf.Clamp(recoil.y + y, MIN_RECOIL_Y, MAX_RECOIL_Y));
    }

    public bool IsShootingBusy()
    {
        return aimInput || fireInput || isReload;
    }

    private async UniTask Reload()
    {
        if (isReload)
            return;

        isReload = true;
        reloadCts?.Cancel();
        reloadCts = new CancellationTokenSource();

        await UniTask.Delay(TimeSpan.FromSeconds(RELOAD_DURATION), cancellationToken: reloadCts.Token);

        isReload = false;
        currentGun.Reload();
    }

    private void ReloadCancel()
    {
        reloadCts?.Cancel();
        isReload = false;
    }
}
