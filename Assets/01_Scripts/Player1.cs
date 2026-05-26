using UnityEngine;
using UnityEngine.InputSystem;

public class Player1 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [Tooltip("Arrastra aqui un objeto hijo ubicado en los pies del personaje. Si es null, usara la posicion del personaje - 0.5f en Y")]
    [SerializeField] private Transform groundCheckPoint;
    [Tooltip("Tamaño del area de deteccion de suelo")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [Tooltip("Capa del suelo. DEBES asignarla en el Inspector!")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Distancia del raycast alternativo (si no usas groundCheckPoint)")]
    [SerializeField] private float raycastDistance = 0.6f;
    [Tooltip("Mostrar logs de debug en consola")]
    [SerializeField] private bool showDebugLogs = true;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private bool isGrounded;
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            Debug.LogError("[Player1] ERROR: No se encontro Rigidbody2D. Agregalo al personaje.");
            enabled = false;
            return;
        }

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // ADVERTENCIA CRITICA: Verificar que groundLayer tenga algo asignado
        if (groundLayer == 0)
        {
            Debug.LogWarning("[Player1] ADVERTENCIA: Ground Layer no esta asignada en el Inspector! El suelo no se detectara. Selecciona la capa del suelo en el campo 'Ground Layer'.");
        }

        // Verificar que groundCheckPoint existe
        if (groundCheckPoint == null)
        {
            Debug.LogWarning("[Player1] ADVERTENCIA: Ground Check Point no esta asignado. Se usara la posicion del personaje. Crea un objeto hijo en los pies y arrastralo aqui para mejor precision.");
        }
    }

    private void Update()
    {
        CheckGround();
        HandleMovementInput();
        HandleJumpInput();
        HandleAttackInput();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyJump();
    }

    private void HandleMovementInput()
    {
        if (Keyboard.current == null) return;

        horizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;

        // Voltear sprite segun direccion
        if (horizontalInput < 0f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (horizontalInput > 0f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void HandleJumpInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            jumpPressed = true;
            if (showDebugLogs) Debug.Log("[Player1] Salto presionado y detectado suelo -> Saltando!");
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame && !isGrounded && showDebugLogs)
        {
            Debug.Log("[Player1] Salto presionado PERO NO HAY SUELO (isGrounded = false). No salta.");
        }
    }

    private void ApplyMovement()
    {
        // Usamos velocity en lugar de linearVelocity para compatibilidad con versiones antiguas
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void ApplyJump()
    {
        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        anim.SetFloat("Velocidad", Mathf.Abs(horizontalInput));
        anim.SetBool("EnSuelo", isGrounded);
    }

    private void CheckGround()
    {
        // Metodo 1: OverlapBox (el original)
        Vector2 checkPosition = groundCheckPoint != null
            ? (Vector2)groundCheckPoint.position
            : (Vector2)transform.position - new Vector2(0, 0.5f);

        isGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);

        // Metodo 2: Raycast de respaldo (mas confiable en algunos casos)
        // Dispara un rayo hacia abajo desde el centro del personaje
        if (!isGrounded)
        {
            Vector2 rayOrigin = (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, raycastDistance, groundLayer);
            isGrounded = hit.collider != null;

            if (showDebugLogs && hit.collider != null)
            {
                Debug.Log($"[Player1] Raycast detecto suelo en: {hit.collider.name}");
            }
        }

        if (showDebugLogs && !isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[Player1] isGrounded = FALSE. Verifica que:\n" +
                     "1. Los objetos del suelo tengan un Collider2D\n" +
                     "2. Los objetos del suelo esten en la capa seleccionada en 'Ground Layer'\n" +
                     "3. El Ground Check Point este en los pies del personaje\n" +
                     "4. En Scene view activa Gizmos para ver el area de deteccion (rojo)");
        }
    }

    private void HandleAttackInput()
    {
        if (Mouse.current == null || anim == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[Player1] Ataque ejecutado");
            anim.SetTrigger("Ataque");
        }
    }

    // Visualizacion en el editor del area de ground check
    private void OnDrawGizmosSelected()
    {
        // Dibujar el OverlapBox
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector2 checkPosition = groundCheckPoint != null
            ? (Vector2)groundCheckPoint.position
            : (Vector2)transform.position - new Vector2(0, 0.5f);
        Gizmos.DrawWireCube(checkPosition, groundCheckSize);

        // Dibujar el Raycast
        Gizmos.color = Color.blue;
        Vector2 rayOrigin = (Vector2)transform.position;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * raycastDistance);
    }

    private void OnGUI()
    {
        // Mostrar estado en pantalla para debug (solo en Play mode)
        if (!Application.isPlaying) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = isGrounded ? Color.green : Color.red;

        string status = isGrounded ? "GROUNDED: SI" : "GROUNDED: NO";
        GUI.Label(new Rect(10, 10, 200, 30), status, style);
    }
}
