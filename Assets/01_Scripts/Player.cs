using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings (Player 1)")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float climbSpeed = 2f;

    [Header("Movement Settings (Player 2)")]
    [SerializeField] private float moveSpeedP2 = 5f;
    [SerializeField] private float jumpForceP2 = 7f; // Ajustada más baja por defecto para que no vuele
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
    [SerializeField] private int vidaMax = 5;
    [SerializeField] private UI_Vida uiVida;
    [SerializeField] private float tiempoInvencibilidad = 1f;

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
    private bool estaMuerto = false; // Variable de control para evitar errores en Static

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
        // Buscamos todas las barras de vida en la escena
        UI_Vida[] todasLasUIVidas = Object.FindObjectsByType<UI_Vida>(FindObjectsSortMode.None);

        // Cada jugador se conecta solo con la interfaz que tenga su mismo Tag
        foreach (UI_Vida ui in todasLasUIVidas)
        {
            if (ui.TagAsociado == gameObject.tag)
            {
                uiVida = ui;
                break;
            }
        }

        if (uiVida == null)
        {
            Debug.LogWarning("No se encontró un HUD de UI_Vida configurado para el tag: " + gameObject.tag);
        }
    }
    private void Update()
    {
        if (estaMuerto) return; // Si está muerto, bloquea todo el procesamiento

        CheckSurroundings();
        MovementInput();
        Jump();
        ClimbInput();
        Attack();
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return; // SOLUCIÓN AL ERROR EN STATIC: Detiene las físicas si ya murió

        // Asignación dinámica de velocidad según el Tag
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

            // Elegimos la fuerza de salto correcta dependiendo del Tag del personaje
            float fuerzaSaltoActual = gameObject.CompareTag("Player") ? jumpForce : jumpForceP2;

            // CORRECCIÓN fÍSICA: Modificamos el eje Y de forma directa y mantenemos la velocidad en X
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSaltoActual);
        }
    }

    public void Attack()
    {
        if (isClimbing) return;

        bool quiereAtacar = false;

        if (gameObject.CompareTag("Player"))
        {
            if (Mouse.current != null) quiereAtacar = Mouse.current.leftButton.wasPressedThisFrame;
        }
        else if (gameObject.CompareTag("Player2"))
        {
            if (Keyboard.current != null) quiereAtacar = Keyboard.current.kKey.wasPressedThisFrame;
        }

        if (quiereAtacar)
        {
            anim.SetTrigger("Ataque");

            Vector2 attackPosition = (Vector2)transform.position + new Vector2(transform.localScale.x * 0.5f, -0.1f);
            Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(attackPosition, attackRange, enemyLayer);

            foreach (Collider2D enemigo in enemigosGolpeados)
            {
                EnemigoMomia momia = enemigo.GetComponent<EnemigoMomia>();
                if (momia != null)
                {
                    momia.RecibirDanio(attackDamage);
                }
            }
        }
    }

    public void RecibirDanio(int cantidadDanio)
    {
        if (esInvencible || vidaActual <= 0 || estaMuerto) return;

        vidaActual -= cantidadDanio;

        if (uiVida != null)
        {
            uiVida.ActualizarVidaUI(vidaActual);
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(RutinaInvencibilidad());
        }
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

    private void Morir()
    {
        estaMuerto = true; // Bloquea los updates inmediatamente
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;

        Debug.Log(gameObject.name + " ha caído en batalla.");
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