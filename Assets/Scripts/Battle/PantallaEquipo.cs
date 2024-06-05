using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PantallaEquipo : MonoBehaviour
{

    [SerializeField] TMP_Text messageText;

    MiembrosEquipoUI[] memberSlots;
    List<Pokemon> pokemons;
    public void Init()
    {
        memberSlots = GetComponentsInChildren<MiembrosEquipoUI>();

    }

    public void SetPartyData(List<Pokemon> pokemons) 
    {
        this.pokemons = pokemons;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
                memberSlots[i].SetData(pokemons[i]);
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Elige un Pokemon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0;i < pokemons.Count;i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

}
