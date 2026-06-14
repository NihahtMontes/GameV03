using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Este método lo llamará el botón Play en el Menú Principal
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Este método lo llamará el botón Exit en el Menú Principal
    public void ExitGame()
    {
        Debug.Log("El jugador cerró el juego.");
        Application.Quit();
    }

    // ¡NUEVO MÉTODO! Lo llamará tu botón en la Pantalla de Victoria
    public void VolverAlMenu()
    {
        Debug.Log("Regresando al menú principal...");
        // "MainMenu" debe ser el nombre exacto de la escena de tu menú
        SceneManager.LoadScene("MainMenu");
    }
}