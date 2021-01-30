using IFancing;
using IFancing.Net;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitRoomCanvas : PanelBase
{
    [SerializeField]
    TMP_Text m_roomNameText;
    [SerializeField]
    GameObject m_levelTogglePrefab;
    [SerializeField]
    GridLayoutGroup m_levelToggleGrid;
    ToggleGroup m_toggleGroup;
    private Toggle[] m_levelToggles;
    [SerializeField]
    private Toggle[] m_playerCountToggles;

    GameObject m_waitingPanel;


    protected override void Awake()
    {
        base.Awake();
        var room = Manager.Instance.Client.Launcher.CurrentRoom;
        var roonName = (string)room.CustomProperties[Launcher.ROOM_FACADE_NAME];
        m_roomNameText.text = roonName;

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
                    GameSceneManager.Instance.LevelIndex = index;
                }
            });
            var bg = obj.GetTargetInChildren<Image>("Background");
            bg.sprite = levels[i].m_sprite;
            var nameText = obj.GetTargetInChildren<Text>("Label");
            nameText.text = levels[i].m_levelName;
        }
        for (int i = 0; i < m_playerCountToggles.Length; i++)
        {
            var index = i;
            m_playerCountToggles[i].onValueChanged.AddListener((v) =>
            {
                if (v)
                {
                    var count = (index + 2) * 2;
                    GameSceneManager.Instance.MaxPlayer = count; 
                    var room = Manager.Instance.Client.Launcher.CurrentRoom;
                    room.MaxPlayers = (byte)count;
                }
            });


        }

        GameSceneManager.Instance.OnLevelIndexChange += (index) =>
        {
            m_levelToggles[index].isOn = true;
        };
        GameSceneManager.Instance.OnMaxPlayerChange += (max) =>
        {
            switch (max)
            {
                case 4:
                    m_playerCountToggles[0].isOn = true;
                    break;
                case 6:
                    m_playerCountToggles[1].isOn = true;
                    break;
            }
        };
        //OnLevelIndexChange?.Invoke((int)room.CustomProperties[Launcher.LEVEL_INDEX]);
        //OnMaxPlayerChange?.Invoke(room.MaxPlayers);
    }
 
    protected override void SubscribeButtonClickEvent(string btnName)
    {
        base.SubscribeButtonClickEvent(btnName);
        switch (btnName)
        {
            case "StartBtn":
                Manager.Instance.Client.NetGameEvent.BroadcastGlobal(NetGameEvent.LOAD_LEVEL,GameSceneManager.Instance.LevelIndex);
                break;
        }
    }
}
