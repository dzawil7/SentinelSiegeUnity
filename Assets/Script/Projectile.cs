using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 15;
    public float lifetime = 2f;
    // <<< TAMBAHAN: Variabel untuk menyimpan siapa "pemilik" proyektil ini.
    public Player2Controller owner; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BossAI boss = other.GetComponent<BossAI>();
        
        // Cek apakah yang disentuh adalah Boss DAN proyektil ini punya pemilik.
        if (boss != null && owner != null)
        {
            // 1. Beri kerusakan pada Boss.
            boss.TakeDamage(damage);

            // 2. <<< TAMBAHAN: Laporkan kerusakan yang diberikan ke 'owner'.
            owner.totalDamageDealt += damage;
            
            // 3. Hancurkan diri sendiri saat mengenai target.
            Destroy(gameObject);
        }
    }
}

