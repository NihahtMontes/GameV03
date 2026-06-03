using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager singleton para manejar la carga de escenas y el posicionamiento del player.
/// Persiste entre escenas con DontDestroyOnLoad.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    // --- Patrón Singleton ---
    public static SceneTransitionManager Instance { get; private set; }

    // Valores pendientes para la transición actual
    private string pendingSceneName;
    private Vector2 pendingSpawnPosition;
    private bool hasPendingTransition = false;

    // Referencias cacheadas (se buscan una sola vez al cargar escena)
    private Camera mainCamera;

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Inicia la carga de una nueva escena guardando el punto de spawn para cuando cargue.
    /// </summary>
    /// <param name="sceneName">Nombre de la escena a cargar.</param>
    /// <param name="spawnPosition">Posición donde aparecerá el player en la nueva escena.</param>
    public void LoadScene(string sceneName, Vector2 spawnPosition)
    {
        pendingSceneName = sceneName;
        pendingSpawnPosition = spawnPosition;
        hasPendingTransition = true;

        Debug.Log($"[SceneTransitionManager] Cargando escena: {sceneName} | Spawn: {spawnPosition}");

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Se ejecuta automáticamente cuando una escena termina de cargar.
    /// Posiciona al player, ajusta la cámara y descongela al player.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasPendingTransition) return;

        Debug.Log($"[SceneTransitionManager] Escena cargada: {scene.name}. Aplicando spawn y cámara...");

        // 1. Buscar al player por tag
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            // Posicionar al player en el punto de spawn
            player.transform.position = new Vector3(pendingSpawnPosition.x, pendingSpawnPosition.y, player.transform.position.z);

            // 2. Buscar y ajustar la cámara principal
            mainCamera = Camera.main;

            if (mainCamera != null)
            {
                // Posicionar la cámara alineada con el spawn del player
                mainCamera.transform.position = new Vector3(
                    pendingSpawnPosition.x, 
                    pendingSpawnPosition.y, 
                    -10f
                );
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] No se encontró Camera.main en la nueva escena.");
            }

            // 3. Descongelar al player
            PlayerFreezeController freezeController = player.GetComponent<PlayerFreezeController>();

            if (freezeController != null)
            {
                freezeController.Unfreeze();
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] El player no tiene PlayerFreezeController. No se pudo descongelar.");
            }
        }
        else
        {
            Debug.LogError("[SceneTransitionManager] No se encontró GameObject con tag 'Player' en la nueva escena.");
        }

        // Limpiar valores pendientes
        hasPendingTransition = false;
        pendingSceneName = string.Empty;
    }
}
