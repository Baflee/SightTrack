using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectInteraction : MonoBehaviour
{
    public Transform player;
    public float raycastRange = 10f;
    public float minimumDistance = 2f;
    public float teleportOffset = 1.5f; // Distance d’arrivée autour de l’objet Jump
    public int rewindFrames = 1000; // Nombre de frames à remonter

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isStableCrouching;
    private bool hasJumped;
    private bool hasStableJumped;
    private bool isRewinding;

    private List<Vector3> positionHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

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
        if (!isRewinding)
        {
            RecordHistory();
        }

        CheckObject();

        if (!isRewinding)
        {
            ApplyGravity();
        }

        if (player != null)
        {
            player.rotation = Quaternion.Euler(0, player.rotation.eulerAngles.y, 0);
        }
    }

    private void CheckObject()
    {
        if (player == null || isRewinding) return;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, raycastRange))
        {
            float distanceToObject = Vector3.Distance(player.position, hit.point);
            if (distanceToObject < minimumDistance) return;

            string objectTag = hit.collider.tag;
            if (!isRewinding) // Empêche toute autre interaction pendant le rewind
            {
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
                    case "PastEcho":
                        if (positionHistory.Count > rewindFrames)
                        {
                            StartCoroutine(RewindTime());
                        }
                        break;
                }
            }
        }
    }

    private void ApplyGravity()
    {
        if (characterController != null && !isRewinding)
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
        Vector3 teleportPosition = targetPoint - (direction * teleportOffset);

        characterController.enabled = false;
        player.position = teleportPosition;
        characterController.enabled = true;
    }

    private void StableJump()
    {
        float stableJumpForce = 8f;
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

    private void RecordHistory()
    {
        if (positionHistory.Count > 2000) // Augmenté pour éviter une surcharge mémoire
        {
            positionHistory.RemoveAt(0);
            rotationHistory.RemoveAt(0);
        }

        positionHistory.Add(player.position);
        rotationHistory.Add(player.rotation);
    }

    private IEnumerator RewindTime()
    {
        isRewinding = true;
        characterController.enabled = false; // Désactiver le CharacterController pendant le rewind

        int framesRewound = 0;

        while (framesRewound < rewindFrames && positionHistory.Count > 1)
        {
            player.position = positionHistory[positionHistory.Count - 1];
            player.rotation = rotationHistory[rotationHistory.Count - 1];

            positionHistory.RemoveAt(positionHistory.Count - 1);
            rotationHistory.RemoveAt(rotationHistory.Count - 1);

            framesRewound++;

            if (framesRewound % 10 == 0) // Petite pause toutes les 10 frames pour fluidifier l'effet
            {
                yield return null;
            }
        }

        characterController.enabled = true; // Réactiver le CharacterController après le rewind
        isRewinding = false;
    }
}
