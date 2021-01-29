using DG.Tweening;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public sealed class LobbyCanvas : PanelBase
{
    [SerializeField]
    TMP_InputField m_playerNameInputField;
    [SerializeField]
    GameObject m_connectPanel;
    [SerializeField]
    GameObject m_waitingPanel;
    [SerializeField]
    GameObject m_lobbyPanel;
    [SerializeField]
    GameObject m_createRoomPanel;
    [SerializeField]
    GameObject m_joinRoomPanel;




    protected override void Awake()
    {
        base.Awake();
        m_connectPanel.SetActive(true);
        m_waitingPanel.SetActive(false);
        m_lobbyPanel.SetActive(false);
        m_createRoomPanel.SetActive(false);
        m_joinRoomPanel.SetActive(false);

      
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        Manager.Instance.Client.Launcher.OnConnectToMasterEvent += OnConnectToMasterServer;
        Manager.Instance.Client.Launcher.OnJoinLobbyEvent += OnJoinLobby;
        Manager.Instance.Client.Launcher.OnDisconnectToMasterEvent += OnDisconnectToMaster;
        m_playerNameInputField.text = $"Player{Random.Range(1000, 9999)}";
    }
    protected override void OnDisable()
    {
        base.OnEnable();
        Manager.Instance.Client.Launcher.OnConnectToMasterEvent -= OnConnectToMasterServer;
        Manager.Instance.Client.Launcher.OnConnectToMasterEvent -= OnJoinLobby;
        Manager.Instance.Client.Launcher.OnDisconnectToMasterEvent -= OnDisconnectToMaster;

    }
    private void OnConnectToMasterServer()
    {

    }
    private void OnJoinLobby()
    {
        m_waitingPanel.SetActive(false);
        m_lobbyPanel.SetActive(true);
    }
    private void OnDisconnectToMaster(DisconnectCause cause)
    {
        m_waitingPanel.SetActive(false);
        m_connectPanel.SetActive(true);
    }

    protected override void SubscribeButtonClickEvent(string btnName)
    {
        base.SubscribeButtonClickEvent(btnName);
        switch (btnName)
        {
            case "GOBtn":
                if (m_playerNameInputField.text != null && m_playerNameInputField.text != "")
                {
                    Manager.Instance.Client.Launcher.ConnectToMaster(m_playerNameInputField.text);
                    m_connectPanel.SetActive(false);
                    m_waitingPanel.SetActive(true);
                }
                break;
            case "CreateRoomBtn":
                m_lobbyPanel.SetActive(false);
                m_createRoomPanel.SetActive(true);
                break;
            case "JoinRoomBtn":
                m_lobbyPanel.SetActive(false);
                m_joinRoomPanel.SetActive(true);
                break;
            case "CreateRoomBackBtn":
                m_lobbyPanel.SetActive(true);
                m_createRoomPanel.SetActive(false);
                break;
            case "JoinRoomBackBtn":
                m_lobbyPanel.SetActive(true);
                m_joinRoomPanel.SetActive(false);
                break;
        }
    }
    protected override void SubscribeInputfieldSelectEvent(TMP_InputField inputField, string str)
    {
        base.SubscribeInputfieldSelectEvent(inputField, str);
        switch (inputField.name)
        {
            case "PlayerNameInputField":
                m_keyboardCachePanel = m_connectPanel;
                m_keyboardCachePanel.SetActive(false);
                break;
        }
    }
}
