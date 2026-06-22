using UnityEngine;

public class BotonPlataforma : MonoBehaviour
{
    [Header("Sprites del Botón")]
    [SerializeField] private SpriteRenderer spriteRendererBoton;
    [SerializeField] private Sprite spriteApagado;
    [SerializeField] private Sprite spritePrendido;

    [Header("Plataforma a Controlar")]
    [SerializeField] private PlataformaMovil plataformaObjetivo;

    private void Update()
    {
        if (plataformaObjetivo == null || spriteRendererBoton == null) return;

        bool abierta = plataformaObjetivo.EstaAbierta();
        spriteRendererBoton.sprite = abierta ? spritePrendido : spriteApagado;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.CompareTag("Player2")) return;
        if (plataformaObjetivo == null) return;

        plataformaObjetivo.Abrir();
    }
}
