using System.Collections;
using UnityEngine;
using TMPro;

public class TriggerUIWithControlledReactivation : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI uiText; // Référence au texte TextMeshPro
    public string message = "Bonjour, je commence à parler..."; // Texte affiché
    public float fadeDuration = 1f; // Durée de l'apparition progressive

    [Header("Trigger Settings")]
    public bool canReactivate = true; // Booléen pour contrôler si le trigger peut se réactiver

    private Coroutine fadeCoroutine;
    private bool hasTriggered = false; // Vérifie si le trigger a déjà été activé

    private void Start()
    {
        Debug.Log("Script démarré. Initialisation des paramètres.");

        if (uiText != null)
        {
            uiText.text = "";
            uiText.alpha = 0f; // Initialiser la transparence
            Debug.Log("Texte initialisé et rendu invisible.");
        }
        else
        {
            Debug.LogWarning("Le champ uiText n'est pas assigné dans l'inspecteur !");
        }
    }

    private void OnEnable()
    {
        Debug.Log("GameObject activé.");
        if (hasTriggered && canReactivate)
        {
            Debug.Log("Réactivation autorisée.");
            hasTriggered = false; // Permet une nouvelle activation si le booléen est vrai
        }
        else
        {
            Debug.Log("Réactivation non autorisée ou déjà déclenchée.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Un objet est entré dans le trigger : {other.name}");
        if (!hasTriggered && other.CompareTag("Player"))
        {
            Debug.Log("Le joueur est entré dans le trigger.");
            hasTriggered = true;
            ShowText();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Un objet est sorti du trigger : {other.name}");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Le joueur est sorti du trigger.");
            HideText();
        }
    }

    private void ShowText()
    {
        if (uiText != null)
        {
            Debug.Log("Affichage du texte avec fade-in.");
            uiText.text = message;

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeText(uiText, 0f, 1f, fadeDuration));
        }
        else
        {
            Debug.LogWarning("uiText est null, le texte ne peut pas être affiché.");
        }
    }

    private void HideText()
    {
        if (uiText != null)
        {
            Debug.Log("Masquage du texte avec fade-out.");

            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeText(uiText, 1f, 0f, fadeDuration));
        }
        else
        {
            Debug.LogWarning("uiText est null, le texte ne peut pas être masqué.");
        }
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        Debug.Log($"Démarrage du fade du texte : de {startAlpha} à {endAlpha} sur {duration} secondes.");
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.alpha = newAlpha; // Appliquer la transparence
            yield return null;
        }

        text.alpha = endAlpha;
        Debug.Log("Fade terminé.");

        if (endAlpha == 0f)
        {
            text.text = "";
            Debug.Log("Texte vidé après disparition.");
        }
    }
}
