using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Paramètres de vie")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("UI des HPs")]
    public Image[] healthImages;

    [Header("Invincibilité")]
    public float invincibilityTime = 2f;
    private bool isInvincible = false;

    [Header("Knockback")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 6f;

    [Header("Dégâts continus")]
    public float damageInterval = 1f; // temps entre deux dégâts si on reste sur un pic

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private PlayerController2D controller;
    private Animator anim;

    private Coroutine damageCoroutine;
    private Coroutine invincibilityCoroutine;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController2D>();
        anim = GetComponent<Animator>();
        UpdateHealthUI();
    }

    public void TakeDamage(int amount, Vector2 hitDirection)
    {
        // Pas de dégâts si invincible ou déjà mort
        if (isInvincible || isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();

        ApplyKnockback(hitDirection);

        // Animation de blessure
        if (anim != null)
            anim.SetTrigger("IsHurt");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (invincibilityCoroutine != null)
                StopCoroutine(invincibilityCoroutine);

            invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
        }
    }

    void ApplyKnockback(Vector2 hitDirection)
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        Vector2 force = new Vector2(hitDirection.x * knockbackForceX, knockbackForceY);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    void UpdateHealthUI()
    {
        if (healthImages == null || healthImages.Length == 0) return;

        for (int i = 0; i < healthImages.Length; i++)
        {
            bool shouldBeVisible = (i < currentHealth);

            if (!shouldBeVisible && healthImages[i].enabled)
            {
                StartCoroutine(BlinkAndHide(healthImages[i]));
            }
            else
            {
                healthImages[i].enabled = shouldBeVisible;
            }
        }
    }

    IEnumerator BlinkAndHide(Image img)
    {
        float blinkDuration = 0.8f;
        float elapsed = 0f;

        Color original = img.color;
        Color transparent = new Color(original.r, original.g, original.b, 0f);

        while (elapsed < blinkDuration)
        {
            img.color = (elapsed % 0.2f < 0.05f) ? transparent : original;
            elapsed += Time.deltaTime;
            yield return null;
        }

        img.enabled = false;
        img.color = original;
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        Color originalColor = sr.color;

        while (elapsed < invincibilityTime)
        {
            // Effet de clignotement
            sr.color = new Color(1f, 0.5f, 0.5f, 0.7f);
            yield return new WaitForSeconds(0.1f);
            sr.color = originalColor;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        isInvincible = false;
        sr.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (controller != null)
        {
            controller.enabled = false;
        }

        // Animation de mort
        if (anim != null)
            anim.SetBool("IsDead", true);

        StartCoroutine(HandleDeath());
    }

    IEnumerator HandleDeath()
    {
        // Attend un peu pour laisser jouer l’animation de mort
        yield return new WaitForSeconds(1.5f);

        // Trouve le script FadeController dans la scène
        FadeController fade = FadeController.Instance;
        if (fade != null)
        {
            yield return fade.FadeOutAndReload();
        }
        else
        {
            // Si pas de fade, recharge directement
            Scene current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }
    }

    // Détection des pics
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Spikes"))
        {
            StartDamageOverTime(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Spikes"))
        {
            StopDamageOverTime();
        }
    }

    void StartDamageOverTime(Collider2D spike)
    {
        if (damageCoroutine == null)
            damageCoroutine = StartCoroutine(DamageOverTime(spike));
    }

    void StopDamageOverTime()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    IEnumerator DamageOverTime(Collider2D spike)
    {
        while (true)
        {
            // Attendre tant qu’on est invincible ou mort
            while (isInvincible || isDead)
                yield return null;

            Vector2 hitDir = (transform.position - spike.transform.position).normalized;
            TakeDamage(1, hitDir);

            // Attendre avant de pouvoir reprendre un dégât
            float t = 0f;
            while (t < damageInterval)
            {
                yield return null;
                t += Time.deltaTime;
            }
        }
    }
}
