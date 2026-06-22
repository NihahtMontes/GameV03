using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement Settings (Player 1)")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float climbSpeed = 2f;

    [Header("Movement Settings (Player 2)")]
    [SerializeField] private float moveSpeedP2 = 5f;
    [SerializeField] private float jumpForceP2 = 7f;
    [SerializeField] private float climbSpeedP2 = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Ladder Check")]
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private Vector2 ladderCheckSize = new Vector2(0.3f, 0.5f);

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Mago Settings (Exclusivo Player 2)")]
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float tiempoEntreDisparos = 0.6f;
    [SerializeField] private float retrasoDisparoMago = 0.25f;

    [Header("Health & UI Settings")]
    [SerializeField] private int vidaMax = 5;
    [SerializeField] private UI_Vida uiVida;
    [SerializeField] private float tiempoInvencibilidad = 1f;

    [Header("Control de Caída al Vacío")]
    [Tooltip("Segundos cayendo antes de morir automáticamente")]
    [SerializeField] private float tiempoMaxCaidaAlVacio = 2.0f;
    private float contadorTiempoCayendo = 0f;

    private bool esInvencible = false;
    private int vidaActual;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isNearLadder;
    private bool isClimbing;
    private float originalGravity;
    private bool estaMuerto = false;

    private float timerCooldownMago;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        originalGravity = rb.gravityScale;

        vidaActual = vidaMax;
    }

    private void Start()
    {
        if (uiVida == null)
        {
            UI_Vida[] todosLosUI = Object.FindObjectsByType<UI_Vida>(FindObjectsSortMode.None);
            foreach (UI_Vida hud in todosLosUI)
            {
                if (hud.TagAsociado == gameObject.tag)
                {
                    uiVida = hud;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (estaMuerto) return;

        if (timerCooldownMago > 0) timerCooldownMago -= Time.deltaTime;

        CheckSurroundings();
        ChequearCaidaAlVacio(); // Monitorea si el personaje cayó al foso
        MovementInput();
        Jump();
        ClimbInput();
        Attack();
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return;

        float velocidadActual = gameObject.CompareTag("Player") ? moveSpeed : moveSpeedP2;
        float velocidadEscaladaActual = gameObject.CompareTag("Player") ? climbSpeed : climbSpeedP2;

        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(horizontalInput * velocidadActual, verticalInput * velocidadEscaladaActual);
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * velocidadActual, rb.linearVelocity.y);
        }
    }

    private bool EsEscenaGriega()
    {
        string escena = SceneManager.GetActiveScene().name;
        return escena == "Griegos" || escena == "Griego2";
    }

    private void ChequearCaidaAlVacio()
    {
        if (rb.linearVelocity.y < -1f && !isClimbing)
        {
            contadorTiempoCayendo += Time.deltaTime;

            if (contadorTiempoCayendo >= tiempoMaxCaidaAlVacio)
            {
                contadorTiempoCayendo = 0f;

                if (EsEscenaGriega())
                {
                    RecibirDanio(1);
                    if (!estaMuerto) RespawnInmediato();
                }
                else
                {
                    Debug.Log(gameObject.name + " cayó al vacío del foso y murió.");
                    Morir();
                }
            }
        }
        else
        {
            contadorTiempoCayendo = 0f;
        }
    }

    private void CheckSurroundings()
    {
        Vector2 groundPosition = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position - new Vector2(0, 0.5f);
        isGrounded = Physics2D.OverlapBox(groundPosition, groundCheckSize, 0f, groundLayer);
        isNearLadder = Physics2D.OverlapBox((Vector2)transform.position, ladderCheckSize, 0f, ladderLayer);

        if (isClimbing)
        {
            anim.SetBool("EnSuelo", true);
        }
        else
        {
            anim.SetBool("EnSuelo", isGrounded);
        }

        if (!isNearLadder && isClimbing)
        {
            StopClimbing();
        }
    }

    private void MovementInput()
    {
        if (Keyboard.current == null) return;

        horizontalInput = 0f;

        if (gameObject.CompareTag("Player"))
        {
            if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;
        }
        else if (gameObject.CompareTag("Player2"))
        {
            if (Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        }

        anim.SetFloat("Velocidad", Mathf.Abs(horizontalInput));

        if (!isClimbing)
        {
            if (horizontalInput < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (horizontalInput > 0f) transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void ClimbInput()
    {
        if (Keyboard.current == null) return;

        verticalInput = 0f;

        if (gameObject.CompareTag("Player"))
        {
            if (Keyboard.current.wKey.isPressed) verticalInput = 1f;
            if (Keyboard.current.sKey.isPressed) verticalInput = -1f;
        }
        else if (gameObject.CompareTag("Player2"))
        {
            if (Keyboard.current.upArrowKey.isPressed) verticalInput = 1f;
            if (Keyboard.current.downArrowKey.isPressed) verticalInput = -1f;
        }

        if (isNearLadder && Mathf.Abs(verticalInput) > 0f)
        {
            isClimbing = true;
        }

        if (isGrounded && verticalInput < 0f && isClimbing)
        {
            StopClimbing();
        }

        if (isClimbing)
        {
            rb.gravityScale = 0f;
            anim.SetBool("Escalando", true);

            if (Mathf.Abs(verticalInput) > 0f || Mathf.Abs(horizontalInput) > 0f)
            {
                anim.speed = 0.5f;
            }
            else
            {
                anim.speed = 0f;
            }
        }
        else
        {
            anim.speed = 1f;
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = originalGravity;
        anim.SetBool("Escalando", false);
    }

    private void Jump()
    {
        if (Keyboard.current == null) return;

        bool quiereSaltar = false;

        if (gameObject.CompareTag("Player"))
        {
            quiereSaltar = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        else if (gameObject.CompareTag("Player2"))
        {
            quiereSaltar = Keyboard.current.upArrowKey.wasPressedThisFrame;
        }

        if (quiereSaltar && (isGrounded || isClimbing))
        {
            StopClimbing();
            float fuerzaSaltoActual = gameObject.CompareTag("Player") ? jumpForce : jumpForceP2;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSaltoActual);

            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.jump);
        }
    }

    public void Attack()
    {
        if (isClimbing) return;

        bool quiereAtacar = false;

        if (Keyboard.current != null)
        {
            // --- CONTROLES NUEVOS DE ATAQUE EN TECLADO ---
            if (gameObject.CompareTag("Player"))
            {
                quiereAtacar = Keyboard.current.gKey.wasPressedThisFrame; // Samurái ataca con la G
            }
            else if (gameObject.CompareTag("Player2"))
            {
                quiereAtacar = Keyboard.current.lKey.wasPressedThisFrame; // Mago ataca con la L
            }
        }

        if (quiereAtacar)
        {
            // --- SAMURÁI (Melee) ---
            if (gameObject.CompareTag("Player"))
            {
                anim.SetTrigger("Ataque");

                if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.espadaEfecto);

                Vector2 attackPosition = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, -0.1f);
                Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

                foreach (Collider2D enemigo in enemigosGolpeados)
                {
                    EnemigoMomia momia = enemigo.GetComponent<EnemigoMomia>();
                    if (momia != null) momia.RecibirDanio(attackDamage);
                }
            }
            // --- MAGO (Distancia) --- ¡Respeta su cadencia original perfectamente!
            else if (gameObject.CompareTag("Player2"))
            {
                if (timerCooldownMago <= 0)
                {
                    anim.SetTrigger("Ataque");

                    StartCoroutine(RutinaDisparoMago());

                    timerCooldownMago = tiempoEntreDisparos;
                }
            }
        }
    }

    private System.Collections.IEnumerator RutinaDisparoMago()
    {
        yield return new WaitForSeconds(retrasoDisparoMago);

        if (!estaMuerto)
        {
            DispararProyectilMago();
        }
    }

    public void DispararProyectilMago()
    {
        if (prefabProyectil != null)
        {
            Vector3 origenDisparo = puntoDisparo != null ? puntoDisparo.position : transform.position + new Vector3(transform.localScale.x * 0.4f, 0f, 0f);
            GameObject proyectilClon = Instantiate(prefabProyectil, origenDisparo, Quaternion.identity);

            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.balaDelMago);

            Proyectil scriptProyectil = proyectilClon.GetComponent<Proyectil>();
            if (scriptProyectil != null)
            {
                scriptProyectil.IniciarDireccion(transform.localScale.x);
            }
        }
    }

    public void RecibirDanio(int cantidadDanio)
    {
        if (esInvencible || vidaActual <= 0 || estaMuerto) return;

        vidaActual -= cantidadDanio;

        if (uiVida != null) uiVida.ActualizarVidaUI(vidaActual);

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.danioJugador);

        if (vidaActual <= 0) Morir();
        else StartCoroutine(RutinaInvencibilidad());
    }

    private System.Collections.IEnumerator RutinaInvencibilidad()
    {
        esInvencible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float tiempoPorParpadeo = tiempoInvencibilidad / 6f;

        for (int i = 0; i < 3; i++)
        {
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(tiempoPorParpadeo);
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f);
            yield return new WaitForSeconds(tiempoPorParpadeo);
        }

        esInvencible = false;
    }

    public bool EstaMuerto()
    {
        return estaMuerto;
    }

    public void MorirInstantaneo()
    {
        if (estaMuerto) return;
        vidaActual = 0;
        if (uiVida != null) uiVida.ActualizarVidaUI(0);
        Morir();
    }

    private void Morir()
    {
        estaMuerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        if (spriteRenderer != null) spriteRenderer.enabled = false;

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.died);

        Debug.Log($"[Player.Morir] {gameObject.name} ha muerto. Llamando GameManager...");

        GameManager gm = GameManager.instance;
        if (gm == null)
        {
            gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                Debug.LogWarning($"[Player.Morir] {gameObject.name} - GameManager.instance era NULL, encontrado via FindFirstObjectByType.");
            }
        }

        if (gm != null)
        {
            gm.VerificarEstadoPartida();
            gm.SolicitarRespawn(gameObject, gameObject.tag);
        }
        else
        {
            Debug.LogError($"[Player.Morir] {gameObject.name} - No hay GameManager en la escena!");
        }
    }

    public void RevivirJugador()
    {
        estaMuerto = false;
        vidaActual = 3;

        if (uiVida != null)
        {
            uiVida.ActualizarVidaUI(vidaActual);
        }
        else
        {
            UI_Vida[] todosLosUI = Object.FindObjectsByType<UI_Vida>(FindObjectsSortMode.None);
            foreach (UI_Vida hud in todosLosUI)
            {
                if (hud.TagAsociado == gameObject.tag)
                {
                    uiVida = hud;
                    break;
                }
            }
            if (uiVida != null) uiVida.ActualizarVidaUI(vidaActual);
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        GetComponent<Collider2D>().enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        StartCoroutine(RutinaInvencibilidad());
    }

    public void RespawnInmediato()
    {
        Vector3 spawn = Vector3.zero;
        bool spawnEncontrado = false;

        if (GameManager.instance != null)
        {
            spawn = (gameObject.tag == "Player")
                ? GameManager.instance.GetSpawnPlayer1()
                : GameManager.instance.GetSpawnPlayer2();
            spawnEncontrado = true;
            Debug.Log($"[RespawnInmediato] {gameObject.name} teletransportando a: {spawn}");
        }
        else
        {
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                spawn = (gameObject.tag == "Player")
                    ? gm.GetSpawnPlayer1()
                    : gm.GetSpawnPlayer2();
                spawnEncontrado = true;
                Debug.Log($"[RespawnInmediato] {gameObject.name} teletransportando a: {spawn} (via FindFirstObjectByType)");
            }
            else
            {
                spawn = transform.position;
                Debug.LogWarning($"[RespawnInmediato] {gameObject.name} - No hay GameManager! Respawn en posición actual.");
            }
        }

        if (spawnEncontrado)
        {
            transform.position = spawn;
        }

        rb.linearVelocity = Vector2.zero;
        contadorTiempoCayendo = 0f;

        StartCoroutine(RutinaInvencibilidad());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 groundPosition = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position - new Vector2(0, 0.5f);
        Gizmos.DrawWireCube(groundPosition, groundCheckSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position, ladderCheckSize);

        Gizmos.color = Color.yellow;
        Vector2 attackPos = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, -0.1f);
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }
}