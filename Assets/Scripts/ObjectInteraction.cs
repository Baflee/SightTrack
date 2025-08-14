using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectInteraction : MonoBehaviour
{
    [Header("Références")]
    public Transform player;

    [Header("Raycast & Interactions")]
    public float raycastRange = 10f;
    public float minimumDistance = 2f;
    public float teleportOffset = 1.5f;
    public int rewindFrames = 1000;

    [Header("Lissage Mouvement")]
    [SerializeField] private float maxForwardSpeed = 6f;
    [SerializeField] private float maxBackwardSpeed = 15f;
    [SerializeField] private float accel = 14f;
    [SerializeField] private float decel = 18f;

    [Header("Téléportation Lissée")]
    [SerializeField] private float teleportDuration = 0.15f; // tween court

    [Header("Rewind Fluide")]
    [SerializeField] private float rewindPlaybackSpeed = 60f; // segments/seconde

    [Header("Saut Stable")]
    [SerializeField] private float stableJumpForce = 10f;
    [SerializeField] private float jumpBuffer = 0.08f; // buffer de 80 ms

    private CharacterController characterController;
    private Transform camTransform;

    // Physique/move
    private Vector3 velocity; // uniquement Y ici; l’horizontal est géré par currentSpeed
    private bool isGrounded;

    // États
    private bool isCrouching;
    private bool isStableCrouching;
    private bool hasStableJumped;
    private bool isRewinding;

    // Lissage vitesses
    private float currentSpeed = 0f;   // m/s (négatif = recul)
    private float targetSpeed = 0f;    // m/s

    // Coroutines
    private Coroutine standUpCoroutine;

    // Crouch data
    private float originalHeight;

    // Historique pour rewind
    private readonly List<Vector3> positionHistory = new List<Vector3>();
    private readonly List<Quaternion> rotationHistory = new List<Quaternion>();

    // Jump buffer
    private float lastStableJumpRequest = -999f;

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
            ApplyGravity(); // ne fait qu'updater velocity.y (pas de Move ici)
        }

        CheckObject(); // met à jour targetSpeed + déclenche interactions

        // Lissage accélération/décélération horizontale
        float rate = Mathf.Abs(targetSpeed) > Mathf.Abs(currentSpeed) ? accel : decel;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        // Déplacement combiné (horizontal lissé + gravité)
        Vector3 fwd = PlanarForward();
        Vector3 horizontal = fwd * currentSpeed;
        Vector3 vertical = new Vector3(0f, velocity.y, 0f);
        characterController.Move((horizontal + vertical) * Time.deltaTime);

        // Forcer le joueur à rester droit (pitch/roll = 0), on conserve l'angle Y.
        player.rotation = Quaternion.Euler(0f, player.rotation.eulerAngles.y, 0f);
    }

    private void CheckObject()
    {
        if (player == null || isRewinding || camTransform == null) return;

        // Par défaut, si aucun tag, on vise l'arrêt
        targetSpeed = 0f;

        if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, raycastRange))
        {
            float distanceToObject = Vector3.Distance(player.position, hit.point);
            if (distanceToObject < minimumDistance) return;

            if (!isRewinding)
            {
                if (hit.collider.CompareTag("Jump"))
                {
                    TeleportToTargetSmooth(hit.point);
                }
                else if (hit.collider.CompareTag("Backward"))
                {
                    targetSpeed = -maxBackwardSpeed;
                }
                else if (hit.collider.CompareTag("Forward"))
                {
                    targetSpeed = maxForwardSpeed;
                }
                else if (hit.collider.CompareTag("Crouch"))
                {
                    targetSpeed = maxForwardSpeed * 0.7f; // optionnel, garde un petit glide
                    Crouch(); // <-- instantané comme avant
                    ResetStandUpTimer();
                }
                else if (hit.collider.CompareTag("StableCrouch"))
                {
                    StableCrouch(); // <-- instantané comme avant
                    ResetStandUpTimer();
                }
                else if (hit.collider.CompareTag("StableJump"))
                {
                    if (!hasStableJumped)
                    {
                        StableJump(); // déclenche avec un mini buffer
                    }
                }
                else if (hit.collider.CompareTag("PastEcho"))
                {
                    if (positionHistory.Count > rewindFrames)
                    {
                        StartCoroutine(RewindTimeSmooth());
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
        // NOTE: on n'appelle plus Move ici; Move est fait dans Update avec l'horizontal
    }

    // --- Téléportation lissée (tween court) ---
    private void TeleportToTargetSmooth(Vector3 targetPoint)
    {
        if (player == null || characterController == null) return;

        Vector3 direction = (targetPoint - player.position).normalized;
        Vector3 targetPos = targetPoint - (direction * teleportOffset);
        StartCoroutine(TeleportTween(targetPos));
    }

    private IEnumerator TeleportTween(Vector3 targetPos)
    {
        characterController.enabled = false;
        Vector3 start = player.position;
        float t = 0f;

        // annule la vélocité verticale pour éviter un kick post-TP
        velocity = Vector3.zero;

        while (t < teleportDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / teleportDuration);
            player.position = Vector3.Lerp(start, targetPos, k);
            yield return null;
        }
        player.position = targetPos;

        characterController.enabled = true;
        characterController.Move(Vector3.zero); // force l’update des collisions
    }

    // --- Saut stable avec mini buffer (plus tolérant et moins "sec") ---
    private void StableJump()
    {
        lastStableJumpRequest = Time.time;
        StartCoroutine(TryDoStableJump());
    }

    private IEnumerator TryDoStableJump()
    {
        // attends 1 frame pour laisser isGrounded se mettre à jour proprement
        yield return null;
        if (characterController.isGrounded && Time.time - lastStableJumpRequest <= jumpBuffer)
        {
            velocity.y = Mathf.Sqrt(2f * stableJumpForce * -Physics.gravity.y);
            hasStableJumped = true;
        }
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

    // --- Crouch instantané (comme l’original) ---
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

    // --- Historique positions/rotations pour rewind ---
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

    // --- Rewind fluide (interpolation entre points) ---
    private IEnumerator RewindTimeSmooth()
    {
        isRewinding = true;

        if (characterController != null)
            characterController.enabled = false;

        int steps = Mathf.Min(rewindFrames, positionHistory.Count - 1);
        int idx = positionHistory.Count - 1;

        while (steps > 0 && idx > 0)
        {
            Vector3 a = positionHistory[idx];
            Vector3 b = positionHistory[idx - 1];
            Quaternion qa = rotationHistory[idx];
            Quaternion qb = rotationHistory[idx - 1];

            float t = 0f;
            float segTime = 1f / rewindPlaybackSpeed; // durée par “segment” consommé
            while (t < segTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / segTime);
                player.position = Vector3.Lerp(a, b, k);
                player.rotation = Quaternion.Slerp(qa, qb, k);
                yield return null;
            }

            // On consomme ce point
            positionHistory.RemoveAt(idx);
            rotationHistory.RemoveAt(idx);
            idx--;
            steps--;
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
