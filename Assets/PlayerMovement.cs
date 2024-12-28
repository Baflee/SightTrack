using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isCrouching;

    private CapsuleCollider playerCollider;
    private float originalHeight;
    public float crouchHeight = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        originalHeight = playerCollider.height;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // Q/D or A/D
        float moveZ = Input.GetAxis("Vertical");   // Z/S or W/S

        // Adjust movement speed if crouching
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 movement = (transform.right * moveX + transform.forward * moveZ).normalized * currentSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            ChangePlayerHeight(crouchHeight);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            ChangePlayerHeight(originalHeight);
        }
    }

    void HandleJump()
    {
        if (isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void ChangePlayerHeight(float height)
    {
        // Adjust collider height
        float heightDifference = playerCollider.height - height;
        playerCollider.height = height;

        // Adjust position to keep feet on the ground
        Vector3 newPosition = transform.position;
        newPosition.y -= heightDifference / 2; // Adjust only half of the height difference
        transform.position = newPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0 && collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.contacts.Length > 0 && collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            isGrounded = false;
        }
    }
}
