using UnityEngine;

public class BalaEnemigo : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private int danio = 1;
    [SerializeField] private float tiempoVidaMax = 5f;

    private Rigidbody2D rb;
    private Vector2 direccionDisparo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Se destruye sola en 5 segundos si no choca con nada para limpiar memoria
        Destroy(gameObject, tiempoVidaMax);
    }

    /// <summary>
    /// Método público que llama el ojo al clonar la bala para darle rumbo
    /// </summary>
    public void IniciarDireccion(Vector2 direccion)
    {
        direccionDisparo = direccion.normalized;

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // Le aplicamos la velocidad física directa hacia el jugador
        rb.linearVelocity = direccionDisparo * velocidad;

        // BORRAMOS LAS LÍNEAS DE ROTACIÓN PARA QUE EL SPRITE QUEDE RECTO
        // Ya no usamos transform.rotation = Quaternion.Euler(...);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca contra los jugadores, hace daño y se destruye
        if (collision.CompareTag("Player") || collision.CompareTag("Player2"))
        {
            Player jugador = collision.GetComponent<Player>();
            if (jugador != null) jugador.RecibirDanio(danio);
            Destroy(gameObject);
            return;
        }

        // CORRECCIÓN: Si choca contra el Ojo o cualquier otra cosa con el script de Enemigo, NO se destruye
        if (collision.GetComponent<EnemigoOjo>() != null)
        {
            return; // Ignora el choque con el Ojo dueño de la bala
        }

        // Si choca contra paredes o suelos sólidos
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}