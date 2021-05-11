using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;          // 用于识别哪辆坦克属于哪位玩家。这是由这个坦克的经理设定的。
    public float m_Speed = 12f;            // 坦克前进和后退的速度
    public float m_TurnSpeed = 180f;       //  坦克每秒转多少度，.
    public AudioSource m_MovementAudio;    //  引用用于播放引擎声音的音频源
    public AudioClip m_EngineIdling;       // 当坦克不移动时播放的音频
    public AudioClip m_EngineDriving;      //  当坦克移动时播放的音频
    public float m_PitchRange = 0.2f;      // 发动机噪音可改变的量

    private string m_MovementAxisName;          // 输入轴的名称用于前进或后退
    private string m_TurnAxisName;              // 输入轴的名称用于转弯
    private Rigidbody m_Rigidbody;              // 参考用来移动坦克
    private float m_MovementInputValue;         // 移动输入的当前值。
    private float m_TurnInputValue;             // 输入当前的值
    private float m_OriginalPitch;              // 音频源在场景开始时的音高。
    private ParticleSystem[] m_particleSystems; // 对储罐使用的所有粒子系统的引用

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        //当坦克打开时，确定他不是运动的
        m_Rigidbody.isKinematic = false;

        //重置输入的值
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;

        //获取坦克的所有粒子系统的子系统，以便能够在停用/激活时停止/玩他们
        //这是必要的，因为我们移动坦克生成时，如果粒子系统正在播放
        //从0点移动到刷出点，创造一个巨大的烟雾尾迹
        //m_particleSystems = GetComponentInChildren<ParticleSystem>();
        //for(int i=0; i<m_particleSystems.Length;++i)
        //{
        //    m_particleSystems[i].Play();
        //}
    }

    private void OnDisable()
    {
        //当坦克关闭，设置它的运动学，使它停止移动。
        m_Rigidbody.isKinematic = true;

        //停止所有粒子系统，让它“重置”它的位置到实际的一个，而不是认为我们移动时，产卵
        //for(int i=0; i<m_particleSystems.Length;++i)
        //{
        //    m_particleSystems[i].Stop();
        //}

    }

    private void Start()
    {
        //坐标轴的名称是基于玩家编号的
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        //存储音频源的原始音高
        m_OriginalPitch = m_MovementAudio.pitch;
        
    }

    private void Update()
    {
        //存储玩家的输入并确保引擎的音频正在播放。
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        EngineAudio();
    }

    private void EngineAudio()
    {
        //根据坦克是否在移动以及当前播放的音频设置正确的音频提示。
        if(Mathf.Abs(m_MovementInputValue)<0.1f&&Mathf.Abs(m_TurnInputValue)<0.1f)
        {
            //如果音频源当前正在播放驾驶剪辑
            if(m_MovementAudio.clip==m_EngineDriving)
            {
                //将音频更改为闲置并播放。
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            //否则，如果坦克正在移动，如果空转夹当前
            if(m_MovementAudio.clip==m_EngineIdling)
            {
                //将音频更改为驾驶和播放
                m_MovementAudio.clip =m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
        Turn();
    }

    private void Move()
    {
        //在坦克面对的方向上创建一个矢量，基于输入，速度和帧之间的时间大小
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

        //将此运动应用到刚体的位置
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    private void Turn()
    {
        //根据帧与帧之间的输入、速度和时间，确定要翻转的角度数。
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

        //使他在Y轴上旋转
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        //将此旋转应用于刚体的旋转。
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}
