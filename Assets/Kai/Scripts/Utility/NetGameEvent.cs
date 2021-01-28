using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
//using Photon.Realtime;

namespace IFancing.Net
{
    public interface INetGameEvent
    {
        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="call">回调方法</param>
        void AddListener(string eventName, Action call);
        void AddListener<T>(string eventName, Action<T> call);
        void AddListener<T, U>(string eventName, Action<T, U> call);
        void AddListener<T, U, O>(string eventName, Action<T, U, O> call);
        void AddListener<T, U, O, P>(string eventName, Action<T, U, O, P> call);
        void AddListener<T, U, O, P, Q>(string eventName, Action<T, U, O, P, Q> call);
        /// <summary>
        /// 移除订阅
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="call">回调方法</param>
        void RemoveListener(string eventName, Action call);
        void RemoveListener<T>(string eventName, Action<T> call);
        void RemoveListener<T, U>(string eventName, Action<T, U> call);
        void RemoveListener<T, U, O>(string eventName, Action<T, U, O> call);
        void RemoveListener<T, U, O, P>(string eventName, Action<T, U, O, P> call);
        void RemoveListener<T, U, O, P, Q>(string eventName, Action<T, U, O, P, Q> call);
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="eventName">回调方法</param>
        void BroadcastGlobal(string eventName);
        void BroadcastGlobal<T>(string eventName, T arg1);
        void BroadcastGlobal<T, U>(string eventName, T arg1, U arg2);
        void BroadcastGlobal<T, U, O>(string eventName, T arg1, U arg2, O arg3);
        void BroadcastGlobal<T, U, O, P>(string eventName, T arg1, U arg2, O arg3, P arg4);
        void BroadcastGlobal<T, U, O, P, Q>(string eventName, T arg1, U arg2, O arg3, P arg4, Q arg5);

        void BroadcastLocal(string eventName);
        void BroadcastLocal<T>(string eventName, T arg1);
        void BroadcastLocal<T, U>(string eventName, T arg1, U arg2);
        void BroadcastLocal<T, U, O>(string eventName, T arg1, U arg2, O arg3);
        void BroadcastLocal<T, U, O, P>(string eventName, T arg1, U arg2, O arg3, P arg4);
        void BroadcastLocal<T, U, O, P, Q>(string eventName, T arg1, U arg2, O arg3, P arg4, Q arg5);
    }
    public sealed class NetGameEvent : MonoBehaviour, INetGameEvent
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public const string MSG_SYSTEM = "MSG_SYSTEM";// <系统信息:string> 广播聊天窗口系统信息
        public const string MSG_CHAT = "MSG_CHAT";// <用户名:string,聊天信息:string> 广播聊天窗口事件
        public const string MSG_INFO = "MSG_INFO";//<通知信息:string> 人物通知信息

        public const string UPDATE_ROOM_PROPERTY = "UPDATE_ROOM_PROPERTY";//<>无参数 广播房间内玩家属性刷新事件
        public const string UPDATE_PLAYER_PROPERTY = "UPDATE_PLAYER_PROPERTY";//<>无参数 广播房间内玩家属性刷新事件
        public const string LOAD_LEVEL = "LOAD_LEVEL";//<levelName:string> 广播场景加载事件

        public const string DETECT_INTERACTABLE = "DETECT_INTERACTABLE";//交互物体： GameObject
        public const string UNDETECT_INTERACTABLE = "UNDETECT_INTERACTABLE";//交互物体：GameObject


        /// <summary>
        /// PhotonView 的缓存
        /// </summary>
        private PhotonView m_pvCache;
        /// <summary>
        /// 自身类型的缓存，减少反射次数
        /// </summary>
        private Type m_typeCache;
        /// <summary>
        /// 自身挂载的PhotonView
        /// </summary>
        private PhotonView PV
        {
            get
            {
                if (m_pvCache == null)
                {
                    m_pvCache = PhotonView.Get(gameObject);
                }
                return m_pvCache;
            }
        }
        /// <summary>
        /// 自身类型的缓存
        /// </summary>
        private Type SelfType
        {
            get
            {
                if (m_typeCache == null)
                {
                    m_typeCache = GetType();
                }
                return m_typeCache;
            }
        }
        /// <summary>
        /// 委托的字典，<事件名:string,委托:Delegate>,用于记录订阅
        /// </summary>
        private Dictionary<string, Delegate> m_eventDic = new Dictionary<string, Delegate>();
        /// <summary>
        /// 事件的队列，接收到广播后存入，在Update中依次取出调用。
        /// </summary>
        private Queue<Action> m_delegateQueue = new Queue<Action>();
        bool IsInRoom()
        {
            return PhotonNetwork.InRoom;
        }
        #region PRIVATE
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        private void Update()
        {
            lock (m_delegateQueue)
            {
                if (m_delegateQueue.Count > 0)
                {
                    Action act = m_delegateQueue.Dequeue();
                    while (act != null)
                    {
                        act();
                        act = null;
                        try
                        {
                            act = m_delegateQueue.Dequeue();
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
            }
        }
        #endregion
        #region UTILITY
        //清零检查
        private void CheckCommad(string command)
        {
            if (m_eventDic[command] == null)
            {
                m_eventDic.Remove(command);
                //m_commandTypeList.Remove(command);
            }
        }
        #endregion
        #region ADD_LISTENER
        //注册监听
        public void AddListener(string command, Action call)
        {
            //if (!m_commandTypeList.Contains(command))
            if (!m_eventDic.ContainsKey(command))
            {
                //m_commandTypeList.Add(command);
                m_eventDic.Add(command, call);
            }
            else
            {
                if (m_eventDic[command] == null)
                {
                    Debug.LogError("Delegate对象异常为NULL Key:" + command);
                    return;
                }
                m_eventDic[command] = /*(CallFunction)*/(Action)m_eventDic[command] + call;
            }
        }
        public void AddListener<T>(string command, Action<T> call)
        {
            //if (!m_commandTypeList.Contains(command))
            if (!m_eventDic.ContainsKey(command))
            {
                //m_commandTypeList.Add(command);
                m_eventDic.Add(command, call);
            }
            else
            {
                if (m_eventDic[command] == null || m_eventDic[command].GetType() != call.GetType())
                {
                    Debug.LogError("Delegate对象异常为NULL Key:" + command);
                    return;
                }
                m_eventDic[command] = (Action<T>)m_eventDic[command] + call;
            }
        }
        public void AddListener<T, U>(string command, Action<T, U> call)
        {
            if (!m_eventDic.ContainsKey(command))
            {
                //m_commandTypeList.Add(command);
                m_eventDic.Add(command, call);
            }
            else
            {
                if (m_eventDic[command] == null || m_eventDic[command].GetType() != call.GetType())
                {
                    Debug.LogError("Delegate对象异常为NULL Key:" + command);
                    return;
                }
                m_eventDic[command] = (Action<T, U>)m_eventDic[command] + call;
            }
        }
        public void AddListener<T, U, O>(string command, Action<T, U, O> call)
        {
            if (!m_eventDic.ContainsKey(command))
            {
                //m_commandTypeList.Add(command);
                m_eventDic.Add(command, call);
            }
            else
            {
                if (m_eventDic[command] == null || m_eventDic[command].GetType() != call.GetType())
                {
                    Debug.LogError("Delegate对象异常为NULL Key:" + command);
                    return;
                }
                m_eventDic[command] = (Action<T, U, O>)m_eventDic[command] + call;
            }
        }
        public void AddListener<T, U, O, P>(string command, Action<T, U, O, P> call)
        {
            if (!m_eventDic.ContainsKey(command))
            {
                //m_commandTypeList.Add(command);
                m_eventDic.Add(command, call);
            }
            else
            {
                if (m_eventDic[command] == null || m_eventDic[command].GetType() != call.GetType())
                {
                    Debug.LogError("Delegate对象异常为NULL Key:" + command);
                    return;
                }
                m_eventDic[command] = (Action<T, U, O, P>)m_eventDic[command] + call;
            }
        }
        public void AddListener<T, U, O, P, Q>(string command, Action<T, U, O, P, Q> call)
        {
            if (!m_eventDic.ContainsKey(command))
            {
                //m_commandTypeList.Add(command);
                m_eventDic.Add(command, call);
            }
            else
            {
                if (m_eventDic[command] == null || m_eventDic[command].GetType() != call.GetType())
                {
                    Debug.LogError("Delegate对象异常为NULL Key:" + command);
                    return;
                }
                m_eventDic[command] = (Action<T, U, O, P, Q>)m_eventDic[command] + call;
            }
        }
        #endregion
        #region REMOVE_LISTENER
        //移除事件
        public void RemoveListener(string command, Action call)
        {

            if (!m_eventDic.ContainsKey(command)) return;

            Delegate @delegate = m_eventDic[command];
            if (@delegate == null)
            {
                Debug.LogError("Delegate结果为NULL Key:" + command);
                return;
            }
            else if (@delegate.GetType() != call.GetType())
            {
                Debug.LogError("Delegate对象不匹配 Key:" + command);
                return;
            }
            m_eventDic[command] = (Action)m_eventDic[command] - call;

            CheckCommad(command);
        }
        public void RemoveListener<T>(string command, Action<T> call)
        {
            if (!m_eventDic.ContainsKey(command)) return;

            Delegate @delegate = m_eventDic[command];
            if (@delegate == null)
            {
                Debug.LogError("Delegate结果为NULL Key:" + command);
                return;
            }
            else if (@delegate.GetType() != call.GetType())
            {
                Debug.LogError("Delegate对象不匹配 Key:" + command);
                return;
            }

            m_eventDic[command] = (Action<T>)m_eventDic[command] - call;

            CheckCommad(command);
        }
        public void RemoveListener<T, U>(string command, Action<T, U> call)
        {
            if (!m_eventDic.ContainsKey(command)) return;

            Delegate @delegate = m_eventDic[command];
            if (@delegate == null)
            {
                Debug.LogError("Delegate结果为NULL Key:" + command);
                return;
            }
            else if (@delegate.GetType() != call.GetType())
            {
                Debug.LogError("Delegate对象不匹配 Key:" + command);
                return;
            }

            m_eventDic[command] = (Action<T, U>)m_eventDic[command] - call;

            CheckCommad(command);
        }
        public void RemoveListener<T, U, O>(string command, Action<T, U, O> call)
        {
            if (!m_eventDic.ContainsKey(command)) return;

            Delegate @delegate = m_eventDic[command];
            if (@delegate == null)
            {
                Debug.LogError("Delegate结果为NULL Key:" + command);
                return;
            }
            else if (@delegate.GetType() != call.GetType())
            {
                Debug.LogError("Delegate对象不匹配 Key:" + command);
                return;
            }

            m_eventDic[command] = (Action<T, U, O>)m_eventDic[command] - call;

            CheckCommad(command);
        }
        public void RemoveListener<T, U, O, P>(string command, Action<T, U, O, P> call)
        {
            if (!m_eventDic.ContainsKey(command)) return;

            Delegate @delegate = m_eventDic[command];
            if (@delegate == null)
            {
                Debug.LogError("Delegate结果为NULL Key:" + command);
                return;
            }
            else if (@delegate.GetType() != call.GetType())
            {
                Debug.LogError("Delegate对象不匹配 Key:" + command);
                return;
            }

            m_eventDic[command] = (Action<T, U, O, P>)m_eventDic[command] - call;

            CheckCommad(command);
        }
        public void RemoveListener<T, U, O, P, Q>(string command, Action<T, U, O, P, Q> call)
        {
            if (!m_eventDic.ContainsKey(command)) return;

            Delegate @delegate = m_eventDic[command];
            if (@delegate == null)
            {
                Debug.LogError("Delegate结果为NULL Key:" + command);
                return;
            }
            else if (@delegate.GetType() != call.GetType())
            {
                Debug.LogError("Delegate对象不匹配 Key:" + command);
                return;
            }

            m_eventDic[command] = (Action<T, U, O, P, Q>)m_eventDic[command] - call;

            CheckCommad(command);
        }
        #endregion
        #region INVOKE
        public void BroadcastGlobal(string command)
        {
            if (IsInRoom())
            {
                PV.RPC("SendBroadcast", RpcTarget.All, command);
            }
            else
            {
                BroadcastLocal(command);
            }
        }
        public void BroadcastGlobal<T>(string command, T arg1)
        {
            if (IsInRoom())
            {
                PV.RPC("SendBroadcast", RpcTarget.All, typeof(T).FullName, command, arg1);
            }
            else
            {
                BroadcastLocal<T>(command, arg1);
            }
        }
        public void BroadcastGlobal<T, U>(string command, T arg1, U arg2)
        {
            if (IsInRoom())
            {
                PV.RPC("SendBroadcast", RpcTarget.All, typeof(T).FullName, typeof(U).FullName, command, arg1, arg2);
            }
            else
            {
                BroadcastLocal<T, U>(command, arg1, arg2);
            }
        }
        public void BroadcastGlobal<T, U, O>(string command, T arg1, U arg2, O arg3)
        {
            if (IsInRoom())
            {
                PV.RPC("SendBroadcast", RpcTarget.All, typeof(T).FullName, typeof(U).FullName, typeof(O).FullName, command, arg1, arg2, arg3);
            }
            else
            {
                BroadcastLocal<T, U, O>(command, arg1, arg2, arg3);
            }
        }
        public void BroadcastGlobal<T, U, O, P>(string command, T arg1, U arg2, O arg3, P arg4)
        {
            if (IsInRoom())
            {
                PV.RPC("SendBroadcast", RpcTarget.All, typeof(T).FullName, typeof(U).FullName, typeof(O).FullName, typeof(P).FullName, command, arg1, arg2, arg3, arg4);
            }
            else
            {
                BroadcastLocal<T, U, O, P>(command, arg1, arg2, arg3, arg4);
            }
        }
        public void BroadcastGlobal<T, U, O, P, Q>(string command, T arg1, U arg2, O arg3, P arg4, Q arg5)
        {
            if (IsInRoom())
            {
                PV.RPC("SendBroadcast", RpcTarget.All, typeof(T).FullName, typeof(U).FullName, typeof(O).FullName, typeof(P).FullName, typeof(Q).FullName, command, arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                BroadcastLocal<T, U, O, P, Q>(command, arg1, arg2, arg3, arg4, arg5);
            }
        }



        private MethodInfo[] m_broadCastMethodInfosCache = null;
        private MethodInfo[] BroadCastMethodInfos
        {
            get
            {
                if (m_broadCastMethodInfosCache == null)
                {
                    m_broadCastMethodInfosCache = new MethodInfo[6];
                    for (int i = 0; i < m_broadCastMethodInfosCache.Length; i++)
                    {
                        m_broadCastMethodInfosCache[i] = SelfType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default).FirstOrDefault(F => F.IsGenericMethod && F.Name == "BroadcastLocal" && F.GetParameters().Length == i + 1);
                    }
                }
                return m_broadCastMethodInfosCache;
            }
        }
        [PunRPC]
        public void SendBroadcast(string command)
        {
            BroadcastLocal(command);
        }
        [PunRPC]
        private void SendBroadcast(string typeNameT, string command, object t1)
        {
            MethodInfo mi1 = BroadCastMethodInfos[1].MakeGenericMethod(new Type[] { ToType(typeNameT)/* Type.GetType(typeNameT)*/ });
            mi1.Invoke(this, new object[] { command, t1 });
        }
        [PunRPC]
        public void SendBroadcast(string typeNameT, string typeNameU, string command, object t1, object t2)
        {
            MethodInfo mi1 = BroadCastMethodInfos[2].MakeGenericMethod(new Type[] { ToType(typeNameT), ToType(typeNameU)/* Type.GetType(typeNameT), Type.GetType(typeNameU)*/ });
            mi1.Invoke(this, new object[] { command, t1, t2 });
        }
        [PunRPC]
        public void SendBroadcast(string typeNameT, string typeNameU, string typeNameV, string command, object t1, object t2, object t3)
        {
            MethodInfo mi1 = BroadCastMethodInfos[3].MakeGenericMethod(new Type[] { ToType(typeNameT), ToType(typeNameU), ToType(typeNameV) /*Type.GetType(typeNameT), Type.GetType(typeNameU), Type.GetType(typeNameV)*/ });
            mi1.Invoke(this, new object[] { command, t1, t2, t3 });
        }
        [PunRPC]
        public void SendBroadcast(string typeNameT, string typeNameU, string typeNameV, string typeNameN, string command, object t1, object t2, object t3, object t4)
        {
            MethodInfo mi1 = BroadCastMethodInfos[4].MakeGenericMethod(new Type[] { ToType(typeNameT), ToType(typeNameU), ToType(typeNameV), ToType(typeNameN)/* Type.GetType(typeNameT), Type.GetType(typeNameU), Type.GetType(typeNameV), Type.GetType(typeNameN)*/ });
            mi1.Invoke(this, new object[] { command, t1, t2, t3, t4 });
        }
        [PunRPC]
        public void SendBroadcast(string typeNameT, string typeNameU, string typeNameV, string typeNameN, string typeNameQ, string command, object t1, object t2, object t3, object t4, object t5)
        {
            MethodInfo mi1 = BroadCastMethodInfos[5].MakeGenericMethod(new Type[] { ToType(typeNameT), ToType(typeNameU), ToType(typeNameV), ToType(typeNameN), ToType(typeNameQ)/* Type.GetType(typeNameT), Type.GetType(typeNameU), Type.GetType(typeNameV), Type.GetType(typeNameN), Type.GetType(typeNameQ) */});
            mi1.Invoke(this, new object[] { command, t1, t2, t3, t4, t5 });
        }
        public void BroadcastLocal(string command)
        {
            if (!m_eventDic.ContainsKey(command)) return;
            Delegate @delegate;
            if (m_eventDic.TryGetValue(command, out @delegate))
            {
                Action call = @delegate as Action;
                if (call != null)
                {
                    lock (m_delegateQueue)
                    {
                        m_delegateQueue.Enqueue(() =>
                        {
                            call();
                        });
                    }
                }
                else
                {
                    Debug.LogError("对应key的De'le'gate为空 Key:" + command);
                }
            }
        }
        public void BroadcastLocal<T>(string command, T arg1)
        {
            if (!m_eventDic.ContainsKey(command)) return;
            Delegate @delegate;
            if (m_eventDic.TryGetValue(command, out @delegate))
            {
                Action<T> call = @delegate as Action<T>;
                if (call != null)
                {
                    lock (m_delegateQueue)
                    {
                        m_delegateQueue.Enqueue(() =>
                        {
                            call(arg1);
                        });
                    }
                }
                else
                {
                    Debug.LogError("对应key的Delegate为空 Key:" + command);
                }
            }
        }
        public void BroadcastLocal<T, U>(string command, T arg1, U arg2)
        {
            if (!m_eventDic.ContainsKey(command)) return;
            Delegate @delegate;
            if (m_eventDic.TryGetValue(command, out @delegate))
            {
                Action<T, U> call = @delegate as Action<T, U>;
                if (call != null)
                {
                    lock (m_delegateQueue)
                    {
                        m_delegateQueue.Enqueue(() =>
                        {
                            call(arg1, arg2);
                        });
                    }
                }
                else
                {
                    Debug.LogError("对应key的De'le'gate为空 Key:" + command);
                }
            }
        }
        public void BroadcastLocal<T, U, O>(string command, T arg1, U arg2, O arg3)
        {
            if (!m_eventDic.ContainsKey(command)) return;
            Delegate @delegate;
            if (m_eventDic.TryGetValue(command, out @delegate))
            {
                Action<T, U, O> call = @delegate as Action<T, U, O>;
                if (call != null)
                {
                    lock (m_delegateQueue)
                    {
                        m_delegateQueue.Enqueue(() =>
                        {
                            call(arg1, arg2, arg3);
                        });
                    }
                }
                else
                {
                    Debug.LogError("对应key的De'le'gate为空 Key:" + command);
                }
            }
        }
        public void BroadcastLocal<T, U, O, P>(string command, T arg1, U arg2, O arg3, P arg4)
        {
            if (!m_eventDic.ContainsKey(command)) return;
            Delegate @delegate;
            if (m_eventDic.TryGetValue(command, out @delegate))
            {
                Action<T, U, O, P> call = @delegate as Action<T, U, O, P>;
                if (call != null)
                {
                    lock (m_delegateQueue)
                    {
                        m_delegateQueue.Enqueue(() =>
                        {
                            call(arg1, arg2, arg3, arg4);
                        });
                    }
                }
                else
                {
                    Debug.LogError("对应key的De'le'gate为空 Key:" + command);
                }
            }
        }
        public void BroadcastLocal<T, U, O, P, Q>(string command, T arg1, U arg2, O arg3, P arg4, Q arg5)
        {
            if (!m_eventDic.ContainsKey(command)) return;
            Delegate @delegate;
            if (m_eventDic.TryGetValue(command, out @delegate))
            {
                Action<T, U, O, P, Q> call = @delegate as Action<T, U, O, P, Q>;
                if (call != null)
                {
                    lock (m_delegateQueue)
                    {
                        m_delegateQueue.Enqueue(() =>
                        {
                            call(arg1, arg2, arg3, arg4, arg5);
                        });
                    }
                }
                else
                {
                    Debug.LogError("对应key的De'le'gate为空 Key:" + command);
                }
            }
        }
        public Type ToType(string name)
        {
            var t = Type.GetType(name);
            if (t != null)
            {
                return t;
            }
            t = Type.GetType(name + ",UnityEngine");
            return t;
        }


        #endregion

    }
}
