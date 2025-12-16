using UnityEngine;

public class Player2Controller : MonoBehaviour
{
    [Header("Status Player")]
    public int health = 100;
    public int maxHealth = 100;
    public int totalDamageDealt = 0;
    public bool isDead = false; // Tambahan state mati

    [Header("Pengaturan Gerakan")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;

    [Header("Pengaturan Serangan Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Referensi untuk Membidik")]
    [SerializeField] private Transform bossTransform; 

    private float nextAttackTime = 0f;
    private Rigidbody2D rb;
    private Animator anim; // Referensi Animator
    private bool isOnGround = true;
    private bool isFacingRight = true;
    private Collider2D col; // Referensi Collider

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead) return; // Stop input jika mati

        // --- Input Gerakan (J, L, I) ---
        float moveDirection = 0f;

        if (Input.GetKey(KeyCode.J)) {
            moveDirection = -1f;
        } else if (Input.GetKey(KeyCode.L)) {
            moveDirection = 1f;
        }
        
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        // Animasi Jalan
        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(moveDirection));
        // Animasi Ground Check (Opsional jika ada parameter IsGrounded)
        if (anim != null) anim.SetBool("IsGrounded", isOnGround);

        // --- Logika Membalikkan Arah Karakter ---
        if (moveDirection > 0 && !isFacingRight) {
            Flip();
        } else if (moveDirection < 0 && isFacingRight) {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.I) && isOnGround) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // Animasi Lompat
            if (anim != null) anim.SetTrigger("Jump");
            
            isOnGround = false; // Mencegah double jump instan
        }

        // --- Input Serangan (K) ---
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
        
        // Memutar firePoint agar peluru keluar ke arah yang benar (karena transform dibalik)
        if(firePoint != null)
        {
             firePoint.Rotate(0f, 180f, 0f);
        }
    }

    // <<< FUNGSI ATTACK() >>>
    private void Attack()
    {
        // Pengaman: Jangan lakukan apa-apa jika Boss belum di-set
        if (bossTransform == null)
        {
            Debug.LogError("Boss Transform belum di-set di Player2Controller! Serangan dibatalkan.");
            return;
        }

        // Animasi Attack
        if (anim != null) anim.SetTrigger("Attack");

        // 1. Hitung arah dari titik tembak (firePoint) ke posisi Boss
        Vector2 directionToBoss = (bossTransform.position - firePoint.position).normalized;

        // 2. Hitung sudut dari arah tersebut (dalam derajat)
        float angle = Mathf.Atan2(directionToBoss.y, directionToBoss.x) * Mathf.Rad2Deg;

        // 3. Buat rotasi Quaternion dari sudut yang sudah dihitung
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        // 4. Tembakkan proyektil dengan posisi dan ROTASI yang sudah benar
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, targetRotation);
        
        // 5. Cek apakah script ada sebelum menetapkan owner
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Jika ada, beri tahu proyektil siapa pemiliknya untuk statistik
            projectileScript.owner = this;
        }
        else
        {
            Debug.LogError("FATAL ERROR: 'projectilePrefab' Anda tidak memiliki script 'Projectile.cs' terpasang!");
        }

        // 6. Set cooldown
        nextAttackTime = Time.time + attackCooldown;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        
        // Animasi Terluka
        if (anim != null) anim.SetTrigger("Hurt");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        health = 0;

        // Trigger Animasi Mati
        if (anim != null) anim.SetBool("IsDead", true); // Atau SetTrigger("Die")

        // Matikan fisika agar mayat tidak gerak/jatuh aneh
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; 
        
        // Matikan collider
        if(col != null) col.enabled = false;

        Debug.Log("Player 2 Died.");
    }

    // --- FUNGSI ANIMATION EVENT (WAJIB DI-SETUP DI UNITY) ---
    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) { isOnGround = true; }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")) { isOnGround = false; }
    }
}