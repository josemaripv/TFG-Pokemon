using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Menu, PartyScreen, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] MovimientoJugador playerController;
    [SerializeField] SistemaBatalla battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PantallaEquipo partyScreen;

    GameState state;

    GameState stateBeforePause;

    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        PokemonDB.Init();
        MoveDB.Init();
        CondicionesDB.Init();
    }

    private void Start()
    {
        battleSystem.onBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<EquipoPokemon>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<EquipoPokemon>();
        var trainerParty = trainer.GetComponent<EquipoPokemon>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () => { };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }

        // Verificar si todos los Pokémon del jugador están fainted
        if (state != GameState.Battle && AllPlayerPokemonFainted())
        {
            // Terminar el juego
            EndGame();
        }
    }

    // Método para verificar si todos los Pokémon del jugador están fainted
    private bool AllPlayerPokemonFainted()
    {
        var playerParty = playerController.GetComponent<EquipoPokemon>().Pokemons;
        foreach (var pokemon in playerParty)
        {
            if (pokemon.HP > 0)
            {
                // Al menos un Pokémon tiene HP mayor que 0, el juego no ha terminado
                return false;
            }
        }
        // Todos los Pokémon están fainted
        return true;
    }

    // Método para terminar el juego
    private void EndGame()
    {
        // Cargar la escena de Game Over
        SceneManager.LoadScene("GameOver");
    }


    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            partyScreen.gameObject.SetActive(true);
            partyScreen.SetPartyData(playerController.GetComponent<EquipoPokemon>().Pokemons);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 2)
        {
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }
}
