using UnityEngine;

public class ItemLlave : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    [SerializeField] private float velocidadSeguimiento = 5f;
    [SerializeField] private Vector3 desfaseJugador = new Vector3(0f, 0.8f, 0f);

    private Transform jugadorObjetivo = null;
    private bool recogida = false;

    // Control para verificar la presencia de ambos jugadores en la zona
    private bool p1EstaTocando = false;
    private bool p2EstaTocando = false;

    private void Update()
    {
        // Si ya cooperaron y la llave fue levantada, sigue al jugador asignado
        if (recogida && jugadorObjetivo != null)
        {
            Vector3 posicionDestino = jugadorObjetivo.position + desfaseJugador;
            transform.position = Vector3.Lerp(transform.position, posicionDestino, velocidadSeguimiento * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (recogida) return;

        // Marcamos si entró el Jugador 1 o el Jugador 2
        if (collision.CompareTag("Player")) p1EstaTocando = true;
        if (collision.CompareTag("Player2")) p2EstaTocando = true;

        // EVALUACIÓN COOPERATIVA: ¿Están los dos juntos sobre la llave?
        if (p1EstaTocando && p2EstaTocando)
        {
            // Se la asignamos por defecto al jugador que completó la activación
            jugadorObjetivo = collision.transform;
            recogida = true;

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX(AudioManager.instance.recoger_llave);
            }

            Debug.Log("¡Trabajo en equipo! Llave recolectada por " + collision.gameObject.name + ". ¡Corran a la salida!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (recogida) return;

        // Si uno de los dos se aleja de la llave antes de que llegue el otro, se cancela su registro
        if (collision.CompareTag("Player")) p1EstaTocando = false;
        if (collision.CompareTag("Player2")) p2EstaTocando = false;
    }

    public bool EsRecogida()
    {
        return recogida;
    }
}