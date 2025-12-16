using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Boss")]
    public Image bossHealthFill;
    [Tooltip("Wajib: Image dengan Tipe Filled, Method Horizontal")]
    public Image bossStaminaFill; 

    [Header("UI Player")]
    public Image player1HealthFill;
    public Image player2HealthFill;

    [Header("Dependencies")]
    public BossAI bossScript;
    public PlayerController player1Script;
    public Player2Controller player2Script;

    void Update()
    {
        // 1. UPDATE BOSS UI
        if (bossScript != null)
        {
            // HP Bar - Gunakan Mathf.Clamp01 agar nilai tidak pernah minus (cegah glitch visual)
            bossHealthFill.fillAmount = Mathf.Clamp01(bossScript.health / bossScript.maxHealth);

            // Stamina Bar (Hanya aktif visualnya di Mode Set B)
            if (bossStaminaFill != null)
            {
                // Cek Mode lewat GameSettings
                if (GameSettings.SelectedMode == AIMode.FuSM_SetB)
                {
                    bossStaminaFill.fillAmount = bossScript.GetStaminaPercent();
                    
                    // UBAH WARNA: Kuning (Normal) -> Abu-abu (Recharging/Lelah)
                    bossStaminaFill.color = bossScript.IsBossRecharging() ? Color.gray : Color.yellow;
                }
                else
                {
                    // Mode lain tidak pakai stamina, penuhkan saja
                    bossStaminaFill.fillAmount = 1f;
                    bossStaminaFill.color = Color.white; 
                }
            }
        }
        else
        {
            // FIX: Jika boss script hilang (mati/destroyed), paksa bar jadi 0
            bossHealthFill.fillAmount = 0f;
            
            if(bossStaminaFill != null) 
                bossStaminaFill.fillAmount = 0f;
        }

        // 2. UPDATE PLAYER UI
        if (player1Script != null)
            player1HealthFill.fillAmount = (float)player1Script.health / player1Script.maxHealth;
        else
            player1HealthFill.fillAmount = 0;
        
        if (player2Script != null)
            player2HealthFill.fillAmount = (float)player2Script.health / player2Script.maxHealth;
        else
            player2HealthFill.fillAmount = 0;
    }
}