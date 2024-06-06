using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipoPokemon : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
        set { pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }


    public void PartyUpdated()
    {
        OnUpdated?.Invoke();
    }
    public Pokemon GetHealthyPokemon() 
    { 
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    
}
