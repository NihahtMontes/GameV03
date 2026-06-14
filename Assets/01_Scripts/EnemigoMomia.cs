using UnityEngine;

public class EnemigoMomia : MonoBehaviour
{
    [Header("Movimiento e IA")]
    [SerializeField] private float velocidadCamino = 2f;
    [SerializeField] private float rangoDeteccionAtaque = 1.2f;
    [SerializeField] private float cooldownAtaque = 1.5f;
    [SerializeField] private LayerMask capaJugadores;

    [Header("Estadísticas")]
    [SerializeField] private int vidaMax = 3;

    [Header("Detección de Obstáculos")]
    // ¡CAMBIADO! Ahora es una máscara genérica para Suelo, Paredes, etc.
    [SerializeField] private LayerMask capaObstaculos;
    [SerializeField] private Vector2 tamanoCajaCheck = new Vector2(0.2f, 0.4f);
    [SerializeField] private float distanciaFrente = 0.3f;

    [Header("Control de Caída Libre")]
    [SerializeField] private float tiempoMaxCaidaAlVacio = 3.5f;
    private float contadorTiempoCayendo = 0f;

    private int vidaActual;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform objetivoActual;

    private int direccion = -1;
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

        ChequearCaidaAlVacio();
        BuscarJugadoresDinamicos();

        if (objetivoActual != null)
        {
            GirarHaciaObjetivo();
            IntentarAtacar();
            return;
        }

        Patrullar();
    }

    private void ChequearCaidaAlVacio()
    {
        if (rb.linearVelocity.y < -0.1f)
        {
            contadorTiempoCayendo += Time.deltaTime;

            if (contadorTiempoCayendo >= tiempoMaxCaidaAlVacio)
            {
                SuicidarseEnVacio();
            }
        }
        else
        {
            contadorTiempoCayendo = 0f;
        }
    }

    private void SuicidarseEnVacio()
    {
        estaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
        Debug.Log(gameObject.name + " cayó al vacío infinito y fue eliminado correctamente.");
    }

    private void BuscarJugadoresDinamicos()
    {
        Collider2D jugadorDetectado = Physics2D.OverlapCircle(transform.position, rangoDeteccionAtaque, capaJugadores);

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

        // ¡CORREGIDO! Ahora revisa si choca contra cualquier objeto en la capa Suelo O Pared
        bool chocoConObstaculo = Physics2D.OverlapBox(posicionCaja, tamanoCajaCheck, 0f, capaObstaculos);

        if (chocoConObstaculo)
        {
            CambiarDireccion();
        }
    }

    private void IntentarAtacar()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (timerAtaque <= 0 && objetivoActual != null)
        {
            anim.SetTrigger("Atacar");
            timerAtaque = cooldownAtaque;

            Player jugadorScript = objetivoActual.GetComponent<Player>();
            if (jugadorScript != null)
            {
                jugadorScript.RecibirDanio(1);
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

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.danioEnemigo);

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