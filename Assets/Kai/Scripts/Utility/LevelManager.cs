using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelInfo
{
    /// <summary>
    /// 关卡名称
    /// </summary>
    public string m_levelName;
    /// <summary>
    /// 所加载的场景名称
    /// </summary>
    public string m_sceneName;
    /// <summary>
    /// 场景对应菜单贴图
    /// </summary>
    public Sprite m_sprite;
}
public class LevelManager : MonoBehaviour
{
    public LevelInfo[] Levels => m_levels;

    [SerializeField]
    LevelInfo[] m_levels;



}
