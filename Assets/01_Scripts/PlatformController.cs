using System.Collections;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public enum PlatformMode
    {
        Collapse,
        PivotBounce,
        Bounce,
        MovingPoint,
        Trap,
        Ghost,
        Dodge,
        Seesaw,
        Emerger
    }

    public enum DireccionTrap
    {
        Izquierda,
        Derecha,
        DetectarJugador
    }

    public enum DireccionDodge
    {
        Izquierda,
        Derecha
    }

    [Header("MODO DE PLATAFORMA")]
    [SerializeField] private PlatformMode modo = PlatformMode.Collapse;

    [Header("EFECTO VISUAL AL PISAR")]
    [Tooltip("Si esta activo, la plataforma hace un pequeño rebote visual al pisarla")]
    [SerializeField] private bool efectoSquash = false;
    [SerializeField] private float squashIntensidad = 0.15f;
    [SerializeField] private float squashDuracion = 0.15f;

    [Header("COLLAPSE")]
    [SerializeField] private float colDuracionTemblor = 1.5f;
    [SerializeField] private float colIntensidadTemblor = 0.06f;
    [SerializeField] private float colFrecuenciaTemblor = 25f;
    [SerializeField] private float colVelocidadCaida = 8f;
    [SerializeField] private float colDistanciaCaida = 5f;
    [SerializeField] private float colTiempoReaparicion = 3f;
    [SerializeField] private bool colUsarTemblorNatural = true;

    [Header("PIVOT BOUNCE")]
    [Tooltip("Angulo al que se levanta la plataforma desde el pivot")]
    [SerializeField] private float pvtAngulo = -90f;
    [SerializeField] private float pvtVelocidadLevantar = 300f;
    [SerializeField] private float pvtVelocidadBajar = 150f;
    [SerializeField] private float pvtTiempoEspera = 0.5f;
    [SerializeField] private float pvtRadioDeteccion = 1.5f;

    [Header("BOUNCE")]
    [SerializeField] private float bncFuerzaVertical = 15f;
    [SerializeField] private float bncFuerzaHorizontal = 0f;
    [SerializeField] private bool bncBasarseEnLado = true;
    [SerializeField] private bool bncHaciaDerecha = true;

    [Header("MOVING POINT")]
    [SerializeField] private Transform mvPuntoA;
    [SerializeField] private Transform mvPuntoB;
    [SerializeField] private float mvVelocidad = 2f;
    [SerializeField] private bool mvIniciarEnB = false;

    [Header("TRAP")]
    [Tooltip("Direccion del primer movimiento de la trampa")]
    [SerializeField] private DireccionTrap traPrimerMovimiento = DireccionTrap.DetectarJugador;
    [Tooltip("Radio de deteccion tipo radar para detectar al jugador")]
    [SerializeField] private float traRadioDeteccion = 3f;
    [SerializeField] private float traDistancia = 3f;
    [SerializeField] private float traVelocidad = 6f;
    [SerializeField] private float traTiempoReaparicion = 4f;
    [SerializeField] private bool traReaparecer = true;

    [Header("GHOST")]
    [Tooltip("La plataforma aparece y desaparece en ciclos")]
    [SerializeField] private float ghsTiempoVisible = 3f;
    [SerializeField] private float ghsTiempoInvisible = 2f;
    [Tooltip("Segundos de parpadeo antes de desaparecer")]
    [SerializeField] private float ghsTiempoAviso = 0.5f;
    [SerializeField] private bool ghsIniciarVisible = true;
    [SerializeField] private bool ghsUsarParpadeo = true;

    [Header("DODGE")]
    [Tooltip("La plataforma se aleja del jugador al pisarla para confundirlo")]
    [SerializeField] private DireccionDodge dodDireccion = DireccionDodge.Derecha;
    [SerializeField] private float dodDistancia = 2f;
    [SerializeField] private float dodVelocidad = 4f;
    [SerializeField] private float dodTiempoEspera = 0.3f;
    [SerializeField] private float dodTiempoReaparicion = 2f;
    [SerializeField] private bool dodReaparecer = true;

    [Header("SEESAW")]
    [Tooltip("Plataforma tipo balancin que se inclina segun el peso del jugador")]
    [SerializeField] private float balAnguloMax = 25f;
    [SerializeField] private float balVelocidadInclinacion = 120f;
    [SerializeField] private float balVelocidadRetorno = 60f;
    [SerializeField] private float balRadioDeteccion = 1.5f;

    [Header("EMERGER")]
    [Tooltip("Trampa que emerge del suelo al detectar jugadores cercanos")]
    [SerializeField] private float emrRadioDeteccion = 3f;
    [SerializeField] private float emrDistanciaEmergencia = 2f;
    [SerializeField] private float emrVelocidadEmerger = 8f;
    [SerializeField] private float emrVelocidadRetraer = 4f;
    [SerializeField] private float emrTiempoEsperaArriba = 1f;
    [SerializeField] private float emrTiempoReaparicion = 3f;

    private SpriteRenderer sr;
    private Collider2D col2d;
    private Rigidbody2D rb2d;
    private Vector3 posOriginal;
    private Quaternion rotOriginal;
    private Vector3 escalaOriginal;

    private bool ocupado = false;
    private bool mvHaciaB = true;
    private Coroutine rutinaActiva;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col2d = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        posOriginal = transform.position;
        rotOriginal = transform.rotation;
        escalaOriginal = transform.localScale;

        ConfigurarRigidbody();
    }

    private void Start()
    {
        if (modo == PlatformMode.MovingPoint)
        {
            if (mvPuntoA != null && mvPuntoB != null)
            {
                transform.position = mvIniciarEnB ? mvPuntoB.position : mvPuntoA.position;
                mvHaciaB = !mvIniciarEnB;
            }
        }
        else if (modo == PlatformMode.Ghost)
        {
            IniciarRutina(GhsRutina());
        }
        else if (modo == PlatformMode.Emerger)
        {
            transform.position = posOriginal - Vector3.up * emrDistanciaEmergencia;
        }
    }

    private void ConfigurarRigidbody()
    {
        if (rb2d != null)
        {
            rb2d.bodyType = RigidbodyType2D.Kinematic;
            rb2d.gravityScale = 0f;
            rb2d.constraints = (modo == PlatformMode.Seesaw) ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeRotation;
            rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void Update()
    {
        if (modo == PlatformMode.PivotBounce) PvtDetectar();
        if (modo == PlatformMode.MovingPoint) MvUpdate();
        if (modo == PlatformMode.Trap) TraDetectar();
        if (modo == PlatformMode.Seesaw) BalUpdate();
        if (modo == PlatformMode.Emerger) EmrDetectar();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!EsJugador(collision.gameObject)) return;

        if (efectoSquash) IniciarSquash();

        switch (modo)
        {
            case PlatformMode.Collapse:
                if (!ocupado) { ocupado = true; IniciarRutina(ColRutina()); }
                break;
            case PlatformMode.Bounce:
                BncRebotar(collision);
                break;
            case PlatformMode.Dodge:
                if (!ocupado) { ocupado = true; IniciarRutina(DodRutina()); }
                break;
            case PlatformMode.PivotBounce:
                if (!ocupado) { ocupado = true; IniciarRutina(PvtRutina()); }
                break;
        }
    }

    // ====================== UTILIDADES ======================
    private bool EsJugador(GameObject obj)
    {
        return obj.CompareTag("Player") || obj.CompareTag("Player2");
    }

    private void IniciarRutina(IEnumerator rutina)
    {
        if (rutinaActiva != null) StopCoroutine(rutinaActiva);
        rutinaActiva = StartCoroutine(rutina);
    }

    private void IniciarSquash()
    {
        StartCoroutine(RutinaSquash());
    }

    private IEnumerator RutinaSquash()
    {
        Vector3 squashed = new Vector3(escalaOriginal.x * (1f + squashIntensidad), escalaOriginal.y * (1f - squashIntensidad), escalaOriginal.z);
        float tiempo = 0f;

        while (tiempo < squashDuracion)
        {
            transform.localScale = Vector3.Lerp(escalaOriginal, squashed, tiempo / squashDuracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        tiempo = 0f;
        while (tiempo < squashDuracion)
        {
            transform.localScale = Vector3.Lerp(squashed, escalaOriginal, tiempo / squashDuracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localScale = escalaOriginal;
    }

    // ====================== 1. COLLAPSE ======================
    private IEnumerator ColRutina()
    {
        float tiempo = colDuracionTemblor;
        while (tiempo > 0f)
        {
            if (colUsarTemblorNatural)
            {
                float offsetX = Mathf.Sin(Time.time * colFrecuenciaTemblor) * colIntensidadTemblor;
                float offsetY = Mathf.Cos(Time.time * colFrecuenciaTemblor * 1.3f) * colIntensidadTemblor * 0.5f;
                transform.position = posOriginal + new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                transform.position = posOriginal + Random.insideUnitSphere * colIntensidadTemblor;
            }
            tiempo -= Time.deltaTime;
            yield return null;
        }
        transform.position = posOriginal;

        float distanciaRecorrida = 0f;
        while (distanciaRecorrida < colDistanciaCaida)
        {
            float paso = colVelocidadCaida * Time.deltaTime;
            transform.position += Vector3.down * paso;
            distanciaRecorrida += paso;
            colVelocidadCaida += colVelocidadCaida * 0.5f * Time.deltaTime;
            yield return null;
        }

        if (sr != null) sr.enabled = false;
        if (col2d != null) col2d.enabled = false;

        yield return new WaitForSeconds(colTiempoReaparicion);

        transform.position = posOriginal;
        if (sr != null) sr.enabled = true;
        if (col2d != null) col2d.enabled = true;
        ocupado = false;
        colVelocidadCaida = Mathf.Max(colVelocidadCaida - colDistanciaCaida * 0.5f, 8f);
    }

    // ====================== 2. PIVOT BOUNCE ======================
    private void PvtDetectar()
    {
        if (ocupado) return;
        if (col2d == null || !col2d.enabled) return;

        Collider2D jugador = Physics2D.OverlapCircle(transform.position, pvtRadioDeteccion);
        if (jugador != null && EsJugador(jugador.gameObject))
        {
            ocupado = true;
            IniciarRutina(PvtRutina());
        }
    }

    private IEnumerator PvtRutina()
    {
        Quaternion rotObjetivo = Quaternion.Euler(0f, 0f, pvtAngulo);

        while (Quaternion.Angle(transform.rotation, rotObjetivo) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotObjetivo, pvtVelocidadLevantar * Time.deltaTime);
            yield return null;
        }
        transform.rotation = rotObjetivo;

        yield return new WaitForSeconds(pvtTiempoEspera);

        while (Quaternion.Angle(transform.rotation, rotOriginal) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotOriginal, pvtVelocidadBajar * Time.deltaTime);
            yield return null;
        }
        transform.rotation = rotOriginal;

        ocupado = false;
    }

    // ====================== 3. BOUNCE ======================
    private void BncRebotar(Collision2D collision)
    {
        Rigidbody2D rbJugador = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rbJugador == null) return;

        float dirX = 0f;
        if (bncBasarseEnLado)
            dirX = (collision.transform.position.x > transform.position.x) ? 1f : -1f;
        else
            dirX = bncHaciaDerecha ? 1f : -1f;

        Vector2 fuerza = new Vector2(bncFuerzaHorizontal * dirX, bncFuerzaVertical);

        if (rbJugador.linearVelocity.y < 0f)
            rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, 0f);

        rbJugador.AddForce(fuerza, ForceMode2D.Impulse);
    }

    // ====================== 4. MOVING POINT ======================
    private void MvUpdate()
    {
        if (mvPuntoA == null || mvPuntoB == null) return;

        Vector3 destino = mvHaciaB ? mvPuntoB.position : mvPuntoA.position;
        transform.position = Vector3.MoveTowards(transform.position, destino, mvVelocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino) < 0.05f)
        {
            transform.position = destino;
            mvHaciaB = !mvHaciaB;
        }
    }

    // ====================== 5. TRAP ======================
    private void TraDetectar()
    {
        if (ocupado) return;
        if (col2d == null || !col2d.enabled) return;

        Collider2D[] detectados = Physics2D.OverlapCircleAll(transform.position, traRadioDeteccion);
        foreach (Collider2D detected in detectados)
        {
            if (detected != null && EsJugador(detected.gameObject))
            {
                ocupado = true;
                float direccion;
                switch (traPrimerMovimiento)
                {
                    case DireccionTrap.Izquierda:
                        direccion = -1f;
                        break;
                    case DireccionTrap.Derecha:
                        direccion = 1f;
                        break;
                    default: // DetectarJugador
                        direccion = detected.transform.position.x > transform.position.x ? 1f : -1f;
                        break;
                }
                IniciarRutina(TraRutina(direccion));
                return;
            }
        }
    }

    private IEnumerator TraRutina(float direccion)
    {
        Vector3 destino = posOriginal + Vector3.right * direccion * traDistancia;

        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, traVelocidad * Time.deltaTime);
            yield return null;
        }
        transform.position = destino;

        Vector3 destinoRebote = posOriginal + Vector3.right * -direccion * traDistancia;

        while (Vector3.Distance(transform.position, destinoRebote) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destinoRebote, traVelocidad * 1.5f * Time.deltaTime);
            yield return null;
        }
        transform.position = destinoRebote;

        while (Vector3.Distance(transform.position, posOriginal) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, posOriginal, traVelocidad * Time.deltaTime);
            yield return null;
        }
        transform.position = posOriginal;

        if (!traReaparecer) yield break;
        yield return new WaitForSeconds(traTiempoReaparicion);
        ocupado = false;
    }

    // ====================== 6. GHOST ======================
    private IEnumerator GhsRutina()
    {
        bool visible = ghsIniciarVisible;
        float alfaOriginal = sr != null ? sr.color.a : 1f;

        while (true)
        {
            if (visible)
            {
                // Estado visible
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alfaOriginal;
                    sr.color = c;
                }
                if (col2d != null) col2d.enabled = true;

                // Parpadeo de aviso antes de desaparecer
                if (ghsUsarParpadeo && ghsTiempoAviso > 0f && sr != null)
                {
                    yield return new WaitForSeconds(Mathf.Max(0f, ghsTiempoVisible - ghsTiempoAviso));

                    float tiempo = 0f;
                    bool encendido = false;
                    while (tiempo < ghsTiempoAviso)
                    {
                        encendido = !encendido;
                        Color c = sr.color;
                        c.a = encendido ? alfaOriginal : alfaOriginal * 0.3f;
                        sr.color = c;
                        tiempo += 0.1f;
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(ghsTiempoVisible);
                }

                // Desaparecer
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 0f;
                    sr.color = c;
                }
                if (col2d != null) col2d.enabled = false;
                visible = false;
            }
            else
            {
                // Estado invisible
                yield return new WaitForSeconds(ghsTiempoInvisible);

                // Reaparecer
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = alfaOriginal;
                    sr.color = c;
                }
                if (col2d != null) col2d.enabled = true;
                visible = true;
            }
        }
    }

    // ====================== 7. DODGE ======================
    private IEnumerator DodRutina()
    {
        float dir = dodDireccion == DireccionDodge.Izquierda ? -1f : 1f;
        Vector3 destino = posOriginal + Vector3.right * dir * dodDistancia;

        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, dodVelocidad * Time.deltaTime);
            yield return null;
        }
        transform.position = destino;

        yield return new WaitForSeconds(dodTiempoEspera);

        while (Vector3.Distance(transform.position, posOriginal) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, posOriginal, dodVelocidad * 0.7f * Time.deltaTime);
            yield return null;
        }
        transform.position = posOriginal;

        if (!dodReaparecer) yield break;
        yield return new WaitForSeconds(dodTiempoReaparicion);
        ocupado = false;
    }

    // ====================== 8. SEESAW ======================
    private void BalUpdate()
    {
        float targetZ = 0f;
        bool jugadorEncima = false;

        if (col2d != null && col2d.enabled)
        {
            Collider2D[] detectados = Physics2D.OverlapCircleAll(transform.position, balRadioDeteccion);
            foreach (Collider2D detected in detectados)
            {
                if (detected != null && EsJugador(detected.gameObject))
                {
                    jugadorEncima = true;
                    float offset = detected.transform.position.x - transform.position.x;
                    float halfWidth = col2d.bounds.extents.x;
                    if (halfWidth > 0f)
                    {
                        float normalizedOffset = Mathf.Clamp(offset / halfWidth, -1f, 1f);
                        targetZ = -normalizedOffset * balAnguloMax;
                    }
                    break;
                }
            }
        }

        float speed = jugadorEncima ? balVelocidadInclinacion : balVelocidadRetorno;
        Quaternion rotObjetivo = Quaternion.Euler(0f, 0f, targetZ);
        Quaternion nuevaRot = Quaternion.RotateTowards(transform.rotation, rotObjetivo, speed * Time.deltaTime);
        if (rb2d != null)
            rb2d.MoveRotation(nuevaRot);
        else
            transform.rotation = nuevaRot;
    }

    // ====================== 9. EMERGER ======================
    private void EmrDetectar()
    {
        if (ocupado) return;
        if (col2d == null || !col2d.enabled) return;

        Collider2D[] detectados = Physics2D.OverlapCircleAll(transform.position, emrRadioDeteccion);
        foreach (Collider2D detected in detectados)
        {
            if (detected != null && EsJugador(detected.gameObject))
            {
                ocupado = true;
                IniciarRutina(EmrRutina());
                return;
            }
        }
    }

    private IEnumerator EmrRutina()
    {
        Vector3 posEscondida = posOriginal - Vector3.up * emrDistanciaEmergencia;

        while (Vector3.Distance(transform.position, posOriginal) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, posOriginal, emrVelocidadEmerger * Time.deltaTime);
            yield return null;
        }
        transform.position = posOriginal;

        yield return new WaitForSeconds(emrTiempoEsperaArriba);

        while (Vector3.Distance(transform.position, posEscondida) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, posEscondida, emrVelocidadRetraer * Time.deltaTime);
            yield return null;
        }
        transform.position = posEscondida;

        yield return new WaitForSeconds(emrTiempoReaparicion);
        ocupado = false;
    }

    // ====================== GIZMOS ======================
    private void OnDrawGizmosSelected()
    {
        switch (modo)
        {
            case PlatformMode.PivotBounce:
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, pvtRadioDeteccion);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(Quaternion.Euler(0, 0, pvtAngulo) * Vector3.right * 2f));
                break;
            case PlatformMode.MovingPoint:
                if (mvPuntoA != null && mvPuntoB != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(mvPuntoA.position, mvPuntoB.position);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(mvPuntoA.position, 0.2f);
                    Gizmos.DrawWireSphere(mvPuntoB.position, 0.2f);
                }
                break;
            case PlatformMode.Trap:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, traRadioDeteccion);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position - Vector3.right * traDistancia, transform.position + Vector3.right * traDistancia);
                break;
            case PlatformMode.Dodge:
                Gizmos.color = Color.magenta;
                float dirDod = dodDireccion == DireccionDodge.Izquierda ? -1f : 1f;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.right * dirDod * dodDistancia);
                break;
            case PlatformMode.Seesaw:
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, balRadioDeteccion);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(Quaternion.Euler(0, 0, -balAnguloMax) * Vector3.right * 2f));
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(Quaternion.Euler(0, 0, balAnguloMax) * Vector3.right * 2f));
                break;
            case PlatformMode.Emerger:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, emrRadioDeteccion);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.up * emrDistanciaEmergencia);
                break;
        }
    }
}