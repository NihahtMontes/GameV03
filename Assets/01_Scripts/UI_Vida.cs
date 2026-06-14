using UnityEngine;

public class UI_Vida : MonoBehaviour
{
    [Header("¿A qué jugador pertenece este HUD?")]
    [Tooltip("Escribe 'Player' para el Samurái o 'Player2' para el Mago")]
    [SerializeField] private string tagAsociado = "Player";

    [Header("Lista de Animators de los Corazones")]
    [SerializeField] private Animator[] corazonesAnimators;

    private int vidaVisualActual;

    public string TagAsociado => tagAsociado; // Propiedad pública para que el jugador la lea

    private void Start()
    {
        vidaVisualActual = corazonesAnimators.Length;
    }

    public void ActualizarVidaUI(int vidaActual)
    {
        vidaActual = Mathf.Clamp(vidaActual, 0, corazonesAnimators.Length);

        // 1. SI LA VIDA BAJA: Hace explotar los corazones usando tu animación original
        while (vidaVisualActual > vidaActual)
        {
            vidaVisualActual--;

            if (corazonesAnimators[vidaVisualActual] != null)
            {
                // Forzamos a reproducir la animación de explosión desde el inicio
                corazonesAnimators[vidaVisualActual].Play("Corazon_Explotar", 0, 0f);
            }
        }

        // 2. SI LA VIDA SUBE (Respawn): Vuelve a prender los corazones con tu animación de latido
        while (vidaVisualActual < vidaActual)
        {
            if (corazonesAnimators[vidaVisualActual] != null)
            {
                // ¡CORREGIDO CON TU NOMBRE REAL! Forzamos a reproducir el latido (Idle)
                corazonesAnimators[vidaVisualActual].Play("Corazon_Latir", 0, 0f);
            }

            vidaVisualActual++;
        }
    }
}