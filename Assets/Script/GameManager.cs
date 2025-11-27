using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Referensi Karakter")]
    public BossAI boss;
    public PlayerController player1;
    public Player2Controller player2; 

    [Header("Referensi UI Gameplay")]
    public TextMeshProUGUI timerText;

    [Header("Referensi UI Akhir Permainan")]
    public GameObject endGamePanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI p1StatsText;
    public TextMeshProUGUI p2StatsText;
    public TextMeshProUGUI bossStatsText;

    private float gameTime = 0f;
    private bool isGameOver = false;

    private int lastP1Damage = 0;
    private int lastP2Damage = 0;
    private int lastP1HP = 0;
    private int lastP2HP = 0;
    private int lastBossDamageDealt = 0;
    private float lastBossHP = 0;


    void Start()
    {
        endGamePanel.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        if (isGameOver) return;

        gameTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(gameTime / 60F);
        int seconds = Mathf.FloorToInt(gameTime % 60F);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (player1 != null)
        {
            lastP1Damage = player1.totalDamageDealt;
            lastP1HP = player1.health;
        }
        if (player2 != null)
        {
            // Pastikan Anda membaca dari script yang benar
            lastP2Damage = player2.totalDamageDealt;
            lastP2HP = player2.health;
        }
        if (boss != null)
        {
            lastBossDamageDealt = boss.totalDamageDealt; 
            lastBossHP = boss.health;
        }


        if (boss == null)
        {
            EndGame(true);
        }
        else if (player1 == null && player2 == null)
        {
            EndGame(false);
        }
    }

    void EndGame(bool playerWon)
    {
        isGameOver = true;
        Time.timeScale = 0;
        endGamePanel.SetActive(true);

        resultText.text = playerWon ? "VICTORY" : "DEFEAT";
        durationText.text = "Duration: " + timerText.text;

        // <<< PERBAIKAN LOGIKA BARU >>>
        if (playerWon)
        {
            // Jika pemain MENANG, berarti Boss-lah yang mati. Paksa HP-nya jadi 0.
            lastBossHP = 0;
        }
        else // (!playerWon)
        {
            // Jika pemain KALAH, berarti Player-lah yang mati. Paksa HP mereka jadi 0.
            lastP1HP = 0;
            lastP2HP = 0;
        }
        // <<< AKHIR PERBAIKAN >>>

        float p1DPS = (gameTime > 0) ? (lastP1Damage / gameTime) : 0;
        p1StatsText.text = string.Format("Player 1\nDamage: {0}\nDPS: {1:F2}\nSisa HP: {2}", lastP1Damage, p1DPS, lastP1HP);

        float p2DPS = (gameTime > 0) ? (lastP2Damage / gameTime) : 0;
        p2StatsText.text = string.Format("Player 2\nDamage: {0}\nDPS: {1:F2}\nSisa HP: {2}", lastP2Damage, p2DPS, lastP2HP);
        
        float bossDPS = (gameTime > 0) ? (lastBossDamageDealt / gameTime) : 0;
        bossStatsText.text = string.Format("The Sentinel\nDamage Dealt: {0}\nDPS: {1:F2}\nSisa HP: {2}", lastBossDamageDealt, bossDPS, Mathf.CeilToInt(lastBossHP));
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

