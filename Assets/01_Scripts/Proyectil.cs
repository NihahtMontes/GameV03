using UnityEngine;

public class Proyectil : MonoBehaviour
{
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private int danio = 1;
    [SerializeField] private float tiempoVidaMax = 4f; // Por si sale del mapa, se destruye solo

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Se autodestruye en 4 segundos si no choca con nada para no saturar la memoria
        Destroy(gameObject, tiempoVidaMax);
    }

    /// <summary>
    /// Método público para darle dirección al proyectil al nacer
    /// </summary>
    public void IniciarDireccion(float direccionX)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Le damos velocidad horizontal fija. Mantiene la escala visual.
        rb.linearVelocity = new Vector2(direccionX * velocidad, 0f);
        transform.localScale = new Vector3(direccionX, 1f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Buscamos si el cuerpo con el que chocó tiene el script de la momia
        EnemigoMomia enemigo = collision.GetComponent<EnemigoMomia>();

        if (enemigo != null)
        {
            enemigo.RecibirDanio(danio); // Le hace daño a la momia
            Destroy(gameObject); // El proyectil desaparece al impactar
            return;
        }

        // Si choca contra el suelo o paredes (Cosas sólidas que no sean el propio jugador ni otros triggers)
        if (!collision.CompareTag("Player") && !collision.CompareTag("Player2") && !collision.isTrigger)
        {
            Destroy(gameObject); // Desaparece al fallar el tiro
        }
    }
}