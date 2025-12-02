using UnityEngine;

public class GrappleController2D : MonoBehaviour, IExternalKinematics
{
    [SerializeField] LayerMask grappleMask;
    [SerializeField] float maxGrappleDistance = 8f;
    [SerializeField] LineRenderer rope;
    [SerializeField] float swingForce = 25f;
    [SerializeField] float radialDamp = 0.2f;
    [SerializeField] float pullForce = 40f;

    [Header("Grapple Aim UI")]
    [SerializeField] LineRenderer aimLine;
    [SerializeField] Transform reticle;
    [SerializeField] Color inRangeColor = Color.green;
    [SerializeField] Color outOfRangeColor = Color.red;

    Rigidbody2D rb;
    PlayerController2D motor;
    DistanceJoint2D joint;

    Vector2 anchor;
    bool active;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        motor = GetComponent<PlayerController2D>();
        joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.enabled = false;
        joint.autoConfigureConnectedAnchor = false;
        joint.maxDistanceOnly = true;
        joint.enableCollision = true;
        if (rope != null) rope.enabled = false;
        if (aimLine != null) aimLine.enabled = false;
        if (reticle != null) reticle.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!active && !GlobalSettings.Instance.IsUIVisible)
        {
            UpdateAimPreview();

            if (Input.GetKeyDown(ControlsManager.GetKey(PlayerAction.Grapple)))
            {
                Vector2 aimDir = GetAimDirection();
                Vector2 origin = transform.position;
                RaycastHit2D hit = Physics2D.Raycast(origin, aimDir, maxGrappleDistance, grappleMask);

                if (hit)
                {
                    anchor = hit.point;
                    Engage(anchor);
                }
            }
        }

        if (active && Input.GetKeyDown(ControlsManager.GetKey(PlayerAction.Jump)) && !GlobalSettings.Instance.IsUIVisible)
        {
            Release();
        }

        if (rope != null && !GlobalSettings.Instance.IsUIVisible)
        {
            if (active)
            {
                rope.enabled = true;
                rope.SetPosition(0, transform.position);
                rope.SetPosition(1, anchor);
            }
            else
            {
                rope.enabled = false;
            }
        }
    }

    void UpdateAimPreview()
    {
        if (aimLine == null && reticle == null) return;

        Vector2 origin = transform.position;
        Vector2 dir = GetAimDirection();

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxGrappleDistance, grappleMask);

        Vector3 endPoint;
        bool valid = false;

        if (hit)
        {
            endPoint = hit.point;
            valid = true;
        }
        else
        {
            endPoint = origin + dir * maxGrappleDistance;
        }

        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, endPoint);
            var c = valid ? inRangeColor : outOfRangeColor;
            aimLine.startColor = c;
            aimLine.endColor = c;
        }

        if (reticle != null)
        {
            reticle.gameObject.SetActive(true);
            reticle.position = endPoint;
            var sr = reticle.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = valid ? inRangeColor : outOfRangeColor;
        }
    }

    void Engage(Vector2 worldAnchor)
    {
        if (aimLine != null) aimLine.enabled = false;
        if (reticle != null) reticle.gameObject.SetActive(false);

        joint.connectedAnchor = worldAnchor;
        float dist = Vector2.Distance(rb.position, worldAnchor);
        joint.distance = Mathf.Min(dist, maxGrappleDistance);
        joint.enabled = true;
        active = true;
        motor.Takeover(this);
    }

    void Release()
    {
        joint.enabled = false;
        active = false;
        motor.ReleaseTakeover();

        if (aimLine != null) aimLine.enabled = true;
        if (reticle != null) reticle.gameObject.SetActive(true);
    }

    public bool Apply(ref Rigidbody2D rigidbodyRef)
    {
        joint.distance = Mathf.Min(joint.distance, maxGrappleDistance);
        float reel = 0f;
        if (Input.GetKey(ControlsManager.GetKey(PlayerAction.ReelUp))) reel += 1f;
        if (Input.GetKey(ControlsManager.GetKey(PlayerAction.ReelDown))) reel -= 1f;

        if (reel < 0f && joint.distance >= maxGrappleDistance)
            reel = 0f;

        if (Mathf.Abs(reel) > 0.1f)
        {
            joint.distance = Mathf.Max(0.5f, joint.distance - reel * 5f * Time.fixedDeltaTime);
        }

        if (active)
        {
            Vector2 toAnchor = anchor - rb.position;
            if (toAnchor.sqrMagnitude > 0.0001f)
            {
                Vector2 dir = toAnchor.normalized;
                Vector2 tangent = new Vector2(dir.y, -dir.x);

                float h = 0f;
                if (Input.GetKey(ControlsManager.GetKey(PlayerAction.MoveRight))) h += 1f;
                if (Input.GetKey(ControlsManager.GetKey(PlayerAction.MoveLeft))) h -= 1f;

                if (Mathf.Abs(h) > 0.01f)
                {
                    rb.AddForce(tangent * h * swingForce, ForceMode2D.Force);
                }

                bool taut = Vector2.Distance(rb.position, anchor) >= joint.distance - 0.01f;

                if (reel > 0f && !taut)
                {
                    rb.AddForce(dir * pullForce, ForceMode2D.Force);
                }

                if (taut && radialDamp > 0f)
                {
                    float vRad = Vector2.Dot(rb.linearVelocity, dir);
                    rb.linearVelocity -= vRad * dir * radialDamp;
                }
            }
        }

        return true;
    }


    Vector2 GetAimDirection()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dir = mouse - transform.position;
        return (Vector2)dir;
    }
}
