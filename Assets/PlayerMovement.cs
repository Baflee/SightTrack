using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;

    public float crouchHeight = 1f;
    private float originalHeight;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        originalHeight = characterController.height;
    }

    void Update()
    {
        // Vérifier si le joueur est au sol
        CheckGrounded();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Petite force pour rester au sol
        }

        HandleMovement();
        HandleCrouch();
        HandleJump();

        // Appliquer la gravité
        velocity.y += gravity * Time.deltaTime;

        // Appliquer le mouvement vertical
        characterController.Move(velocity * Time.deltaTime);
    }

    void CheckGrounded()
    {
        // Utiliser isGrounded du CharacterController et un Raycast comme fallback
        isGrounded = characterController.isGrounded ||
                     Physics.Raycast(transform.position, Vector3.down, characterController.height / 2 + 0.1f);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        characterController.Move(move * currentSpeed * Time.deltaTime);
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
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void ChangePlayerHeight(float height)
    {
        characterController.height = height;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (characterController.height / 2 + 0.1f));
    }
}