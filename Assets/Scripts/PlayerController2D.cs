using UnityEngine;

public enum MovementMode { Normal, Grapple }

public interface IExternalKinematics
{
    bool Apply(ref Rigidbody2D rb);
}

public class PlayerController2D : MonoBehaviour
{
    [Header("Mouvement horizontal")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float airAccel = 40f;
    [SerializeField] float airMax = 8f;

    [Header("Saut: cibles")]
    [SerializeField] float desiredJumpHeight = 3f;
    [SerializeField] float timeToApex = 0.4f;
    [SerializeField] float timeToFall = 0.35f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Tolérances d'entrée")]
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;

    [Header("Détection sol")]
    [SerializeField] LayerMask groundLayer = ~0;
    [SerializeField] float groundCheckDistance = 0.06f;
    [SerializeField] float groundCheckInset = 0.02f;

<<<<<<< HEAD
    [Header("Barre d'eau")]
    public WaterBarController waterBar;  // référence à ton script WaterBarController

    [Header("Poussée d’objets")]
    public float pushDetectionDistance = 0.5f;
    public LayerMask pushableLayer;

    private float jumpVelocity;
    private float gravityScaleUp;
    private float gravityScaleDown;
    private float gravityScaleLowJump;

    Rigidbody2D rigidBody;
    BoxCollider2D boxCollider;
    SpriteRenderer spriteRenderer;
    Animator animator;
=======
    MovementMode movementMode = MovementMode.Normal;
    IExternalKinematics ext;
    Rigidbody2D rb;
    BoxCollider2D box;
    SpriteRenderer sr;
    Animator anim;
>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c

    Vector2 input;
    bool grounded;
    float coyoteTimer;
    float jumpBufferTimer;
    bool jumpHeld;
<<<<<<< HEAD
    private bool canMove = true;
=======
    bool canMove = true;

    float jumpVelocity;
    float gravityScaleUp;
    float gravityScaleDown;
    float gravityScaleLowJump;
>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        RecomputeJumpParameters();
    }

    void Update()
    {
        if (!canMove)
        {
            input.x = 0f;
            UpdateGroundingAndCoyote();
            UpdateJumpBuffer();
            jumpHeld = false;
            sr.flipX = false;
            anim.SetBool("IsWalking", false);
            return;
        }

        ReadInput();
        UpdateGroundingAndCoyote();
        UpdateFacingAndAnim();
        UpdateJumpBuffer();
    }

    void FixedUpdate()
    {
        switch (movementMode)
        {
            case MovementMode.Normal:
                ApplyMovement();
                break;
            case MovementMode.Grapple:
                ext.Apply(ref rb);
                break;
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!canMove)
        {
            input = Vector2.zero;
            var v = rb.linearVelocity;
            rb.linearVelocity = new Vector2(0f, v.y);
        }
    }

    public void RecomputeJumpParameters()
    {
        float gUp = 2f * desiredJumpHeight / (timeToApex * timeToApex);
        float gDown = 2f * desiredJumpHeight / (timeToFall * timeToFall);
        float gUnity = -Physics2D.gravity.y;

        gravityScaleUp = gUp / gUnity;
        gravityScaleDown = gDown / gUnity;
        gravityScaleLowJump = gravityScaleUp * Mathf.Max(1f, lowJumpMultiplier);
        jumpVelocity = gUp * timeToApex;
    }

    void ReadInput()
    {
<<<<<<< HEAD
        if (!canMove)
        {
            input.x = 0;
            grounded = IsGrounded();
            coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
            spriteRenderer.flipX = false;
            animator.SetBool("IsWalking", false);
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
            jumpHeld = false;
            return;
        }

=======
>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c
        input.x = Input.GetAxisRaw("Horizontal");
        jumpHeld = Input.GetButton("Jump");
    }

    void UpdateGroundingAndCoyote()
    {
        grounded = IsGrounded();
        coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
    }

<<<<<<< HEAD
        // Gère l'orientation du sprite
        bool isWalking = Mathf.Abs(input.x) > 0.1f;
        animator.SetBool("IsWalking", isWalking);

        // ✅ Ne change le flip que si le joueur bouge vraiment
        if (isWalking)
        {
            spriteRenderer.flipX = input.x < 0;
        }
=======
    void UpdateFacingAndAnim()
    {
        bool walking = movementMode == MovementMode.Normal && Mathf.Abs(input.x) > 0.1f;
        anim.SetBool("IsWalking", walking);
        anim.SetBool("IsSwinging", movementMode == MovementMode.Grapple);
>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c

        sr.flipX = input.x < 0f;
    }

    void UpdateJumpBuffer()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
<<<<<<< HEAD

        jumpHeld = Input.GetButton("Jump");

        // Détection de poussée
        DetectPushable();
=======
>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c
    }

    void ApplyMovement()
    {
<<<<<<< HEAD
        // --- Ajustement selon le niveau d’eau ---
        float waterFactor = 1f;
        if (waterBar != null)
        {
            // Quand waterLevel = 1 → vitesse normale
            // Quand waterLevel = 0 → vitesse et saut divisés par 2
            waterFactor = Mathf.Lerp(0.5f, 1f, waterBar.waterLevel);
        }

        float adjustedMoveSpeed = moveSpeed * waterFactor;
        float adjustedJumpVelocity = jumpVelocity * waterFactor;

        float vx = input.x * adjustedMoveSpeed;
        float vy = rigidBody.linearVelocityY;
=======
        float vx = rb.linearVelocity.x;
        float vy = rb.linearVelocity.y;
>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            vy = adjustedJumpVelocity;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (grounded)
        {
            vx = input.x * moveSpeed;
        }
        else
        {
            float target = input.x * airMax;
            float maxDelta = airAccel * Time.fixedDeltaTime;
            vx = Mathf.MoveTowards(vx, target, maxDelta);
        }

        float vyNow = rb.linearVelocity.y;
        if (vyNow > 0.01f)
            rb.gravityScale = jumpHeld ? gravityScaleUp : gravityScaleLowJump;
        else if (vyNow < -0.01f)
            rb.gravityScale = gravityScaleDown;
        else
            rb.gravityScale = gravityScaleDown;

        rb.linearVelocity = new Vector2(vx, vy);
    }

<<<<<<< HEAD
=======

>>>>>>> 528bcef8ee184b453056d892fe86ba166768093c
    bool IsGrounded()
    {
        Bounds b = box.bounds;
        Vector2 left = new Vector2(b.min.x + groundCheckInset, b.min.y);
        Vector2 mid = new Vector2(b.center.x, b.min.y);
        Vector2 right = new Vector2(b.max.x - groundCheckInset, b.min.y);

        return
            HitGround(left) || HitGround(mid) || HitGround(right);
    }

    bool HitGround(Vector2 origin)
    {
        return Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }

    public void Takeover(IExternalKinematics external)
    {
        ext = external;
        movementMode = MovementMode.Grapple;
    }

    public void ReleaseTakeover()
    {
        ext = null;
        movementMode = MovementMode.Normal;
    }

    void DetectPushable()
    {
        // ✅ direction du regard du joueur
        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        // ✅ point de départ depuis le bord du collider
        Bounds b = boxCollider.bounds;
        Vector2 origin = spriteRenderer.flipX
            ? new Vector2(b.min.x - 0.02f, b.center.y) // côté gauche
            : new Vector2(b.max.x + 0.02f, b.center.y); // côté droit

        // ✅ Raycast de détection
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, pushDetectionDistance, pushableLayer);

        // Debug visible dans la scène
        Debug.DrawRay(origin, direction * pushDetectionDistance, Color.yellow);

        if (hit.collider != null)
        {
            Pushable pushable = hit.collider.GetComponent<Pushable>();
            if (pushable != null && Mathf.Abs(input.x) > 0.1f && grounded)
            {
                // Le joueur pousse
                Vector2 pushDir = new Vector2(input.x, 0);
                pushable.Push(pushDir);
                animator.SetBool("IsPushing", true);
            }
            else
            {
                animator.SetBool("IsPushing", false);
            }
        }
        else
        {
            animator.SetBool("IsPushing", false);
        }
    }
}
