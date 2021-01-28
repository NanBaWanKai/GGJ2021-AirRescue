using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class LobbyCanvas : MonoBehaviour
{
    [SerializeField]
    Button m_GOBtn;
    [SerializeField]
    TMP_InputField m_playerNameInputField;
    [SerializeField]
    GameObject m_connectPanel;
    [SerializeField]
    GameObject m_keyboardPanel;
    [SerializeField]
    GameObject m_loadingPanel;
    [SerializeField]
    GameObject m_lobbyPanel;


    private void Awake()
    {
        m_connectPanel.SetActive(true);
        m_keyboardPanel.SetActive(true);
        m_loadingPanel.SetActive(false);
        m_lobbyPanel.SetActive(false);
        m_GOBtn.onClick.AddListener(() =>
        {
            if (m_playerNameInputField.text != null && m_playerNameInputField.text != "")
            {
                Manager.Instance.Client.Launcher.ConnectToMaster(m_playerNameInputField.text);
                m_connectPanel.SetActive(false);
                m_loadingPanel.SetActive(true);
            }
        });
        m_playerNameInputField.onSelect.AddListener((str) =>
        {
            m_connectPanel.SetActive(false);
            m_keyboardPanel.GetComponent<DOTweenAnimation>().DORewind();
            m_keyboardPanel.GetComponent<DOTweenAnimation>().DOPlayForward();

        });
        m_keyboardPanel.GetComponent<KeyboardVR>().OnPressEnter += (str) =>
        {
            m_connectPanel.SetActive(true);
            m_keyboardPanel.GetComponent<DOTweenAnimation>().DOPlayBackwards();
            m_playerNameInputField.text = str;
        };


    }
    private void OnEnable()
    {
        Manager.Instance.Client.Launcher.OnConnectToMasterEvent += OnConnectToMasterEvent;
    }

    private void OnDisable()
    {
        Manager.Instance.Client.Launcher.OnConnectToMasterEvent -= OnConnectToMasterEvent;
    }

    private void OnConnectToMasterEvent()
    {
        m_loadingPanel.SetActive(false);
        m_lobbyPanel.SetActive(true);
    }

}
