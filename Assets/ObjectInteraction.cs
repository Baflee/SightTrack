using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Transform player;             // Assignation manuelle du joueur via l'inspecteur
    public float raycastRange = 10f;     // Distance maximale du Raycast
    public float minimumDistance = 2f;  // Distance minimale pour déclencher l'effet
    public float jumpForce = 5f;         // Force du saut
    public float moveSpeed = 5f;         // Vitesse pour avancer/reculer
    public float crouchSpeed = 2f;       // Vitesse de l'accroupissement

    private Rigidbody playerRb;          // Rigidbody du joueur

    private void Start()
    {
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            if (playerRb == null)
            {
                Debug.LogError("Le joueur n'a pas de Rigidbody assigné !");
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

        // Fixer la rotation du joueur pour empêcher une rotation involontaire
        if (player != null)
        {
            player.rotation = Quaternion.Euler(0, player.rotation.eulerAngles.y, 0);
        }
    }

    private void CheckObject()
    {
        if (player == null) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            float distanceToObject = Vector3.Distance(player.position, hit.point);

            if (distanceToObject >= minimumDistance)
            {
                string objectTag = hit.collider.tag;

                if (objectTag == "Jump")
                {
                    JumpForward();
                }
                else if (objectTag == "Backward")
                {
                    MoveBackward();
                }
                else if (objectTag == "Forward")
                {
                    MoveForward();
                }
                else if (objectTag == "Crouch")
                {
                    Crouch();
                }
            }
            else
            {
                Debug.Log("Trop proche de l'objet pour déclencher une action.");
            }
        }
    }

    private void JumpForward()
    {
        if (playerRb != null)
        {
            Vector3 jumpDirection = Camera.main.transform.forward + Vector3.up;
            playerRb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            Debug.Log("Jumping forward!");
        }
    }

    private void MoveBackward()
    {
        if (player != null)
        {
            Vector3 backwardDirection = -Camera.main.transform.forward;
            player.Translate(backwardDirection * moveSpeed * Time.deltaTime, Space.World);
            Debug.Log("Moving backward!");
        }
    }

    private void MoveForward()
    {
        if (player != null)
        {
            Vector3 forwardDirection = Camera.main.transform.forward;
            player.Translate(forwardDirection * moveSpeed * Time.deltaTime, Space.World);
            Debug.Log("Moving forward!");
        }
    }

    private void Crouch()
    {
        if (player != null)
        {
            Vector3 crouchDirection = Camera.main.transform.forward;
            player.Translate(crouchDirection * crouchSpeed * Time.deltaTime, Space.World);
            Debug.Log("Crouching!");
        }
    }
}