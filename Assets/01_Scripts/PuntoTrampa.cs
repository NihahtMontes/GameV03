using UnityEngine;

public class PuntoTrampa : MonoBehaviour
{
    [Header("Proyectil a Spawnear")]
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private Transform puntoSpawn;

    [Header("Waypoints (Puntos Vacíos)")]
    [SerializeField] private Transform[] waypoints;

    [Header("Cooldown")]
    [SerializeField] private float tiempoCooldown = 3f;

    private bool enCooldown = false;
    private float timerCooldown = 0f;

    private void Update()
    {
        if (enCooldown)
        {
            timerCooldown -= Time.deltaTime;
            if (timerCooldown <= 0f)
            {
                enCooldown = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enCooldown) return;
        if (!collision.CompareTag("Player") && !collision.CompareTag("Player2")) return;
        if (prefabProyectil == null) return;
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 spawnPos = puntoSpawn != null ? puntoSpawn.position : transform.position;
        GameObject proyectil = Instantiate(prefabProyectil, spawnPos, Quaternion.identity);

        ProyectilMovil scriptProyectil = proyectil.GetComponent<ProyectilMovil>();
        if (scriptProyectil != null)
        {
            scriptProyectil.SetWaypoints(waypoints);
        }

        enCooldown = true;
        timerCooldown = tiempoCooldown;
    }
}
