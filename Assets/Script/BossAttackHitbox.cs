using UnityEngine;

// Script ini adalah untuk serangan Ground Slam Boss
public class BossAttackHitbox : MonoBehaviour
{
    public int damage = 25;
    // Referensi ke Boss yang melakukan serangan ini, untuk melaporkan statistik
    public BossAI owner; 

    // Hancurkan hitbox ini setelah 0.5 detik
    void Start()
    {
        Destroy(gameObject, 0.5f);
    }

    // Fungsi ini akan dipanggil saat trigger hitbox ini menyentuh collider lain
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang disentuh adalah Player 1
        PlayerController player1 = other.GetComponent<PlayerController>();
        if (player1 != null)
        {
            player1.TakeDamage(damage);
            // Laporkan kerusakan kembali ke Boss jika owner sudah di-set
            if (owner != null) owner.totalDamageDealt += damage;
        }

        // Cek apakah yang disentuh adalah Player 2
        Player2Controller player2 = other.GetComponent<Player2Controller>();
        if (player2 != null)
        {
            player2.TakeDamage(damage);
            // Laporkan kerusakan kembali ke Boss jika owner sudah di-set
            if (owner != null) owner.totalDamageDealt += damage;
        }
    }
}

