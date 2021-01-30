using IFancing;
using IFancing.Net;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public class GameSceneManager : PunMonoBehaviour
{
    private int m_levelIndex = -1;
    public event Action<int> OnLevelIndexChange;
    private int m_maxPlayer = -1;
    public event Action<int> OnMaxPlayerChange;
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



    private static GameSceneManager s_intance;

    [System.Serializable] public class GameobjectEvent : UnityEvent<GameObject> { }
    public GameobjectEvent onLocalPlayerSpawn;

    public static GameSceneManager Instance
    {
        get
        {
            if (s_intance == null)
            {
                s_intance = FindObjectOfType<GameSceneManager>();
            }
            return s_intance;
        }
    }

    const string PLAYER_PATH = "NetworkPlayer";
    [SerializeField]
    Transform m_playerSpawn;

    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected && !PhotonNetwork.OfflineMode)
            SceneManager.LoadScene(0);

        base.Awake();
        s_intance = this;
    }
    protected override void Start()
    {
        base.Start();
        var room = Manager.Instance.Client.Launcher.CurrentRoom;
        OnLevelIndexChange?.Invoke((int)room.CustomProperties[Launcher.LEVEL_INDEX]);
        OnMaxPlayerChange?.Invoke(room.MaxPlayers);
        SpawnPlayer();
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

            if (LevelIndex != -1)
            {
                var sceneName = Manager.Instance.LevelManager.Levels[LevelIndex].m_sceneName;
                Manager.Instance.Client.SceneLoader.LoadScene(sceneName);
            }
            else
            {
                Manager.Instance.Client.SceneLoader.LoadScene("WaitRoom");
            }
        }
    }
    void SpawnPlayer()
    {
        var random = UnityEngine.Random.Range(-3f, 3f);
        var go = PhotonNetwork.Instantiate(PLAYER_PATH, m_playerSpawn.position + m_playerSpawn.transform.right * random, m_playerSpawn.rotation);
        go.name += "(local)";
        onLocalPlayerSpawn.Invoke(go);
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
}
