using UnityEngine;

public class Slide : MonoBehaviour
{

    bool isSliding = false;
    bool slideHeld;
    Vector2 normalColliderSize;
    Vector2 normalColliderOffset;
    Vector2 input;
    bool grounded;
    float coyoteTime = 0.1f;
    float coyoteTimer;
    BoxCollider2D box;
    Animator anim;
    [Header("Détection sol")]
    [SerializeField] LayerMask groundLayer = ~0;
    [SerializeField] float groundCheckDistance = 0.06f;
    [SerializeField] float groundCheckInset = 0.02f;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();

        normalColliderSize = box.size;
        normalColliderOffset = box.offset;
    }

    void Update()
    {
        slideHeld = false;
        ReadInput();
        UpdateGroundingAndCoyote();
        UpdateSlide();
    }

    void ReadInput()
    {
        slideHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    void UpdateGroundingAndCoyote()
    {
        grounded = IsGrounded();
        coyoteTimer = grounded ? coyoteTime : Mathf.Max(0f, coyoteTimer - Time.deltaTime);
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

}
