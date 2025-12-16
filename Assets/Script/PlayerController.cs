using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Identitas Player")]
    [Tooltip("Isi 1 untuk Player 1 (WASD), Isi 2 untuk Player 2 (Panah)")]
    public int playerID = 1;

    [Header("Status Player")]
    public int health = 100;
    public int maxHealth = 100;
    public int totalDamageDealt = 0;
    public bool isDead = false;

    [Header("Pengaturan Gerakan")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 16f;

    [Header("Pengaturan Serangan Melee")]
    [SerializeField] private GameObject meleeHitboxPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // Komponen
    private Rigidbody2D rb;
    private Animator anim;

    // Variabel Internal
    private bool isOnGround = true;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        // --- 1. Cek Tanah (Ground Check) ---
        if (groundCheck != null)
        {
            // Update status tanah
            isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Kirim info tanah ke Animator (PENTING untuk transisi Mendarat/Landing)
        if (anim != null) anim.SetBool("IsGrounded", isOnGround);

        // --- 2. Input Gerakan ---
        float moveInput = 0f;
        if (playerID == 1)
        {
            if (Input.GetKey(KeyCode.D)) moveInput = 1;
            else if (Input.GetKey(KeyCode.A)) moveInput = -1;
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow)) moveInput = 1;
            else if (Input.GetKey(KeyCode.LeftArrow)) moveInput = -1;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (anim != null) anim.SetFloat("Speed", Mathf.Abs(moveInput));

        // --- 3. Logika Flip ---
        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();

        // --- 4. Input Lompat (FIX ANIMASI STUCK) ---
        KeyCode jumpKey = (playerID == 1) ? KeyCode.W : KeyCode.UpArrow;

        if (Input.GetKeyDown(jumpKey) && isOnGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // FIX: Gunakan Trigger untuk memulai lompat (sekali panggil)
            // Daripada mengandalkan bool IsGrounded yang bisa bikin looping
            if (anim != null) anim.SetTrigger("Jump"); 
            
            isOnGround = false; // Paksa false agar tidak double jump di frame yang sama
        }

        // --- 5. Input Serangan ---
        bool attackInput = (playerID == 1) 
            ? (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
            : (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightControl));

        if (attackInput && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        if (meleeHitboxPrefab != null && attackPoint != null)
        {
            GameObject hitboxGO = Instantiate(meleeHitboxPrefab, attackPoint.position, attackPoint.rotation);
            AttackHitbox hitboxScript = hitboxGO.GetComponent<AttackHitbox>();
            // SAYA PERTAHANKAN: owner = this sesuai kode asli Anda
            if (hitboxScript != null) hitboxScript.owner = this; 
        }
        
        nextAttackTime = Time.time + attackCooldown;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;
        if (anim != null) anim.SetTrigger("Hurt");

        if (health <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return; // Mencegah pemanggilan ganda

        isDead = true;
        health = 0;
        if (anim != null) anim.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Opsional: Mematikan collider agar mayat tidak menghalangi (opsional, bisa dihapus jika tidak perlu)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("Player " + playerID + " Died.");
        
        // MODIFIKASI: Destroy dihapus dari sini agar animasi bisa jalan dulu.
        // Destroy akan dipanggil via Event.
    }

    public void AddDamageScore(int damage)
    {
        totalDamageDealt += damage;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // --- FUNGSI TAMBAHAN UNTUK ANIMATION EVENT ---
    // Cara Setup: Window > Animation > Pilih Clip Death > Frame Terakhir > Add Event > Pilih Fungsi Ini
    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }
}