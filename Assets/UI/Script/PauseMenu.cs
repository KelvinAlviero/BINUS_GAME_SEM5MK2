using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private bool isPaused = false;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void PauseGame()
    {

        Time.timeScale = 0f; // Freeze the game
        isPaused = true;
    }

    public void ResumeGame()
    {

        Time.timeScale = 1f; // Resume normal time
        isPaused = false;
    }

    public void RestartButton()
    {
        Time.timeScale = 1f; // Reset time scale before loading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1f; // Reset time scale before loading
        SceneManager.LoadScene("MainMenu");
    }

}