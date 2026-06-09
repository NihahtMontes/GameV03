using UnityEngine;

public class EnemigoMomia : MonoBehaviour
{
    [Header("Movimiento e IA")]
    [SerializeField] private float velocidadCamino = 2f;
    [SerializeField] private float rangoDeteccionAtaque = 1.2f;
    [SerializeField] private float cooldownAtaque = 1.5f;
    [SerializeField] private LayerMask capaJugadores; // Configurar como "Default" o la capa de tus héroes

    [Header("Estadísticas")]
    [SerializeField] private int vidaMax = 3;

    [Header("Detección de Obstáculos")]
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private Vector2 tamanoCajaCheck = new Vector2(0.2f, 0.4f);
    [SerializeField] private float distanciaFrente = 0.3f;

    private int vidaActual;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform objetivoActual; // Jugador objetivo dinámico

    private int direccion = -1; // -1 = Izquierda, 1 = Derecha
    private float timerAtaque;
    private bool estaMuerto = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        vidaActual = vidaMax;
    }

    private void Update()
    {
        if (estaMuerto) return;

        if (timerAtaque > 0) timerAtaque -= Time.deltaTime;

        // Buscamos si hay algún jugador cerca usando el área de colisión física
        BuscarJugadoresDinamicos();

        if (objetivoActual != null)
        {
            GirarHaciaObjetivo();
            IntentarAtacar();
            return; // Se frena a atacar e ignora el comportamiento de patrulla
        }

        // Si no hay nadie cerca, patrulla tranquilamente
        Patrullar();
    }

    private void BuscarJugadoresDinamicos()
    {
        // Lanzamos un círculo invisible en su rango para detectar colisionadores en la capa asignada
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, rangoDeteccionAtaque, capaJugadores);

        // Verificamos que el colisionador tenga cualquiera de tus dos etiquetas de jugador
        if (jugadorDetectado != null && (jugadorDetectado.CompareTag("Player") || jugadorDetectado.CompareTag("Player2")))
        {
            objetivoActual = jugadorDetectado.transform;
        }
        else
        {
            objetivoActual = null;
        }
    }

    private void Patrullar()
    {
        rb.linearVelocity = new Vector2(direccion * velocidadCamino, rb.linearVelocity.y);

        Vector2 posicionCaja = (Vector2)transform.position + new Vector2(direccion * distanciaFrente, -0.2f);
        bool chocoConObstaculo = Physics2D.OverlapBox(posicionCaja, tamanoCajaCheck, 0f, capaSuelo);

        if (chocoConObstaculo)
        {
            CambiarDireccion();
        }
    }

    private void IntentarAtacar()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Se frena en seco para golpear

        if (timerAtaque <= 0 && objetivoActual != null)
        {
            anim.SetTrigger("Atacar");
            timerAtaque = cooldownAtaque;

            // Le mandamos el daño directo al script único 'Player' del objetivo que está enfrente
            Player jugadorScript = objetivoActual.GetComponent<Player>();
            if (jugadorScript != null)
            {
                jugadorScript.RecibirDanio(1); // Le resta 1 corazón
            }
        }
    }

    private void GirarHaciaObjetivo()
    {
        if (objetivoActual == null) return;

        if (objetivoActual.position.x > transform.position.x && direccion == -1)
        {
            CambiarDireccion();
        }
        else if (objetivoActual.position.x < transform.position.x && direccion == 1)
        {
            CambiarDireccion();
        }
    }

    private void CambiarDireccion()
    {
        direccion *= -1;
        transform.localScale = new Vector3(-direccion, 1f, 1f);
    }

    public void RecibirDanio(int cantidadDanio)
    {
        if (estaMuerto) return;

        vidaActual -= cantidadDanio;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) StartCoroutine(EfectoGolpe(sr));

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private System.Collections.IEnumerator EfectoGolpe(SpriteRenderer sr)
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    private void Morir()
    {
        estaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        anim.SetTrigger("Morir");
        Destroy(gameObject, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccionAtaque);

        Gizmos.color = Color.blue;
        Vector2 posicionCaja = (Vector2)transform.position + new Vector2(direccion * distanciaFrente, -0.2f);
        Gizmos.DrawWireCube(posicionCaja, tamanoCajaCheck);
    }
}