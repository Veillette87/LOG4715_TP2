using UnityEngine;

public enum MovementMode { Normal, Grapple, Slide }

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

    MovementMode movementMode = MovementMode.Normal;
    IExternalKinematics ext;
    Rigidbody2D rb;
    BoxCollider2D box;
    SpriteRenderer sr;
    Animator anim;

    Vector2 input;
    bool grounded;
    float coyoteTimer;
    float jumpBufferTimer;
    bool jumpHeld;
    bool canMove = true;

    float jumpVelocity;
    float gravityScaleUp;
    float gravityScaleDown;
    float gravityScaleLowJump;

    bool isSliding = false;
    bool slideHeld;
    Vector2 normalColliderSize;
    Vector2 normalColliderOffset;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        normalColliderSize = box.size;
        normalColliderOffset = box.offset;
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
            slideHeld = false;
            sr.flipX = false;
            anim.SetBool("IsWalking", false);
            return;
        }

        ReadInput();
        UpdateGroundingAndCoyote();
        UpdateSlide();
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
        input.x = Input.GetAxisRaw("Horizontal");
        jumpHeld = Input.GetButton("Jump");
        slideHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    void UpdateGroundingAndCoyote()
    {
        grounded = IsGrounded();
        coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
    }

    void UpdateFacingAndAnim()
    {
        bool walking = movementMode == MovementMode.Normal && Mathf.Abs(input.x) > 0.1f;
        anim.SetBool("IsWalking", walking);
        anim.SetBool("IsSwinging", movementMode == MovementMode.Grapple);

        sr.flipX = input.x < 0f;
    }

    void UpdateJumpBuffer()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);
    }

    void ApplyMovement()
    {
        float vx = rb.linearVelocity.x;
        float vy = rb.linearVelocity.y;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            vy = jumpVelocity;
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


    bool IsGrounded()
    {
        Bounds b = box.bounds;
        Vector2 left = new Vector2(b.min.x + groundCheckInset, b.min.y);
        Vector2 mid = new Vector2(b.center.x, b.min.y);
        Vector2 right = new Vector2(b.max.x - groundCheckInset, b.min.y);

        return
            HitGround(left) || HitGround(mid) || HitGround(right);
    }

    void UpdateSlide()
    {
        if (slideHeld && grounded)
        {
            if (!isSliding)
            {
                StartSlide();
            }
        }
        else
        {
            EndSlide();
        }
    }

    void StartSlide()
    {
        isSliding = true;
        anim.SetBool("IsSliding", isSliding);

        float oldHeight = box.size.y;
        float newHeight = oldHeight / 2f;
        float heightDiff = oldHeight - newHeight;

        box.size = new Vector2(box.size.x, newHeight);
        box.offset = new Vector2(box.offset.x, box.offset.y - heightDiff / 2f);
    }

    void EndSlide()
    {
        if (!isSliding) return;

        if (HasCeilingAbove())
            return;

        isSliding = false;
        box.size = normalColliderSize;
        box.offset = normalColliderOffset;
        anim.SetBool("IsSliding", isSliding);
    }

    bool HasCeilingAbove()
    {
        Vector2 cliffCenter = (Vector2)transform.position + normalColliderOffset;
        Vector2 size = normalColliderSize;
        size -= new Vector2(0.01f, 0.01f);
        RaycastHit2D hit = Physics2D.BoxCast(cliffCenter, size, 0f, Vector2.up, 0f, groundLayer);
        return hit.collider != null;
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
}
