using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum EstadoBatalla { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class SistemaBatalla : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] DialogosCombate dialogBox;
    [SerializeField] PantallaEquipo partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> onBattleOver;

    EstadoBatalla state;
    
    int currentAction;
    int currentMove;

    bool aboutToUseChoice = true;

    EquipoPokemon playerParty;
    EquipoPokemon trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    MovimientoJugador player;
    TrainerController trainer;

    int escapeAttempts;

    private AudioManager audioManager;


    public void StartBattle(EquipoPokemon playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        isTrainerBattle = false;
        AudioManager.Instance.PlayBattleMusic();
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(EquipoPokemon playerParty, EquipoPokemon trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<MovimientoJugador>();
        trainer = trainerParty.GetComponent<TrainerController>();

        AudioManager.Instance.PlayBattleMusic();
        StartCoroutine(SetupBattle());
    }


    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // Setup Player and Enemy Units for Wild Pokemon Battle
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"¡Un {enemyUnit.Pokemon.Base.Name} salvaje ha aparecido!");
        }
        else
        {
            // Setup for Trainer Battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} te reta a un combate");

            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} envia a {enemyPokemon.Base.Name}");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"¡Adelante {playerPokemon.Base.Name}!");
            dialogBox.SetMovesNames(playerUnit.Pokemon.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void ActionSelection()
    {
        state = EstadoBatalla.ActionSelection;
        dialogBox.SetDialog("Elige una accion");
        dialogBox.EnableActionSelector(true);
    }

    void BattleOver(bool won)
    {
        state = EstadoBatalla.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        onBattleOver(won);
        AudioManager.Instance.PlayMusicForCurrentScene(); // Agregar esta línea para volver a la música de la escena actual

    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = EstadoBatalla.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = EstadoBatalla.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }



    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = EstadoBatalla.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.movimientoBase.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.movimientoBase.Priority;

            bool playerGoesFirts = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirts = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirts = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firtsUnit = (playerGoesFirts) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirts) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            yield return RunMove(firtsUnit, secondUnit, firtsUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firtsUnit);
            if (state == EstadoBatalla.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                yield return RunMove(secondUnit, firtsUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == EstadoBatalla.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = EstadoBatalla.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == EstadoBatalla.BattleOver) yield break;
        }

        if (state != EstadoBatalla.BattleOver)
            ActionSelection();
    }


    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Movimiento move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);


        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} ha usado {move.movimientoBase.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.movimientoBase.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.movimientoBase.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.movimientoBase.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.movimientoBase.Secondaries != null && move.movimientoBase.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.movimientoBase.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} ha sido derrotado");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);


            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} ha fallado el ataque");

        }

        
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if (effects.status != ConditionID.none)
        {
            target.SetStatus(effects.status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == EstadoBatalla.BattleOver) yield break;
        yield return new WaitUntil(() => state == EstadoBatalla.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} ha sido derrotado");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
            yield return new WaitUntil(() => state == EstadoBatalla.RunningTurn);

        }
    }

    bool CheckIfMoveHits(Movimiento move, Pokemon source, Pokemon target)
    {
        if (move.movimientoBase.AlwaysHits)
            return true;

        float moveAccuracy = move.movimientoBase.Accuracy;

        int accuracy = source.StatBoosts[Stat.Precision];
        int evasion = target.StatBoosts[Stat.Precision];

        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];


        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
             BattleOver(false); 
        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                    StartCoroutine(AboutToUse(nextPokemon));
                else
                    BattleOver(true);
            }

        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("¡Golpe crítico!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("¡Es super efectivo!");

        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("¡Es poco efectivo!");

    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = EstadoBatalla.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} va a usar a {newPokemon.Base.Name}. ¿Quieres cambiar de pokemon?");

        state = EstadoBatalla.AboutToUse;
        dialogBox.EnableChoiceBox(true);

    }


    public void HandleUpdate()
    {
        if (state == EstadoBatalla.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == EstadoBatalla.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == EstadoBatalla.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == EstadoBatalla.AboutToUse)
        {
            HandleAboutToUse();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);



        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Luchar
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Mochila
            }
            else if (currentAction == 2)
            {
                // Pokemon
                
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Huir
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }

    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);


        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }


    void HandlePartySelection()
    {

        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("No puedes enviar un pokemon debilitado");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("No puedes cambiar con el mismo pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == EstadoBatalla.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = EstadoBatalla.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == EstadoBatalla.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }
            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("Tienes que elegir un pokemon para continuar");
                return;
            }


            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == EstadoBatalla.AboutToUse)
            {

                StartCoroutine(SendNextTrainerPokemon());
            }

            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) 
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                
                OpenPartyScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon (Pokemon newPokemon, bool isTrainerAboutToUse=false)
    {
        if (playerUnit.Pokemon.HP > 0) { 

            yield return dialogBox.TypeDialog($"Vuelve {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

        }

        playerUnit.Setup(newPokemon);
        dialogBox.SetMovesNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"¡Adelante {newPokemon.Base.Name}!");

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = EstadoBatalla.RunningTurn;

        

    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = EstadoBatalla.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} envia a {nextPokemon.Base.Name}");

        state = EstadoBatalla.RunningTurn;

    }

    IEnumerator TryToEscape()
    {
        state = EstadoBatalla.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"No puedes huir de un combate con un entrenador");
            state = EstadoBatalla.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Has escapado con exito");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Has escapado con exito");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"No puedes escapar");
                state = EstadoBatalla.RunningTurn;
            }
        }
    }





}
