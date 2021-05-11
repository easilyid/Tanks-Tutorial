using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 3;            //用来识别不同的玩家
    public Rigidbody m_Shell;                 //壳子的预设 
    public Transform m_FireTransform;         //生成炮弹的坦克的一个子体
    public Slider m_AimSlider;               //显示当前发射部队的坦克的子元素。
    public AudioSource m_ShootingAudio;       //参考用于播放拍摄音频的音频源。注:不同于运动音源。
    public AudioClip m_ChargingClip;         //当每次射击都在充电时播放的音频。
    public AudioClip m_FireClip;             //每次射击时播放的音频。
    public float m_MinLauchForce = 15f;      //当不按下开火按钮时给she11的力。
    public float m_MaxLauchForce = 30f;      //在最大充电时间内，按下发射按钮后炮弹所受的力。
    public float m_MaxChargrTime = 0.75f;    //炮弹在最大威力发射前能充能多久。


    private string m_FireButton;              //。用于启动shel1的输入轴。
    private float m_CurrentLaunchForce;        //当发射按钮被释放时，炮弹所受到的力。
    private float m_ChargeSpeed;             //基于最大冲锋时间的发射力增加速度。
    private bool m_Fired;                   //shel1是否已按下此按钮启动。



    private void OnEnable()
    {
        //坦克打开后，重置发射力和UI
        m_CurrentLaunchForce = m_MinLauchForce;
        m_AimSlider.value = m_MinLauchForce;
    }

    private void Start()
    {
        // 火力轴以玩家号为基准
        m_FireButton = "Fire" + m_PlayerNumber;

        //发射力充能的速度，就是最大充能时间所可能的力的范围
        m_ChargeSpeed = (m_MaxLauchForce - m_MinLauchForce) / m_MaxChargrTime;
    }

    private void Update()
    {
        // 滑块应该有一个最小发射力的默认值。
        m_AimSlider.value = m_MinLauchForce;

        // 如果超过了最大力量，而炮弹还没有发射... ...
        if (m_CurrentLaunchForce>=m_MaxLauchForce && !m_Fired)
        {
            // ...... 使用最大的力量，发射炮弹。
            m_CurrentLaunchForce = m_MaxLauchForce;
            Fire();
        }
        // 否则，如果刚开始按下火警按钮时
        else if (Input.GetButtonDown(m_FireButton))
        {
            //...... 重置发射旗帜和重置发射力。
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLauchForce;

            //改变片段并开始播放
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();

        }
        // 否则，如果开火按钮被按住，而炮弹还没有发射... ...
        else if (Input.GetButton(m_FireButton)&&!m_Fired)
        {
            // 增加发射力并更新滑块。
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

            m_AimSlider.value = m_CurrentLaunchForce; 
        }
        //否则，如果开火按钮被松开，炮弹还没有发射... ...
        else if (Input.GetButtonUp(m_FireButton)&&!m_Fired)
        {
            // ... 启动shell。
            Fire();
        }
    }

    private void Fire()
    {
        //设置fire标志，所以只有Fire只被调用一次。
        m_Fired = true;

        // 创建一个shell的实例，并存储一个对它的刚体的引用。
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        //将炮弹的速度设置为火力位置前进方向的发射力。
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        //将弹夹改为射击弹夹并播放。
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        //重置发射力。 这是为了防止遗漏按钮事件时的预防措施。
        m_CurrentLaunchForce = m_CurrentLaunchForce = m_MinLauchForce;
    }
}
