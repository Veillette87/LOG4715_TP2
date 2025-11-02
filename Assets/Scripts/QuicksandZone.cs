using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuicksandZone : MonoBehaviour
{
    public Transform center;
    public float pullForce = 12f;
    public float sinkForce = 10f;
    public float maxSinkSpeed = 2f;
    public float sinkSmoothing = 5f;
    public float horizontalDamp = 10f;
    public float gravityInSand = 0.35f;


    Dictionary<Rigidbody2D, Vector2> originalParams = new Dictionary<Rigidbody2D, Vector2>();

    Dictionary<Rigidbody2D, float> exemptUntil = new Dictionary<Rigidbody2D, float>();
    public float exemptDurationDefault = 0.6f;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb == null) return;

        if (exemptUntil.TryGetValue(rb, out var t) && Time.time < t)
        {

            if (!originalParams.ContainsKey(rb))
                originalParams[rb] = new Vector2(rb.gravityScale, rb.linearDamping);
            Debug.Log("Entered sand but body is exempt until " + t);
            return;
        }


        originalParams[rb] = new Vector2(rb.gravityScale, rb.linearDamping);

        rb.gravityScale = gravityInSand;
        rb.linearDamping = 3f;
        Debug.Log("Entered sand");
    }


    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb == null) return;


        if (exemptUntil.TryGetValue(rb, out var until) && Time.time < until)
            return;


        Vector2 toCenter = ((Vector2)center.position - rb.position);
        if (toCenter.sqrMagnitude > 0.0001f)
            rb.AddForce(toCenter.normalized * pullForce, ForceMode2D.Force);


        var v = rb.linearVelocity;
        float desiredVy = -Mathf.Abs(maxSinkSpeed);
        v.y = Mathf.Lerp(v.y, desiredVy, sinkSmoothing * Time.fixedDeltaTime);
        rb.linearVelocity = v;


        var hv = rb.linearVelocity;
        hv.x = Mathf.Lerp(hv.x, 0f, horizontalDamp * Time.fixedDeltaTime);
        rb.linearVelocity = hv;


    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb == null) return;

        if (originalParams.TryGetValue(rb, out var v))
        {
            rb.gravityScale = v.x;
            rb.linearDamping = v.y;
            originalParams.Remove(rb);
        }
        exemptUntil.Remove(rb);
        Debug.Log("Exited sand");
    }


    public void ReleaseBody(Rigidbody2D rb, float duration = -1f)
    {
        if (rb == null) return;
        if (duration <= 0f) duration = exemptDurationDefault;

        if (originalParams.TryGetValue(rb, out var v))
        {
            rb.gravityScale = v.x;
            rb.linearDamping = v.y;
        }

        exemptUntil[rb] = Time.time + duration;
        Debug.Log("QuicksandZone: Released body for " + duration + "s");
    }
}
