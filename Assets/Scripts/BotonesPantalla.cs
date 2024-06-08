using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotonesPantalla : MonoBehaviour
{
    [SerializeField] Button salirDelJuegoButton; // Aseg�rate de asignar el bot�n desde el editor

    void Start()
    {
        // Agrega un manejador de eventos al bot�n para detectar el clic
        salirDelJuegoButton.onClick.AddListener(SalirDelJuego);
    }

    public void SalirDelJuego()
    {
        // Llama a la funci�n que cierra el juego
        Application.Quit();
    }
}