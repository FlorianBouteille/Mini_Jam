using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        Vector3 velocity = move * speed;

        // keep current Y velocity
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // rotate player to face movement direction
        if (move.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
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
    }
}
