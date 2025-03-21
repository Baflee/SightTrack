using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float bounceForce = 10f; // Force de rebond à appliquer

    [Header("Particle Effect")]
    public ParticleSystem bounceEffect; // Référence à l'effet de particules

    private void OnCollisionEnter(Collision collision)
    {
        // Vérifiez si l'objet qui touche le trampoline possède un Rigidbody
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Appliquez une force vers le haut pour simuler le rebond
            Vector3 bounceDirection = Vector3.up;
            rb.linearVelocity = Vector3.zero; // Réinitialisez la vitesse actuelle
            rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

            // Jouez l'effet de particules au point de contact
            if (bounceEffect != null)
            {
                bounceEffect.transform.position = collision.contacts[0].point;
                bounceEffect.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
                bounceEffect.Play();
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Affiche un repère visuel pour indiquer la direction du rebond
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
    }
}