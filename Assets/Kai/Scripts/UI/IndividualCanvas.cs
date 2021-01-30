using DG.Tweening;
using IFancing;
using IFancing.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualCanvas : PanelBase
{
    Transform m_panel;

    protected override void Awake()
    {
        base.Awake();
        m_panel = transform.GetTargetInChildren<Transform>("Panel");

    }

    protected override void SubscribeButtonClickEvent(string btnName)
    {
        base.SubscribeButtonClickEvent(btnName);
        switch (btnName)
        {
            case "ResetSceneBtn":

                break;
            case "RestartLevelBtn":
                Manager.Instance.Client.NetGameEvent.BroadcastGlobal(NetGameEvent.LOAD_LEVEL, -1);//·µ»ØWaitRoom
                break;
        }
    }

    protected override void SubscribeToggleValueChangeEvent(string toggleName, bool value)
    {
        base.SubscribeToggleValueChangeEvent(toggleName, value);
        switch (toggleName)
        {
            case "SpreadToggle":
                var dot = m_panel.GetComponent<DOTweenAnimation>();
                if (value)
                {
                    dot.DOPlayForward();
                }
                else
                {
                    dot.DOPlayBackwards();
                }
                break;
        }
    }
}
