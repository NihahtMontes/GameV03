using System.Collections;
using UnityEngine;

public class BotonPresion : MonoBehaviour
{
    private float yOriginal = -17.499f;
    private float yPresionado = -18.57f;

    [SerializeField] private float velocidadAparicion = 2f;

    [SerializeField] private float tiempoEsperaSegundaPlataforma = 3f;

    [Header("Plataformas a Activar")]
    [SerializeField] private SpriteRenderer subida1;
    [SerializeField] private SpriteRenderer subida2;

    private bool secuenciasActivada = false;
    private Coroutine rutinaPlataformas;
    private Coroutine rutinaSalida;

    private void Start()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, yOriginal, transform.localPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (rutinaSalida != null) StopCoroutine(rutinaSalida);

            // Baja el botón al instante
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
        if (collision.CompareTag("Player"))
        {
            // Mantiene el botón abajo firme
            transform.localPosition = new Vector3(transform.localPosition.x, yPresionado, transform.localPosition.z);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (rutinaSalida != null) StopCoroutine(rutinaSalida);

            // COMPROBACIÓN DE SEGURIDAD: Solo arranca la corrutina si el GameObject está activo
            if (gameObject.activeInHierarchy)
            {
                rutinaSalida = StartCoroutine(EsperaParaSalir());
            }
            else
            {
                // Si el botón ya se desactivó por completo, hacemos el cambio de golpe sin corrutinas
                transform.localPosition = new Vector3(transform.localPosition.x, yOriginal, transform.localPosition.z);
                if (secuenciasActivada)
                {
                    secuenciasActivada = false;
                    if (subida1 != null) { Color c = subida1.color; c.a = 0f; subida1.color = c; if (subida1.GetComponent<BoxCollider2D>() != null) subida1.GetComponent<BoxCollider2D>().enabled = false; }
                    if (subida2 != null) { Color c = subida2.color; c.a = 0f; subida2.color = c; if (subida2.GetComponent<BoxCollider2D>() != null) subida2.GetComponent<BoxCollider2D>().enabled = false; }
                }
            }
        }
    }

    private IEnumerator EsperaParaSalir()
    {
        yield return new WaitForSeconds(0.15f);

        // Sube el botón al instante a su estado original
        transform.localPosition = new Vector3(transform.localPosition.x, yOriginal, transform.localPosition.z);

        if (secuenciasActivada)
        {
            secuenciasActivada = false;

            // Otra capa de seguridad antes de lanzar la desaparición suave
            if (rutinaPlataformas != null) StopCoroutine(rutinaPlataformas);

            if (gameObject.activeInHierarchy)
            {
                rutinaPlataformas = StartCoroutine(SecuenciaPlataformas(false));
            }
        }
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