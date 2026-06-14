using System.Collections;
using UnityEngine;

public class BotonPresion : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Cuánto va a bajar el botón en el eje Y cuando lo pisen. Ejemplo: 0.15 o 0.2")]
    [SerializeField] private float distanciaHundimiento = 0.15f;

    [Header("Tiempos y Velocidades")]
    [SerializeField] private float velocidadAparicion = 2f;
    [SerializeField] private float tiempoEsperaSegundaPlataforma = 3f;

    [Header("Plataformas a Activar")]
    [SerializeField] private SpriteRenderer subida1;
    [SerializeField] private SpriteRenderer subida2;

    // Posiciones calculadas dinámicamente según dónde pongas el botón en el mapa
    private float yOriginal;
    private float yPresionado;

    private bool secuenciasActivada = false;
    private Coroutine rutinaPlataformas;
    private Coroutine rutinaSalida;

    // Contador cooperativo para evitar que suba si queda alguien encima
    private int jugadoresEncima = 0;

    private void Start()
    {
        // Guardamos la posición Y exacta que tiene en el Inspector de Unity
        yOriginal = transform.localPosition.y;

        // Calculamos la posición presionada restándole solo un poquito a la original
        yPresionado = yOriginal - distanciaHundimiento;

        // Nos aseguramos de que inicie en su lugar
        transform.localPosition = new Vector3(transform.localPosition.x, yOriginal, transform.localPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta tanto al Samurái (Player) como al Mago (Player2)
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            jugadoresEncima++;

            if (rutinaSalida != null) StopCoroutine(rutinaSalida);

            // Baja el botón a su nueva posición calculada
            transform.localPosition = new Vector3(transform.localPosition.x, yPresionado, transform.localPosition.z);

            if (!secuenciasActivada)
            {
                secuenciasActivada = true;
                if (rutinaPlataformas != null) StopCoroutine(rutinaPlataformas);
                rutinaPlataformas = StartCoroutine(SecuenciaPlataformas(true));
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            // Mantiene el botón abajo firme mientras haya héroes encima
            transform.localPosition = new Vector3(transform.localPosition.x, yPresionado, transform.localPosition.z);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            jugadoresEncima--;

            // Solo inicia la secuencia de subida si el botón se quedó completamente vacío
            if (jugadoresEncima <= 0)
            {
                jugadoresEncima = 0; // Seguridad anti-negativos

                if (rutinaSalida != null) StopCoroutine(rutinaSalida);

                if (gameObject.activeInHierarchy)
                {
                    rutinaSalida = StartCoroutine(EsperaParaSalir());
                }
                else
                {
                    // Reset instantáneo si el objeto se apaga
                    transform.localPosition = new Vector3(transform.localPosition.x, yOriginal, transform.localPosition.z);
                    ResetearPlataformasGolpe();
                }
            }
        }
    }

    private IEnumerator EsperaParaSalir()
    {
        yield return new WaitForSeconds(0.15f);

        // Regresa suavemente a su posición Y original capturada en el Start
        transform.localPosition = new Vector3(transform.localPosition.x, yOriginal, transform.localPosition.z);

        if (secuenciasActivada)
        {
            secuenciasActivada = false;

            if (rutinaPlataformas != null) StopCoroutine(rutinaPlataformas);

            if (gameObject.activeInHierarchy)
            {
                rutinaPlataformas = StartCoroutine(SecuenciaPlataformas(false));
            }
        }
    }

    private void ResetearPlataformasGolpe()
    {
        if (secuenciasActivada)
        {
            secuenciasActivada = false;
            if (subida1 != null) { Color c = subida1.color; c.a = 0f; subida1.color = c; DesactivarCollider(subida1); }
            if (subida2 != null) { Color c = subida2.color; c.a = 0f; subida2.color = c; DesactivarCollider(subida2); }
        }
    }

    private void DesactivarCollider(SpriteRenderer sprite)
    {
        BoxCollider2D collider = sprite.GetComponent<BoxCollider2D>();
        if (collider != null) collider.enabled = false;
    }

    private IEnumerator SecuenciaPlataformas(bool aparecer)
    {
        if (aparecer)
        {
            yield return StartCoroutine(FadeSprite(subida1, 1f));
            yield return new WaitForSeconds(tiempoEsperaSegundaPlataforma);
            yield return StartCoroutine(FadeSprite(subida2, 1f));
        }
        else
        {
            StartCoroutine(FadeSprite(subida1, 0f));
            StartCoroutine(FadeSprite(subida2, 0f));
        }
    }

    private IEnumerator FadeSprite(SpriteRenderer sprite, float alphaObjetivo)
    {
        if (sprite == null) yield break;

        BoxCollider2D collider = sprite.GetComponent<BoxCollider2D>();

        if (alphaObjetivo > 0.5f && collider != null) collider.enabled = true;

        Color colorActual = sprite.color;

        while (!Mathf.Approximately(colorActual.a, alphaObjetivo))
        {
            colorActual.a = Mathf.MoveTowards(colorActual.a, alphaObjetivo, velocidadAparicion * Time.deltaTime);
            sprite.color = colorActual;
            yield return null;
        }

        if (alphaObjetivo < 0.5f && collider != null) collider.enabled = false;
    }
}