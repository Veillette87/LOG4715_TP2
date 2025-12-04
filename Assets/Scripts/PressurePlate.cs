using UnityEngine;
using System.Collections;

public enum ShootDirection
{
    Left,
    Right,
    Up,
    Down
}

public class PressurePlateObject : MonoBehaviour
{
    public Sprite idleSprite;
    public Sprite activatedSprite;

    private SpriteRenderer spriteRenderer;
    private bool activated = false;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    public Transform[] shootPoints;
    public ShootDirection shootDirection = ShootDirection.Left;
    public float shootSpeed = 10f;
    public float resetTime = 2f; private float boxColliderOffsetPressedY = -0.35f;
    private float boxColliderOffsetReadyY = -0.3f;
    private float boxColliderSizePressedY = 0.3f;
    private float boxColliderSizeReadyY = 0.4f;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer.sprite = idleSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            spriteRenderer.sprite = activatedSprite;
            boxCollider.offset = new Vector2(boxCollider.offset.x, boxColliderOffsetPressedY);
            boxCollider.size = new Vector2(boxCollider.size.x, boxColliderSizePressedY);
            activated = true;

            ShootArrow();
            StartCoroutine(ResetPlate());
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (activated) return;
        if (other.CompareTag("Player"))
        {
            spriteRenderer.sprite = activatedSprite;
            boxCollider.offset = new Vector2(boxCollider.offset.x, boxColliderOffsetPressedY);
            boxCollider.size = new Vector2(boxCollider.size.x, boxColliderSizePressedY);
            activated = true;

            ShootArrow();
            StartCoroutine(ResetPlate());
        }
    }

    private void ShootArrow()
    {
        if (arrowPrefab != null && shootPoints != null && shootPoints.Length > 0)
        {
            foreach (Transform shootPoint in shootPoints)
            {
                if (shootPoint != null)
                {
                    GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, shootPoint.rotation);
                    Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direction = GetShootDirection();
                        rb.linearVelocity = direction * shootSpeed;

                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
                    }
                }
            }
        }
    }

    private Vector2 GetShootDirection()
    {
        switch (shootDirection)
        {
            case ShootDirection.Left:
                return Vector2.left;
            case ShootDirection.Right:
                return Vector2.right;
            case ShootDirection.Up:
                return Vector2.up;
            case ShootDirection.Down:
                return Vector2.down;
            default:
                return Vector2.left;
        }
    }

    private IEnumerator ResetPlate()
    {
        yield return new WaitForSeconds(resetTime);
        spriteRenderer.sprite = idleSprite;
        boxCollider.offset = new Vector2(boxCollider.offset.x, boxColliderOffsetReadyY);
        boxCollider.size = new Vector2(boxCollider.size.x, boxColliderSizeReadyY);
        activated = false;
    }

}
