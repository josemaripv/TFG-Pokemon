using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

[System.Serializable]
public class Pokemon
{

    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    public PokemonBase Base
    {
        get
        {
            return _base;
        }
    }

    public int Level
    {
        get
        {
            return level;
        }
    }


    public int HP { get; set; }

    public List<Movimiento> Moves { get; set; }

    public Movimiento CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condiciones Status { get; private set; }

    public int StatusTime { get; set; }

    public Condiciones VolatileStatus { get; private set; }
    public int VolatileStatusTime { get;  set; }


    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();

    public bool HpChanged { get;  set; }

    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;
    public void Init()
    {


        Moves = new List<Movimiento>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Movimiento(move.Base));

            if (Moves.Count >= 4)
                break;
        }

        CalculateStats();
        HP = MaxHp;

        ResetStatBoost();
        Status = null;
        StatusChanges = new Queue<string>();
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetPokemonByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;

        if (saveData.statusId != null)
            Status = CondicionesDB.Conditions[saveData.statusId.Value];
        else 
            Status = null;


       Moves = saveData.moves.Select(s => new Movimiento(s)).ToList();

        CalculateStats();
        ResetStatBoost();
        StatusChanges = new Queue<string>();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData
        {
            name = Base.Name,
            hp = HP,
            level = Level,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }


    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Ataque, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defensa, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.AtaqueEspecial, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.DefensaEspecial, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Velocidad, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 +  Level;
    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Ataque, 0 },
            { Stat.Defensa, 0 },
            { Stat.AtaqueEspecial, 0 },
            { Stat.DefensaEspecial, 0 },
            { Stat.Velocidad, 0 },
            { Stat.Precision, 0 },
            { Stat.Evasion, 0 }
        };
    }



    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else 
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);


        return statVal;

    }

    public void ApplyBoosts (List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts) 
        {
            var stat = statBoost.stat; 
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] +  boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"¡El {stat} de {Base.Name} ha aumentado!");
            else
                StatusChanges.Enqueue($"¡El {stat} de {Base.Name} ha disminuido!");

            Debug.Log($"{stat} has been bossted to {StatBoosts[stat]}");
        }
    }


    public void Heal()
    {
        HP = MaxHp;
        OnHPChanged?.Invoke();
    }


    public int MaxHp { get; private set; }


    public int Attack
    {
        get { return GetStat(Stat.Ataque); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defensa); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.AtaqueEspecial); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.DefensaEspecial); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Velocidad); }
    }

    public DamageDetails TakeDamage(Movimiento move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;


        float type = TypeChart.GetEffectiveness(move.movimientoBase.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.movimientoBase.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false,
        };

        float attack = (move.movimientoBase.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.movimientoBase.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.movimientoBase.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);


        return damageDetails;
    }


    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;
        Status = CondicionesDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();

    }

    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;
        VolatileStatus = CondicionesDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;

    }

    public Movimiento GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;
        }
        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;
        }
        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}