using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;

namespace IFancing.Net
{
    /// <summary>
    /// Photon Client 启动器，负责与Photon Master Server 连接
    /// </summary>
    public interface ILauncher
    {
        /// <summary>
        /// 是否是房主
        /// </summary>
        bool IsMasterClient { get; }
        /// <summary>
        /// 是否在大厅
        /// </summary>
        /// <returns></returns>
        bool IsInLobby();
        /// <summary>
        /// 是否在房间
        /// </summary>
        /// <returns></returns>
        bool IsInRoom();
        /// <summary>
        /// 连接Master服务器
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        void ConnectToMaster(string nickName/*, string ipAddress, int port*/);
        void JoinLobby();
        void LeaveLobby();
        /// <summary>
        /// 与Master服务器断开连接
        /// </summary>
        void DisconnectToMaster();
        /// <summary>
        /// 创建房间
        /// </summary>
        /// <param name="facadeName">房间假名</param>
        void CreateRoom(RoomOptions options);
        /// <summary>
        /// 加入或者创建房间
        /// </summary>
        /// <param name="options">房间参数</param>
        void JoinOrCreateRoom(RoomOptions options);
        /// <summary>
        /// 离开当前房间
        /// </summary>
        void LeaveRoom();
        /// <summary>
        /// 添加回调对象
        /// </summary>
        /// <param name="obj"></param>
        void AddCallback(object obj);
        /// <summary>
        /// 移除回调对象
        /// </summary>
        /// <param name="obj"></param>
        void RemoveCallback(object obj);
        /// <summary>
        /// 实例化网络物体
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="group"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null);
        /// <summary>
        /// 删除网络物体
        /// </summary>
        /// <param name="targetView"></param>
        void Destroy(PhotonView targetView);
        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, RoomInfo> CachedRoomList { get; }
        /// <summary>
        /// 房间内的玩家序号
        /// </summary>
        int ActorNumber { get; }
        /// <summary>
        /// 当与Master服务器连接时回调
        /// </summary>
        event Action OnConnectToMasterEvent;
        /// <summary>
        /// 
        /// </summary>
        event Action<DisconnectCause> OnDisconnectToMasterEvent;
        /// <summary>
        /// 当房间列表刷新时回调
        /// </summary>
        event Action<List<RoomInfo>> OnRoomUpdate;
        /// <summary>
        /// 当加入大厅时回调
        /// </summary>
        event Action OnJoinLobbyEvent;
        /// <summary>
        /// 当离开大厅时回调
        /// </summary>
        event Action OnLeaveLobbyEvent;
        /// <summary>
        /// 当创建房间的时回调
        /// </summary>
        event Action OnCreateRoomEvent;
        /// <summary>
        /// 当创建房间失败时回调
        /// </summary>
        event Action<short, string> OnCreateRoomFailedEvent;
        /// <summary>
        /// 当开始加入房间时回调
        /// </summary>
        event Action OnJoinRoomStartEvent;
        /// <summary>
        /// 当结束加入房间回调
        /// </summary>
        event Action<RoomOptions> OnJoinRoomEndEvent;
        /// <summary>
        /// 当加入房间失败时回调
        /// </summary>
        event Action<short, string> OnJoinRoomFailedEvent;
        /// <summary>
        /// 当离开大厅开始时回调
        /// </summary>
        event Action OnLeaveRoomStartEvent;
        /// <summary>
        /// 当离开大厅结束时回调
        /// </summary>
        event Action OnLeaveRoomEndEvent;
        /// <summary>
        /// 当有玩家加入房间时回调
        /// </summary>
        event Action<Player> OnPlayerEnterRoomEvent;
        /// <summary>
        /// 当有玩家退出房间时回调
        /// </summary>
        event Action<Player> OnPlayerLeaveRoomEvent;
        /// <summary>
        /// 当前大厅
        /// </summary>
        TypedLobby CurrentLobby { get; }
        /// <summary>
        /// 当前房间
        /// </summary>
        Room CurrentRoom { get; }
        /// <summary>
        /// 本地玩家，指自己
        /// </summary>
        Player LocalPlayer { get; }
        /// <summary>
        /// 是否已经连接Master Server
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 获取本地玩家的属性
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        object GetLocalPlayerProperty(object key);
        /// <summary>
        /// 设置本地玩家的属性
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void SetLocalPlayerProperty(object key, object value);
    }
    public class Launcher : MonoBehaviourPunCallbacks, ILauncher
    {
        public const string ROOM_NAME = "ROOM_NAME";
        public const string ROOM_FACADE_NAME = "ROOM_FACADE_NAME";
        public const string LEVEL_INDEX = "LEVEL_INDEX";
        public const string ROOM_STATE = "ROOM_STATE";// 房间状态, "0"：准备中，"1"：游戏中，"100"：结算中

        public const string PLAYER_READY = "PLAYER_READY";
        private ExitGames.Client.Photon.Hashtable m_playerPropertyCache = new ExitGames.Client.Photon.Hashtable();


        //public const byte MAX_PLAYER = 6;
        //[SerializeField]
        //private ServerSettings m_serverSettings;
        private RoomOptions m_roomOptionsCache = null;
        [SerializeField]
        private Dictionary<string, RoomInfo> m_cachedRoomList = new Dictionary<string, RoomInfo>();

        public Dictionary<string, RoomInfo> CachedRoomList => m_cachedRoomList;
        public int ActorNumber => PhotonNetwork.LocalPlayer.ActorNumber;
        public Room CurrentRoom => PhotonNetwork.CurrentRoom;
        public TypedLobby CurrentLobby => PhotonNetwork.CurrentLobby;
        public bool IsMasterClient => PhotonNetwork.IsMasterClient;
        public Player LocalPlayer => PhotonNetwork.LocalPlayer;
        public bool IsConnected => PhotonNetwork.IsConnected;

        public event Action OnJoinLobbyEvent;
        public event Action OnLeaveLobbyEvent;
        public event Action<List<RoomInfo>> OnRoomUpdate;
        public event Action OnLeaveRoomStartEvent;
        public event Action OnLeaveRoomEndEvent;
        public event Action OnJoinRoomStartEvent;
        public event Action OnCreateRoomEvent;
        public event Action<short, string> OnCreateRoomFailedEvent;

        public event Action<RoomOptions> OnJoinRoomEndEvent;
        public event Action OnConnectToMasterEvent;
        public event Action<DisconnectCause> OnDisconnectToMasterEvent;

        public event Action<short, string> OnJoinRoomFailedEvent;
        public event Action<Player> OnPlayerEnterRoomEvent;
        public event Action<Player> OnPlayerLeaveRoomEvent;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            PhotonNetwork.AutomaticallySyncScene = true;
            m_playerPropertyCache.Add(PLAYER_READY, false);
            LocalPlayer.SetCustomProperties(m_playerPropertyCache);
        }


        public GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null)
        {

            return PhotonNetwork.Instantiate(prefabName, position, rotation, group, data);
        }
        public void Destroy(PhotonView targetView)
        {
            PhotonNetwork.Destroy(targetView);
        }
        private void GetRoomList()
        {
            PhotonNetwork.GetCustomRoomList(TypedLobby.Default, null);
        }
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            OnConnectToMasterEvent?.Invoke();
            JoinLobby();
        }
        public void JoinLobby()
        {
            PhotonNetwork.JoinLobby();
        }
        public void LeaveLobby()
        {
            PhotonNetwork.LeaveLobby();
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            OnDisconnectToMasterEvent?.Invoke(cause);
        }
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            OnJoinLobbyEvent?.Invoke();
            //ProjectManager.Instance.Debug.Debug("Joined Lobby");
        }
        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
            OnLeaveLobbyEvent?.Invoke();
        }
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            UpdateCachedRoomList(roomList);
            OnRoomUpdate?.Invoke(roomList);
        }
        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (m_cachedRoomList.ContainsKey(info.Name))
                    {
                        m_cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (m_cachedRoomList.ContainsKey(info.Name))
                {
                    m_cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    m_cachedRoomList.Add(info.Name, info);
                }
            }
        }
        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            print("OnCreateRoom");
            OnCreateRoomEvent?.Invoke();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            OnCreateRoomFailedEvent?.Invoke(returnCode, message);

        }
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            OnJoinRoomEndEvent?.Invoke(m_roomOptionsCache);
            print("OnJoinedRoom");
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            base.OnJoinRoomFailed(returnCode, message);
            OnJoinRoomFailedEvent?.Invoke(returnCode, message);
        }
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            OnLeaveRoomEndEvent?.Invoke();
            ResetLocalPlayerProperty();
        }
        public void ConnectToMaster(string nickName/*, string ipAddress, int port*/)
        {
            PhotonNetwork.NickName = nickName;
            // m_serverSettings.AppSettings.Server = ipAddress; //ipAddress.Equals("") ? m_masterServer : ipAddress;
            //m_serverSettings.AppSettings.Port = port; //m_port;
            PhotonNetwork.ConnectUsingSettings();
        }
        public void DisconnectToMaster()
        {
            PhotonNetwork.Disconnect();
        }
        public void CreateRoom(RoomOptions options)
        {
            var roomName = (string)options.CustomRoomProperties[ROOM_NAME];
            m_roomOptionsCache = options;
            PhotonNetwork.CreateRoom(roomName, options, default);
            OnJoinRoomStartEvent?.Invoke();

            //var name= (string)ProjectManager.Instance.Client.Launcher.CurrentRoom.CustomProperties[ROOM_NAME];
        }
        public void JoinOrCreateRoom(RoomOptions options)
        {
            m_roomOptionsCache = options;
            var roomName = (string)options.CustomRoomProperties[ROOM_NAME];
            PhotonNetwork.JoinOrCreateRoom(roomName, options, default);
            OnJoinRoomStartEvent?.Invoke();
        }
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            OnLeaveRoomStartEvent?.Invoke();
        }
        public bool IsInLobby()
        {
            return PhotonNetwork.InLobby;
        }
        public bool IsInRoom()
        {
            return PhotonNetwork.InRoom;
        }

        public void AddCallback(object obj)
        {
            PhotonNetwork.AddCallbackTarget(obj);
        }

        public void RemoveCallback(object obj)
        {
            PhotonNetwork.RemoveCallbackTarget(obj);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            OnPlayerEnterRoomEvent?.Invoke(newPlayer);
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            OnPlayerLeaveRoomEvent?.Invoke(otherPlayer);
        }

        public object GetLocalPlayerProperty(object key)
        {
            return LocalPlayer.CustomProperties[key];
        }

        public void SetLocalPlayerProperty(object key, object value)
        {
            var hash = LocalPlayer.CustomProperties;
            hash[key] = value;
            LocalPlayer.SetCustomProperties(hash);
        }

        void ResetLocalPlayerProperty()
        {
            m_playerPropertyCache[PLAYER_READY] = false;
            LocalPlayer.SetCustomProperties(m_playerPropertyCache);
        }

    }

}