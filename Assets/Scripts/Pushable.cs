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

        // ✅ Laisse la gravité active
        rb.gravityScale = 10f;

        // Évite la rotation mais garde les mouvements
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Rend les mouvements un peu plus “lourds” sans annuler la gravité
        rb.linearDamping = 2f;
    }

    void FixedUpdate()
    {
        // ✅ On n’annule plus complètement la vélocité,
        // sinon la gravité ne s’applique jamais !
        if (!isBeingPushed)
        {
            // On peut juste freiner légèrement le mouvement horizontal
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
