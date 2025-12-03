using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float lifetime = 10f;

    public AudioSource arrowSource;
    public AudioClip hitWallClip;
    public AudioClip hitPlayerClip;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GlobalSettings.Instance.PlayArrowSound();
            PlayerHealth player = collision.collider.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Vector2 hitDir = (collision.transform.position - transform.position).normalized;

                player.TakeDamage(1, hitDir);
            }
            Destroy(gameObject);
        }
        arrowSource.PlayOneShot(hitWallClip);
    }
}