using UnityEngine;
using System.Collections;

public class ObjectInteraction : MonoBehaviour
{
    public Transform player;
    public float raycastRange = 10f;
    public float minimumDistance = 2f;
    public float teleportOffset = 1.5f; // Distance d’arrivée autour de l’objet Jump

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isStableCrouching;
    private bool hasJumped;
    private bool hasStableJumped;

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
                    TeleportToTarget(hit.point);
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
                    if (!hasStableJumped)
                    {
                        StableJump();
                    }
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
            hasStableJumped = false;
        }
    }

    private void ApplyGravity()
    {
        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded) velocity.y = -2f;
            velocity.y += -9.81f * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    private void TeleportToTarget(Vector3 targetPoint)
    {
        Vector3 direction = (targetPoint - player.position).normalized;
        Vector3 teleportPosition = targetPoint - (direction * teleportOffset); // Place le joueur juste à côté

        characterController.enabled = false; // Désactive temporairement le CharacterController pour éviter les collisions
        player.position = teleportPosition;
        characterController.enabled = true; // Réactive le CharacterController
    }

    private void StableJump()
    {
        float stableJumpForce = 8f; // Augmente la hauteur du saut
        velocity.y = Mathf.Sqrt(stableJumpForce * -2f * -9.81f);
        characterController.Move(velocity * Time.deltaTime);
        hasStableJumped = true;
    }

    private void MoveBackward()
    {
        characterController.Move(-Camera.main.transform.forward * 5f * Time.deltaTime);
    }

    private void MoveForward()
    {
        characterController.Move(Camera.main.transform.forward * 5f * Time.deltaTime);
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
