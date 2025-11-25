using UnityEngine;

public class Player_Input : MonoBehaviour
{
    [Header("Reference")]
    public GameObject playerInventoryUI;

    private bool isInventoryOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
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
