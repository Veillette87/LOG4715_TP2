using UnityEngine;
using System.Collections;


public class PressurePlateObject : MonoBehaviour
{
    public Sprite idleSprite;
    public Sprite activatedSprite;

    private SpriteRenderer spriteRenderer;
    private bool activated = false;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;     
    public Transform shootPoint;       
    public float shootSpeed = 10f;
    public float resetTime = 2f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = idleSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            spriteRenderer.sprite = activatedSprite;
            activated = true;

            ShootArrow();
            StartCoroutine(ResetPlate());
        }
    }

    private void ShootArrow()
    {
        if (arrowPrefab != null && shootPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = -shootPoint.right;
                rb.linearVelocity = direction * shootSpeed;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private IEnumerator ResetPlate()
    {
        yield return new WaitForSeconds(resetTime);
        spriteRenderer.sprite = idleSprite;
        activated = false;
    }

}
