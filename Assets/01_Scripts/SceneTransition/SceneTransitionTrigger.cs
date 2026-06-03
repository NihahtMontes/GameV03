using UnityEngine;

/// <summary>
/// Trigger que inicia el scroll de cámara y teleport del player.
/// Se coloca en un GameObject con BoxCollider2D (IsTrigger = true).
/// Todos los parámetros son configurables desde el Inspector.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("=== CONFIGURACIÓN DE SCROLL ===")]
    [Tooltip("Velocidad horizontal de la cámara durante el scroll.")]
    [SerializeField] private float scrollSpeed = 3f;

    [Tooltip("Posición X FINAL donde la cámara se detiene (DEBE ser MAYOR que Teleport X).")]
    [SerializeField] private float limitX = 70f;

    [Tooltip("Tiempo en segundos desde que empieza el scroll hasta que el player se teletransporta.")]
    [SerializeField] private float teleportDelay = 2f;

    [Header("=== CONFIGURACIÓN DE TELEPORT ===")]
    [Tooltip("Posición X donde aparecerá el player después del teleport (DEBE ser MENOR que Limit X).")]
    [SerializeField] private float teleportX = 50f;

    [Tooltip("Posición Y donde aparecerá el player después del teleport.")]
    [SerializeField] private float teleportY = -1.28f;

    [Header("=== CONFIGURACIÓN DE OFFSET DE CÁMARA ===")]
    [Tooltip("Offset en X respecto al player cuando la cámara vuelve a seguirlo después del scroll.")]
    [SerializeField] private float cameraOffsetX = 0f;

    [Tooltip("Offset en Y respecto al player cuando la cámara vuelve a seguirlo después del scroll.")]
    [SerializeField] private float cameraOffsetY = 0f;

    [Header("=== REFERENCIAS (NO MODIFICAR) ===")]
    [Tooltip("Referencia al controlador de freeze del player.")]
    [SerializeField] private PlayerFreezeController playerFreeze;

    [Tooltip("Referencia al scroller de cámara.")]
    [SerializeField] private CameraScroller cameraScroller;

    private void Start()
    {
        // Buscar referencias si no están asignadas desde el Inspector
        if (cameraScroller == null)
        {
            cameraScroller = FindFirstObjectByType<CameraScroller>();
        }

        if (playerFreeze == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerFreeze = player.GetComponent<PlayerFreezeController>();
            }
        }

        // Validación de configuración
        if (cameraScroller == null)
        {
            Debug.LogError("[SceneTransitionTrigger] No se encontró CameraScroller en la escena.");
        }

        if (limitX <= teleportX)
        {
            Debug.LogWarning($"[SceneTransitionTrigger] ATENCIÓN: Limit X ({limitX}) debe ser MAYOR que Teleport X ({teleportX}). " +
                             "La cámara no llegará más allá del player. Ajusta los valores.");
        }
    }

    /// <summary>
    /// Detecta cuando el player entra en el trigger.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo reaccionar al player
        if (!collision.CompareTag("Player")) return;

        Debug.Log($"[SceneTransitionTrigger] Player detectado. Scroll: X={limitX}, Teleport: ({teleportX}, {teleportY}) en {teleportDelay}s | Offset: ({cameraOffsetX}, {cameraOffsetY})");

        // Obtener referencias si no las teníamos
        if (playerFreeze == null)
        {
            playerFreeze = collision.GetComponent<PlayerFreezeController>();
        }

        if (playerFreeze == null)
        {
            Debug.LogError("[SceneTransitionTrigger] El player no tiene PlayerFreezeController.");
            return;
        }

        // Congelar al player inmediatamente
        playerFreeze.Freeze();

        // Iniciar el scroll de la cámara con los offsets
        if (cameraScroller != null)
        {
            cameraScroller.StartScroll(limitX, scrollSpeed, teleportDelay, teleportX, teleportY, cameraOffsetX, cameraOffsetY);
        }
        else
        {
            Debug.LogError("[SceneTransitionTrigger] CameraScroller no disponible.");
        }
    }

    /// <summary>
    /// Método público para cambiar el límite de scroll dinámicamente.
    /// </summary>
    public void SetLimitX(float newLimitX)
    {
        limitX = newLimitX;
    }

    /// <summary>
    /// Método público para cambiar la posición de teleport dinámicamente.
    /// </summary>
    public void SetTeleportPosition(float newTeleportX, float newTeleportY)
    {
        teleportX = newTeleportX;
        teleportY = newTeleportY;
    }

    /// <summary>
    /// Método público para cambiar los offsets de cámara dinámicamente.
    /// </summary>
    public void SetCameraOffset(float newOffsetX, float newOffsetY)
    {
        cameraOffsetX = newOffsetX;
        cameraOffsetY = newOffsetY;
    }
}