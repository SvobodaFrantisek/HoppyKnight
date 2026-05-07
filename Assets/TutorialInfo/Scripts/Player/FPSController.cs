using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    // --------------------
    // Nastaveni pohybu
    // --------------------
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;
    public float groundedStickForce = 5f;
    public float jumpTimeout = 0.5f;

    public bool hasDoubleJump = false;
    public int jumpCount = 0;

    public float climbSpeed = 4f;
    public float wallCheckDistance = 0.5f;
    public bool isClimbing = false;

    // --------------------
    // Dodge
    // --------------------
    [Header("Dodge Settings")]
    public float dodgeDistance = 2.5f;
    public float dodgeDuration = 0.18f;
    public float dodgeCooldown = 0.6f;

    [Header("Animation")]
    public Animator animator;
    public bool inputLocked = false;
    public bool movementLocked = false;

    // --------------------
    // Reference a stav behem hry
    // --------------------
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dodgeAction;
    private CharacterController cc;

    private Vector3 velocity;
    private MovingPlatform currentPlatform;
    private Vector3 dodgeDirection;

    private float animationMoveSpeed;
    private float groundedAnimationGraceTime = 0.15f;
    private float groundedAnimationTimer;
    private float jumpTimeoutDelta;
    private float dodgeTimer;
    private float dodgeCooldownTimer;

    private bool wasClimbing = false;
    private bool isDodging;

    // --------------------
    // Vypocet rychlosti dodge
    // --------------------
    private float DodgeSpeed
    {
        get
        {
            if (dodgeDuration > 0f)
            {
                return dodgeDistance / dodgeDuration;
            }

            return 0f;
        }
    }

    // --------------------
    // Unity zivotni cyklus
    // --------------------
    void Awake()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void OnEnable()
    {
        moveAction = playerInput.actions.FindAction("Move");
        jumpAction = playerInput.actions.FindAction("Jump");
        dodgeAction = playerInput.actions.FindAction("Sprint");

        if (moveAction != null)
        {
            moveAction.Enable();
        }

        if (jumpAction != null)
        {
            jumpAction.Enable();
        }

        if (dodgeAction != null)
        {
            dodgeAction.Enable();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.Disable();
        }

        if (dodgeAction != null)
        {
            dodgeAction.Disable();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Dodge");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();
    }

    // --------------------
    // API pro jine scripty
    // --------------------
    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;

        if (locked)
        {
            isDodging = false;
            dodgeTimer = 0f;

            if (animator != null)
            {
                animator.ResetTrigger("Dodge");
            }
        }
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            isDodging = false;
            dodgeTimer = 0f;
            animationMoveSpeed = 0f;

            if (animator != null)
            {
                animator.ResetTrigger("Dodge");
            }
        }
    }

    public bool IsDodging()
    {
        return isDodging;
    }

    public void SetCurrentPlatform(MovingPlatform platform)
    {
        currentPlatform = platform;
    }

    public void ClearCurrentPlatform(MovingPlatform platform)
    {
        if (currentPlatform == platform)
        {
            currentPlatform = null;
        }
    }

    // --------------------
    // Hlavni flow pohybu
    // --------------------
    void HandleMovement()
    {
        Vector2 moveInput = ReadMoveInput();

        UpdateDodgeCooldown();

        bool hitWall;
        Vector3 wallNormal;
        UpdateClimbState(moveInput, out hitWall, out wallNormal);
        ApplyClimbEdgeBoost(hitWall, moveInput);

        wasClimbing = isClimbing;

        if (isClimbing)
        {
            HandleClimbMovement(moveInput, wallNormal);
        }
        else
        {
            HandleStandardMovement(moveInput);
        }

        UpdateGroundedAnimationTimer();
    }

    // --------------------
    // Animator
    // --------------------
    void HandleAnimations()
    {
        if (animator == null)
        {
            return;
        }

        if (inputLocked)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsClimbing", false);
            return;
        }

        animator.SetFloat("Speed", animationMoveSpeed);
        animator.SetBool("IsGrounded", GetGroundedStateForAnimation());
        animator.SetBool("IsClimbing", isClimbing);
    }

    // --------------------
    // Input
    // --------------------
    Vector2 ReadMoveInput()
    {
        Vector2 moveInput = Vector2.zero;

        if (moveAction != null)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }

        if (inputLocked || movementLocked || Cursor.lockState != CursorLockMode.Locked)
        {
            moveInput = Vector2.zero;
        }

        return moveInput;
    }

    // --------------------
    // Dodge cooldown a climb
    // --------------------
    void UpdateDodgeCooldown()
    {
        if (dodgeCooldownTimer > 0f)
        {
            dodgeCooldownTimer -= Time.deltaTime;
        }
    }

    void UpdateClimbState(Vector2 moveInput, out bool hitWall, out Vector3 wallNormal)
    {
        isClimbing = false;
        hitWall = false;
        wallNormal = Vector3.zero;

        Vector3 wallCheckRay = transform.position + Vector3.up * 0.5f;
        Debug.DrawRay(wallCheckRay, transform.forward * wallCheckDistance, Color.red);

        if (Physics.Raycast(wallCheckRay, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.CompareTag("Climbable"))
            {
                hitWall = true;
                wallNormal = hit.normal;

                if (moveInput.y > 0 && !cc.isGrounded)
                {
                    isClimbing = true;
                }
            }
        }
    }

    void ApplyClimbEdgeBoost(bool hitWall, Vector2 moveInput)
    {
        if (wasClimbing && !hitWall && moveInput.y > 0)
        {
            velocity.y = jumpSpeed * 0.6f;
        }
    }

    void HandleClimbMovement(Vector2 moveInput, Vector3 wallNormal)
    {
        jumpCount = 0;

        RotateTowardsWall(wallNormal);

        velocity.y = climbSpeed;

        Vector3 direction = transform.right * moveInput.x;
        Vector3 finalDirection = direction * climbSpeed;
        finalDirection.y = velocity.y;

        cc.Move(finalDirection * Time.deltaTime);
    }

    void RotateTowardsWall(Vector3 wallNormal)
    {
        if (wallNormal == Vector3.zero)
        {
            return;
        }

        Vector3 lookDirection = -wallNormal;
        lookDirection.y = 0f;

        if (lookDirection == Vector3.zero)
        {
            return;
        }

        Quaternion targetWallRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetWallRotation, rotationSpeed * Time.deltaTime);
    }

    // --------------------
    // Chuze, skok a dodge
    // --------------------
    void HandleStandardMovement(Vector2 moveInput)
    {
        HandleGroundAndAirState();

        Vector3 moveDirection = CalculateCameraRelativeMoveDirection(moveInput);

        TryStartDodge(moveDirection);
        moveDirection = UpdateActiveDodge(moveDirection);

        RotateTowardsMoveDirection(moveDirection);

        float currentSpeed = GetCurrentMoveSpeed();
        UpdateAnimationMoveSpeed(moveDirection, currentSpeed);
        ApplyCharacterMovement(moveDirection, currentSpeed);
        HandleCeilingCollision();
    }

    // --------------------
    // Grounded a skok
    // --------------------
    void HandleGroundAndAirState()
    {
        if (cc.isGrounded)
        {
            jumpTimeoutDelta = 0.0f;
            jumpCount = 0;

            if (velocity.y < 0.0f)
            {
                velocity.y = -groundedStickForce;
            }

            if (!inputLocked && !movementLocked && jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                velocity.y = jumpSpeed;
                jumpCount++;

                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }

                jumpTimeoutDelta = jumpTimeout;
            }

            return;
        }

        velocity.y -= gravity * Time.deltaTime;

        if (jumpTimeoutDelta >= 0.0f)
        {
            jumpTimeoutDelta -= Time.deltaTime;
        }

        if (!inputLocked && !movementLocked && jumpAction != null && jumpAction.WasPressedThisFrame() && hasDoubleJump && jumpCount < 2)
        {
            velocity.y = jumpSpeed;
            jumpCount++;

            if (animator != null)
            {
                animator.SetTrigger("doublejump");
            }
        }
    }

    // --------------------
    // Pohyb podle kamery
    // --------------------
    Vector3 CalculateCameraRelativeMoveDirection(Vector2 moveInput)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * moveInput.y + camRight * moveInput.x).normalized;
    }

    void TryStartDodge(Vector3 moveDirection)
    {
        if (isDodging)
        {
            return;
        }

        if (inputLocked || movementLocked)
        {
            return;
        }

        if (dodgeAction == null)
        {
            return;
        }

        if (!dodgeAction.WasPressedThisFrame())
        {
            return;
        }

        if (!cc.isGrounded)
        {
            return;
        }

        if (dodgeCooldownTimer > 0f)
        {
            return;
        }

        isDodging = true;
        dodgeTimer = dodgeDuration;
        dodgeCooldownTimer = dodgeCooldown;

        if (moveDirection != Vector3.zero)
        {
            dodgeDirection = moveDirection;
        }
        else
        {
            dodgeDirection = transform.forward;
        }

        dodgeDirection.y = 0f;
        dodgeDirection.Normalize();

        if (animator != null)
        {
            animator.ResetTrigger("Dodge");
            animator.SetTrigger("Dodge");
        }
    }

    Vector3 UpdateActiveDodge(Vector3 moveDirection)
    {
        if (!isDodging)
        {
            return moveDirection;
        }

        dodgeTimer -= Time.deltaTime;
        moveDirection = dodgeDirection;

        if (dodgeTimer <= 0f)
        {
            isDodging = false;

            if (animator != null)
            {
                animator.ResetTrigger("Dodge");
            }
        }

        return moveDirection;
    }

    void RotateTowardsMoveDirection(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    float GetCurrentMoveSpeed()
    {
        if (isDodging)
        {
            return DodgeSpeed;
        }

        return moveSpeed;
    }

    // --------------------
    // CharacterController Move
    // --------------------
    void UpdateAnimationMoveSpeed(Vector3 moveDirection, float currentSpeed)
    {
        if (moveDirection != Vector3.zero)
        {
            animationMoveSpeed = currentSpeed;
        }
        else
        {
            animationMoveSpeed = 0f;
        }
    }

    void ApplyCharacterMovement(Vector3 moveDirection, float currentSpeed)
    {
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;

        Vector3 platformMove = Vector3.zero;

        if (currentPlatform != null)
        {
            platformMove = currentPlatform.frameDelta;
        }

        cc.Move(platformMove + finalMove * Time.deltaTime);
    }

    void HandleCeilingCollision()
    {
        if ((cc.collisionFlags & CollisionFlags.Above) != 0)
        {
            if (velocity.y > 0)
            {
                velocity.y = -2f;
            }
        }
    }

    // --------------------
    // Grounded stav pro animace
    // --------------------
    void UpdateGroundedAnimationTimer()
    {
        if (cc.isGrounded)
        {
            groundedAnimationTimer = groundedAnimationGraceTime;
            return;
        }

        if (groundedAnimationTimer > 0f)
        {
            groundedAnimationTimer -= Time.deltaTime;
        }

        if (groundedAnimationTimer < 0f)
        {
            groundedAnimationTimer = 0f;
        }
    }

    bool GetGroundedStateForAnimation()
    {
        if (cc.isGrounded)
        {
            return true;
        }

        if (groundedAnimationTimer > 0f)
        {
            return true;
        }

        return false;
    }
}
