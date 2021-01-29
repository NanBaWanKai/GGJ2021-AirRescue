using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelInfo
{
    /// <summary>
    /// �ؿ�����
    /// </summary>
    public string m_levelName;
    /// <summary>
    /// �����صĳ�������
    /// </summary>
    public string m_sceneName;
    /// <summary>
    /// ������Ӧ�˵���ͼ
    /// </summary>
    public Sprite m_sprite;
}
public class LevelManager : MonoBehaviour
{
    public LevelInfo[] Levels => m_levels;

    [SerializeField]
    LevelInfo[] m_levels;



}
