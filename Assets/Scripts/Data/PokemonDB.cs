using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB 
{
    static Dictionary<string, PokemonBase> pokemons;

    public static void Init()
    {

        pokemons = new Dictionary<string, PokemonBase>();

        var pokemonArray = Resources.LoadAll<PokemonBase>("");
        foreach (var pokemon in pokemonArray)
        {
            if (pokemons.ContainsKey(pokemon.Name))
            {
                Debug.LogError($"Hay dos pokemons con el nombre {pokemon.Name}");
                continue;
            }


            pokemons[pokemon.Name] = pokemon;   
        }
    }

    public static PokemonBase GetPokemonByName (string name)
    {
        if (!pokemons.ContainsKey(name))
        {
            Debug.LogError($"El pokemon con nombre {name} no se encuentra en la bbdd");
            return null;
        }

        return pokemons[name];
    }
}
