using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed Limits")]
    public float maxFallSpeed = -10f;  // negative, weil Y‐Achse nach oben positiv ist
    public float maxRiseSpeed = 15f;  // optional: verhindert Überhochsprüngen
    [Header("Movement")]
    public int maxJumps = 2;
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public int jumpCount = 0;
    public bool hasJumped = false;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public bool isOnPlatform { get; private set; }
    private bool isGrounded;
    private bool onGround;
    private bool wasGroundedLastFrame = false;
    public bool isFalling;
    public float fallEnterSpeed = -0.1f; // wird fallend, wenn vy < -0.2
    public float fallExitSpeed = -0.05f; // hört auf fallend zu sein, wenn vy > -0.05

    [Header("Fall Through")]
    public float fallThroughTime = 0.5f;
    private List<Collider2D> ignoredPlatforms = new List<Collider2D>();

    [Header("Wall Slide")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;
    public LayerMask hangebaleObjectsLayer;
    public float wallSlideSpeed = 2f;
    private bool isTouchingWall;
    private bool isWallSliding;
    private RaycastHit2D lastWallHit;

    [Header("Wall Jump")]
    public float wallJumpHorizontalForce = 8f;
    public float wallJumpVerticalForce = 12f;
    public float movementLockTime = 0.2f;
    private bool movementLocked = false;
    private float lockedVelocityX = 0f;

    [Header("Audio")]
    public PlayerAudio playerAudio;
    private bool wasWallSlidingLastFrame = false;

    private PlayerParticles playerParticles;

    [Header("Death Settings")]
    public float deathPushForce = 5f;
    public float deathTorque = 1000f;
    public float deathFallDelay = 0.2f;
    public bool isDead = false;

    public Rigidbody2D rb;
    private Collider2D col;
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerAudio = GetComponent<PlayerAudio>();
        animator = GetComponent<Animator>();
        playerParticles = GetComponent<PlayerParticles>();
    }

    void Update()
    {
        if (isDead) return;

        CheckGrounded();

        HandleFallThrough();
        ResetDoubleJump();
        HandleMovement();
        HandleJump();
        CheckWallSlide();
        UpdateAnimatorParameters();
        wasGroundedLastFrame = isGrounded;
    }
    void FixedUpdate()
    {
        Vector2 v = rb.velocity;
        v.y = Mathf.Clamp(v.y, maxFallSpeed, maxRiseSpeed);
        rb.velocity = v;
        UpdateFallingState();
    }

    private void CheckGrounded()
    {
        onGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer | hangebaleObjectsLayer);
        isOnPlatform = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, platformLayer);
        isGrounded = onGround || isOnPlatform;
    }

    void UpdateFallingState()
    {
        if (isGrounded) { isFalling = false; return; }
        float vy = rb.velocity.y;
        if (!isFalling)
        {
            // Eintritt: erst "fallend", wenn wir klar nach unten unterwegs sind
            if (vy < fallEnterSpeed) isFalling = true;
        }
        else
        {
            // Austritt: erst aufhören, wenn wir fast keine Abwärtsbewegung mehr haben
            if (vy > fallExitSpeed) isFalling = false;
        }
    }

    private void HandleFallThrough()
    {
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && isGrounded)
        {
            Collider2D[] platforms = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, platformLayer);
            if (platforms.Length > 0)
                StartCoroutine(DisablePlatformCollision(platforms));
        }
    }

    private IEnumerator DisablePlatformCollision(Collider2D[] platforms)
    {
        foreach (var platform in platforms)
        {
            Physics2D.IgnoreCollision(col, platform, true);
            ignoredPlatforms.Add(platform);
        }
        yield return new WaitForSeconds(fallThroughTime);
        foreach (var platform in ignoredPlatforms)
            Physics2D.IgnoreCollision(col, platform, false);
        ignoredPlatforms.Clear();
    }

    public void ResetDoubleJump()
    {
        if (isGrounded && !wasGroundedLastFrame)
        {

            if (onGround || isOnPlatform)
            {
                jumpCount = 0;
                animator.ResetTrigger("DoubleJump");
                hasJumped = false;
                playerAudio.PlayJumpLandSound();
                playerParticles.SpawnLandingDust();
            }
        }
    }

    private void HandleMovement()
    {
        if (movementLocked)
        {
            rb.velocity = new Vector2(lockedVelocityX, rb.velocity.y);
            SetFacingDirection(lockedVelocityX);
            return;
        }
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        if (moveInput != 0)
            SetFacingDirection(moveInput);
    }

    private void SetFacingDirection(float direction)
    {
        transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
    }

    private void CheckWallSlide()
    {
        float dir = GetFacingDirection();
        isTouchingWall = DetectWall(dir);
        isWallSliding = ShouldWallSlide(dir);
        UpdateWallSlideState();
    }

    private float GetFacingDirection()
    {
        return transform.localScale.x > 0f ? 1f : -1f;
    }

    private bool DetectWall(float dir)
    {
        lastWallHit = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, wallCheckDistance, wallLayer | hangebaleObjectsLayer);
        return lastWallHit.collider != null;
    }

    private bool ShouldWallSlide(float dir)
    {
        bool pressingTowardsWall = Input.GetAxisRaw("Horizontal") == dir;
        bool falling = rb.velocity.y < 0f;
        return isTouchingWall && !isGrounded && falling && pressingTowardsWall;
    }

    private void UpdateWallSlideState()
    {
        animator.SetBool("isWallSliding", isWallSliding);
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            if (!wasWallSlidingLastFrame)
                playerAudio.StartWallSlideLoop();
        }
        else if (wasWallSlidingLastFrame)
        {
            playerAudio.StopWallSlideLoop();
        }
        wasWallSlidingLastFrame = isWallSliding;
    }

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (isGrounded)
        {
            PerformJump();
            hasJumped = true;
            jumpCount = 1;
            return;
        }
        else if (isWallSliding)
        {
            PerformWallJump();
            return;
        }
        else if (hasJumped && jumpCount < maxJumps)
        {
            PerformJump();
            TriggerDoubleJump();
            return;
        }
        else if (!isGrounded && isFalling && jumpCount < maxJumps)
        {
            Debug.Log("Double Jump Triggered");
            jumpCount = 1;
            PerformJump();
            TriggerDoubleJump();
            return;
        }
    }


    public void PerformJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        playerAudio.PlayJumpSound();
        playerParticles.SpawnJumpDust();
    }
    public void JumpWithLock(Vector2 impulse)
    {
        // 1) setze Impuls
        rb.velocity = impulse;

        // 2) locke horizontale Komponente
        lockedVelocityX = impulse.x;
        StartCoroutine(LockMovement());
    }

    private void PerformWallJump()
    {
        lockedVelocityX = lastWallHit.normal.x * wallJumpHorizontalForce;
        rb.velocity = new Vector2(0, wallJumpVerticalForce);
        playerAudio.PlayJumpSound();
        playerParticles.SpawnJumpDust();
        jumpCount = 1;
        hasJumped = true;
        isWallSliding = false;
        StartCoroutine(LockMovement());
    }

    private IEnumerator LockMovement()
    {
        movementLocked = true;
        yield return new WaitForSeconds(movementLockTime);
        movementLocked = false;
        lockedVelocityX = 0f;
    }

    private void TriggerDoubleJump()
    {
        jumpCount++;
        animator.SetTrigger("DoubleJump");
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
        animator.SetFloat("HorizontalSpeed", rb.velocity.x);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Hit");
        FindObjectOfType<CameraFollow2D>()?.StopFollowing();
        StartCoroutine(AfterHitFall());

    }


    private IEnumerator AfterHitFall()
    {
        yield return new WaitForSeconds(deathFallDelay);
        StartCoroutine(BeginDeathFall());
    }

    private IEnumerator BeginDeathFall()
    {
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        float facing = transform.localScale.x;
        rb.velocity = new Vector2(-facing * deathPushForce, rb.velocity.y);
        rb.freezeRotation = false;
        rb.angularVelocity = deathTorque * facing;
        playerAudio.PlayGameOverSound();
        yield return new WaitForSeconds(1.5f);
        FindObjectOfType<UIManager>().ShowDeathMenu();
        Destroy(gameObject);
    }
}
