using IFancing;
using IFancing.Net;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 客户端接口
/// </summary>
public interface IClient
{
    /// <summary>
    /// GateServer网关服务器的用户名
    /// </summary>
    string UserName { get; }
    /// <summary>
    /// 是否已经登录网关服务器
    /// </summary>
    bool IsLogin { get; }
    /// <summary>
    /// 登录网关服务器
    /// </summary>
    /// <param name="user_name">用户名</param>
    /// <param name="user_pwd">密码</param>
    /// <param name="callback">请求回调</param>
    void Login(string user_name, string user_pwd, Action<Hashtable> callback);
    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="user_name">用户名</param>
    /// <param name="user_pwd">密码</param>
    /// <param name="callback">请求回调</param>
    void Register(string user_name, string user_pwd, Action<Hashtable> callback);
    /// <summary>
    /// 账号下线
    /// </summary>
    /// <param name="callback">请求回调</param>
    void Logout(Action<Hashtable> callback);

    //#region 徐隆飞_新增_提交答题结果 
    //void SetCrisisprResult(string roomName, string id, string answer, string score, Action<Hashtable> callback);
    //void GetCrisisprResult(string roomName, Action<Hashtable> callback);
    //#endregion


    IGateServerConnect GateServer { get; }
    /// <summary>
    /// Photon Client 启动器，负责和Photon Master Server 连接。
    /// </summary>
    ILauncher Launcher { get; }
    /// <summary>
    /// 负责事件订阅和广播
    /// </summary>
    INetGameEvent NetGameEvent { get; }
    /// <summary>
    /// 负责加载关卡 Level
    /// </summary>
    ISceneLoader SceneLoader { get; }
}
public class Client : MonoBehaviour, IClient
{
    [SerializeField]
    [Tooltip("用户名")]
    string m_userName = null;
    [SerializeField]
    [Tooltip("是否登录")]
    bool m_isLogin = false;
    [SerializeField]
    [Tooltip("网关服务器连接")]
    GateServerConnect m_gateConn;
    [SerializeField]
    [Tooltip("Master服务器连接")]
    Launcher m_launcher;
    [SerializeField]
    [Tooltip("事件订阅和广播")]
    NetGameEvent m_netGameEvent;
    [SerializeField]
    [Tooltip("Level的加载和卸载")]
    SceneLoader m_levelLoader;
    //[SerializeField]
    //[Tooltip("Master服务器的IP地址")]
    //string m_masterIP;
    //[SerializeField]
    //[Tooltip("Master服务器的端口")]
    //int m_masterPort = -1;
    public string UserName => m_userName;
    public bool IsLogin => m_isLogin;
    public ILauncher Launcher => m_launcher;
    public INetGameEvent NetGameEvent => m_netGameEvent;
    public ISceneLoader SceneLoader => m_levelLoader;

    public IGateServerConnect GateServer => m_gateConn;

    public void Login(string user_name, string user_pwd, Action<Hashtable> callback)
    {
        var form = new WWWForm();
        form.AddField("user_name", user_name);
        form.AddField("user_pwd", user_pwd);
        callback += (hash) =>
        {
            if (hash["code"].Equals("200"))
            {
                m_userName = user_name;
                m_isLogin = true;
                //m_masterIP = (string)hash["master_ip"];
                //m_masterPort = int.Parse((string)hash["master_port"]);
                //m_launcher.ConnectToMaster(m_userName, m_masterIP, m_masterPort);
            }
        };
        m_gateConn.Post("/login", form, callback);
    }
    public void Logout(Action<Hashtable> callback)
    {
        var form = new WWWForm();
        form.AddField("user_name", m_userName);
        callback += (hash) =>
        {
            if (hash["code"].Equals("200"))
            {
                m_userName = null;
                m_isLogin = false;
                //m_masterIP = null;
                //m_masterPort = -1;
                m_launcher.DisconnectToMaster();
            }
        };
        m_gateConn.Post("/logout", form, callback);
    }
    public void Register(string user_name, string user_pwd, Action<Hashtable> callback)
    {
        var form = new WWWForm();
        form.AddField("user_name", user_name);
        form.AddField("user_pwd", user_pwd);
        m_gateConn.Post("/register", form, callback);
    }
    private void OnEnable()
    {
        m_launcher.OnLeaveRoomStartEvent += OnLeaveRoomStart;
    }

    private void OnDisable()
    {
        m_launcher.OnLeaveRoomStartEvent -= OnLeaveRoomStart;
    }

    void OnLeaveRoomStart()
    {
        m_levelLoader.LoadScene("Lobby");
    }
}
