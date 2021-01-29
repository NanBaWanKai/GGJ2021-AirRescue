using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Manager : MonoBehaviour
{

    private static Manager s_intance;
    public static Manager Instance => s_intance;

    [SerializeField]
    string m_defaultSceneName;
    [SerializeField]
    Client m_client;
    [SerializeField]
    LevelManager m_levelManager;

    public Client Client => m_client;
    public LevelManager LevelManager => m_levelManager;



    private void Awake()
    {
        s_intance = this;
        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        m_client.SceneLoader.LoadScene(m_defaultSceneName);
    }




}
