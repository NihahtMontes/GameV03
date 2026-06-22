using UnityEngine;

public class ProyectilMovil : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;
    [SerializeField] private float distanciaLlegada = 0.1f;

    [Header("Rotación (Disco)")]
    [SerializeField] private float velocidadRotacion = 360f;
    [SerializeField] private Vector3 ejeRotacion = new Vector3(0f, 0f, 1f);

    private Transform[] waypoints;
    private int indiceWaypointActual = 0;
    private bool estaActivo = false;

    public void SetWaypoints(Transform[] nuevosWaypoints)
    {
        waypoints = nuevosWaypoints;
        indiceWaypointActual = 0;
        estaActivo = true;
    }

    private void Update()
    {
        if (!estaActivo || waypoints == null || waypoints.Length == 0) return;

        transform.Rotate(ejeRotacion * velocidadRotacion * Time.deltaTime);

        Transform objetivo = waypoints[indiceWaypointActual];
        if (objetivo == null)
        {
            DestruirProyectil();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            objetivo.position,
            velocidadMovimiento * Time.deltaTime
        );

        float distancia = Vector3.Distance(transform.position, objetivo.position);
        if (distancia <= distanciaLlegada)
        {
            indiceWaypointActual++;

            if (indiceWaypointActual >= waypoints.Length)
            {
                DestruirProyectil();
            }
        }
    }

    private void DestruirProyectil()
    {
        Destroy(gameObject);
    }
}
