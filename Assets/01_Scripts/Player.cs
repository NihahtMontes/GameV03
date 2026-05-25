using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nos aseguramos de congelar la rotación física en Z
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        CheckGround();
        MovementInput();
        Jump();
        Attack();
    }

    private void FixedUpdate()
    {
        // Aplicamos movimiento lateral manteniendo la caída física
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void CheckGround()
    {
        // Si tu personaje no tiene un punto "groundCheckPoint", usará su propia posición (los pies)
        Vector2 checkPosition = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position - new Vector2(0, 0.5f);

        // Detecta si hay colisión con la capa del suelo
        isGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);

        // Pasamos el dato directamente al Animator
        anim.SetBool("EnSuelo", isGrounded);
    }

    private void MovementInput()
    {
        if (Keyboard.current == null) return;

        horizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;

        // Cambiado a "Velocidad" (sin la X) para que no te dé el error de antes
        anim.SetFloat("Velocidad", Mathf.Abs(horizontalInput));

        // --- NUEVO SISTEMA PARA VOLTEAR TODO EL OBJETO ---
        if (horizontalInput < 0f)
        {
            // Mira a la izquierda rotando la escala en X a negativo
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (horizontalInput > 0f)
        {
            // Mira a la derecha restaurando la escala a positivo
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void Jump()
    {
        if (Keyboard.current == null) return;

        // Si presionas Espacio y el script detecta que estás pisando el suelo
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void Attack()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("aver si sale esta confirmacion dx");
            anim.SetTrigger("Ataque");
        }
    }

    // Dibuja una cajita roja en el editor para que veas dónde detecta el suelo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 checkPosition = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position - new Vector2(0, 0.5f);
        Gizmos.DrawWireCube(checkPosition, groundCheckSize);
    }
}