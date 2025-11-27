using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk bisa mengakses komponen UI

public class UIManager : MonoBehaviour
{
    [Header("Referensi Komponen UI")]
    public Image bossHealthFill;
    public Image player1HealthFill;
    public Image player2HealthFill;

    [Header("Referensi Script Karakter")]
    public BossAI bossScript;
    public PlayerController player1Script;
    public Player2Controller player2Script;

    // Update dipanggil setiap frame, cocok untuk memperbarui UI secara real-time
    void Update()
    {
        // --- Update Health Bar Boss ---
        // Cek dulu apakah boss-nya masih ada di scene
        if (bossScript != null)
        {
            // Hitung persentase health (nilai antara 0.0 dan 1.0)
            float healthPercentage = bossScript.health / bossScript.maxHealth;
            // Atur nilai Fill Amount pada komponen Image
            bossHealthFill.fillAmount = healthPercentage;
        }
        else
        {
            bossHealthFill.fillAmount = 0; // Kosongkan bar jika boss sudah kalah
        }

        // --- Update Health Bar Player 1 ---
        if (player1Script != null)
        {
            // '(float)' memastikan hasil pembagiannya berupa desimal
            float healthPercentage = (float)player1Script.health / player1Script.maxHealth;
            player1HealthFill.fillAmount = healthPercentage;
        }
        else
        {
            player1HealthFill.fillAmount = 0;
        }

        // --- Update Health Bar Player 2 ---
        if (player2Script != null)
        {
            float healthPercentage = (float)player2Script.health / player2Script.maxHealth;
            player2HealthFill.fillAmount = healthPercentage;
        }
        else
        {
            player2HealthFill.fillAmount = 0;
        }
    }
}

