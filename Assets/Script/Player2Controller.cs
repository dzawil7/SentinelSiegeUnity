using UnityEngine;

public class Player2Controller : MonoBehaviour
{
    [Header("Status Player")]
    public int health = 100;
    public int maxHealth = 100;
    public int totalDamageDealt = 0;

    [Header("Pengaturan Gerakan")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;

    [Header("Pengaturan Serangan Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 0.8f;

    [Header("Referensi untuk Membidik")]
    [SerializeField] private Transform bossTransform; // <<< TAMBAHAN: Slot untuk referensi Boss

    private float nextAttackTime = 0f;
    private Rigidbody2D rb;
    private bool isOnGround = true;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- Input Gerakan (J, L, I) ---
        float moveDirection = 0f;

        if (Input.GetKey(KeyCode.J)) {
            moveDirection = -1f;
        } else if (Input.GetKey(KeyCode.L)) {
            moveDirection = 1f;
        }
        
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        // --- Logika Membalikkan Arah Karakter ---
        if (moveDirection > 0 && !isFacingRight) {
            Flip();
        } else if (moveDirection < 0 && isFacingRight) {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.I) && isOnGround) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
    }

    // <<< FUNGSI ATTACK() SEKARANG MENGHITUNG ARAH KE BOSS >>>
    private void Attack()
    {
        // Pengaman: Jangan lakukan apa-apa jika Boss belum di-set
        if (bossTransform == null)
        {
            Debug.LogError("Boss Transform belum di-set di Player2Controller! Serangan dibatalkan.");
            return;
        }

        // 1. Hitung arah dari titik tembak (firePoint) ke posisi Boss
        Vector2 directionToBoss = (bossTransform.position - firePoint.position).normalized;

        // 2. Hitung sudut dari arah tersebut (dalam derajat)
        float angle = Mathf.Atan2(directionToBoss.y, directionToBoss.x) * Mathf.Rad2Deg;

        // 3. Buat rotasi Quaternion dari sudut yang sudah dihitung
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        // 4. Tembakkan proyektil dengan posisi dan ROTASI yang sudah benar
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, targetRotation);
        
        // 5. <<< PERUBAHAN: Cek apakah script ada sebelum menetapkan owner >>>
        Projectile projectileScript = projectileGO.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            // Jika ada, beri tahu proyektil siapa pemiliknya untuk statistik
            projectileScript.owner = this;
        }
        else
        {
            // Jika tidak ada, beri pesan error yang jelas di Console!
            Debug.LogError("FATAL ERROR: 'projectilePrefab' Anda tidak memiliki script 'Projectile.cs' terpasang! Statistik tidak akan tercatat.");
        }

        // 6. Set cooldown
        nextAttackTime = Time.time + attackCooldown;
    }


    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
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

