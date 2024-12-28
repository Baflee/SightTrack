using UnityEngine;

public class NPCPosition : MonoBehaviour
{
    void LateUpdate()
    {
        // Obtenir la position de la caméra
        Vector3 cameraPosition = Camera.main.transform.position;

        // Calculer la direction horizontale vers la caméra
        Vector3 direction = cameraPosition - transform.position;
        direction.y = 0; // Ignorer la composante verticale

        // Appliquer la rotation vers la caméra
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Conserver l'orientation originale sur l'axe X (90°)
            transform.rotation = Quaternion.Euler(90, targetRotation.eulerAngles.y, 0);
        }
    }
}
