using UnityEngine;

public class ObstaculoDanio : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private int cantidadDanio = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[ObstaculoDanio] Colisión con: {collision.gameObject.name} | Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Player2"))
        {
            Player jugador = collision.gameObject.GetComponent<Player>();
            if (jugador != null)
            {
                Debug.Log($"[ObstaculoDanio] Jugador encontrado. Muerto: {jugador.EstaMuerto()}");

                if (!jugador.EstaMuerto())
                {
                    jugador.RecibirDanio(cantidadDanio);
                    Debug.Log($"[ObstaculoDanio] Daño aplicado. Ahora muerto: {jugador.EstaMuerto()}");

                    if (!jugador.EstaMuerto())
                    {
                        Debug.Log("[ObstaculoDanio] Respawn inmediato...");
                        jugador.RespawnInmediato();
                    }
                    else
                    {
                        Debug.Log("[ObstaculoDanio] Jugador murió, GameManager debería reiniciar.");
                    }
                }
            }
        }
    }
}
