using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //running movement
    [Header("Run Settings")]
    [SerializeField] private float moveSpeed = 5f;

    //charge jump settings
    [Header("Jump Charge Settings")]
    [SerializeField] private float maxChargeTime = 0.8f;         
    [SerializeField] private float minJumpVertical = 6f; 
    [SerializeField] private float maxJumpVertical = 18f;         
    [SerializeField] private float minJumpHorizontal = 0f;       
    [SerializeField] private float maxJumpHorizontal = 8f;       

    //ground check settings (tag comparison)
    [Header("Ground Check (by Tag)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private string groundTag = "GroundTag";

    //sprite flipping
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    //references
    [Header("Gameplay References")]
    [SerializeField] private GameplayMenu gameplayMenu;
    [SerializeField] private GameMechanics gameMechanics;

    //end trigger behavior
    [Header("End Trigger Settings")]
    [SerializeField] private float endDelaySeconds = 3f; 

    //state
    [Header("State")]
    [SerializeField] private bool gameComplete = false; 

    private Rigidbody2D rb;
    private bool isGrounded;

    //charge state
    private bool isCharging;
    private float chargeTimer;
    //-1 left, 0 up, +1 right
    private int aimDirection = 0; 

    private bool endSequenceRunning;

    private void Start()
    {
        //check if rigidbody2d is null and get component
        rb = GetComponent<Rigidbody2D>();
        //check if sprite renderer is null and get component in children
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        //check if rigidbody2d is null and log error message
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the player object");
            return;
        }

        //update grounded state
        updateGrounded();

        //horizontal input for running
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        //if grounded and not charging, allow running
        if (isGrounded && !isCharging)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        //begin charge only while grounded
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            chargeTimer = 0f;
            aimDirection = getAimDirection(horizontalInput);
        }

        //holding charge while still on ground
        if (isCharging && isGrounded && Input.GetKey(KeyCode.Space))
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer > maxChargeTime) chargeTimer = maxChargeTime;

            //update aim direction based on current horizontal input
            int dir = getAimDirection(horizontalInput);
            if (dir != 0) 
                aimDirection = dir;

            // Lock horizontal movement during charge
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // Flip to match aim
            if (spriteRenderer != null && aimDirection != 0)
                spriteRenderer.flipX = aimDirection < 0;
        }

        //release to jump
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            float t = (maxChargeTime <= 0f) ? 1f : Mathf.Clamp01(chargeTimer / maxChargeTime);
            float vy = Mathf.Lerp(minJumpVertical, maxJumpVertical, t);
            float vx = Mathf.Lerp(minJumpHorizontal, maxJumpHorizontal, t) * aimDirection;
            rb.linearVelocity = new Vector2(vx, vy);
        }

        //airborne sprite facing locked to velocity sign
        if (!isGrounded && spriteRenderer != null)
        {
            if (rb.linearVelocity.x > 0.01f) spriteRenderer.flipX = false;
            else if (rb.linearVelocity.x < -0.01f) spriteRenderer.flipX = true;
        }

        //if grounded and not charging, face run direction
        if (isGrounded && !isCharging && spriteRenderer != null && Mathf.Abs(horizontalInput) > 0.01f)
        {
            spriteRenderer.flipX = horizontalInput < 0f;
        }
    }

    private int getAimDirection(float horizontalInput)
    {
        //right
        if (horizontalInput > 0.1f) 
            return 1;
        //left
        if (horizontalInput < -0.1f) 
            return -1;
        // straight up
        return 0;
    }

    private void updateGrounded()
    {
        isGrounded = false;
        if (groundCheck == null)
            return;
        //check for overlaps with ground tagged objects
        var colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        //loop through colliders to see if any match ground tag
        foreach (var c in colliders)
        {
            //ignore self and triggers
            if (c == null)
                continue;
            //if the collider is part of the player, skip it
            if (c.transform == transform || c.transform.IsChildOf(transform)) 
                continue;
            //if it's a trigger, skip it
            if (c.isTrigger) 
                continue;
            //check tag and set grounded to true and break
            if (c.CompareTag(groundTag))
            { 
                isGrounded = true;
                break; 
            }
        }
    }
    //unit built in function called when another collider enters a trigger collider attached to this object
    private void OnTriggerEnter2D(Collider2D other)
    {
        //check if not already running end sequence and collided with "End" tagged object
        //in addition other is not null
        if (!endSequenceRunning && other != null && other.CompareTag("End"))
        {
            //start end sequence coroutine
            StartCoroutine(EndSequenceCoroutine());
        }
    }
    //coroutine that runs the end of game sequence with a 3 sec delay.
    private System.Collections.IEnumerator EndSequenceCoroutine()
    {
        //set endSequenceRunning flag to true
        endSequenceRunning = true;
        //wait for the number of seconds specified by endDelaySeconds
        yield return new WaitForSeconds(endDelaySeconds);
        //set gameComplete to true and stop timer if exists
        gameComplete = true;
        stopTimerIfExists();
        //show gameplay menu panel
        GameplayMenu menu = gameplayMenu;
        //find it in scene if not assigned
        if (menu == null)
        {
#if UNITY_2023_1_OR_NEWER
            menu = Object.FindFirstObjectByType<GameplayMenu>(FindObjectsInactive.Include);
#else
            menu = Object.FindObjectOfType<GameplayMenu>(true);
#endif
        }
        //if menu is not null
        if (menu != null)
        {
            //show menu panel
            menu.showMenuPanel();
        }
        else
        {
            //otherwise log warning
            Debug.LogWarning("GameplayMenu not found in scene to show menu panel");
        }
        //set endSequenceRunning flag to false
        endSequenceRunning = false;
    }

    //a function to stop the timer in GameMechanics if it exists in the scene
    private void stopTimerIfExists()
    {
        //start with the GameMechanics reference assigned in the Inspector.
        GameMechanics gm = gameMechanics;
        //if not assigned, try to find it in the scene
        if (gm == null)
        {
#if UNITY_2023_1_OR_NEWER
            gm = Object.FindFirstObjectByType<GameMechanics>(FindObjectsInactive.Include);
#else
            gm = Object.FindObjectOfType<GameMechanics>(true);
#endif
        }
        //if found, stop the timer
        if (gm != null)
            gm.stopTimer();
    }

    //unity builtin function to draw gizmos in the editor when the object is selected
    private void OnDrawGizmosSelected()
    {
        //if groundcheck is null
        if (groundCheck != null)
        {
            //draw a red wire sphere at the ground check position with the specified radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
