using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private const float MAX_SPEED_GROUND = 60f;
    private const float MAX_SPEED_AIR = 12.5f;
    private const float MAX_ACCEL_GROUND = 2500f;
    private const float MAX_ACCEL_AIR = 2500f;
    private const float DRAG_GROUND = 7f;
    private const float DRAG_AIR = 0;
    private const float GRAVITY = 24.53f;
    private Vector3 flatVel;
    public Vector3 FlatVel => flatVel;
    private bool isDash;
    private bool isFall;
    private float heightTimer;
    private float wishHeight;
    private Vector3 moveDirection;
    private MovementState state;
    public MovementState State => state;

    [Header("Ground Check")]
    private const float PLAYER_HEIGHT = 2f;
    private const float GROUND_CHECK_DISTANCE = 0.2f;
    private bool isGround;
    private bool isGroundPrev;

    [Header("Jumping")]
    private const float JUMP_FORCE = 14f;
    private const float JUMP_COOL_DOWN = 0.05f;
    private const float AIR_JUMP_SPAN = 0.25f;
    private float jumpTimer;
    private bool isDoubleJump;

    [Header("Sliding")]
    private const float SLIDE_FORCE = 110f;
    private const float SLIDE_DURATION = 1f;
    private const float SLIDE_THRESHOLD = 5f;
    private const float SLIDE_TIME_MULTIPLIER_THRESHOLD = 20f;
    private const float SLIDE_MAX_TIME_MULTIPLIER = 5f;
    private const float SLIDE_ACCEL_TIMER_MULTIPLIER = 2f;
    private float SLIDE_SPAN = 1.5f;
    private float slideTimer;
    private float slideAcceleTimer;
    private float slideCoolTimer;

    [Header("Wallrunning")]
    private const float WALL_RUN_FORCE = 60f;
    private const float WALL_RUN_DURATION = 4f;
    private const float WALL_RUN_SPAN = 1.5f;
    private const float WALL_RUN_JUMP_UP_FORCE = 14f;
    private const float WALL_RUN_JUMP_SIDE_FORCE = 12f;
    private const float WALL_CHECK_DISTANCE = 0.6f;
    private float wallRunTimer;
    private float wallRunCoolTimer;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool isWallLeft;
    private bool isWallRight;
    public bool IsWallRight => isWallRight;

    [Header("Slope Handling")]
    private const float MAX_SLOPE_ANGLE = 90f;
    private const float SLOPE_CHECK_DISTANCE = 0.3f;
    private RaycastHit slopeHit;

    [Header("Climbing")]
    private const float CLIMB_SPEED = 3f;
    private const float MAX_CLIMB_TIME = 2.5f;
    private float climbTimer;
    private const float CLIMB_JUMP_UP_FORCE = 10f;
    private const float CLIMB_JUMP_BACK_FORCE = 0;
    private const float CLIMB_DETECTION_LENGTH = 0.7f;
    private const float CLIMB_DETECTION_RADIUS = 0.25f;
    private const float MAX_WALL_LOOK_ANGLE = 30f;
    private float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool isWallFront;
    private bool isClimbGrace;
    private const float CLIMB_GRACE_DURATION = 0.5f;

    [Header("Hanging")]
    private bool isWallAbove;
    private RaycastHit aboveWallHit;
    private bool isMantleBusy;
    private const float HANG_DURATION = 0.5f;
    private const float MANTLE_A_DURATION = 0.5f;
    private const float MANTLE_B_DURATION = 0.1f;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform body;
    [SerializeField] private PlayerCamera playerCam;
    [SerializeField] private Animator animator;

    private float horizontalInput;
    private float verticalInput;
    private bool crouchInput;
    private bool jumpInput;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private CapsuleCollider selfCollider;
    private PlayerShooting ps;

    public enum MovementState
    {
        Neutral,
        Crouching,
        Sliding,
        Wallrunning,
        Climbing,
        Hanging
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        playerInput = GetComponent<PlayerInput>();
        selfCollider = GetComponent<CapsuleCollider>();
        ps = GetComponent<PlayerShooting>();

        isDoubleJump = false;
    }

    private void OnEnable()
    {
        if (playerInput == null)
            return;

        playerInput.actions["Move"].performed += OnMove;
        playerInput.actions["Move"].canceled += OnMove;
        playerInput.actions["Jump"].started += OnJump;
        playerInput.actions["Jump"].canceled += OnJump;
        playerInput.actions["Crouch"].started += OnCrouch;
        playerInput.actions["Crouch"].canceled += OnCrouch;
    }

    private void OnDisable()
    {
        if (playerInput == null)
            return;

        playerInput.actions["Move"].performed -= OnMove;
        playerInput.actions["Move"].canceled -= OnMove;
        playerInput.actions["Jump"].started -= OnJump;
        playerInput.actions["Jump"].canceled -= OnJump;
        playerInput.actions["Crouch"].started -= OnCrouch;
        playerInput.actions["Crouch"].canceled -= OnCrouch;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (context.action.name != "Move")
            return;

        Vector2 axis = context.ReadValue<Vector2>();

        horizontalInput = axis.x;
        verticalInput = axis.y;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.action.name != "Jump")
            return;

        switch (context.phase)
        {
            case InputActionPhase.Started:
                JumpHandler();
                jumpInput = true;
                break;

            case InputActionPhase.Canceled:
                jumpInput = false;
                break;
        }
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.action.name != "Crouch")
            return;

        switch (context.phase)
        {
            case InputActionPhase.Started:
                crouchInput = true;
                break;

            case InputActionPhase.Canceled:
                crouchInput = false;
                break;
        }
    }

    private void Update()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, PLAYER_HEIGHT * 0.5f + GROUND_CHECK_DISTANCE);
        isWallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, WALL_CHECK_DISTANCE);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, WALL_CHECK_DISTANCE);
        isWallFront = Physics.SphereCast(transform.position, CLIMB_DETECTION_RADIUS, orientation.forward, out frontWallHit, CLIMB_DETECTION_LENGTH);
        isWallAbove = Physics.SphereCast(transform.position + Vector3.up * PLAYER_HEIGHT * 0.25f, CLIMB_DETECTION_RADIUS, orientation.forward, out aboveWallHit, CLIMB_DETECTION_LENGTH);

        isDash = !ps.IsAim || (verticalInput >= 0.5f);
        isFall = (rb.velocity.y < 0);

        flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        if (isGround)
            jumpTimer += Time.deltaTime;

        if (isGroundPrev != isGround && !isGroundPrev && rb.velocity.y < -3f && !isMantleBusy)
            SoundManager.Instance.PlaySE("jumpEnd");

        isGroundPrev = isGround;

        if (state == MovementState.Sliding)
            SoundManager.Instance.PlayLoopSE("sliding");
        else
            SoundManager.Instance.StopLoopSE("sliding");

        StateHandler();
        AnimationHandler();
        ConstraintsHandler();
        MantleHandler();
        HeightHandler();
        CameraEffect();
        SlideTimerCheck();
        WallRunTimerCheck();
        ClimbTimerCheck();
    }

    private void LateUpdate()
    {
        RotationHandler();

    }

    private void JumpHandler()
    {
        if (CanJump())
            Jump();

        if (state == MovementState.Wallrunning)
            WallRunJump();

        if (CanAirJump())
        {
            Jump();
            isDoubleJump = false;
        }
        else if (isGround || state == MovementState.Wallrunning || state == MovementState.Climbing)
            isDoubleJump = false;
    }

    private void StateHandler()
    {
        if (CanSliding() && crouchInput)
            state = MovementState.Sliding;

        else if (crouchInput || ExistOverhead())
            state = MovementState.Crouching;

        else if (CanHang())
            StartHang();

        else if (CanWallRun())
            StartWallRun();

        else if (CanClimb())
            StartClimb();

        else
            state = MovementState.Neutral;
    }

    private void AnimationHandler()
    {
        if (state == MovementState.Crouching || state == MovementState.Sliding)
            animator.SetBool("isCrouch", true);
        else
            animator.SetBool("isCrouch", false);

        Vector3 localVel = body.InverseTransformDirection(rb.velocity);
        animator.SetFloat("velocityX", localVel.x, 0.1f, Time.deltaTime);
        animator.SetFloat("velocityY", localVel.y, 0.1f, Time.deltaTime);
        animator.SetFloat("velocityZ", localVel.z, 0.1f, Time.deltaTime);

        animator.SetBool("isGround", isGround);
        animator.SetBool("isSlide", state == MovementState.Sliding);
        animator.SetBool("isWallrunLeft", state == MovementState.Wallrunning && isWallLeft);
        animator.SetBool("isWallrunRight", state == MovementState.Wallrunning && isWallRight);
        animator.SetBool("isClimb", state == MovementState.Climbing || isClimbGrace);
        animator.SetBool("isHang", state == MovementState.Hanging);

        if (IsClimbState())
        {
            ApplyUpperLayer(0);
            ApplyArmLayer(0);
        }
        else
        {
            if (ps.IsShootingBusy())
            {
                ApplyUpperLayer(1);
                ApplyArmLayer(0);
            }
            else
            {
                ApplyUpperLayer(0);
                ApplyArmLayer(1);
            }
        }
    }

    private void ApplyUpperLayer(float next)
    {
        float current = animator.GetLayerWeight(1);
        current = Mathf.Lerp(current, next, Time.deltaTime * 5f);
        animator.SetLayerWeight(1, current);
    }
    private void ApplyArmLayer(float next)
    {
        float current = animator.GetLayerWeight(2);
        current = Mathf.Lerp(current, next, Time.deltaTime * 5f);
        animator.SetLayerWeight(2, current);
    }

    private void ConstraintsHandler()
    {
        if (state == MovementState.Hanging)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationY
                | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationY
                | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void MantleHandler()
    {
        if (CanMantle())
            Mantle().Forget();
    }

    private void HeightHandler()
    {
        float height = wishHeight;
        if (state == MovementState.Crouching || state == MovementState.Sliding)
        {
            wishHeight = PLAYER_HEIGHT * 0.65f;
        }
        else
        {
            wishHeight = PLAYER_HEIGHT;
        }

        heightTimer += Time.deltaTime;
        if (wishHeight != height)
        {
            heightTimer = 0;
        }

        heightTimer = Math.Min(heightTimer, 1f);
        selfCollider.height = Mathf.Lerp(selfCollider.height, wishHeight, heightTimer);
        selfCollider.center = new Vector3(0, -1 + height / PLAYER_HEIGHT, 0);
    }

    private void RotationHandler()
    {
        if ((state == MovementState.Neutral || state == MovementState.Crouching) && !ps.IsAim)
        {
            if (moveDirection.magnitude > 0)
            {
                Quaternion next = Quaternion.LookRotation(moveDirection, Vector3.up);
                body.rotation = Quaternion.RotateTowards(body.rotation, next, 360f * Time.deltaTime);
            }
        }
        else
        {
            Quaternion next = Quaternion.LookRotation(orientation.forward, Vector3.up);
            body.rotation = Quaternion.RotateTowards(body.rotation, next, 360f * Time.deltaTime);
        }
    }

    private bool ExistOverhead()
    {
        return Physics.Raycast(transform.position, Vector3.up, PLAYER_HEIGHT * 0.25f + GROUND_CHECK_DISTANCE);
    }

    private void CameraEffect()
    {
        if (ps.IsAim)
            return;

        if (state == MovementState.Wallrunning)
        {
            playerCam.DoFov(90f);
            if (isWallLeft) playerCam.DoTilt(-10f);
            if (isWallRight) playerCam.DoTilt(10f);
            playerCam.DoAdjust(false);
        }
        else
        {
            playerCam.DoFov(60f);
            playerCam.DoTilt(0);
            playerCam.DoAdjust(false);
        }
    }

    private void FixedUpdate()
    {
        float fixedGravity = GravityCalculate(GRAVITY);
        rb.AddForce(Vector3.down * fixedGravity, ForceMode.Force);

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (state == MovementState.Sliding)
            SlidingMovement();

        if (state == MovementState.Wallrunning)
        {
            WallRunningMovement();
            GroundMovement();
        }

        if (state == MovementState.Climbing)
        {
            ClimbingMovement();
            GroundMovement();
        }

        if (isGround)
            GroundMovement();

        if (!isGround && state != MovementState.Wallrunning && state != MovementState.Climbing)
            AirMovement();
    }

    private void GroundMovement()
    {
        float fixedMaxAccel = MaxAccelCalculate(MAX_ACCEL_GROUND);
        Vector3 fixedMoveDirection = OnSlope() ? GetSlopeMoveDirection(moveDirection) : moveDirection;

        rb.drag = DRAG_GROUND;

        Vector3 wishDir = fixedMoveDirection * MAX_SPEED_GROUND;

        Vector3 currentSpeed = Vector3.Project(flatVel, wishDir);

        Vector3 addSpeed = wishDir - currentSpeed;
        addSpeed = addSpeed.normalized * Mathf.Clamp(addSpeed.magnitude, 0, fixedMaxAccel * Time.deltaTime);

        rb.AddForce(addSpeed, ForceMode.Force);
    }

    private void AirMovement()
    {
        rb.drag = DRAG_AIR;

        Vector3 wishDir = moveDirection * MAX_SPEED_AIR;

        Vector3 currentSpeed = Vector3.Project(flatVel, wishDir);

        Vector3 addSpeed = wishDir - currentSpeed;
        addSpeed = addSpeed.normalized * Mathf.Clamp(addSpeed.magnitude, 0, MAX_ACCEL_AIR * Time.deltaTime);

        rb.AddForce(addSpeed, ForceMode.Force);
    }

    private float MaxAccelCalculate(float maxAccel)
    {
        if (state == MovementState.Sliding)
        {
            if (OnSlope())
                return maxAccel * 0.25f;
            else
                return maxAccel * 0.25f;
        }

        if (state == MovementState.Crouching)
            return maxAccel * 0.25f;

        if (state == MovementState.Neutral)
        {
            if (isDash)
                return maxAccel;
            else
                return maxAccel * 0.6f;
        }

        if (state == MovementState.Wallrunning)
            return maxAccel * 0.1f;

        if (state == MovementState.Climbing)
            return maxAccel * 0.5f;

        return maxAccel;
    }

    private float GravityCalculate(float gravity)
    {
        if (OnSlope())
            return 0;

        if (state == MovementState.Wallrunning)
            return gravity * 0.1f;

        return gravity;
    }

    private void Jump()
    {
        jumpTimer = 0;
        rb.velocity = flatVel;

        rb.AddForce(transform.up * JUMP_FORCE, ForceMode.Impulse);
        animator.SetTrigger("jump");
        SoundManager.Instance.PlaySE("jumpStart");
    }

    private void WallRunJump()
    {
        Vector3 wallNormal = isWallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * WALL_RUN_JUMP_UP_FORCE + wallNormal * WALL_RUN_JUMP_SIDE_FORCE;

        rb.velocity = flatVel;

        rb.AddForce(forceToApply, ForceMode.Impulse);
        animator.SetTrigger("jump");
        SoundManager.Instance.PlaySE("jumpStart");
    }

    private void ClimbJump()
    {
        Vector3 forceToApply = transform.up * CLIMB_JUMP_UP_FORCE + frontWallHit.normal * CLIMB_JUMP_BACK_FORCE;

        rb.velocity = flatVel;
        rb.AddForce(forceToApply, ForceMode.Impulse);
        animator.SetTrigger("jump");
        SoundManager.Instance.PlaySE("jumpStart");

        ClimbGrace().Forget();
    }

    private bool CanJump()
    {
        return (JUMP_COOL_DOWN <= jumpTimer) && isGround && !ExistOverhead();
    }

    private bool CanAirJump()
    {
        return isDoubleJump && !isGround && (state != MovementState.Wallrunning);
    }

    public async UniTask EnableDoubleJump()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(AIR_JUMP_SPAN));
        isDoubleJump = true;
    }

    private void SlidingMovement()
    {
        bool forceFinish = (slideTimer >= SLIDE_DURATION);
        float timeDecay = 1 - (slideTimer / SLIDE_DURATION);
        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        float angleMultiplier = angle * 0.1f;
        float timeMultiplier = Mathf.Clamp(slideAcceleTimer, 0, SLIDE_MAX_TIME_MULTIPLIER);
        timeMultiplier = flatVel.magnitude >= SLIDE_TIME_MULTIPLIER_THRESHOLD ? SLIDE_MAX_TIME_MULTIPLIER : timeMultiplier;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection(Vector3.down).normalized * GRAVITY * angleMultiplier * timeMultiplier, ForceMode.Force);

            if (!forceFinish)
            {
                rb.AddForce(GetSlopeMoveDirection(flatVel.normalized) * SLIDE_FORCE * timeDecay * 0.5f, ForceMode.Force);
            }
        }
        else if (!forceFinish)
        {
            rb.AddForce(flatVel.normalized * SLIDE_FORCE * timeDecay, ForceMode.Force);
        }
    }

    private bool CanSliding()
    {
        if (OnSlope())
            return isFall && isGround;

        return (flatVel.magnitude > SLIDE_THRESHOLD) && isGround;
    }

    private void SlideTimerCheck()
    {
        if (state == MovementState.Sliding)
        {
            slideTimer += Time.deltaTime;
            slideCoolTimer = 0;
        }
        else
        {
            slideCoolTimer += Time.deltaTime;

            if (slideCoolTimer >= SLIDE_SPAN)
            {
                slideCoolTimer -= SLIDE_SPAN;
                slideTimer = 0;
            }
        }

        if (state == MovementState.Sliding && OnSlope() && isFall)
            slideAcceleTimer += Time.deltaTime * SLIDE_ACCEL_TIMER_MULTIPLIER;
        else if (isGround && (flatVel.magnitude <= SLIDE_THRESHOLD))
            slideAcceleTimer = 0;
    }

    private void StartWallRun()
    {
        if (state == MovementState.Wallrunning)
            return;

        rb.velocity = flatVel;

        state = MovementState.Wallrunning;
    }

    private void WallRunningMovement()
    {
        Vector3 wallNormal = isWallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        float timeDecay = 1 - (wallRunTimer / WALL_RUN_DURATION);

        rb.AddForce(wallForward * WALL_RUN_FORCE * timeDecay, ForceMode.Force);

        if (!(isWallLeft && horizontalInput > 0) && !(isWallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private bool CanWallRun()
    {
        return (isWallLeft || isWallRight) && verticalInput >= 0.5f && (WALL_RUN_DURATION > wallRunTimer) && !isGround;
    }

    private void WallRunTimerCheck()
    {
        if (state == MovementState.Wallrunning)
        {
            wallRunTimer += Time.deltaTime;
            wallRunCoolTimer = 0;
        }
        else
        {
            wallRunCoolTimer += Time.deltaTime;

            if (WALL_RUN_SPAN <= wallRunCoolTimer)
            {
                wallRunCoolTimer -= WALL_RUN_SPAN;
                wallRunTimer = 0;
            }
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, PLAYER_HEIGHT * 0.5f + SLOPE_CHECK_DISTANCE))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < MAX_SLOPE_ANGLE && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal);
    }

    private void ClimbingMovement()
    {
        float timeDecay = climbTimer / MAX_CLIMB_TIME;
        float inputMultiplier = Mathf.Clamp(verticalInput, 0, 1f);
        rb.velocity = flatVel + Vector3.up * CLIMB_SPEED * timeDecay * inputMultiplier;
    }

    private void ClimbTimerCheck()
    {
        if (state == MovementState.Climbing)
        {
            climbTimer -= Time.deltaTime;
            if (climbTimer <= 0)
            {
                climbTimer = 0;
                if (verticalInput >= 0.5f)
                {
                    ClimbJump();
                }
            }
        }

        if (isGround)
            climbTimer = MAX_CLIMB_TIME;
    }

    private bool CanClimb()
    {
        return isWallFront && wallLookAngle < MAX_WALL_LOOK_ANGLE && climbTimer > 0 && jumpInput;
    }

    private void StartClimb()
    {
        if (state == MovementState.Climbing)
            return;

        state = MovementState.Climbing;
    }

    private bool CanHang()
    {
        return !isWallAbove && isWallFront && wallLookAngle < MAX_WALL_LOOK_ANGLE && !IsLeavingWall() && IsClimbState();
    }

    private bool IsLeavingWall()
    {
        return !isMantleBusy && verticalInput < 0;
    }

    private void StartHang()
    {
        if (state == MovementState.Hanging)
            return;

        state = MovementState.Hanging;
        Hang().Forget();
    }

    private async UniTask Mantle()
    {
        if (isMantleBusy)
            return;

        isMantleBusy = true;

        animator.SetTrigger("mantle");
        SoundManager.Instance.PlaySE("mantle");

        Vector3 nextA = transform.position + orientation.forward * 0.5f + orientation.up * PLAYER_HEIGHT * 0.25f;
        Vector3 nextB = transform.position + orientation.forward + orientation.up * PLAYER_HEIGHT * 0.5f;

        await transform.DOMove(nextA, MANTLE_A_DURATION)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();

        await transform.DOMove(nextB, MANTLE_B_DURATION)
            .SetEase(Ease.OutQuad)
            .AsyncWaitForCompletion();

        isMantleBusy = false;
    }

    private async UniTask Hang()
    {
        if (isMantleBusy)
            return;

        isMantleBusy = true;
        await UniTask.Delay(TimeSpan.FromSeconds(HANG_DURATION));
        isMantleBusy = false;
    }

    private bool CanMantle()
    {
        return state == MovementState.Hanging && !isMantleBusy && verticalInput >= 0.5f;
    }

    private async UniTask ClimbGrace()
    {
        if (isClimbGrace)
            return;

        isClimbGrace = true;
        await UniTask.Delay(TimeSpan.FromSeconds(CLIMB_GRACE_DURATION));
        isClimbGrace = false;
    }

    public bool IsClimbState()
    {
        return state == MovementState.Climbing || isClimbGrace || state == MovementState.Hanging;
    }
}
