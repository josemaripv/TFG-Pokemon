using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] Sprite characterSprite;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    bool battleLost = false;

    Character character;

    private void Awake()
    {
        character = gameObject.GetComponent<Character>();
    }

    private void Start()
    {
        if (fov != null)
        {
            SetFovRotation(character.Animator.DefaultDirection);
        }
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {

            yield return DialogManager.Instance.ShowDialog(dialog, characterSprite);

            GameController.Instance.StartTrainerBattle(this);

           
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle, characterSprite);
        }
    }

    public IEnumerator TriggerTrainerBattle(MovimientoJugador player)
    {
        if (exclamation != null)
        {
            exclamation.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            exclamation.SetActive(false);
        }

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        yield return DialogManager.Instance.ShowDialog(dialog, characterSprite);
        GameController.Instance.StartTrainerBattle(this);

       
    }

    public void BattleLost()
    {
        battleLost = true;
        if (fov != null)
        {
            fov.gameObject.SetActive(false);
        }
    }

    public void SetFovRotation(FacingDirection dir)
    {
        if (fov == null)
            return;

        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        
        if (battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
