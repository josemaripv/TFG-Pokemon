using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public void Heal(Transform initiator, Dialog dialog, Sprite characterSprite, EquipoPokemon equipoPokemon)
    {
        // Aqu� podr�as agregar un di�logo o efecto visual si lo deseas
        // Por simplicidad, vamos a imprimir un mensaje en la consola
        Debug.Log("�Tus Pok�mon han sido curados!");

        // Acceder al equipo Pok�mon del jugador y curar a todos los Pok�mon
        equipoPokemon = initiator.GetComponent<EquipoPokemon>();
        if (equipoPokemon != null)
        {
            foreach (Pokemon pokemon in equipoPokemon.Pokemons)
            {
                pokemon.Heal();
            }
        }
    }
}