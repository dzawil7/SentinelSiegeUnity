using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Status Player")]
    public int health = 100;
    public int maxHealth = 100;
    public int totalDamageDealt = 0;

    [Header("Pengaturan Gerakan")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;

    [Header("Pengaturan Serangan Melee")]
    [SerializeField] private GameObject meleeHitboxPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private bool isOnGround = true;
    private bool isFacingRight = true; // <<< TAMBAHAN: Untuk mengingat arah hadap

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // --- Input Gerakan (A, D) ---
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // --- Logika Membalikkan Arah Karakter ---
        // <<< TAMBAHAN: Panggil fungsi Flip() jika perlu
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        // --- Input Lompat (W) ---
        if (Input.GetKeyDown(KeyCode.W) && isOnGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // --- Input Serangan (S) ---
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Attack();
            }
        }
    }
    
    // <<< TAMBAHAN: Fungsi baru untuk membalikkan karakter
    private void Flip()
    {
        // Ubah status arah hadap
        isFacingRight = !isFacingRight;
        
        // Dapatkan skala saat ini
        Vector3 theScale = transform.localScale;
        // Balikkan nilai sumbu x
        theScale.x *= -1;
        // Terapkan skala yang sudah dibalik
        transform.localScale = theScale;
    }

    private void Attack()
    {
       // 1. Buat kloningan dari prefab
    GameObject hitboxGO = Instantiate(meleeHitboxPrefab, attackPoint.position, attackPoint.rotation);
    
    // 2. Ambil script dari kloningan tersebut
    AttackHitbox hitboxScript = hitboxGO.GetComponent<AttackHitbox>();

    // 3. Set 'owner' dari script kloningan tersebut
    if (hitboxScript != null)
    {
        hitboxScript.owner = this;
    }
    else
    {
        Debug.LogError("FATAL ERROR: Prefab 'MeleeHitbox' Anda tidak memiliki script 'AttackHitbox.cs'!");
    }
    
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

