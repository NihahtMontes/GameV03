using UnityEngine;
using UnityEngine.SceneManagement; // Línea obligatoria para poder cambiar de escenas

public class MainMenuController : MonoBehaviour
{
    // Este método lo llamará el botón Play
    public void PlayGame()
    {
        // "SampleScene" debe ser el nombre exacto de la escena de tu juego
        SceneManager.LoadScene("SampleScene");
    }

    // Este método lo llamará el botón Exit (Salir)
    public void ExitGame()
    {
        Debug.Log("El jugador cerró el juego.");
        Application.Quit(); // Cierra el juego (funciona solo en el juego ya compilado/exportado)
    }
}