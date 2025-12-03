using UnityEngine;

public class Player_Input : MonoBehaviour
{
    [Header("Reference")]
    public GameObject playerInventoryUI;
    private Player_Attack playerAttackScript;
    private Player_Movement playerMovementScript;

    private bool isInventoryOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAttackScript = GetComponent<Player_Attack>();
        playerMovementScript = GetComponent<Player_Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        GetWalkingInput();
        GetJumpingInput();
        GetDashingInput();
        GetAttackInput();
        GetToggleInventoryInput();
    }

    private void GetDashingInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            playerMovementScript.TryDashingInput();
        }
    }

    private void GetJumpingInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            playerMovementScript.TryJumpingInput(); 
        }
    }

    private void GetWalkingInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        playerMovementScript.TryWalkingInput(horizontalInput);
    }

    private void GetAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
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
