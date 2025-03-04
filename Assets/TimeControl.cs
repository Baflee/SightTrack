using UnityEngine;

public class TimeControl : MonoBehaviour
{
    public float slowMotionScale = 0.2f; // Facteur de ralenti
    public float fastMotionScale = 2f; // Facteur d’accélération
    private float normalTimeScale = 1f; // Vitesse normale du jeu

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Clic droit → Ralentir le temps
        {
            Time.timeScale = slowMotionScale;
        }
        else if (Input.GetMouseButtonDown(0)) // Clic gauche → Accélérer le temps
        {
            Time.timeScale = fastMotionScale;
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) // Relâcher → Revenir à la normale
        {
            Time.timeScale = normalTimeScale;
        }
    }
}

