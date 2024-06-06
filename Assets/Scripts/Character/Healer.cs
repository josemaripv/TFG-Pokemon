using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public void Heal(Transform initiator, Dialog dialog, Sprite characterSprite, EquipoPokemon equipoPokemon)
    {
        // Aquí podrías agregar un diálogo o efecto visual si lo deseas
        // Por simplicidad, vamos a imprimir un mensaje en la consola
        Debug.Log("¡Tus Pokémon han sido curados!");

        // Acceder al equipo Pokémon del jugador y curar a todos los Pokémon
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