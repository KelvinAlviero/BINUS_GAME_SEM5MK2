using UnityEngine;
using UnityEngine.SceneManagement;

public class YourDeadMenu : MonoBehaviour
{
    GameObject deathMenu;

    public void RestartButton()
    {
        

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        deathMenu.SetActive(false);
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1f; // Reset time scale before loading
        SceneManager.LoadScene("MainMenu");
        deathMenu.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        deathMenu = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
