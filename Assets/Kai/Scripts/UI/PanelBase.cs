using DG.Tweening;
using IFancing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class PanelBase : PunMonoBehaviour
{
    [SerializeField]
    GameObject m_keyboardPanel;

    protected TMP_InputField m_selectInputField = null;
    protected GameObject m_keyboardCachePanel = null;
    protected override void Awake()
    {
        base.Awake();
        var buttons = transform.GetComponentsInChildren<Button>(true);
        foreach (var item in buttons)
        {
            var btnName = item.name;
            item.onClick.AddListener(() =>
            {
                SubscribeButtonClickEvent(btnName);
            });
        }
        var inputFields = transform.GetComponentsInChildren<TMP_InputField>(true);
        foreach (var item in inputFields)
        {
            var i = item;
            item.onSelect.AddListener((str) =>
            {
                SubscribeInputfieldSelectEvent(i, str);
            });
        }
        var toggles = transform.GetComponentsInChildren<Toggle>(true);
        foreach (var item in toggles)
        {
            var name = item.name;
            item.onValueChanged.AddListener((value) =>
            {
                SubscribeToggleValueChangeEvent(name, value);
            });
        }


        m_keyboardPanel.SetActive(true);
        m_keyboardPanel.GetComponent<KeyboardVR>().OnPressEnter += (str) =>
        {
            m_keyboardCachePanel?.SetActive(true);
            m_keyboardPanel.GetComponent<DOTweenAnimation>().DOPlayBackwards();
            m_selectInputField.text = str;
        };

    }

    protected virtual void SubscribeButtonClickEvent(string btnName) { }
    protected virtual void SubscribeInputfieldSelectEvent(TMP_InputField inputField, string str)
    {
        m_selectInputField = inputField;
        m_keyboardPanel.GetComponent<DOTweenAnimation>().DORewind();
        m_keyboardPanel.GetComponent<DOTweenAnimation>().DOPlayForward();
        m_keyboardPanel.GetComponent<KeyboardVR>().Input = str;
    }


    protected virtual void SubscribeToggleValueChangeEvent(string toggleName, bool value)
    {

    }
}
