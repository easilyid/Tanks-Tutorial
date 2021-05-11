using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;                //每辆坦克开始时的生命
    public Slider m_Slider;                             //用来表示坦克当前有多少生命值的滑块。
    public Image m_FillImage;                           //图像的滑块组件
    public Color m_FullHealthColor = Color.green;        //当处于完全健康状态下时，健康条的颜色为绿
    public Color m_ZeroHealthColor = Color.red;          //当处于非健康状态下时，健康条的颜色为红
    public GameObject m_ExplosionPrefb;                 //一个预制件，将被实例化在觉醒，然后使用坦克死亡。

    private AudioSource m_ExplosionAudio;                //当坦克爆炸时播放的音频源，
    private ParticleSystem m_ExplosionParticles;         //当坦克被摧毁时，所运行的粒子系统
    private float m_CurrentHealth;                      //坦克当前有多少血量
    private bool m_Dead;                               //坦克是否死亡
   

    private void Awake()
    {
        //实例化爆炸预制件并在其上获得粒子系统的引用。
        m_ExplosionParticles = Instantiate(m_ExplosionPrefb).GetComponent<ParticleSystem>();

        //在实例化预制件上获得音频源的引用
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        //禁用预制件，以便在需要的时候激活它。
        m_ExplosionParticles.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        //当坦克被启用时，重置坦克的生命值和是否死亡。
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        //更新运行状况滑块的值和颜色。
        SetHealthUI();
    }

    //减少当前的健康状况
    public void TakeDamage(float amount)
    {
        //减少当前生命值的伤害量
        m_CurrentHealth -= amount;

        //适当的更改UI元素
        SetHealthUI();

        //检查我们是否已经死了
        if(m_CurrentHealth <=0f &&  !m_Dead)
        {
            OnDeath();
        }

    }

    private void SetHealthUI()
    {
        //设置滑块的值
        m_Slider.value = m_CurrentHealth;

        //根据起始运行状况的当前百分比，在选择的颜色之间插入条的颜色。
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    private void OnDeath()
    {
        //设置标记确保这个函数只被调用一次
        m_Dead = true;

        //移动实例化的爆炸预制到坦克的位置，并打开它
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        //玩粒子系统的坦克爆炸。
        m_ExplosionParticles.Play();

        //播放坦克爆炸音效。
        m_ExplosionAudio.Play();

       // Turn the tank off
        gameObject.SetActive(false);
    }


}
