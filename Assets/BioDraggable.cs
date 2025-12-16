using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BioDraggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Surgery Stats")]
    public float pullForce = 25f; // How snappy the movement is
    public float dragLimit = 10f; // Max speed to prevent clipping

    [Header("Damage")]
    public float collisionRisk = 0.5f; // 50% chance to damage on hit

    [Header("Item Type")]
    public bool isReplacement = false; // False = Broken Trash. True = New Part.

    private Rigidbody2D rb;
    private RectTransform rectTransform;
    private Canvas myCanvas;
    private bool isDragging = false;
    private Vector2 targetPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rectTransform = GetComponent<RectTransform>();
        
        // Find the canvas automatically to help with coordinate conversion
        myCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        rb.linearVelocity = Vector2.zero; // Reset momentum
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        rb.linearVelocity = Vector2.zero; // Stop immediately (Zero G feel)
    }

    public void OnDrag(PointerEventData eventData)
    {
        // This function is required for the EventSystem to track dragging
        // We calculate the world position of the mouse on the Canvas plane
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)myCanvas.transform, 
            eventData.position, 
            myCanvas.worldCamera, 
            out Vector3 worldPoint
        );

        targetPosition = worldPoint;
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            // PHYSICS MOVEMENT:
            // Instead of teleporting (transform.position = mouse), we use Force.
            // This ensures collisions happen!
            
            Vector2 direction = (targetPosition - (Vector2)transform.position);
            
            // Move towards mouse
            rb.linearVelocity = direction * pullForce;
            
            // Clamp speed (safety)
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, dragLimit);
        }
        else
        {
            // Apply slight friction when not holding
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
        }
    }

    // --- COLLISION LOGIC ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("HealthyStrand"))
        {
            // Shake effect or Red Flash could go here!
            
            float roll = Random.value;
            if (roll < collisionRisk)
            {
                Debug.Log($"<color=red>SURGERY ERROR!</color> You bumped the wall! Damage dealt.");
                // Call your PlayerStats.TakeDamage(5) here!
            }
            else
            {
                Debug.Log("<color=yellow>Careful!</color> Near miss.");
            }
        }
    }
}