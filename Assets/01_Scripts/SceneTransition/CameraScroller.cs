using UnityEngine;

/// <summary>
/// Controla el movimiento de la cámara: scroll horizontal durante transiciones
/// y seguimiento suave del player durante gameplay normal.
/// Se coloca en la cámara principal.
/// 
/// FLUJO:
/// 1. StartScroll() → la cámara empieza a scrollear hacia limitX
/// 2. Después de teleportDelay → player se teleporta a (teleportX, teleportY)
/// 3. La cámara CONTINÚA scrolleando hacia limitX (que es más allá del player)
/// 4. Cámara llega a limitX → deja de scrollear, aplica offset personalizado, vuelve a seguir al player
/// </summary>
public class CameraScroller : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN DE FOLLOW ===")]
    [Tooltip("El target que la cámara seguirá cuando no esté en scroll.")]
    [SerializeField] private Transform playerTarget;

    [Tooltip("Offset en X respecto al player.")]
    [SerializeField] private float offsetX = 0f;

    [Tooltip("Offset en Y respecto al player.")]
    [SerializeField] private float offsetY = 0f;

    [Tooltip("Velocidad de seguimiento suave. Mayor = más rápido.")]
    [SerializeField] private float followSpeed = 5f;

    [Header("=== ESTADO (SOLO LECTURA) ===")]
    [Tooltip("¿La cámara está siguiendo al player?")]
    [SerializeField] private bool isFollowing = true;

    [Tooltip("¿La cámara está en modo scroll?")]
    [SerializeField] private bool isScrollingState = false;

    // Estado del scroll
    private float targetLimitX;
    private float scrollSpeed;
    private float teleportDelay;
    private float teleportX;
    private float teleportY;
    private float scrollStartTime;
    private bool hasPlayerTeleported = false;

    // Offset personalizado para después del scroll
    private float customOffsetX;
    private float customOffsetY;
    private bool hasCustomOffset = false;

    // Referencia al player
    private GameObject playerGameObject;

    private void Awake()
    {
        // Buscar el player automáticamente si no está asignado desde el Inspector
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                playerGameObject = player;
            }
        }
        else
        {
            // Si playerTarget fue asignado desde el Inspector, obtener el GameObject
            playerGameObject = playerTarget.gameObject;
        }
    }

    private void Update()
    {
        // Si no estamos en scroll y tenemos target, seguir al player suavemente
        if (!isScrollingState && playerTarget != null)
        {
            FollowPlayer();
        }

        // Si estamos en scroll, procesar el scroll
        if (isScrollingState)
        {
            ProcessScroll();
        }
    }

    /// <summary>
    /// Sigue al player suavemente usando Lerp.
    /// </summary>
    private void FollowPlayer()
    {
        // Calcular posición objetivo
        float targetX = playerTarget.position.x + offsetX;
        float targetY = playerTarget.position.y + offsetY;

        // Posición actual de la cámara
        float currentX = transform.position.x;
        float currentY = transform.position.y;

        // Suavizar el movimiento con Lerp
        float newX = Mathf.Lerp(currentX, targetX, followSpeed * Time.deltaTime);
        float newY = Mathf.Lerp(currentY, targetY, followSpeed * Time.deltaTime);

        // Aplicar posición (mantener Z constante)
        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    /// <summary>
    /// Procesa el scroll horizontal de la cámara.
    /// </summary>
    private void ProcessScroll()
    {
        // Mover la cámara hacia el límite
        float currentX = transform.position.x;
        float newX = Mathf.MoveTowards(currentX, targetLimitX, scrollSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Verificar si es hora de hacer el teleport Y todavía no se ha hecho
        float elapsedTime = Time.time - scrollStartTime;

        if (!hasPlayerTeleported && elapsedTime >= teleportDelay)
        {
            PerformTeleport();
        }

        // Verificar si llegamos al límite para terminar
        // IMPORTANTE: Solo terminar si el player YA fue teletransportado
        if (Mathf.Abs(newX - targetLimitX) < 0.05f)
        {
            // Si el player aún no se ha teleportado y la cámara llegó al límite,
            // esperar al teleport antes de terminar
            if (hasPlayerTeleported)
            {
                FinishScroll();
            }
            else
            {
                // La cámara llegó al límite pero el player aún no se teleporta.
                // Esperar... el teleport ocurrirá en el siguiente frame si ya pasó el delay
            }
        }
    }

    /// <summary>
    /// Inicia el scroll horizontal de la cámara.
    /// </summary>
    /// <param name="limitX">Posición X donde la cámara se detiene AL FINAL (debe ser MAYOR que teleportX).</param>
    /// <param name="speed">Velocidad de movimiento horizontal.</param>
    /// <param name="delay">Tiempo en segundos hasta que ocurre el teleport.</param>
    /// <param name="teleportPosX">Posición X donde aparecerá el player después del teleport (debe ser MENOR que limitX).</param>
    /// <param name="teleportPosY">Posición Y donde aparecerá el player después del teleport.</param>
    /// <param name="camOffsetX">Offset en X que se aplicará cuando la cámara vuelva a seguir al player.</param>
    /// <param name="camOffsetY">Offset en Y que se aplicará cuando la cámara vuelva a seguir al player.</param>
    public void StartScroll(float limitX, float speed, float delay, float teleportPosX, float teleportPosY, float camOffsetX = 0f, float camOffsetY = 0f)
    {
        if (isScrollingState)
        {
            Debug.LogWarning("[CameraScroller] Ya hay un scroll en progreso. Ignorando.");
            return;
        }

        // Buscar el player si no lo tenemos
        if (playerGameObject == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerGameObject = player;
                if (playerTarget == null) playerTarget = player.transform;
            }
        }

        targetLimitX = limitX;
        scrollSpeed = speed;
        teleportDelay = delay;
        teleportX = teleportPosX;
        teleportY = teleportPosY;
        scrollStartTime = Time.time;
        hasPlayerTeleported = false;
        isScrollingState = true;
        isFollowing = false;

        // Guardar los offsets personalizados (si son 0, se usan los default del Inspector)
        if (camOffsetX != 0f || camOffsetY != 0f)
        {
            customOffsetX = camOffsetX;
            customOffsetY = camOffsetY;
            hasCustomOffset = true;
        }
        else
        {
            // Usar los offsets por defecto del Inspector
            customOffsetX = offsetX;
            customOffsetY = offsetY;
            hasCustomOffset = false;
        }

        Debug.Log($"[CameraScroller] Scroll iniciado → LimitX: {limitX}, Teleport: ({teleportPosX}, {teleportPosY}) en {delay}s | Offset: ({customOffsetX}, {customOffsetY})");
    }

    /// <summary>
    /// Teletransporta al player a la posición configurada.
    /// </summary>
    private void PerformTeleport()
    {
        hasPlayerTeleported = true;

        if (playerGameObject != null)
        {
            // Teletransportar al player a la posición del trigger
            playerGameObject.transform.position = new Vector3(teleportX, teleportY, playerGameObject.transform.position.z);
            Debug.Log($"[CameraScroller] Player teletransportado a ({teleportX}, {teleportY})");

            // La cámara NO se mueve aquí - sigue scrolleando hacia limitX
        }
        else
        {
            Debug.LogError("[CameraScroller] No se encontró el player para teleport.");
        }
    }

    /// <summary>
    /// Termina el scroll y vuelve a seguir al player con el offset configurado.
    /// </summary>
    private void FinishScroll()
    {
        isScrollingState = false;
        isFollowing = true;

        Debug.Log($"[CameraScroller] Scroll terminado. Volviendo a seguir al player con offset ({customOffsetX}, {customOffsetY})");

        // Aplicar los offsets al CameraScroller para que FollowPlayer los use
        offsetX = customOffsetX;
        offsetY = customOffsetY;

        // Descongelar al player
        if (playerGameObject != null)
        {
            PlayerFreezeController freezeController = playerGameObject.GetComponent<PlayerFreezeController>();

            if (freezeController != null)
            {
                freezeController.Unfreeze();
            }
        }

        // Resetear estado
        hasPlayerTeleported = false;
        hasCustomOffset = false;
    }

    /// <summary>
    /// Fuerza que la cámara se reposicione sobre el player con el offset actual.
    /// Útil al cargar una nueva escena.
    /// </summary>
    public void SnapToPlayer()
    {
        if (playerTarget != null)
        {
            transform.position = new Vector3(
                playerTarget.position.x + offsetX,
                playerTarget.position.y + offsetY,
                transform.position.z
            );
        }
    }

    /// <summary>
    /// Permite cambiar el target de follow dinámicamente.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        playerTarget = newTarget;
        if (newTarget != null)
        {
            playerGameObject = newTarget.gameObject;
        }
    }

    /// <summary>
    /// Permite cambiar los offsets dinámicamente.
    /// </summary>
    public void SetOffset(float newOffsetX, float newOffsetY)
    {
        offsetX = newOffsetX;
        offsetY = newOffsetY;
    }

    // Getters para el estado actual
    public bool IsFollowing => isFollowing;
    public bool IsScrolling => isScrollingState;
}