using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RepairNode : MonoBehaviour
{
    [Header("Settings")]
    public float timeToFailure = 30f;
    
    private DNABaseType affectedBase;
    
    private Image connectorImage; // The ladder rung (to fix if we succeed)
    private Image baseImage;      // The A/T/G/C icon (to kill if we fail)
    
    private Sprite fixedConnectorSprite;
    private Sprite deadBaseSprite;
    
    private Player_Stats statsScript;
    private bool isActive = false;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnRepairClicked);
        statsScript = FindObjectOfType<Player_Stats>();
    }

    // UPDATED INITIALIZE FUNCTION
    public void Initialize(DNABaseType _base, Image _connector, Image _baseTarget, Sprite _fixed, Sprite _dead)
    {
        affectedBase = _base;
        connectorImage = _connector;
        baseImage = _baseTarget;         // Save the Base Image
        fixedConnectorSprite = _fixed;
        deadBaseSprite = _dead;          // Save the Grey Base Sprite

        isActive = true;
        gameObject.SetActive(true);
        StartCoroutine(CountdownToFailure());
    }

    IEnumerator CountdownToFailure()
    {
        float timer = timeToFailure;
        while (timer > 0 && isActive)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        if (isActive) FailRepair();
    }     
    void OnRepairClicked()
    {
        if (!isActive) return;

        // INSTEAD of fixing it here, we launch the surgery!
        SurgeryManager surgery = FindObjectOfType<SurgeryManager>();
        
        if (surgery != null)
        {
            // Pass the data so the surgery knows what we are fixing
            // (You might want to instantiate specific prefabs based on the base type A/T/G/C later)
            surgery.microscopePanel.SetActive(true);
            surgery.currentPhase = SurgeryManager.SurgeryPhase.Extraction;
            // You'll need to spawn a fresh "Broken Piece" here in the surgery script
        }
        
        // Pause the timer on this node while surgery happens? 
        // Or let the panic continue? (Letting it continue is more horror!)
    }

    void FailRepair()
    {
        isActive = false;

        Debug.Log($"<color=red>FAILURE!</color> {affectedBase} died!");

        // FAILURE: Target the BASE IMAGE now
        if (baseImage != null && deadBaseSprite != null)
        {
            baseImage.sprite = deadBaseSprite; // Turn the Hexagon Grey
            baseImage.color = Color.gray;      // Tint it to look dead
        }
        
        // Note: We usually leave the connector broken (snapped) visually to show the damage is permanent.

        if (statsScript != null)
        {
            statsScript.ApplyGeneticDebuff(affectedBase);
        }

        Destroy(gameObject);
    }
}