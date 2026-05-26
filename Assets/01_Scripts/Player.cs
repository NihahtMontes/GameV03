using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float climbSpeed = 2f; // Reducida a 2f para que suba más lento y real

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Ladder Check")]
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private Vector2 ladderCheckSize = new Vector2(0.3f, 0.5f);

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isNearLadder;
    private bool isClimbing;
    private float originalGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        originalGravity = rb.gravityScale;
    }

    private void Update()
    {
        CheckSurroundings();
        MovementInput();
        Jump();
        ClimbInput();
        Attack();
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, verticalInput * climbSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void CheckSurroundings()
    {
        // 1. Detectar si físicamente hay suelo
        Vector2 groundPosition = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position - new Vector2(0, 0.5f);
        isGrounded = Physics2D.OverlapBox(groundPosition, groundCheckSize, 0f, groundLayer);

        // 2. Detectar si hay escalera
        isNearLadder = Physics2D.OverlapBox((Vector2)transform.position, ladderCheckSize, 0f, ladderLayer);

        // --- SOLUCIÓN ANTIMANCHAS DE SALTO ---
        // Si está escalando, engañamos al Animator diciéndole que está en el suelo.
        // Esto evita por completo que las transiciones de Any State activen el "samurai_jump".
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
        if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;

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
        if (Keyboard.current.wKey.isPressed) verticalInput = 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput = -1f;

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

            // Si se mueve en la escalera, la animación corre normal
            if (Mathf.Abs(verticalInput) > 0f || Mathf.Abs(horizontalInput) > 0f)
            {
                anim.speed = 0.5f;
            }
            else
            {
                // Si se detiene, congelamos la animación exactamente en el frame de espalda
                // en el que se encuentra dentro del estado loop.
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

        if (Keyboard.current.spaceKey.wasPressedThisFrame && (isGrounded || isClimbing))
        {
            StopClimbing();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void Attack()
    {
        if (Mouse.current == null || isClimbing) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            anim.SetTrigger("Ataque");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 groundPosition = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position - new Vector2(0, 0.5f);
        Gizmos.DrawWireCube(groundPosition, groundCheckSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position, ladderCheckSize);
    }
}