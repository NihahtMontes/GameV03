using UnityEngine;

public class EnemigoOjo : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private int vidaMax = 3;
    private int vidaActual;

    [Header("Configuración de Ataque")]
    [SerializeField] private GameObject prefabBalaOjo;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float tiempoEntreAtaques = 2f;
    [SerializeField] private float retrasoAnimacionCarga = 0.4f;
    [SerializeField] private float rangoDeteccion = 8f;

    private Animator anim;
    private float timerAtaque;
    private Transform jugadorObjetivo;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        vidaActual = vidaMax;
    }

    private void Update()
    {
        if (timerAtaque > 0) timerAtaque -= Time.deltaTime;

        BuscarJugadoresPorTag();

        // Si detecta a un jugador y el temporizador está listo, ataca
        if (jugadorObjetivo != null && timerAtaque <= 0)
        {
            Atacar();
        }
    }

    private void BuscarJugadoresPorTag()
    {
        jugadorObjetivo = null;
        float distanciaMasCercana = rangoDeteccion;

        // 1. Buscamos al Samurái (Player 1)
        GameObject p1 = GameObject.FindWithTag("Player");
        if (p1 != null)
        {
            float dist = Vector2.Distance(transform.position, p1.transform.position);
            if (dist <= distanciaMasCercana)
            {
                distanciaMasCercana = dist;
                jugadorObjetivo = p1.transform;
            }
        }

        // 2. Buscamos al Mago (Player 2)
        GameObject p2 = GameObject.FindWithTag("Player2");
        if (p2 != null)
        {
            float dist = Vector2.Distance(transform.position, p2.transform.position);
            if (dist <= distanciaMasCercana)
            {
                distanciaMasCercana = dist;
                jugadorObjetivo = p2.transform;
            }
        }
    }

    private void Atacar()
    {
        timerAtaque = tiempoEntreAtaques;

        if (anim != null) anim.SetTrigger("Atacar");

        StartCoroutine(RutinaDisparoOjo());
    }

    private System.Collections.IEnumerator RutinaDisparoOjo()
    {
        yield return new WaitForSeconds(retrasoAnimacionCarga);

        if (prefabBalaOjo != null && puntoDisparo != null && jugadorObjetivo != null)
        {
            float distanciaActual = Vector2.Distance(transform.position, jugadorObjetivo.position);

            if (distanciaActual <= rangoDeteccion)
            {
                // CORRECCIÓN CLAVE: Instanciamos asegurando que la Z sea 0f para evitar pérdidas en el plano
                Vector3 posicionSpawn = new Vector3(puntoDisparo.position.x, puntoDisparo.position.y, 0f);
                GameObject balaClon = Instantiate(prefabBalaOjo, posicionSpawn, Quaternion.identity);

                // CORRECCIÓN CLAVE: Convertimos las posiciones explícitamente a Vector2 antes de restar
                Vector2 posJugador2D = new Vector2(jugadorObjetivo.position.x, jugadorObjetivo.position.y);
                Vector2 posOrigen2D = new Vector2(puntoDisparo.position.x, puntoDisparo.position.y);

                Vector2 direccion = (posJugador2D - posOrigen2D).normalized;

                BalaEnemigo scriptBala = balaClon.GetComponent<BalaEnemigo>();
                if (scriptBala != null)
                {
                    scriptBala.IniciarDireccion(direccion);
                }
            }
        }
    }

    public void RecibirDanio(int cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Destroy(gameObject);
        Debug.Log("El Ojo fue destruido");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}