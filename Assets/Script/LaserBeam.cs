using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public float speed = 25f;
    public int damage = 15;
    public float lifetime = 3f;
    public BossAI owner; // <<< TAMBAHAN: Referensi ke Boss

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player1 = other.GetComponent<PlayerController>();
        if (player1 != null)
        {
            player1.TakeDamage(damage);
            if (owner != null) owner.totalDamageDealt += damage; // <<< LAPORKAN DAMAGE
            Destroy(gameObject);
        }

        Player2Controller player2 = other.GetComponent<Player2Controller>();
        if (player2 != null)
        {
            player2.TakeDamage(damage);
            if (owner != null) owner.totalDamageDealt += damage; // <<< LAPORKAN DAMAGE
            Destroy(gameObject);
        }
    }
}
