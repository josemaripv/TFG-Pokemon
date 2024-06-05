using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CondicionesDB 
{

    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    
    public static Dictionary<ConditionID, Condiciones> Conditions { get; set; } = new Dictionary<ConditionID, Condiciones>()
    {
        {
            ConditionID.psn,
            new Condiciones()
            {
                Name = "Poison",
                StartMessage = "Ha sido envenedado",
                OnAfterTurn = (Pokemon Pokemon) =>
                {
                    Pokemon.UpdateHP(Pokemon.MaxHp / 8);
                    Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} ha sido herido debido al veneno");
                }
            }
        },
        {
            ConditionID.brn,
            new Condiciones()
            {
                Name = "Burn",
                StartMessage = "Ha sido quemado",
                OnAfterTurn = (Pokemon Pokemon) =>
                {
                    Pokemon.UpdateHP(Pokemon.MaxHp / 12);
                    Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} ha sido herido debido al quemado");
                }
            }
        },
        {
            ConditionID.par,
            new Condiciones()
            {
                Name = "Paralyzed",
                StartMessage = "Ha sido paralizado",
                OnBeforeMove = (Pokemon Pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                         
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} esta paralizado y no puede moverse");
                        return false;
                    }
                   return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condiciones()
            {
                Name = "Freeze",
                StartMessage = "Ha sido congelado",
                OnBeforeMove = (Pokemon Pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        Pokemon.CureStatus();
                        Pokemon.StatusChanges.Enqueue($"{Pokemon.Base.Name} ya no esta congelado");
                        return true;
                    }
                   return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condiciones()
            {
                Name = "Freeze",
                StartMessage = "ha sido dormido",
                OnStart = (Pokemon Pokemon) =>
                {
                   Pokemon.StatusTime = Random.Range(1, 4);
                   Debug.Log($"Will be asleep for {Pokemon.StatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} se ha despertado!");
                        return true;

                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} esta durmiendo");
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condiciones()
            {
                Name = "Confusion",
                StartMessage = "ha sido confundido",
                OnStart = (Pokemon Pokemon) =>
                {
                   Pokemon.VolatileStatusTime = Random.Range(1, 4);
                   Debug.Log($"Will be confused for {Pokemon.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"¡{pokemon.Base.Name} ya no esta confuso!");
                        return true;

                    }

                    pokemon.VolatileStatusTime--;

                    if (Random.Range(1, 3) == 1)
                        return true;

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} esta confundido");
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} se ha golpeado a si mismo");
                    return false;
                }
            }
        }
    };
    
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
