using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;

    public Color HighlightColor => highlightedColor;

    public static GlobalSettings i {  get; private set; }

    private void Awake()
    {
        i = this;
    }
}
