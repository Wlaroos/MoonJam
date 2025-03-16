using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBase : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    [SerializeField] private float stunTime = 0.25f;
    [SerializeField] private float iFrameDuration = 1f;
    
    private bool isInvincible = false;
    private bool isDead = false;
    
    // Events to notify other systems (like UI, sound, etc.)
    public UnityEvent OnHealthChanged;
    public UnityEvent OnDeath;
    
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private void Awake()
    {
        // Get references (ensure the enemy has a Rigidbody2D, SpriteRenderer and optionally an Animator)
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        
        currentHealth = maxHealth;
        
        // Find the player by tag (make sure your player GameObject is tagged "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            // Calculate the normalized direction vector from enemy to player
            Vector2 direction = (player.position - transform.position).normalized;

            // Move the enemy towards the player
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);

            // Flip sprite based on the horizontal direction
            if (sr != null)
            {
                sr.flipX = direction.x < 0;
            }
            
            // Update animator parameter if using animations
            if (anim != null)
            {
                anim.SetBool("isMoving", direction != Vector2.zero);
            }
        }
    }
    
    /// <summary>
    /// Applies damage to the enemy along with an optional knockback force.
    /// </summary>
    /// <param name="force">Force vector to apply as knockback.</param>
    /// <param name="damage">Amount of damage to subtract from health.</param>
    public void TakeDamage(Vector2 force, int damage)
    {
        if (!isInvincible && !isDead)
        {
            // Apply knockback effect
            StartCoroutine(Knockback(force, stunTime));
            
            // Subtract health
            currentHealth -= damage;
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Trigger invincibility frames
                StopCoroutine(InvincibilityFrames(iFrameDuration));
                StartCoroutine(InvincibilityFrames(iFrameDuration));
            }
            
            // Notify any listeners that health has changed
            OnHealthChanged?.Invoke();
        }
    }
    
    private IEnumerator InvincibilityFrames(float duration)
    {
        isInvincible = true;
        // Optionally, you can change the sprite's color here to visually indicate iFrames.
        yield return new WaitForSeconds(duration);
        if (sr != null)
        {
            sr.color = Color.white;
        }
        isInvincible = false;
    }
    
    private IEnumerator Knockback(Vector2 force, float duration)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
        if (sr != null)
        {
            sr.color = Color.red; // Change color to indicate damage
        }
        yield return new WaitForSeconds(duration);
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }
    
    private void Die()
    {
        isDead = true;
        // Optionally, trigger a death animation
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        
        OnDeath?.Invoke();
        
        // Destroy the enemy after a short delay to allow death animations/effects to play
        Destroy(gameObject, 1f);
    }
}
