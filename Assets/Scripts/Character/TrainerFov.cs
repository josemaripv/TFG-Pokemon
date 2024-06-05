using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, PlayerTriggerable
{
    public void OnPlayerTriggered(MovimientoJugador player)
    {
        GameController.Instance.OnEnterTrainersView(GetComponentInParent<TrainerController>());
    }
}
