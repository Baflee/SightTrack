using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private string mouseXInputName = "Mouse X"; // Axe horizontal de la souris
    [SerializeField] private string mouseYInputName = "Mouse Y"; // Axe vertical de la souris
    [SerializeField] private float mouseSensitivity = 100f;     // Sensibilité de la souris

    private float xAxisClamp = 0f; // Limite de rotation verticale (haut/bas)

    private void Awake()
    {
        LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Verrouille le curseur au centre de l'écran
    }

    private void Update()
    {
        CameraRotation();
    }

    private void CameraRotation()
    {
        // Lire les mouvements de la souris
        float mouseX = Input.GetAxis(mouseXInputName) * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis(mouseYInputName) * mouseSensitivity * Time.deltaTime;

        // Rotation verticale (haut/bas) limitée
        xAxisClamp -= mouseY;
        xAxisClamp = Mathf.Clamp(xAxisClamp, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xAxisClamp, 0f, 0f);

        // Rotation horizontale (gauche/droite) uniquement pour la caméra
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
