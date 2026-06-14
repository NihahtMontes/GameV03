using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaSalida : MonoBehaviour
{
    private bool nivelCambiando = false; // Control para evitar dobles cargas

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (nivelCambiando) return;

        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            ItemLlave llaveDelNivel = Object.FindFirstObjectByType<ItemLlave>();

            if (llaveDelNivel != null && llaveDelNivel.EsRecogida())
            {
                nivelCambiando = true; // Bloqueamos nuevas entradas
                Debug.Log("¡Puerta abierta con éxito! Avanzando al Nivel 2...");

                int escenaActualIndex = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(escenaActualIndex + 1);
            }
            else
            {
                Debug.Log("La puerta está cerrada. Necesitas encontrar la llave cooperativamente primero.");
            }
        }
    }
}