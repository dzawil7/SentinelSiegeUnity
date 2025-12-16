using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 20;
    // Pastikan variabel 'owner' ini ada dan bertipe 'PlayerController'
    public PlayerController owner; 

    void Start() { Destroy(gameObject, 0.2f); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BossAI boss = other.GetComponent<BossAI>();
        
        // Cek kritis: Apakah boss ada DAN owner sudah di-set?
        if (boss != null && owner != null)
        {
            boss.TakeDamage(damage);
            // Baris ini adalah yang menambahkan statistik. Pastikan ini ada.
            owner.totalDamageDealt += damage;
            Destroy(gameObject);
        }
    }
}