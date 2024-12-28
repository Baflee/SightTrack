using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Transform player;             // Assignation manuelle du joueur via l'inspecteur
    public float raycastRange = 10f;     // Distance maximale du Raycast
    public float minimumDistance = 2f;  // Distance minimale pour déclencher l'effet
    public float jumpForce = 5f;         // Force du saut
    public float forwardForce = 10f;
    public float moveSpeed = 5f;         // Vitesse pour avancer/reculer
    public float crouchSpeed = 2f;       // Vitesse de l'accroupissement
    public float gravity = -9.81f;       // Force gravitationnelle

    private CharacterController characterController; // Composant CharacterController
    private Vector3 velocity;                        // Stocker la gravité et les mouvements verticaux
    private bool isGrounded;                         // Détection si le joueur est au sol

    public float jumpCooldown = 0.1f;  // Temps d'attente entre deux sauts
    private float lastJumpTime = -1f;  // Heure du dernier saut

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

        // Appliquer la gravité
        ApplyGravity();

        // Fixer la rotation du joueur pour empêcher une rotation involontaire
        if (player != null)
        {
            player.rotation = Quaternion.Euler(0, player.rotation.eulerAngles.y, 0);
        }
    }

    private void CheckObject()
    {
        if (player == null) return;

        // Perform a raycast from the camera's forward direction
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, raycastRange))
        {
            float distanceToObject = Vector3.Distance(player.position, hit.point);

            if (distanceToObject < minimumDistance)
            {
                Debug.Log("Trop proche de l'objet pour déclencher une action.");
                return;
            }

            // Determine the action based on the object's tag
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
                    Crouch();
                    break;

                default:
                    Debug.Log($"Aucune action définie pour l'objet avec le tag : {objectTag}");
                    break;
            }
        }
        else
        {
            Debug.Log("Aucun objet détecté.");
        }
    }   

    private void ApplyGravity()
    {
        if (characterController != null)
        {
            isGrounded = characterController.isGrounded;

            if (isGrounded)
            {
                // Appliquez une friction manuelle pour éviter le glissement
                velocity.x *= 0.8f; 
                velocity.z *= 0.8f;

                // Réinitialisez la vélocité si elle est très faible
                if (velocity.magnitude < 0.1f)
                {
                    velocity.x = 0f;
                    velocity.z = 0f;
                }

                if (velocity.y < 0)
                {
                    velocity.y = -2f; // Petite force pour rester au sol
                }
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    private void StopGravity()
    {
        if (characterController != null)
        {
            // Réinitialiser la vélocité verticale à zéro pour arrêter la gravité
            velocity.y = 0f;
            Debug.Log("Gravité arrêtée temporairement.");
        }
    }


    private void JumpTowards(Vector3 targetPoint)
    {
        if (characterController != null)
        {
            // Réinitialiser tout mouvement existant
            velocity = Vector3.zero;
            characterController.Move(Vector3.zero); // Stop immédiat du mouvement précédent

            // Calculer la direction vers l'objet rouge
            Vector3 jumpDirection = (targetPoint - player.position).normalized;

            // Appliquer la force verticale pour le saut
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            // Appliquer la force horizontale vers la direction de l'objet
            velocity.x = jumpDirection.x * forwardForce;
            velocity.z = jumpDirection.z * forwardForce;

            // Appliquer immédiatement le saut
            characterController.Move(velocity * Time.deltaTime);

            // Log pour vérifier le comportement
            Debug.Log($"Nouveau saut vers : {targetPoint}. Vélocité actuelle : {velocity}");
        }
    }

    private void MoveBackward()
    {
        if (characterController != null)
        {
            Vector3 backwardDirection = -Camera.main.transform.forward;
            characterController.Move(backwardDirection * moveSpeed * Time.deltaTime);
            Debug.Log("Moving backward!");
        }
    }

    private void MoveForward()
    {
        if (characterController != null)
        {
            Vector3 forwardDirection = Camera.main.transform.forward;
            characterController.Move(forwardDirection * moveSpeed * Time.deltaTime);
            Debug.Log("Moving forward!");
        }
    }

    private void Crouch()
    {
        if (characterController != null)
        {
            Vector3 crouchDirection = Camera.main.transform.forward;
            characterController.Move(crouchDirection * crouchSpeed * Time.deltaTime);
            Debug.Log("Crouching!");
        }
    }
}
