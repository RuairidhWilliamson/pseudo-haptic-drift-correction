using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRLogger : MonoBehaviour
{
    private static VRLogger _instance;

    private TMP_Text _text;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
    }

    public static void Log(string message)
    {
        _instance._text.text += $"{message}\n";
    }
}
