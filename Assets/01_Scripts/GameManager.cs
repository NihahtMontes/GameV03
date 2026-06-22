using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuración de Respawn")]
    [SerializeField] private float tiempoParaRevivir = 5f; // Segundos que tarda en revivir

    // Guardaremos las posiciones iniciales donde arranca cada jugador en el nivel
    private Vector3 spawnPlayer1;
    private Vector3 spawnPlayer2;

    private Player scriptP1;
    private Player scriptP2;

    private bool gameOverActivado = false;

    private void Awake()
    {
        // Configuración para volverlo un Singleton eterno que sobrevive entre pantallas
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Evita que se borre al cambiar de escena
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // Le decimos a Unity que cada vez que cambie la escena, ejecute la función para buscar a los jugadores
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ¡NUEVO! Este método se activa automáticamente en cuanto se carga un nuevo nivel
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameOverActivado = false; // Reseteamos la bandera por si venimos de un reinicio

        // Buscamos y guardamos los nuevos puntos de inicio del Nivel 2, Nivel 3, etc.
        GameObject p1 = GameObject.FindWithTag("Player");
        GameObject p2 = GameObject.FindWithTag("Player2");

        if (p1 != null)
        {
            spawnPlayer1 = p1.transform.position;
            scriptP1 = p1.GetComponent<Player>();
        }
        if (p2 != null)
        {
            spawnPlayer2 = p2.transform.position;
            scriptP2 = p2.GetComponent<Player>();
        }
    }

    private void Start()
    {
        // Buscamos dinámicamente a ambos jugadores en la escena al iniciar
        GameObject p1 = GameObject.FindWithTag("Player");
        GameObject p2 = GameObject.FindWithTag("Player2");

        if (p1 != null)
        {
            spawnPlayer1 = p1.transform.position;
            scriptP1 = p1.GetComponent<Player>();
        }
        if (p2 != null)
        {
            spawnPlayer2 = p2.transform.position;
            scriptP2 = p2.GetComponent<Player>();
        }
    }

    /// <summary>
    /// Método que llama un jugador en el instante en que sus corazones llegan a 0
    /// </summary>
    public Vector3 GetSpawnPlayer1() => spawnPlayer1;
    public Vector3 GetSpawnPlayer2() => spawnPlayer2;

    private bool EsEscenaGriega()
    {
        string escena = SceneManager.GetActiveScene().name;
        return escena == "Griegos" || escena == "Griego2";
    }

    public void VerificarEstadoPartida()
    {
        if (gameOverActivado) return;

        bool p1Muerto = scriptP1 == null || scriptP1.EstaMuerto();
        bool p2Muerto = scriptP2 == null || scriptP2.EstaMuerto();

        Debug.Log($"VerificarEstadoPartida() - Escena: {SceneManager.GetActiveScene().name} | P1 Muerto: {p1Muerto} | P2 Muerto: {p2Muerto} | EsGriega: {EsEscenaGriega()}");

        if (EsEscenaGriega())
        {
            if (p1Muerto || p2Muerto)
            {
                Debug.Log("GAME OVER GRIEGO detectado! Iniciando corrutina...");
                gameOverActivado = true;
                StartCoroutine(GameOverGriegoCoroutine());
            }
        }
        else
        {
            if (p1Muerto && p2Muerto)
            {
                TriggerGameOver();
            }
        }
    }

    /// <summary>
    /// Inicia el temporizador invisible para revivir a un héroe específico
    /// </summary>
    public void SolicitarRespawn(GameObject jugador, string tagJugador)
    {
        if (gameOverActivado) return;
        StartCoroutine(RutinaRespawn(jugador, tagJugador));
    }

    private IEnumerator RutinaRespawn(GameObject jugador, string tagJugador)
    {
        yield return new WaitForSeconds(tiempoParaRevivir);

        // Si el otro jugador no murió en estos segundos y no es Game Over, revivimos al compañero
        if (!gameOverActivado && jugador != null)
        {
            Vector3 puntoSpawn = (tagJugador == "Player") ? spawnPlayer1 : spawnPlayer2;

            // Reubicamos al jugador en el inicio y lo restablecemos
            jugador.transform.position = puntoSpawn;

            Player scriptPlayer = jugador.GetComponent<Player>();
            if (scriptPlayer != null)
            {
                scriptPlayer.RevivirJugador();
            }

            Debug.Log("¡" + jugador.name + " ha revivido y regresó al combate!");
        }
    }

    private void TriggerGameOver()
    {
        gameOverActivado = true;
        Debug.Log("¡AMBOS JUGADORES HAN MUERTO! Reiniciando nivel actual...");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator GameOverGriegoCoroutine()
    {
        Debug.Log("GameOverGriegoCoroutine iniciada. Matando jugadores restantes...");

        if (scriptP1 != null && !scriptP1.EstaMuerto()) scriptP1.MorirInstantaneo();
        if (scriptP2 != null && !scriptP2.EstaMuerto()) scriptP2.MorirInstantaneo();

        yield return new WaitForSeconds(0.5f);

        Debug.Log("¡GAME OVER! Reiniciando escena Griegos...");
        SceneManager.LoadScene("Griegos");
    }
}