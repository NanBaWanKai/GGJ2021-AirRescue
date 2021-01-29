using IFancing;
using IFancing.Net;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitRoomCanvas : PanelBase
{
    private int m_levelIndex = -1;
    private event Action<int> OnLevelIndexChange;
    private int m_maxPlayer = -1;
    private event Action<int> OnMaxPlayerChange;

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

    public int LevelIndex
    {
        get => m_levelIndex; set
        {
            ChangeIntGlobal(ref m_levelIndex, value, OnLevelIndexChange);
        }
    }
    public int MaxPlayer
    {
        get => m_maxPlayer;
        set
        {
            ChangeIntGlobal(ref m_maxPlayer, value, OnMaxPlayerChange);
        }
    }

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
                    LevelIndex = index;
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
                    MaxPlayer = count;
                    var room = Manager.Instance.Client.Launcher.CurrentRoom;
                    room.MaxPlayers = (byte)count;
                }
            });


        }

        OnLevelIndexChange += (index) =>
        {
            m_levelToggles[index].isOn = true;
        };
        OnMaxPlayerChange += (max) =>
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
        OnLevelIndexChange?.Invoke((int)room.CustomProperties[Launcher.LEVEL_INDEX]);
        OnMaxPlayerChange?.Invoke(room.MaxPlayers);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        Manager.Instance.Client.NetGameEvent.AddListener<int>(NetGameEvent.LOAD_LEVEL, OnLoadLevel);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        Manager.Instance.Client.NetGameEvent.RemoveListener<int>(NetGameEvent.LOAD_LEVEL, OnLoadLevel);
    }

    void OnLoadLevel(int levelIndex)
    {
        if (Manager.Instance.Client.Launcher.IsMasterClient)
        {
            var room = Manager.Instance.Client.Launcher.CurrentRoom;
            room.MaxPlayers = (byte)MaxPlayer;
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add(Launcher.ROOM_STATE, 1);
            hash.Add(Launcher.LEVEL_INDEX, LevelIndex);
            room.SetCustomProperties(hash);
            var sceneName = Manager.Instance.LevelManager.Levels[LevelIndex].m_sceneName;
            Manager.Instance.Client.SceneLoader.LoadScene(sceneName);
        }
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting)
        {
            stream.SendNext(m_levelIndex);
            stream.SendNext(m_maxPlayer);
        }
        else
        {
            var levelIndex = (int)stream.ReceiveNext();
            ChangeIntLocal(ref m_levelIndex, levelIndex, OnLevelIndexChange);
            var maxPlayer = (int)stream.ReceiveNext();
            ChangeIntLocal(ref m_maxPlayer, maxPlayer, OnMaxPlayerChange);
        }
    }
    protected override void SubscribeButtonClickEvent(string btnName)
    {
        base.SubscribeButtonClickEvent(btnName);
        switch (btnName)
        {
            case "StartBtn":
                Manager.Instance.Client.NetGameEvent.BroadcastGlobal(NetGameEvent.LOAD_LEVEL, LevelIndex);
                break;
        }
    }
}
