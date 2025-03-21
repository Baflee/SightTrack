using UnityEngine;

public class OrbitFruits : MonoBehaviour
{
    public Transform center; // Le point autour duquel le fruit tourne
    public float speed = 20f; // Vitesse de rotation

    void Update()
    {
        if (center != null)
        {
            // Faire tourner le fruit autour du centre
            transform.RotateAround(center.position, Vector3.up, speed * Time.deltaTime);
        }
    }
}
