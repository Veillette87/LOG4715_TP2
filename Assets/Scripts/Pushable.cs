using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Pushable : MonoBehaviour
{
    [Header("Paramètres de poussée")]
    public float pushForce = 10f;
    public float maxSpeed = 1.2f;

    private Rigidbody2D rb;
    private bool isBeingPushed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 10f;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.linearDamping = 2f;
    }

    void FixedUpdate()
    {
        if (!isBeingPushed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
        }

        isBeingPushed = false;
    }

    public void Push(Vector2 direction)
    {
        rb.AddForce(direction * pushForce, ForceMode2D.Force);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        isBeingPushed = true;
    }
}
