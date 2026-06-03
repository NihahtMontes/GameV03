using UnityEngine;

/// <summary>
/// Controlador que congela y descongela al player durante las transiciones de escena.
/// Se coloca en el mismo GameObject que el player (o referenciado desde él).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFreezeController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al Rigidbody2D del player. Se auto-asigna si está en el mismo objeto.")]
    [SerializeField] private Rigidbody2D rb;

    // Estado público para que otros scripts puedan consultar si el player está congelado
    public bool IsFrozen { get; private set; } = false;

    // Variables para guardar el estado original antes de congelar
    private Vector2 savedVelocity;
    private float savedGravityScale;
    private bool hasSavedState = false;

    private void Awake()
    {
        // Si no se asignó desde el Inspector, intenta obtenerlo del mismo GameObject
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            Debug.LogError("[PlayerFreezeController] No se encontró Rigidbody2D. Asigna uno desde el Inspector o coloca este script en el player.");
        }
    }

    /// <summary>
    /// Congela al player: detiene su movimiento, gravedad e input.
    /// Guarda el estado actual para poder restaurarlo después.
    /// </summary>
    public void Freeze()
    {
        if (rb == null || IsFrozen) return;

        // Guardar estado actual
        savedVelocity = rb.linearVelocity;
        savedGravityScale = rb.gravityScale;
        hasSavedState = true;

        // Detener completamente al player
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        IsFrozen = true;

        Debug.Log("[PlayerFreezeController] Player congelado.");
    }

    /// <summary>
    /// Descongela al player: restaura gravedad y reactiva el input.
    /// </summary>
    public void Unfreeze()
    {
        if (rb == null || !IsFrozen) return;

        // Restaurar gravedad
        if (hasSavedState)
        {
            rb.gravityScale = savedGravityScale;
        }

        // Limpiar velocidad para evitar movimientos inesperados al descongelar
        rb.linearVelocity = Vector2.zero;

        IsFrozen = false;
        hasSavedState = false;

        Debug.Log("[PlayerFreezeController] Player descongelado.");
    }
}
