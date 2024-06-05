using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LongGrass : MonoBehaviour, PlayerTriggerable
{
    public void OnPlayerTriggered(MovimientoJugador player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            
            GameController.Instance.StartBattle();
        }
    }
}
