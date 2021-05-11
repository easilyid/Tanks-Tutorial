using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TankManager 
{
    // 该类用于管理坦克的各种设置。
    // 它与GameManager类一起工作，控制坦克的行为。
    //以及玩家是否可以控制自己的坦克在。
    //游戏的不同阶段。

    public Color m_PlayerColor;                             // 这就是这个坦克的颜色。
    public Transform m_SpawnPoint;                          // 坦克产生时的位置和方向。
    [HideInInspector] public int m_PlayerNumber;             // 这指定了哪个球员的经理。
    [HideInInspector] public string m_ColoredPlayerText;      // 一个代表玩家的字符串，其号码的颜色与他们的坦克相匹配。
    [HideInInspector] public GameObject m_Instance;          // 创建坦克实例时的引用。
    [HideInInspector] public int m_Wins;                    // 该玩家目前的胜利次数。


    private TankMovement m_Movement;                        // 引用坦克的移动脚本，用于禁用和启用控制。
    private TankShooting m_Shooting;                        // 参考坦克的射击脚本，用于禁用和启用控制。
    private GameObject m_CanvasGameObject;                  // 用于在每轮开始和结束阶段禁用世界空间界面。


    public void Setup()
    {
        // 获取对组件的引用。
        m_Movement = m_Instance.GetComponent<TankMovement>();
        m_Shooting = m_Instance.GetComponent<TankShooting>();
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

        // 设置各脚本中的玩家数量一致。
        m_Movement.m_PlayerNumber = m_PlayerNumber;
        m_Shooting.m_PlayerNumber = m_PlayerNumber;

        // 根据坦克的颜色和玩家的编号，用正确的颜色创建一个字符串，写上 "PLAYER 1 "等。
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

        // 获取坦克的所有渲染器。
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

        // 通过所有的渲染器... ...
        for (int i = 0; i < renderers.Length; i++)
        {
            //......将它们的材料颜色设置为本坦克特有的颜色。
            renderers[i].material.color = m_PlayerColor;
        }
    }


    // 在游戏中玩家不应该能够控制自己的坦克的阶段使用。
    public void DisableControl()       //禁用控制
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;

        m_CanvasGameObject.SetActive(false);
    }


    // 在游戏阶段使用，玩家应该能够控制自己的坦克。
    public void EnableControl()       //启用控制
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;

        m_CanvasGameObject.SetActive(true);
    }


    // 在每轮开始时使用，使坦克进入默认状态。
    public void Reset()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}
