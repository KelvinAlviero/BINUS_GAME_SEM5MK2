using UnityEngine;

public class Player_Input : MonoBehaviour
{
    [Header("Reference")]
    public GameObject playerInventoryUI;
    [SerializeField] private GameObject playerPauseMenuUI;
    private Player_Attack playerAttackScript;
    private Player_Movement playerMovementScript;
    private Player_Stats playerStatsScript;
    private PauseMenu pauseMenuScript;

    private bool isInventoryOpen = false;
    private bool isPauseMenuOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseMenuScript = playerPauseMenuUI.GetComponent<PauseMenu>();
        playerAttackScript = GetComponent<Player_Attack>();
        playerMovementScript = GetComponent<Player_Movement>();
        playerStatsScript = GetComponent<Player_Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        GetPauseMenuInput();
        if (!isPauseMenuOpen) 
        {
            GetBlockingInput();
            GetWalkingInput();
            GetJumpingInput();
            GetDashingInput();
            GetAttackInput();
            GetToggleInventoryInput();
        }
       
    }

    private void GetBlockingInput()
    {
        if (Input.GetMouseButton(1))
        {
            Debug.Log("Blocking");
            playerStatsScript.isBlocking = true;
        }
        else
        {
            playerStatsScript.isBlocking = false;
        }
    }

    private void GetDashingInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !playerStatsScript.isBlocking)
        {
            Debug.Log("Dashing");
            playerMovementScript.TryDashingInput();
        }
    }

    private void GetJumpingInput()
    {
        if (Input.GetButtonDown("Jump") && !playerStatsScript.isBlocking)
        {
            playerMovementScript.TryJumpingInput();
        }
    }

    private void GetWalkingInput()
    {
        float horizontalInput = playerStatsScript.isBlocking ? 0 : Input.GetAxisRaw("Horizontal");
        playerMovementScript.TryWalkingInput(horizontalInput);
    }

    private void GetPauseMenuInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void GetAttackInput()
    {
        if (Input.GetMouseButtonDown(0) && !playerStatsScript.isBlocking)
        {
            playerAttackScript.TryAttack();
        }
    }

    // UI Section
    private void GetToggleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void TogglePause()
    {
        isPauseMenuOpen = !isPauseMenuOpen;
        if (isPauseMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerPauseMenuUI.SetActive(true);
            pauseMenuScript.PauseGame();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerPauseMenuUI.SetActive(false);
            pauseMenuScript.ResumeGame();
        }
    }


    private void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (isInventoryOpen)
        {
            OpenInventory();
        }
        else
        {
            CloseInventory();
        }
    }

    private void CloseInventory()
    {
        playerInventoryUI.SetActive(false);
    }

    private void OpenInventory()
    {
        playerInventoryUI.SetActive(true);
    }
}
