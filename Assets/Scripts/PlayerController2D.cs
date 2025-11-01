using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Mouvement horizontal")]
    public float moveSpeed = 8f;
    public float accel = 60f;
    public float airAccel = 25f;

    [Header("Saut: cibles")]
    public float desiredJumpHeight = 3f;
    public float timeToApex = 0.4f;
    public float timeToFall = 0.35f;
    public float lowJumpMultiplier = 2f;

    [Header("Tolérances d'entrée")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Détection sol")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.06f;

    private float jumpVelocity;
    private float gravityScaleUp;
    private float gravityScaleDown;
    private float gravityScaleLowJump;

    Rigidbody2D rigidBody;
    BoxCollider2D boxCollider;
    SpriteRenderer spriteRenderer;
    Animator animator;

    Vector2 input;
    bool grounded;
    float coyoteTimer;
    float jumpBufferTimer;
    bool jumpHeld;
    private bool canMove = true;
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        RecomputeJumpParameters();
    }

    void RecomputeJumpParameters()
    {
        float gUp = 2f * desiredJumpHeight / (timeToApex * timeToApex);
        float gDown = 2f * desiredJumpHeight / (timeToFall * timeToFall);
        float gUnity = -Physics2D.gravity.y;

        gravityScaleUp = gUp / gUnity;
        gravityScaleDown = gDown / gUnity;
        gravityScaleLowJump = gravityScaleUp * Mathf.Max(1f, lowJumpMultiplier);
        jumpVelocity = gUp * timeToApex;
    }

    void Update()
    {

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

        input.x = Input.GetAxisRaw("Horizontal");

        grounded = IsGrounded();
        coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);

        spriteRenderer.flipX = input.x < 0;
        bool IsWalking = Mathf.Abs(input.x) > 0.1f;
        animator.SetBool("IsWalking", IsWalking);

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

        jumpHeld = Input.GetButton("Jump");
    }

    void FixedUpdate()
    {
        float vx = input.x * moveSpeed;
        float vy = rigidBody.linearVelocityY;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            vy = jumpVelocity;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (rigidBody.linearVelocityY > 0.01f)
            rigidBody.gravityScale = jumpHeld ? gravityScaleUp : gravityScaleLowJump;
        else if (rigidBody.linearVelocityY < -0.01f)
            rigidBody.gravityScale = gravityScaleDown;
        else
            rigidBody.gravityScale = gravityScaleDown;

        rigidBody.linearVelocity = new Vector2(vx, vy);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!canMove)
        {
            input = Vector2.zero;
            rigidBody.linearVelocity = new Vector2(0, rigidBody.linearVelocityY);
        }
    }
    bool IsGrounded()
    {
        Bounds b = boxCollider.bounds;
        Vector2 left = new Vector2(b.min.x + 0.02f, b.min.y);
        Vector2 mid = new Vector2(b.center.x, b.min.y);
        Vector2 right = new Vector2(b.max.x - 0.02f, b.min.y);

        return
            Physics2D.Raycast(left, Vector2.down, groundCheckDistance, groundLayer).collider != null ||
            Physics2D.Raycast(mid, Vector2.down, groundCheckDistance, groundLayer).collider != null ||
            Physics2D.Raycast(right, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }
}
