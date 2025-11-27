using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Nama scene gameplay Anda (pastikan sama dengan di Build Settings)
    public string gameSceneName = "GameScene"; 

    public void SelectFSM()
    {
        GameSettings.SelectedMode = AIMode.FSM_Conventional;
        StartGame();
    }

    public void SelectFuSM_SetA()
    {
        GameSettings.SelectedMode = AIMode.FuSM_SetA;
        StartGame();
    }

    public void SelectFuSM_SetB()
    {
        GameSettings.SelectedMode = AIMode.FuSM_SetB;
        StartGame();
    }

    private void StartGame()
    {
        Debug.Log("Mode AI Terpilih: " + GameSettings.SelectedMode);
        SceneManager.LoadScene(gameSceneName);
    }
}