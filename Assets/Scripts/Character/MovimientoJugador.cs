using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MovimientoJugador : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    

    private Vector2 inputDirection;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            inputDirection.x = Input.GetAxisRaw("Horizontal");
            inputDirection.y = Input.GetAxisRaw("Vertical");

            // Asegurarse de que no se puede mover en diagonal
            if (inputDirection.x != 0)
                inputDirection.y = 0;

            if (inputDirection != Vector2.zero)
            {
                StartCoroutine(character.Move(inputDirection, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(Interact());
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position , 0.2f, GameLayers.i.TriggerableLayers);

        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<PlayerTriggerable>();
            if (triggerable != null)
            {
                character.Animator.IsMoving = true;
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<EquipoPokemon>().Pokemons.Select(p => p.GetSaveData()).ToList()
        };
        
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData) state;

        var pos = saveData.position;    
        transform.position = new Vector3(pos[0], pos[1]);

        GetComponent<EquipoPokemon>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}
