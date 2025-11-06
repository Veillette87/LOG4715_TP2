using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerQuicksand : MonoBehaviour
{
    public KeyCode jumpKey = KeyCode.Space;
    public int jumpsNeeded = 5;
    public float escapeImpulse = 15f;

    int jumps;
    // Expose whether the player is currently inside quicksand so movement can adapt
    public bool InSand { get; private set; }
    Rigidbody2D rb;
    QuicksandZone currentZone;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning("PlayerQuicksand: Rigidbody2D missing on this GameObject.");

        var col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogWarning("PlayerQuicksand: Collider2D missing on this GameObject.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {


        var zone = other.GetComponent<QuicksandZone>();
        if (zone != null)
        {
            Debug.Log("Player: Entered quicksand zone: " + other.gameObject.name);
            InSand = true; // mark public flag
            jumps = 0;
            currentZone = zone;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var zone = other.GetComponent<QuicksandZone>();
        if (zone != null && zone == currentZone)
        {
            Debug.Log("Player: Exited quicksand zone: " + other.gameObject.name);
            InSand = false; // clear public flag
            jumps = 0;
            currentZone = null;
        }
    }

    void Update()
    {
        if (!InSand) return;

        if (Input.GetKeyDown(jumpKey))
        {
            jumps++;
            Debug.Log("Player: quicksand jump press " + jumps + "/" + jumpsNeeded);

            if (jumps >= jumpsNeeded)
            {
                if (rb == null) rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    currentZone?.ReleaseBody(rb);

                    rb.gravityScale = 1f;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

                    // high jump
                    rb.AddForce(Vector2.up * escapeImpulse, ForceMode2D.Impulse);

                    StartCoroutine(TemporaryImmunity(0.25f));

                    Debug.Log("Player: escaped quicksand");
                }

                jumps = 0;
                InSand = false;
                currentZone = null;
            }

        }
    }

    System.Collections.IEnumerator TemporaryImmunity(float delay)
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
            yield return new WaitForSeconds(delay);
            col.enabled = true;
        }
    }
}
