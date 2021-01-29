using IFancing;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameSceneManager : PunMonoBehaviour
{

    private int m_score = -1;
    public event Action<int> OnScoreChange;


    private static GameSceneManager s_intance;
    public static GameSceneManager Instance => s_intance;

    public int Score
    {
        get => m_score;
        set
        {
            ChangeIntGlobal(ref m_score, value, OnScoreChange);
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

        OnScoreChange += (s) =>
        {
            print("  ");
        };
    }
    protected override void Start()
    {
        base.Start();
        //Invoke("SpawnPlayer", 1f);
        SpawnPlayer();
    }
    void SpawnPlayer()
    {
        var random = UnityEngine.Random.Range(-5f, 5f);
        var go=PhotonNetwork.Instantiate(PLAYER_PATH, m_playerSpawn.position + m_playerSpawn.transform.right * random, m_playerSpawn.rotation);
        go.name += "(local)";
    }
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting)
        {
            stream.SendNext(m_score);
        }
        else
        {
            var score = (int)stream.ReceiveNext();
            ChangeIntLocal(ref m_score, score, OnScoreChange);
        }
    }
}
