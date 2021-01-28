using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRKeyboard.Utils;

public interface IKeyboardVR
{
    string Input { get; set; }
    event Action<string> OnPressEnter;
}
public class KeyboardVR : MonoBehaviour, IKeyboardVR
{
    private const string MATERIAL_PATH = "Materials/UIOverlay";

    [SerializeField]
    private KeyboardManager m_keyboardManager;

    private void Awake()
    {
        m_keyboardManager.m_enterKey.onClick.AddListener(() =>
        {
            OnPressEnter?.Invoke(Input);
        });
    }


    public string Input { get => m_keyboardManager.Input; set => m_keyboardManager.Input = value; }

    public event Action<string> OnPressEnter;
}
