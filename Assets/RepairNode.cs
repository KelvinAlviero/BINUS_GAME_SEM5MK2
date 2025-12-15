using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RepairNode : MonoBehaviour
{
    [Header("Settings")]
    public float timeToFailure = 30f;
    
    // Internal Data (Assigned by DNAStructure when spawned)
    private DNABaseType affectedBase;
    private Image connectorImage;
    private Sprite fixedSprite;
    private Sprite deadSprite; // The "Greyed Out" sprite
    private Player_Stats statsScript;
    
    private bool isActive = false;
    private Button myButton;

    void Awake()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnRepairClicked);
        statsScript = FindObjectOfType<Player_Stats>();
    }

    public void Initialize(DNABaseType _base, Image _connector, Sprite _fixed, Sprite _dead)
    {
        affectedBase = _base;
        connectorImage = _connector;
        fixedSprite = _fixed;
        deadSprite = _dead;

        // Start timer
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
            // Optional: Make the button flash faster as time runs out?
            yield return null;
        }

        if (isActive)
        {
            FailRepair();
        }
    }

    void OnRepairClicked()
    {
        if (!isActive) return;

        // SUCCESS: Repair the strand
        isActive = false;
        
        // 1. Visually fix the connector
        if (connectorImage != null && fixedSprite != null)
        {
            connectorImage.sprite = fixedSprite;
            connectorImage.color = Color.white;
        }

        Debug.Log($"<color=green>SAVED!</color> {affectedBase} strand repaired in time.");
        
        // Destroy this button (job done)
        Destroy(gameObject);
    }

    void FailRepair()
    {
        isActive = false;

        // FAILURE: Apply Penalty
        Debug.Log($"<color=red>FAILURE!</color> {affectedBase} connection died!");

        // 1. Turn the connector Grey/Dead
        if (connectorImage != null && deadSprite != null)
        {
            connectorImage.sprite = deadSprite; // The "Grey" sprite
            connectorImage.color = Color.gray;  // Tint it grey to be sure
        }

        // 2. Apply Permanent Stat Debuff
        if (statsScript != null)
        {
            statsScript.ApplyGeneticDebuff(affectedBase);
        }

        // Hide button (too late to repair now)
        Destroy(gameObject);
    }
}