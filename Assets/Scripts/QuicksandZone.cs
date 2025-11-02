using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class QuicksandZone : MonoBehaviour
{
    public Transform center;        // assigne ton Empty "Center"
    public float pullForce = 12f;   // attire vers le centre
    public float sinkForce = 10f;   // (legacy) force vers le bas — remplacé par maxSinkSpeed/smoothing
    public float maxSinkSpeed = 2f; // vitesse maximale d'enfoncement (valeur positive, en unités/s)
    public float sinkSmoothing = 5f; // rapidité avec laquelle on approche la vitesse de sink
    public float horizontalDamp = 10f; // freine horizontalement
    public float gravityInSand = 0.35f;
    public float dps = 6f;          // dégâts par seconde (optionnel)

    // sauvegarder les valeurs originales pour restaurer à la sortie
    // stocke les paramètres originaux par Rigidbody (x = gravityScale, y = linearDamping)
    Dictionary<Rigidbody2D, Vector2> originalParams = new Dictionary<Rigidbody2D, Vector2>();
    // corps exemptés jusqu'à un certain temps
    Dictionary<Rigidbody2D, float> exemptUntil = new Dictionary<Rigidbody2D, float>();
    public float exemptDurationDefault = 0.6f; // durée pendant laquelle le corps n'est pas affecté après release

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        // si ce corps est temporairement exempté, ne pas ré-appliquer les effets
        if (exemptUntil.TryGetValue(rb, out var t) && Time.time < t)
        {
            // ensure original params are registered so we can restore later if needed
            if (!originalParams.ContainsKey(rb))
                originalParams[rb] = new Vector2(rb.gravityScale, rb.linearDamping);
            Debug.Log("Entered sand but body is exempt until " + t);
            return;
        }

        // sauvegarde des valeurs avant modification (par corps)
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

        // si exempté temporairement, ne rien appliquer
        if (exemptUntil.TryGetValue(rb, out var until) && Time.time < until)
            return;

        // attirer vers le centre
        Vector2 toCenter = ((Vector2)center.position - rb.position);
        if (toCenter.sqrMagnitude > 0.0001f)
            rb.AddForce(toCenter.normalized * pullForce, ForceMode2D.Force);

        // enfoncement graduel : on cible une vitesse verticale descendante
        // (on évite d'appliquer une grosse force qui ferait "tomber" le joueur)
        var v = rb.linearVelocity;
        float desiredVy = -Mathf.Abs(maxSinkSpeed); // vitesse descendante cible (négative)
        v.y = Mathf.Lerp(v.y, desiredVy, sinkSmoothing * Time.fixedDeltaTime);
        rb.linearVelocity = v;

        // immobiliser progressivement à l’horizontale
        var hv = rb.linearVelocity;
        hv.x = Mathf.Lerp(hv.x, 0f, horizontalDamp * Time.fixedDeltaTime);
        rb.linearVelocity = hv;

        // dégâts optionnels si tu as un système de vie:
        // GetComponentInParent<Health>()?.TakeDamage(dps * Time.deltaTime);
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        // restaurer les valeurs sauvegardées (par corps)
        if (originalParams.TryGetValue(rb, out var v))
        {
            rb.gravityScale = v.x;
            rb.linearDamping = v.y;
            originalParams.Remove(rb);
        }
        exemptUntil.Remove(rb);
        Debug.Log("Exited sand");
    }

    // appelé par le player lorsqu'il s'échappe ; restaure immédiatement les paramètres
    // et ignore ce Rigidbody pendant 'duration' secondes
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
