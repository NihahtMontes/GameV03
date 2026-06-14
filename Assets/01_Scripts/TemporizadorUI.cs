using System.Collections;
using UnityEngine;

public class TemporizadorUI : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoEnPantalla = 5f; // Segundos que durará activo

    private void Start()
    {
        // Iniciamos la cuenta regresiva en cuanto el mapa carga en pantalla
        StartCoroutine(RutinaDesactivar());
    }

    private IEnumerator RutinaDesactivar()
    {
        // Espera los segundos configurados (5f)
        yield return new WaitForSeconds(tiempoEnPantalla);

        // Apaga el objeto por completo en la jerarquía
        gameObject.SetActive(false);

        Debug.Log(gameObject.name + " se ha ocultado automáticamente.");
    }
}