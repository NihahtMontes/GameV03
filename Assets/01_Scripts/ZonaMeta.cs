using UnityEngine;
using UnityEngine.SceneManagement;

public class ZonaMeta : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool requiereAmbosJugadores = true;

    private bool p1Dentro = false;
    private bool p2Dentro = false;
    private bool transicionando = false;

    private bool AmbosJugadoresVivos()
    {
        GameObject p1 = GameObject.FindWithTag("Player");
        GameObject p2 = GameObject.FindWithTag("Player2");
        if (p1 == null || p2 == null) return false;

        Player scriptP1 = p1.GetComponent<Player>();
        Player scriptP2 = p2.GetComponent<Player>();
        if (scriptP1 == null || scriptP2 == null) return false;

        return !scriptP1.EstaMuerto() && !scriptP2.EstaMuerto();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (transicionando) return;

        if (collision.CompareTag("Player")) p1Dentro = true;
        if (collision.CompareTag("Player2")) p2Dentro = true;

        if (!requiereAmbosJugadores || (p1Dentro && p2Dentro && AmbosJugadoresVivos()))
        {
            transicionando = true;
            int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(siguiente);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) p1Dentro = false;
        if (collision.CompareTag("Player2")) p2Dentro = false;
    }
}
