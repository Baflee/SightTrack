using System.Collections.Generic;
using UnityEngine;

public class RewindSystem : MonoBehaviour
{
    private List<Vector3> positions = new List<Vector3>(); // Stocke les positions
    private List<Quaternion> rotations = new List<Quaternion>(); // Stocke les rotations
    private bool isRewinding = false;
    private CharacterController controller;
    private PlayerMovement playerMovement; // Ton script de mouvement

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>(); // Récupère ton script de mouvement
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Appuie sur 'R' pour activer le rewind
        {
            StartRewind();
        }
        if (Input.GetKeyUp(KeyCode.R)) // Relâche 'R' pour arrêter
        {
            StopRewind();
        }
    }

    void FixedUpdate()
    {
        if (isRewinding)
        {
            Rewind();
        }
        else
        {
            Record();
        }
    }

    void Record()
    {
        if (positions.Count > 500) // Garde en mémoire 500 frames max
        {
            positions.RemoveAt(0);
            rotations.RemoveAt(0);
        }
        positions.Add(transform.position);
        rotations.Add(transform.rotation);
    }

    void Rewind()
    {
        if (positions.Count > 0)
        {
            controller.enabled = false; // Désactive le Character Controller pour le repositionnement
            transform.position = positions[positions.Count - 1];
            transform.rotation = rotations[rotations.Count - 1];
            positions.RemoveAt(positions.Count - 1);
            rotations.RemoveAt(rotations.Count - 1);
            controller.enabled = true; // Réactive après le déplacement
        }
        else
        {
            StopRewind();
        }
    }

    void StartRewind()
    {
        isRewinding = true;
        playerMovement.enabled = false; // Désactive le script de mouvement pour éviter les conflits
    }

    void StopRewind()
    {
        isRewinding = false;
        playerMovement.enabled = true; // Réactive le script de mouvement
    }
}

