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

        while (vidaVisualActual > vidaActual)
        {
            vidaVisualActual--;

            if (corazonesAnimators[vidaVisualActual] != null)
            {
                corazonesAnimators[vidaVisualActual].SetTrigger("Explotar");
            }
        }
    }
}