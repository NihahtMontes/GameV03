using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Efectos de Sonido (AudioClips)")]
    public AudioClip balaDelMago;
    [Range(0f, 1f)] public float volumenMago = 1f;

    public AudioClip danioEnemigo;
    [Range(0f, 1f)] public float volumenEnemigo = 1f;

    public AudioClip danioJugador;
    [Range(0f, 1f)] public float volumenJugador = 1f;

    public AudioClip died;
    [Range(0f, 1f)] public float volumenDied = 1f;

    public AudioClip espadaEfecto;
    [Range(0f, 1f)] public float volumenEspada = 1f;

    public AudioClip jump;
    [Range(0f, 1f)] public float volumenJump = 1f;

    public AudioClip recoger_llave;
    [Range(0f, 1f)] public float volumenLlave = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        float volumenFinal = 1f;
        bool amplificarSuperFuerte = false;
        bool boostExclusivoMago = false; // Bandera para identificar el disparo del mago

        // Asignamos el volumen correspondiente según el clip recibido
        if (clip == balaDelMago)
        {
            volumenFinal = volumenMago;
            boostExclusivoMago = true; // Activamos el identificador para la bala mágica
        }
        else if (clip == danioEnemigo) volumenFinal = volumenEnemigo;
        else if (clip == danioJugador) volumenFinal = volumenJugador;
        else if (clip == died) volumenFinal = volumenDied;
        else if (clip == espadaEfecto) volumenFinal = volumenEspada;
        else if (clip == jump) volumenFinal = volumenJump;
        else if (clip == recoger_llave) volumenFinal = volumenLlave;

        // Si en el Inspector pusiste el slider al máximo (1.0), activamos la amplificación
        if (volumenFinal >= 1f)
        {
            amplificarSuperFuerte = true;
        }

        // 1. Reproducción de la onda base original
        audioSource.PlayOneShot(clip, volumenFinal);

        // 2. ¡SÚPER AMPLIFICACIÓN GENERAL! (Ráfaga doble si está al máximo)
        if (amplificarSuperFuerte)
        {
            audioSource.PlayOneShot(clip, volumenFinal);
        }

        // 3. ¡BOOST EXCLUSIVO DEL MAGO! Si es su disparo, le encimamos una tercera onda para triplicar la potencia
        if (boostExclusivoMago && volumenFinal >= 1f)
        {
            audioSource.PlayOneShot(clip, volumenFinal);
        }
    }
}