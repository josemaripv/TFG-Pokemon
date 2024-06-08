using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotonesPantalla : MonoBehaviour
{
    [SerializeField] Button salirDelJuegoButton; // Asegúrate de asignar el botón desde el editor

    void Start()
    {
        // Agrega un manejador de eventos al botón para detectar el clic
        salirDelJuegoButton.onClick.AddListener(SalirDelJuego);
    }

    public void SalirDelJuego()
    {
        // Llama a la función que cierra el juego
        Application.Quit();
    }
}