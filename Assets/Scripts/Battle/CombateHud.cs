using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CombateHud : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Image statusImage;
    [SerializeField] HPBar hpBar;

    [SerializeField] Sprite psnColor;
    [SerializeField] Sprite brnColor;
    [SerializeField] Sprite slpColor;
    [SerializeField] Sprite parColor;
    [SerializeField] Sprite frzColor;

    Pokemon _pokemon;

    Dictionary<ConditionID, Sprite> statusColors;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Nvl " + pokemon.Level;
        hpBar.SetHP((float) pokemon.HP / pokemon.MaxHp);

        statusColors = new Dictionary<ConditionID, Sprite>()
        {
            {ConditionID.psn ,psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor}
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    private void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusImage.gameObject.SetActive(false);
        }
        else
        {
            statusImage.gameObject.SetActive(true);
            statusImage.sprite = statusColors[(ConditionID)_pokemon.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (_pokemon.HpChanged) 
        {
            yield return hpBar.SetHPmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
        
    }
}
