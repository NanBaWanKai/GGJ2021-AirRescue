using IFancing;
using IFancing.Net;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.RectTransform;

public class JoinRoomPanel : PanelBase
{
    [SerializeField]
    GameObject m_roomCellPrefab;
    [SerializeField]
    GridLayoutGroup m_grid;
    [SerializeField]
    GameObject m_content;
    [SerializeField]
    GameObject m_waitPanel;


    private Dictionary<string, GameObject> roomListEntries = new Dictionary<string, GameObject>();

    

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateRoomListView();
        Manager.Instance.Client.Launcher.OnRoomUpdate += OnRoomUpdate;
        Manager.Instance.Client.Launcher.OnJoinRoomFailedEvent += OnJoinRoomFailedEvent;

    }
    protected override void OnDisable()
    {
        base.OnDisable();
        Manager.Instance.Client.Launcher.OnRoomUpdate -= OnRoomUpdate;
        Manager.Instance.Client.Launcher.OnJoinRoomFailedEvent -= OnJoinRoomFailedEvent;


    }
    private void OnJoinRoomFailedEvent(short errorCode, string msg)
    {
        m_waitPanel.SetActive(false);
    }
    private void OnRoomUpdate(List<RoomInfo> roomInfo)
    {
        print("OnRoomUpdate");
        ClearRoomListView();
        UpdateRoomListView();
    }

    void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();
    }
    void UpdateRoomListView()
    {

        var list = Manager.Instance.Client.Launcher.CachedRoomList;
        print("UpdateRoom List "+list.Count);

        foreach (var k in list.Keys)
        {
            var info = list[k];
            var cell = Instantiate(m_roomCellPrefab, m_grid.transform);
            roomListEntries.Add(k, cell);
            var roomNameText = cell.GetTargetInChildren<TMP_Text>("RoomNameText");
            roomNameText.text = (string)info.CustomProperties[Launcher.ROOM_FACADE_NAME];
            var playerCountText = cell.GetTargetInChildren<TMP_Text>("PlayerCountText");
            playerCountText.text = info.PlayerCount.ToString() + "/" + info.MaxPlayers;
            var join_Btn = cell.GetTargetInChildren<Button>("JoinRoomBtn");
            join_Btn.onClick.AddListener(() =>
            {
                var option = new RoomOptions();
                option.CustomRoomProperties = info.CustomProperties;
                Manager.Instance.Client.Launcher.JoinOrCreateRoom(option);
                m_waitPanel.SetActive(true);
            });
        }
        var rect = m_content.GetComponent<RectTransform>();
        var h = (m_grid.cellSize.y + m_grid.spacing.y) * list.Keys.Count + 1000;
        rect.SetSizeWithCurrentAnchors(Axis.Vertical, h);
    }



}
