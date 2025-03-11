using UnityEngine;
using System.Collections;

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
    private float crouchCameraHeight = 0.85f;
    private float standingCameraHeight = 1.7f;
    private Transform cameraTransform;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController non trouvé sur " + gameObject.name);
            return;
        }

        cameraTransform = Camera.main.transform;
        if (cameraTransform == null)
        {
            Debug.LogError("Aucune caméra principale trouvée !");
            return;
        }

        originalHeight = characterController.height;
    }

    void Update()
    {
        CheckGrounded();
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        HandleMovement();
        HandleCrouch();
        HandleJump();
        AutoStandUp(); // Vérifie si on peut se relever automatiquement

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void CheckGrounded()
    {
        isGrounded = characterController.isGrounded ||
                     Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, characterController.height / 2 + 0.2f);
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        float currentSpeed = isCrouching ? crouchSpeed : moveSpeed;
        characterController.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isCrouching)
        {
            isCrouching = true;
            ChangePlayerHeight(crouchHeight, crouchCameraHeight);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && isCrouching && CanStandUp())
        {
            isCrouching = false;
            ChangePlayerHeight(originalHeight, standingCameraHeight);
        }
    }

    void HandleJump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void AutoStandUp()
    {
        if (isCrouching && CanStandUp() && !Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = false;
            ChangePlayerHeight(originalHeight, standingCameraHeight);
        }
    }

    void ChangePlayerHeight(float targetHeight, float targetCameraHeight)
    {
        StartCoroutine(SmoothCrouchTransition(targetHeight, targetCameraHeight));
    }

    IEnumerator SmoothCrouchTransition(float targetHeight, float targetCameraHeight)
    {
        float startHeight = characterController.height;
        float startCameraHeight = cameraTransform.localPosition.y;
        float time = 0f;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            characterController.height = Mathf.Lerp(startHeight, targetHeight, time / duration);
            characterController.center = new Vector3(0, characterController.height / 2, 0);
            cameraTransform.localPosition = new Vector3(0, Mathf.Lerp(startCameraHeight, targetCameraHeight, time / duration), 0);
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = new Vector3(0, targetHeight / 2, 0);
        cameraTransform.localPosition = new Vector3(0, targetCameraHeight, 0);
        characterController.Move(Vector3.down * 0.1f);
    }

    bool CanStandUp()
    {
        float checkHeight = originalHeight - crouchHeight;
        Vector3 rayOrigin = transform.position + Vector3.up * crouchHeight;
        float rayDistance = checkHeight;

        bool obstacleAbove = Physics.Raycast(rayOrigin, Vector3.up, rayDistance);
        return !obstacleAbove;
    }

    void OnDrawGizmos()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null) return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * (characterController.height / 2 + 0.1f));
    }
}
