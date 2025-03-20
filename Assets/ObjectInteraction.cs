using UnityEngine;
using System.Collections;

public class ObjectInteraction : MonoBehaviour
{
    public Transform player;
    public float raycastRange = 10f;
    public float minimumDistance = 2f;
    public float jumpForce = 5f;
    public float forwardForce = 10f;
    public float moveSpeed = 5f;
    public float crouchSpeed = 2f;
    public float gravity = -9.81f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isStableCrouching;
    private bool hasJumped;

    public float jumpCooldown = 0.1f;
    private float lastJumpTime = -1f;

    private void Start()
    {
        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("Le joueur n'a pas de CharacterController assigné !");
            }
        }
        else
        {
            Debug.LogError("Aucun joueur assigné. Assignez le joueur manuellement dans l'inspecteur.");
        }
    }

    private void Update()
    {
        CheckObject();
        ApplyGravity();
        if (player != null)
        {
            player.rotation = Quaternion.Euler(0, player.rotation.eulerAngles.y, 0);
        }
    }

    private void CheckObject()
    {
        if (player == null) return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, raycastRange))
        {
            float distanceToObject = Vector3.Distance(player.position, hit.point);
            if (distanceToObject < minimumDistance) return;

            string objectTag = hit.collider.tag;
            switch (objectTag)
            {
                case "Jump":
                    StopGravity();
                    JumpTowards(hit.point);
                    break;
                case "Backward":
                    MoveBackward();
                    break;
                case "Forward":
                    MoveForward();
                    break;
                case "Crouch":
                    MoveForward();
                    Crouch();
                    break;
                case "StableCrouch":
                    StableCrouch();
                    break;
                case "StableJump":
                    StableJump();
                    break;
            }
        }
        else
        {
            if (isCrouching && CanStandUp())
            {
                StandUp();
            }
            if (isStableCrouching && CanStandUp())
            {
                StandUp();
                isStableCrouching = false;
            }
            hasJumped = false;
            velocity.x = 0f;
            velocity.z = 0f;
        }
    }

    private void ApplyGravity()
    {
        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    private void StopGravity()
    {
        velocity.y = 0f;
    }

    private void JumpTowards(Vector3 targetPoint)
    {
        velocity = Vector3.zero;
        Vector3 jumpDirection = (targetPoint - player.position).normalized;
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        velocity.x = jumpDirection.x * forwardForce;
        velocity.z = jumpDirection.z * forwardForce;
        characterController.Move(velocity * Time.deltaTime);
        
        StartCoroutine(ResetVelocityAfterJump());
    }
    
    private IEnumerator ResetVelocityAfterJump()
    {
        yield return new WaitForSeconds(0.5f);
        velocity.x = 0f;
        velocity.z = 0f;
    }

    private void StableJump()
    {
        if (!hasJumped && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasJumped = true;
        }
    }

    private void MoveBackward()
    {
        characterController.Move(-Camera.main.transform.forward * moveSpeed * Time.deltaTime);
    }

    private void MoveForward()
    {
        characterController.Move(Camera.main.transform.forward * moveSpeed * Time.deltaTime);
    }

    private void Crouch()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            characterController.height /= 2;
        }
    }

    private void StableCrouch()
    {
        if (!isStableCrouching)
        {
            isStableCrouching = true;
            characterController.height /= 2;
        }
    }

    private void StandUp()
    {
        isCrouching = false;
        isStableCrouching = false;
        characterController.height *= 2;
    }

    private bool CanStandUp()
    {
        float checkHeight = characterController.height * 2 - characterController.height;
        Vector3 rayOrigin = player.position + Vector3.up * characterController.height;
        float rayDistance = checkHeight;

        bool obstacleAbove = Physics.Raycast(rayOrigin, Vector3.up, rayDistance);
        return !obstacleAbove;
    }
}
