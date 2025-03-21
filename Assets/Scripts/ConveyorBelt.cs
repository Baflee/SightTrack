using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Vector3 direction = Vector3.forward; // Direction du déplacement
    public float speed = 5f; // Vitesse du tapis
    private bool isPlayerOnBelt = false;
    private CharacterController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnBelt = true;
            playerController = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isPlayerOnBelt && playerController != null)
        {
            Vector3 movement = direction.normalized * speed * Time.deltaTime;
            playerController.Move(movement);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnBelt = false;
            playerController = null;
        }
    }
}
