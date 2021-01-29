using IFancing;
using IFancing.Net;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : PanelBase
{
    [SerializeField]
    TMP_InputField m_roomNameInputField;
    [SerializeField]
    GameObject m_levelTogglePrefab;
    [SerializeField]
    GridLayoutGroup m_levelToggleGrid;
    [SerializeField]
    GameObject m_waitingPanel;
    ToggleGroup m_toggleGroup;
    private Toggle[] m_levelToggles;
    int m_selectLevelIndex = -1;
    [SerializeField]
    private Toggle m_defaultMaxPlayerToggle;
    [SerializeField]
    byte m_maxPlayer = 4;
    protected override void Awake()
    {
        base.Awake();
        m_toggleGroup = m_levelToggleGrid.GetComponent<ToggleGroup>();
        var childCount = m_levelToggleGrid.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(m_levelToggleGrid.transform.GetChild(0).gameObject);
        }
        var levels = Manager.Instance.LevelManager.Levels;
        m_levelToggles = new Toggle[levels.Length];
        for (int i = 0; i < m_levelToggles.Length; i++)
        {
            var obj = Instantiate(m_levelTogglePrefab, m_levelToggleGrid.transform);
            m_levelToggles[i] = obj.GetComponentInChildren<Toggle>();
            m_levelToggles[i].group = m_toggleGroup;
            var index = i;
            m_levelToggles[i].onValueChanged.AddListener((v) =>
            {
                if (v)
                {
                    m_selectLevelIndex = index;
                }
            });
            var bg = obj.GetTargetInChildren<Image>("Background");
            bg.sprite = levels[i].m_sprite;
            var nameText = obj.GetTargetInChildren<Text>("Label");
            nameText.text = levels[i].m_levelName;

        }

    }
    protected override void OnEnable()
    {
        base.OnEnable();
        m_roomNameInputField.text = Manager.Instance.Client.Launcher.LocalPlayer.NickName + "'s room";
        m_levelToggles[0].isOn = true;
        m_selectLevelIndex = 0;
        m_waitingPanel.SetActive(false);
        m_defaultMaxPlayerToggle.isOn = true;
        m_maxPlayer = 4;
        Manager.Instance.Client.Launcher.OnCreateRoomFailedEvent += OnCreateRoomFailed;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        Manager.Instance.Client.Launcher.OnCreateRoomFailedEvent -= OnCreateRoomFailed;
    }
    private void OnCreateRoomFailed(short errorCode, string message)
    {
        m_waitingPanel.SetActive(false);
    }


    protected override void SubscribeButtonClickEvent(string btnName)
    {
        base.SubscribeButtonClickEvent(btnName);
        switch (btnName)
        {
            case "CreateRoomBtn":
                if (m_roomNameInputField.text == "" || m_roomNameInputField.text == null)
                {
                    break;
                }
                if (m_selectLevelIndex == -1)
                {
                    break;
                }
                var options = new RoomOptions();
                var roomName = Guid.NewGuid().ToString();
                options.MaxPlayers = m_maxPlayer;
                options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
                options.CustomRoomPropertiesForLobby = new string[] { Launcher.ROOM_NAME, Launcher.ROOM_FACADE_NAME, Launcher.LEVEL_INDEX, Launcher.ROOM_STATE };
                options.CustomRoomProperties.Add(Launcher.ROOM_NAME, roomName);
                options.CustomRoomProperties.Add(Launcher.ROOM_FACADE_NAME, m_roomNameInputField.text);
                options.CustomRoomProperties.Add(Launcher.LEVEL_INDEX, m_selectLevelIndex);
                options.CustomRoomProperties.Add(Launcher.ROOM_STATE, 0);



                Manager.Instance.Client.Launcher.CreateRoom(options);

                m_waitingPanel.SetActive(true);
                break;
        }
    }
    protected override void SubscribeToggleValueChangeEvent(string toggleName, bool value)
    {
        base.SubscribeToggleValueChangeEvent(toggleName, value);
        if (value)
        {
            switch (toggleName)
            {
                case "FourPlayerToggle":
                    m_maxPlayer = 4;
                    print(4);
                    break;
                case "SixPlayerToggle":
                    m_maxPlayer = 6;
                    print(6);
                    break;
            }
        }
    }
}
