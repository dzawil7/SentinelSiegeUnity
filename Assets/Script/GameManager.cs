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

    [Header("Referensi UI Akhir Permainan (Game Over)")]
    public GameObject endGamePanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI p1StatsText;
    public TextMeshProUGUI p2StatsText;
    public TextMeshProUGUI bossStatsText;

    [Header("Referensi UI Pause Menu")]
    public GameObject pausePanel;
    public TextMeshProUGUI pauseTitleText; // Tulisan "GAME PAUSED"
    public TextMeshProUGUI pauseDurationText;
    public TextMeshProUGUI pauseP1Stats;
    public TextMeshProUGUI pauseP2Stats;
    public TextMeshProUGUI pauseBossStats;

    private float gameTime = 0f;
    private bool isGameOver = false;
    private bool isPaused = false;

    // Variabel memori terakhir
    private int lastP1Damage = 0; private int lastP2Damage = 0;
    private int lastP1HP = 0; private int lastP2HP = 0;
    private int lastBossDamageDealt = 0; private float lastBossHP = 0;

    void Start()
    {
        endGamePanel.SetActive(false);
        if(pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        // Input Pause (Tombol ESC)
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }

        if (isGameOver || isPaused) return;

        // Update Timer
        gameTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(gameTime / 60F);
        int seconds = Mathf.FloorToInt(gameTime % 60F);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Rekam data Real-time
        if (player1 != null) { lastP1Damage = player1.totalDamageDealt; lastP1HP = player1.health; }
        if (player2 != null) { lastP2Damage = player2.totalDamageDealt; lastP2HP = player2.health; }
        if (boss != null) { lastBossDamageDealt = boss.totalDamageDealt; lastBossHP = boss.health; }

        // Cek Game Over
        if (boss == null) EndGame(true);
        else if (player1 == null && player2 == null) EndGame(false);
    }

    // =================================================================
    // LOGIKA PAUSE
    // =================================================================
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0; // Bekukan Waktu
            ShowPauseStats();   // Tampilkan Stats saat ini
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1; // Lanjutkan Waktu
            pausePanel.SetActive(false);
        }
    }

    void ShowPauseStats()
    {
        pauseTitleText.text = "GAME PAUSED";
        pauseDurationText.text = "Duration: " + timerText.text;

        // Hitung Statistik Sementara (Sama seperti EndGame tapi ambil data live)
        float currentP1HP = player1 != null ? player1.health : 0;
        float currentP2HP = player2 != null ? player2.health : 0;
        float currentBossHP = boss != null ? boss.health : 0;

        float p1DPS = (gameTime > 0) ? (lastP1Damage / gameTime) : 0;
        pauseP1Stats.text = string.Format("Player 1\nDamage: {0}\nDPS: {1:F2}\nHP: {2}", lastP1Damage, p1DPS, currentP1HP);

        float p2DPS = (gameTime > 0) ? (lastP2Damage / gameTime) : 0;
        pauseP2Stats.text = string.Format("Player 2\nDamage: {0}\nDPS: {1:F2}\nHP: {2}", lastP2Damage, p2DPS, currentP2HP);

        float bossDPS = (gameTime > 0) ? (lastBossDamageDealt / gameTime) : 0;
        pauseBossStats.text = string.Format("The Sentinel\nDamage Dealt: {0}\nDPS: {1:F2}\nHP: {2}", lastBossDamageDealt, bossDPS, Mathf.CeilToInt(currentBossHP));
    }

    // =================================================================
    // LOGIKA GAME OVER
    // =================================================================
    void EndGame(bool playerWon)
    {
        isGameOver = true;
        Time.timeScale = 0;
        endGamePanel.SetActive(true);
        // Pastikan panel pause mati kalau tiba-tiba game over
        if(pausePanel != null) pausePanel.SetActive(false); 

        resultText.text = playerWon ? "VICTORY" : "DEFEAT";
        durationText.text = "Duration: " + timerText.text;

        float displayP1HP = playerWon ? lastP1HP : 0; 
        float displayP2HP = playerWon ? lastP2HP : 0;
        float displayBossHP = playerWon ? 0 : lastBossHP; 

        float p1DPS = (gameTime > 0) ? (lastP1Damage / gameTime) : 0;
        p1StatsText.text = string.Format("Player 1\nDamage: {0}\nDPS: {1:F2}\nSisa HP: {2}", lastP1Damage, p1DPS, Mathf.CeilToInt(displayP1HP));

        float p2DPS = (gameTime > 0) ? (lastP2Damage / gameTime) : 0;
        p2StatsText.text = string.Format("Player 2\nDamage: {0}\nDPS: {1:F2}\nSisa HP: {2}", lastP2Damage, p2DPS, Mathf.CeilToInt(displayP2HP));
        
        float bossDPS = (gameTime > 0) ? (lastBossDamageDealt / gameTime) : 0;
        bossStatsText.text = string.Format("The Sentinel\nDamage Dealt: {0}\nDPS: {1:F2}\nSisa HP: {2}", lastBossDamageDealt, bossDPS, Mathf.CeilToInt(displayBossHP));
    }

    // --- FUNGSI TOMBOL ---

    public void ResumeGame()
    {
        // Fungsi khusus untuk tombol "Resume"
        TogglePause(); 
    }

    public void RestartGame()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("MainMenu"); 
    }
}