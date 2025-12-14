using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnNeuralNetworkClick()
    {
        SceneManager.LoadScene("TestNeuralNetwork");
    }
    public void OnStateMachineClick()
    {
        SceneManager.LoadScene("TestStateMachine");
    }

    public void OnComputationalBiologyClick()
    {
        Debug.Log("Computational Bio button clicked!");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
