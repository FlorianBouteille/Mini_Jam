using NUnit.Framework;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private bool isMoving;
    private bool hasApp;
    private bool isCharging;
    private string currentAnimName = "Idle";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Get Animator from this object or children
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        if (animator == null)
            Debug.LogWarning("PlayerControls: Animator not found on player or children!");
        
        // Listen for app changes
        UIManager.OnActiveAppChanged += OnAppChanged;
    }

    void OnDestroy()
    {
        UIManager.OnActiveAppChanged -= OnAppChanged;
    }

    void OnAppChanged(int appIndex)
    {
        hasApp = appIndex >= 0;
    }

    void Update()
    {
        // Movement WASD
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // movement relative to camera for third-person
        Vector3 camForward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = Camera.main != null ? Camera.main.transform.right : Vector3.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 move = (camForward * z + camRight * x).normalized;
        isMoving = move.magnitude > 0.1f;
        Vector3 velocity = move * speed;

        // keep current Y velocity
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // rotate player to face movement direction
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Update animation state with CrossFade (only when animation changes)
        if (animator != null)
        {
            string nextAnimName = GetCurrentAnimationName();
            if (nextAnimName != currentAnimName)
            {
                animator.CrossFade(nextAnimName, 0.1f);
                currentAnimName = nextAnimName;
            }
        }
        else
        {
            Debug.LogWarning("PlayerControls: Animator not found!");
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // simple check sol (basique mais ok pour jam)
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Charger"))
        {
            isCharging = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Charger"))
        {
            isCharging = false;
        }
    }

    // Public property so UIManager can check charging status
    public bool IsCharging => isCharging;

    string GetCurrentAnimationName()
    {
        // Determine animation based on state
        string suffix = hasApp ? "_phone" : "";
        
        if (!isGrounded)
        {
            return "Jump" + suffix;
        }
        else if (isMoving)
        {
            return "Walk" + suffix;
        }
        else
        {
            return "Idle" + suffix;
        }
    }
}
