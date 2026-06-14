using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Clonación")]
    [SerializeField] private GameObject enemigoPrefab; // Arrastra el Prefab de la Momia aquí
    [SerializeField] private Transform[] puntosSpawn;   // Lista de tus puntos de aparición

    [Header("Tiempos y Límites")]
    [SerializeField] private float tiempoEntreSpawns = 4f; // Cada cuántos segundos aparece uno nuevo
    [SerializeField] private int maxEnemigosEnPantalla = 5; // Cuántos enemigos puede haber como máximo a la vez
    [SerializeField] private int totalEnemigosNivel = 15;   // Límite absoluto de oleada para este nivel (0 = infinito)


    private List<GameObject> enemigosVivos = new List<GameObject>();
    private int enemigosGeneradosTotales = 0;
    private bool sePuedeSpawnear = true;
    private bool llaveAparecida = false; // Control interno para que la llave se encienda una sola vez

    private void Start()
    {
        if (puntosSpawn.Length > 0 && enemigoPrefab != null)
        {
            StartCoroutine(RutinaSpawn());
        }
    }

    private void Update()
    {
        // Limpiamos constantemente la lista eliminando las momias muertas o caídas al foso
        enemigosVivos.RemoveAll(item => item == null);

        // CONDICIÓN DE VICTORIA: Si ya no se pueden generar más Y no queda ninguna momia viva en el mapa
        if (!sePuedeSpawnear && enemigosVivos.Count == 0 && !llaveAparecida)
        {
            llaveAparecida = true;
            AparecerLlave();
        }
    }

    private IEnumerator RutinaSpawn()
    {
        while (sePuedeSpawnear)
        {
            yield return new WaitForSeconds(tiempoEntreSpawns);

            // COMPROBACIONES: Que no supere el límite en pantalla y que no supere la oleada total del nivel
            bool cumpleLimitePantalla = enemigosVivos.Count < maxEnemigosEnPantalla;
            bool cumpleLimiteTotal = totalEnemigosNivel <= 0 || enemigosGeneradosTotales < totalEnemigosNivel;

            if (cumpleLimitePantalla && cumpleLimiteTotal)
            {
                SpawnearEnemigo();
            }

            // Si ya generamos todos los enemigos configurados para el nivel, rompemos el bucle
            if (totalEnemigosNivel > 0 && enemigosGeneradosTotales >= totalEnemigosNivel)
            {
                sePuedeSpawnear = false;
            }
        }
    }

    private void SpawnearEnemigo()
    {
        // Elige un punto de la lista de forma aleatoria
        int indiceAleatorio = Random.Range(0, puntosSpawn.Length);
        Transform puntoElegido = puntosSpawn[indiceAleatorio];

        // Clona la momia en la posición y rotación del punto elegido
        GameObject nuevoEnemigo = Instantiate(enemigoPrefab, puntoElegido.position, puntoElegido.rotation);

        // Lo añadimos a la lista de control
        enemigosVivos.Add(nuevoEnemigo);
        enemigosGeneradosTotales++;
    }

    private void AparecerLlave()
    {
        // Busca la llave que está desactivada (oculta) en la escena y la enciende
        ItemLlave llaveEnEscena = Object.FindFirstObjectByType<ItemLlave>(FindObjectsInactive.Include);

        if (llaveEnEscena != null)
        {
            llaveEnEscena.gameObject.SetActive(true);
            Debug.Log("¡Piso limpio de momias! La llave animada ha aparecido en el mapa.");
        }
        else
        {
            Debug.LogWarning("Oleada completada, pero no se encontró ningún objeto con el script ItemLlave en la jerarquía.");
        }
    }
}