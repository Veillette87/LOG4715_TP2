using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerQuicksand : MonoBehaviour
{
    public KeyCode jumpKey = KeyCode.Space;
    public int jumpsNeeded = 5;
    public float escapeImpulse = 8f;

    int jumps;
    bool inSand;
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
        // Helpful log so you can see which object triggered and whether it has the QuicksandZone component
        Debug.Log("Player: OnTriggerEnter2D -> " + other.gameObject.name + " (has QuicksandZone: " + (other.GetComponent<QuicksandZone>() != null) + ")");

        var zone = other.GetComponent<QuicksandZone>();
        if (zone != null)
        {
            Debug.Log("Player: Entered quicksand zone: " + other.gameObject.name);
            inSand = true;
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
            inSand = false;
            jumps = 0;
            currentZone = null;
        }
    }

    void Update()
    {
        if (!inSand) return;

        if (Input.GetKeyDown(jumpKey))
        {
            jumps++;
            Debug.Log("Player: quicksand jump press " + jumps + "/" + jumpsNeeded);

            if (jumps >= jumpsNeeded)
            {
                if (rb == null) rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // release body from current quicksand zone so the zone doesn't counter the escape
                    currentZone?.ReleaseBody(rb);

                    // reset vertical velocity then apply upward impulse
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    rb.AddForce(Vector2.up * escapeImpulse, ForceMode2D.Impulse);
                    Debug.Log("Player: escaped quicksand");
                }
                jumps = 0;
                // mark as out for the player's logic; the zone will still see the player inside the trigger
                inSand = false;
                currentZone = null;
            }
        }
    }
}
