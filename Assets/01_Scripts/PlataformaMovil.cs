using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Cuánto se mueve la plataforma al abrirse. Ej: (0,-5) baja 5 unidades")]
    [SerializeField] private Vector2 offsetAbierto = new Vector2(0f, -3f);
    [SerializeField] private float velocidadApertura = 3f;
    [SerializeField] private float velocidadCierre = 2f;

    [Header("Auto-Cierre")]
    [SerializeField] private bool autoCierre = true;
    [Tooltip("Segundos que permanece abierta antes de cerrarse sola")]
    [SerializeField] private float tiempoAutoCierre = 3f;

    private Vector3 posicionCerrada;
    private Vector3 posicionAbierta;
    private bool estaAbierta = false;
    private float timerAutoCierre = 0f;

    private void Awake()
    {
        posicionCerrada = transform.position;
        posicionAbierta = posicionCerrada + new Vector3(offsetAbierto.x, offsetAbierto.y, 0f);
    }

    private void Update()
    {
        Vector3 destino = estaAbierta ? posicionAbierta : posicionCerrada;
        float velocidad = estaAbierta ? velocidadApertura : velocidadCierre;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );

        if (estaAbierta && autoCierre)
        {
            timerAutoCierre -= Time.deltaTime;
            if (timerAutoCierre <= 0f)
            {
                Cerrar();
            }
        }
    }

    public void Abrir()
    {
        if (estaAbierta) return;
        estaAbierta = true;
        timerAutoCierre = tiempoAutoCierre;
    }

    public void Cerrar()
    {
        if (!estaAbierta) return;
        estaAbierta = false;
    }

    public bool EstaAbierta()
    {
        return estaAbierta;
    }
}
