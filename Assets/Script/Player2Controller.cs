using UnityEngine;

public class Player2Controller : MonoBehaviour
{
    [Header("Status Player")]
    public int health = 100;
    public int maxHealth = 100;
    public int totalDamageDealt = 0;
    public bool isDead = false;

    [Header("Pengaturan Gerakan")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;

    [Header("Pengaturan Serangan Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Referensi untuk Membidik")]
    [SerializeField] private Transform bossTransform; 

    // --- Variabel Ground Check Asli ---
    // Variabel ini dipertahankan di Inspector, tetapi tidak digunakan di kode karena menggunakan Collision Events
    [Header("Ground Check (Collision Based)")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // --- Internal State ---
    private float nextAttackTime = 0f;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    
    private bool isOnGround = true; // Diatur oleh OnCollisionEnter/Exit
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead) return;

        // --- 1. Input Gerakan (J, L) ---
        float moveDirection = 0f;

        if (Input.GetKey(KeyCode.J)) {
            moveDirection = -1f;
        } else if (Input.GetKey(KeyCode.L)) {
            moveDirection = 1f;
        }
        
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        // Animasi
        if (anim != null) 
        {
            anim.SetFloat("Speed", Mathf.Abs(moveDirection));
            anim.SetBool("IsGrounded", isOnGround);
        }

        // --- 2. Logika Flip ---
        if (moveDirection > 0 && !isFacingRight) {
            Flip();
        } else if (moveDirection < 0 && isFacingRight) {
            Flip();
        }

        // --- 3. Input Lompat (LOGIKA ASLI TANPA BUFFER/COYOTE) ---
        if (Input.GetKeyDown(KeyCode.I) && isOnGround) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // Integrasi Animasi Jump
            if (anim != null) anim.SetTrigger("Jump");
            
            isOnGround = false; // Mencegah double jump instan
        }

        // --- 4. Input Serangan (K) ---
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Attack();
            }
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        
        if(firePoint != null)
        {
            firePoint.Rotate(0f, 180f, 0f);
        }
    }

    private void Attack()
    {
        // Integrasi Animasi Attack
        if (anim != null) anim.SetTrigger("Attack");

        if (bossTransform == null)
        {
            Debug.LogError("Boss Transform belum di-set di Player2Controller!");
            return;
        }

        Vector2 directionToBoss = (bossTransform.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(directionToBoss.y, directionToBoss.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, targetRotation);
        
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();
        if (projectileScript != null)
        {
             projectileScript.owner = this;
        }

        nextAttackTime = Time.time + attackCooldown;
    }

    public void TakeDamage(int damage)
    { 
        if (isDead) return;
        health -= damage;
        
        // Integrasi Animasi Hurt
        if (anim != null) anim.SetTrigger("Hurt");

        if (health <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        health = 0;

        // Integrasi Animasi Death
        if (anim != null) anim.SetBool("IsDead", true); 

        rb.velocity = Vector2.zero;
        rb.isKinematic = true; 
        
        if(col != null) col.enabled = false;

        Debug.Log("Player 2 Died.");
    }

    // --- LOGIKA DETEKSI TANAH ASLI (COLLISION) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) { isOnGround = true; }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) { isOnGround = false; }
    }
    
    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }
}