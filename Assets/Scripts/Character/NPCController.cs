using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] Sprite characterSprite;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;
    Healer healer;
    EquipoPokemon equipoPokemon;

    private void Awake()
    {
        character = GetComponent<Character>();
        healer = GetComponent<Healer>();
        equipoPokemon = FindObjectOfType<EquipoPokemon>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (healer != null && equipoPokemon != null)
        {
            healer.Heal(initiator, dialog, characterSprite, equipoPokemon);
        }

        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            yield return DialogManager.Instance.ShowDialog(dialog, characterSprite);

            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }

        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        yield return character.Move(movementPattern[currentPattern]);
        currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialog }
