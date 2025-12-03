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

    [Header("Quicksand")]
    [Tooltip("Multiplier applied to horizontal speed when the player is in quicksand (0..1)")]
    [SerializeField] float sandSpeedMultiplier = 0.5f;

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

    [Header("Barre d'eau")]
    public WaterBarController waterBar;

    [Header("Poussée d’objets")]
    public float pushDetectionDistance = 0.5f;
    public LayerMask pushableLayer;

    [Header("Sons")]
    public AudioSource walkSource;
    public AudioClip walkClip;
    public AudioClip sandWalkClip;
    public AudioClip jumpSoundClip;


    // Physique et états
    MovementMode movementMode = MovementMode.Normal;
    IExternalKinematics ext;
    Rigidbody2D rb;
    BoxCollider2D box;
    SpriteRenderer sr;
    Animator anim;
    // Reference to the PlayerQuicksand component so we can slow movement when in sand
    PlayerQuicksand pq;

    Vector2 input;
    bool grounded;
    float coyoteTimer;
    float jumpBufferTimer;
    bool jumpHeld;
    bool canMove = true;

    // Calculs de gravité / saut
    float jumpVelocity;
    float gravityScaleUp;
    float gravityScaleDown;
    float gravityScaleLowJump;

    // Slide
    bool isSliding = false;
    bool slideHeld;

    // Orientation
    float lastFacingDir = 1f; // 1 = droite, -1 = gauche
    const float faceThreshold = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        pq = GetComponent<PlayerQuicksand>(); // may be null if the player doesn't have the component
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
                ext?.Apply(ref rb);
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
        // Horizontal movement using MoveLeft / MoveRight keys
        bool left = Input.GetKey(ControlsManager.GetKey(PlayerAction.MoveLeft));
        bool right = Input.GetKey(ControlsManager.GetKey(PlayerAction.MoveRight));
        input.x = (right ? 1f : 0f) - (left ? 1f : 0f);

        // Jump
        jumpHeld = Input.GetKey(ControlsManager.GetKey(PlayerAction.Jump));

        // Slide
        slideHeld = Input.GetKey(ControlsManager.GetKey(PlayerAction.Slide));
    }

    void UpdateGroundingAndCoyote()
    {
        grounded = IsGrounded();
        coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
    }

    void UpdateFacingAndAnim()
    {
        bool walking = movementMode == MovementMode.Normal && Mathf.Abs(input.x) > faceThreshold;
        anim.SetBool("IsWalking", walking);
        anim.SetBool("IsSwinging", movementMode == MovementMode.Grapple);

        if (walking && !walkSource.isPlaying && grounded && pq.InSand)
        {
            walkSource.pitch = 1f;
            walkSource.clip = sandWalkClip;
            walkSource.Play();
        }
        else if (walking && !walkSource.isPlaying && grounded)
        {
            walkSource.pitch = 1f + Mathf.Abs(input.x) * 0.3f;
            walkSource.clip = walkClip;
            walkSource.Play();
        }

        if ((!walking || !grounded) && walkSource.isPlaying)
        {
            walkSource.Stop();
        }

        if (Mathf.Abs(input.x) > faceThreshold)
            lastFacingDir = Mathf.Sign(input.x);

        sr.flipX = lastFacingDir < 0f;
    }

    void UpdateJumpBuffer()
    {
        if (Input.GetKeyDown(ControlsManager.GetKey(PlayerAction.Jump)))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - Time.deltaTime);

        jumpHeld = Input.GetKey(ControlsManager.GetKey(PlayerAction.Jump));

        DetectPushable();
    }

    void ApplyMovement()
    {
        float waterFactor = 1f;
        if (waterBar != null)
        {
            // Only slow down when water is completely empty (waterLevel = 0)
            waterFactor = (waterBar.waterLevel <= 0f) ? 0.5f : 1f;
        }

        float sandFactor = (pq != null && pq.InSand) ? sandSpeedMultiplier : 1f;

        float totalSpeedMultiplier = waterFactor * sandFactor;

        float adjustedMoveSpeed = moveSpeed * totalSpeedMultiplier;
        float adjustedJumpVelocity = jumpVelocity * waterFactor;

        float vx = rb.linearVelocity.x;
        float vy = rb.linearVelocity.y;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            vy = adjustedJumpVelocity;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            if (jumpSoundClip != null)
            {
                AudioManager.Instance.PlaySoundClip(jumpSoundClip, 0.5f);
            }
        }

        if (grounded)
        {
            vx = input.x * adjustedMoveSpeed;
        }
        else
        {
            float target = input.x * airMax * totalSpeedMultiplier;
            float maxDelta = airAccel * Time.fixedDeltaTime;
            vx = Mathf.MoveTowards(vx, target, maxDelta);
        }

        if (vy > 0.01f)
            rb.gravityScale = jumpHeld ? gravityScaleUp : gravityScaleLowJump;
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

        return HitGround(left) || HitGround(mid) || HitGround(right);
    }

    bool HitGround(Vector2 origin)
    {
        return Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer).collider != null;
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

        float oldHeight = box.size.y;
        float newHeight = oldHeight * 2f;
        float heightDiff = newHeight - oldHeight;

        box.size = new Vector2(box.size.x, newHeight);
        box.offset = new Vector2(box.offset.x, box.offset.y + heightDiff / 2f);
        anim.SetBool("IsSliding", isSliding);
    }

    bool HasCeilingAbove()
    {
        Vector2 cliffCenter = (Vector2)transform.position + box.offset;
        Vector2 size = box.size;
        size -= new Vector2(0.01f, 0.01f);
        RaycastHit2D hit = Physics2D.BoxCast(cliffCenter, size, 0f, Vector2.up, 0f, groundLayer);
        return hit.collider != null;
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
        Vector2 direction = sr.flipX ? Vector2.left : Vector2.right;

        Bounds b = box.bounds;
        Vector2 origin = sr.flipX
            ? new Vector2(b.min.x - 0.02f, b.center.y) // côté gauche
            : new Vector2(b.max.x + 0.02f, b.center.y); // côté droit

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, pushDetectionDistance, pushableLayer);

        Debug.DrawRay(origin, direction * pushDetectionDistance, Color.yellow);

        if (hit.collider != null)
        {
            Pushable pushable = hit.collider.GetComponent<Pushable>();
            bool isPushing = Mathf.Abs(input.x) > 0.1f && grounded;
            EndSlide(); // Cannot slide while pushing
            anim.SetBool("IsPushing", isPushing);

            if (isPushing && pushable != null)
            {
                Vector2 pushDir = new Vector2(input.x, 0);
                pushable.Push(pushDir);
            }
        }
        else
        {
            anim.SetBool("IsPushing", false);
        }
    }
}
