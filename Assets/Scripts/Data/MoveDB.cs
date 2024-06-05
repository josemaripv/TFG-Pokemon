using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB 
{
    static Dictionary<string, MovimientoBase> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MovimientoBase> ();

        var moveList = Resources.LoadAll<MovimientoBase>("");
        foreach ( var move in moveList )
        {
            if (moves.ContainsKey(move.Name))
            {
                Debug.LogError($"Hay dos movimientos con el nombre {move.Name}");
                continue;
            }

            moves[move.Name] = move;
        }
    }

    public static MovimientoBase GetMoveByName( string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Movimiento con nombre {name} no se encuentra en la bbdd");
            return null;
        }

        return moves[name]; 
    }
}
