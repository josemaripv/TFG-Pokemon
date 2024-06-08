using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movimiento
{
    public MovimientoBase movimientoBase { get; set; }

    public int PP { get; set; }

    public Movimiento(MovimientoBase baseMovimiento)
    {
        movimientoBase = baseMovimiento;
        PP = baseMovimiento.PP;
    }

    public Movimiento(MoveSaveData saveData)
    {
        movimientoBase = MoveDB.GetMoveByName(saveData.name);
        PP = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = movimientoBase.Name,
            pp = PP
        };
        return saveData;
    }

    // Método para restaurar los PP del movimiento al máximo disponible
    public void RestorePP()
    {
        PP = movimientoBase.PP;
    }
}

[Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
