using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

public interface IButton_DOTween
{
    /// <summary>
    /// 获取是否被选中
    /// </summary>
    /// <returns></returns>
    bool GetSelected();
    /// <summary>
    /// 设置被选中
    /// </summary>
    /// <param name="value"></param>
    void SetSelected(bool value);
    /// <summary>
    /// 当鼠标进入
    /// </summary>
    UnityEvent PointerEnter { get; set; }
    /// <summary>
    /// 当鼠标离开
    /// </summary>
    UnityEvent PointerExit { get; set; }
    /// <summary>
    /// 当鼠标按下
    /// </summary>
    UnityEvent PointerDown { get; set; }
    /// <summary>
    /// 当鼠标弹起
    /// </summary>
    UnityEvent PointerUp { get; set; }
    ButtonClickedEvent Clicked { get; set; }
}
public abstract class BasicButton_DOTween : Button, IButton_DOTween
{
    protected UnityEvent m_pointerEnter = new UnityEvent();
    protected UnityEvent m_pointerExit = new UnityEvent();
    protected UnityEvent m_pointerDown = new UnityEvent();
    protected UnityEvent m_pointerUp = new UnityEvent();
    protected bool m_isSelected = false;
    protected abstract void PointerEnter_Method();
    protected abstract void PointerExit_Method();
    protected abstract void PointerDown_Method();
    protected abstract void PointerUp_Method();
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if (!m_isSelected)
        {
            PointerEnter_Method();
        }
        m_pointerEnter.Invoke();
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (!m_isSelected)
        {
            PointerExit_Method();
        }
        m_pointerExit.Invoke();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        PointerDown_Method();
        m_pointerDown.Invoke();
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        PointerUp_Method();
        m_pointerUp.Invoke();
    }
    public UnityEvent PointerEnter { get => m_pointerEnter; set => m_pointerEnter = value; }
    public UnityEvent PointerExit { get => m_pointerExit; set => m_pointerExit = value; }
    public UnityEvent PointerDown { get => m_pointerDown; set => m_pointerDown = value; }
    public UnityEvent PointerUp { get => m_pointerUp; set => m_pointerUp = value; }
    public ButtonClickedEvent Clicked { get => onClick; set => onClick = value; }

    public virtual bool GetSelected()
    {
        return m_isSelected;
    }
    public virtual void SetSelected(bool value)
    {
        if (m_isSelected == true && value == false)
        {
            PointerExit_Method();
        }
        else if (m_isSelected == false && value == true)
        {
            PointerEnter_Method();
        }
        interactable = !value;
        m_isSelected = value;
    }
}
public class Button_DOTween : BasicButton_DOTween
{
    [SerializeField]
    private DOTweenAnimation[] m_highlight_ani;
    [SerializeField]
    private DOTweenAnimation[] m_click_ani;
    [SerializeField]
    private Text m_text;

    protected override void Awake()
    {
        base.Awake();
        if (m_text)
        {
            //m_text = GetComponentInChildren<Text>();
            m_text.color = colors.normalColor;
        }
    }
    protected override void PointerEnter_Method()
    {
        foreach (var item in m_highlight_ani)
        {
            item?.DOPlayForward();
        }
        if (m_text)
        {
            m_text.color = colors.highlightedColor;
        }
    }
    protected override void PointerExit_Method()
    {
        foreach (var item in m_highlight_ani)
        {
            item?.DOPlayBackwards();
        }
        if (m_text)
        {
            m_text.color = colors.normalColor;
        }
    }
    protected override void PointerDown_Method()
    {
        foreach (var item in m_click_ani)
        {
            item?.DOPlayForward();
        }
        if (m_text)
        {
            m_text.color = colors.pressedColor;
        }
    }
    protected override void PointerUp_Method()
    {
        foreach (var item in m_click_ani)
        {
            item?.DOPlayBackwards();
        }
        if (m_text)
        {
            m_text.color = colors.normalColor;
        }
    }
    public override void SetSelected(bool value)
    {
        base.SetSelected(value);
    }

    protected override void Reset()
    {
        base.Reset();
        var nav = Navigation.defaultNavigation;
        nav.mode = Navigation.Mode.None;
        navigation = nav;
        transition = Transition.None;
    }
}
