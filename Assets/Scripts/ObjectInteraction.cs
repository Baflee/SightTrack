using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectInteraction : MonoBehaviour
{
    public Transform player;
    public float raycastRange = 10f;
    public float minimumDistance = 2f;
    public float teleportOffset = 1.5f;
    public int rewindFrames = 1000;

    private CharacterController characterController;
    private Transform camTransform;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isStableCrouching;
    private bool hasStableJumped;
    private bool isRewinding;
    private Coroutine standUpCoroutine;
    private float originalHeight;

    private readonly List<Vector3> positionHistory = new List<Vector3>();
    private readonly List<Quaternion> rotationHistory = new List<Quaternion>();

    private void Start()
    {
        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("Le joueur n'a pas de CharacterController assigné !");
            }
            else
            {
                originalHeight = characterController.height;
            }
        }
        else
        {
            Debug.LogError("Aucun joueur assigné. Assignez le joueur manuellement dans l'inspecteur.");
        }

        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Aucune caméra avec le tag MainCamera trouvée.");
        }
    }

    private void Update()
    {
        if (player == null || characterController == null) return;

        if (!isRewinding)
        {
            RecordHistory();
            ApplyGravity();
        }

        CheckObject();

        // Forcer le joueur à rester droit (pitch/roll = 0), on conserve l'angle Y.
        player.rotation = Quaternion.Euler(0f, player.rotation.eulerAngles.y, 0f);
    }

    private void CheckObject()
    {
        if (player == null || isRewinding || camTransform == null) return;

        if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, raycastRange))
        {
            float distanceToObject = Vector3.Distance(player.position, hit.point);
            if (distanceToObject < minimumDistance) return;

            if (!isRewinding)
            {
                // Utiliser CompareTag pour plus de robustesse/perf
                if (hit.collider.CompareTag("Jump"))
                {
                    TeleportToTarget(hit.point);
                }
                else if (hit.collider.CompareTag("Backward"))
                {
                    MoveBackward();
                }
                else if (hit.collider.CompareTag("Forward"))
                {
                    MoveForward();
                }
                else if (hit.collider.CompareTag("Crouch"))
                {
                    MoveForward();
                    Crouch();
                    ResetStandUpTimer();
                }
                else if (hit.collider.CompareTag("StableCrouch"))
                {
                    StableCrouch();
                    ResetStandUpTimer();
                }
                else if (hit.collider.CompareTag("StableJump"))
                {
                    if (!hasStableJumped)
                    {
                        StableJump();
                    }
                }
                else if (hit.collider.CompareTag("PastEcho"))
                {
                    if (positionHistory.Count > rewindFrames)
                    {
                        StartCoroutine(RewindTime());
                    }
                }
            }
        }
        else
        {
            if ((isCrouching || isStableCrouching) && standUpCoroutine == null)
            {
                standUpCoroutine = StartCoroutine(StandUpAfterDelay());
            }
        }
    }

    private void ApplyGravity()
    {
        if (characterController == null || isRewinding) return;

        isGrounded = characterController.isGrounded;
        if (isGrounded)
        {
            // Coller au sol et réinitialiser le saut stable
            if (velocity.y < 0f) velocity.y = -2f;
            hasStableJumped = false;
        }

        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void TeleportToTarget(Vector3 targetPoint)
    {
        if (player == null || characterController == null) return;

        Vector3 direction = (targetPoint - player.position).normalized;
        Vector3 teleportPosition = targetPoint - (direction * teleportOffset);

        characterController.enabled = false;
        player.position = teleportPosition;
        characterController.enabled = true;

        // Éviter les à-coups après téléportation
        velocity = Vector3.zero;

        // Petit move nul pour forcer l’update des collisions
        characterController.Move(Vector3.zero);
    }

    private void StableJump()
    {
        if (characterController == null) return;

        // Interprétation: stableJumpForce ~ "hauteur" ou facteur d'impulsion
        float stableJumpForce = 10f;
        velocity.y = Mathf.Sqrt(2f * stableJumpForce * -Physics.gravity.y);
        characterController.Move(velocity * Time.deltaTime);
        hasStableJumped = true;
    }

    private Vector3 PlanarForward()
    {
        if (camTransform == null) return Vector3.zero;
        Vector3 fwd = camTransform.forward;
        fwd.y = 0f;
        float mag = fwd.magnitude;
        if (mag < 1e-4f) return Vector3.zero;
        return fwd / mag;
    }

    private void MoveBackward()
    {
        if (characterController == null) return;
        Vector3 fwd = PlanarForward();
        if (fwd == Vector3.zero) return;
        characterController.Move(-fwd * 15f * Time.deltaTime);
    }

    private void MoveForward()
    {
        if (characterController == null) return;
        Vector3 fwd = PlanarForward();
        if (fwd == Vector3.zero) return;
        characterController.Move(fwd * 6f * Time.deltaTime);
    }

    private void Crouch()
    {
        if (characterController == null) return;

        // Éviter la double division de la hauteur
        if (!isCrouching && characterController.height == originalHeight)
        {
            isCrouching = true;
            characterController.height = originalHeight / 2f;
        }
    }

    private void StableCrouch()
    {
        if (characterController == null) return;

        if (!isStableCrouching && characterController.height == originalHeight)
        {
            isStableCrouching = true;
            characterController.height = originalHeight / 2f;
        }
    }

    private void StandUp()
    {
        if (characterController == null) return;

        isCrouching = false;
        isStableCrouching = false;
        characterController.height = originalHeight;
        standUpCoroutine = null;
    }

    private void ResetStandUpTimer()
    {
        if (standUpCoroutine != null)
        {
            StopCoroutine(standUpCoroutine);
            standUpCoroutine = null;
        }
        standUpCoroutine = StartCoroutine(StandUpAfterDelay());
    }

    private IEnumerator StandUpAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        StandUp();
    }

    private void RecordHistory()
    {
        if (player == null) return;

        if (positionHistory.Count > 2000)
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

        if (characterController != null)
            characterController.enabled = false;

        int framesRewound = 0;
        while (framesRewound < rewindFrames && positionHistory.Count > 1)
        {
            player.position = positionHistory[positionHistory.Count - 1];
            player.rotation = rotationHistory[rotationHistory.Count - 1];

            positionHistory.RemoveAt(positionHistory.Count - 1);
            rotationHistory.RemoveAt(rotationHistory.Count - 1);

            framesRewound++;
            if (framesRewound % 10 == 0)
            {
                yield return null;
            }
        }

        if (characterController != null)
        {
            characterController.enabled = true;
            // Réinitialiser la vélocité après le rewind
            velocity = Vector3.zero;
            characterController.Move(Vector3.zero);
        }

        isRewinding = false;
    }
}
