using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Participant ID Setup")]
    [SerializeField] private TMP_InputField participantIDInput;
    [SerializeField] private Button enterButton;

    [Header("Game Mode Buttons")]
    [SerializeField] private Button neuralNetworkButton;
    [SerializeField] private Button stateMachineButton;
    [SerializeField] private Button computationalBiologyButton;
    [SerializeField] private Button exitButton;

    [Header("Optional Error Display")]
    [SerializeField] private Text errorText;

    private bool idIsSet = false;

    void Start()
    {
        // Disable game mode buttons until ID is entered
        DisableGameButtons();

        // Hide error text if it exists
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        // Set default ID for easy testing (optional - you can remove this)
        if (participantIDInput != null)
            participantIDInput.text = "P001";

        // Add listener to Enter button
        if (enterButton != null)
            enterButton.onClick.AddListener(OnEnterButtonClicked);
    }

    void Update()
    {
        // Allow pressing Enter key to submit ID
        if (Input.GetKeyDown(KeyCode.Return) && !idIsSet)
        {
            OnEnterButtonClicked();
        }
    }

    // NEW: Handle Enter button click to set Participant ID
    void OnEnterButtonClicked()
    {
        if (participantIDInput == null) return;

        string inputID = participantIDInput.text.Trim();

        // Validate input
        if (string.IsNullOrEmpty(inputID))
        {
            ShowError("Please enter a Participant ID!");
            return;
        }

        if (inputID.Length < 2)
        {
            ShowError("ID must be at least 2 characters!");
            return;
        }

        // Set the participant ID in DataLogger
        if (DataLogger.Instance != null)
        {
            DataLogger.Instance.participantID = inputID;
            idIsSet = true;

            Debug.Log($"✅ Participant ID set to: {inputID}");
            ShowError($"✅ ID Set: {inputID} - Select game mode", Color.green);

            // Lock the input and enter button
            participantIDInput.interactable = false;
            enterButton.interactable = false;

            // Enable game mode buttons
            EnableGameButtons();
        }
        else
        {
            ShowError("ERROR: DataLogger not found!");
            Debug.LogError("DataLogger.Instance is null! Make sure DataLogger exists in scene.");
        }
    }

    // Your existing methods - now check if ID is set
    public void OnNeuralNetworkClick()
    {
        if (!CheckIDSet()) return;

        Debug.Log($"Loading Neural Network with ID: {DataLogger.Instance.participantID}");
        SceneManager.LoadScene("TestNeuralNetwork");
    }

    public void OnStateMachineClick()
    {
        if (!CheckIDSet()) return;

        Debug.Log($"Loading State Machine with ID: {DataLogger.Instance.participantID}");
        SceneManager.LoadScene("TestStateMachine");
    }

    public void OnComputationalBiologyClick()
    {
        if (!CheckIDSet()) return;

        Debug.Log($"Computational Bio button clicked! ID: {DataLogger.Instance.participantID}");
    }

    public void OnExitClick()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Helper method to check if ID is set before loading scenes
    bool CheckIDSet()
    {
        if (!idIsSet)
        {
            ShowError("Please enter Participant ID first!");
            return false;
        }
        return true;
    }

    void DisableGameButtons()
    {
        if (neuralNetworkButton != null)
            neuralNetworkButton.interactable = false;

        if (stateMachineButton != null)
            stateMachineButton.interactable = false;

        if (computationalBiologyButton != null)
            computationalBiologyButton.interactable = false;
    }

    void EnableGameButtons()
    {
        if (neuralNetworkButton != null)
            neuralNetworkButton.interactable = true;

        if (stateMachineButton != null)
            stateMachineButton.interactable = true;

        if (computationalBiologyButton != null)
            computationalBiologyButton.interactable = true;
    }

    void ShowError(string message, Color? color = null)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = color ?? Color.red;
            errorText.gameObject.SetActive(true);
        }

        Debug.Log($"[MainMenu] {message}");
    }
}