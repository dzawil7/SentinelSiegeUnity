using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Konfigurasi Scene")]
    [Tooltip("Nama scene gameplay harus persis sama dengan di Build Settings")]
    public string gameSceneName = "GameScene"; 

    [Header("Referensi Panel UI")]
    [SerializeField] private GameObject mainPanel;       // Panel Utama (Judul, Play, Tutorial, Exit)
    [SerializeField] private GameObject modeSelectPanel; // Panel Pilih AI (3 Tombol Mode)
    [SerializeField] private GameObject tutorialPanel;   // Panel Cara Bermain

    private void Start()
    {
        // Pastikan saat game mulai, hanya Main Panel yang aktif
        ShowMainPanel();
    }

    // =================================================================
    // NAVIGASI PANEL (Dipasang di Button OnClick)
    // =================================================================

    public void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        modeSelectPanel.SetActive(false);
        tutorialPanel.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        // Sembunyikan Main, Tampilkan Mode Select
        mainPanel.SetActive(false);
        modeSelectPanel.SetActive(true);
    }

    public void OnTutorialButtonPressed()
    {
        // Sembunyikan Main, Tampilkan Tutorial
        mainPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    public void OnBackButtonPressed()
    {
        // Kembali ke menu utama dari panel manapun
        ShowMainPanel();
    }

    // =================================================================
    // LOGIKA SELEKSI MODE & START GAME
    // =================================================================

    // Fungsi ini dipanggil oleh tombol-tombol di ModeSelectPanel
    // 0 = FSM Conventional
    // 1 = FuSM Set A
    // 2 = FuSM Set B (Stamina)
    public void SelectModeAndStart(int modeIndex)
    {
        switch (modeIndex)
        {
            case 0:
                GameSettings.SelectedMode = AIMode.FSM_Conventional;
                Debug.Log("Mode Selected: FSM Conventional");
                break;
            case 1:
                GameSettings.SelectedMode = AIMode.FuSM_SetA;
                Debug.Log("Mode Selected: FuSM Set A");
                break;
            case 2:
                GameSettings.SelectedMode = AIMode.FuSM_SetB;
                Debug.Log("Mode Selected: FuSM Set B (Stamina)");
                break;
        }

        // Simpan preferensi untuk sesi berikutnya (opsional)
        PlayerPrefs.SetInt("LastSelectedMode", modeIndex);
        PlayerPrefs.Save();

        // Load Gameplay
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnExitButtonPressed()
    {
        Debug.Log("Quit Game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}