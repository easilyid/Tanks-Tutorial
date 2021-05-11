using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask;                      //用于过滤影响爆炸的图层
    public ParticleSystem m_ExplosionParticles;        //参考粒子发挥爆炸
    public AudioSource m_ExplosionAudio;              //引用将在爆炸上播放的音频。
    public float m_MaxDamage = 100f;                 //当爆炸集中在坦克上时所造成的损害
    public float m_ExplosionForce = 1000f;           //在爆炸中心注入坦克的力量
    public float m_MaxLifeTime = 2f;                //shell被移除前的时间（以秒为单位）
    public float m_ExplosionRadius = 5f;            //距离爆炸罐的最大距离


    private void Start()
    {
        //如果到那时它还没有被摧毁，在它的使用寿命之后摧毁它。
        Destroy(gameObject, m_MaxLifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        //收集一个球体中从炮弹当前位置到爆炸半径的所有碰撞器；
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        //检查所有的碰撞器
        for(int i=0;i<colliders.Length;i++)
        {
            //找到他们的刚体
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            //如果他们没有刚体，继续下一个
            if (!targetRigidbody)
                continue;

            //添加爆炸力
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            //找到与刚体关联的TankHealth脚本。
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

            //如果游戏对象上没有附加TankHealth脚本，则继续到下一个碰撞器。
            if (!targetHealth)
                continue;

            //根据目标与炮弹的距离计算其所承受的伤害。
            float damage = CalculateDamage(targetRigidbody.position);

            //对坦克造成此伤害。
            targetHealth.TakeDamage(damage);

            //从外壳取消粒子的父级。
            m_ExplosionParticles.transform.parent = null;

            //玩粒子系统
            m_ExplosionParticles.Play();

            //播放爆炸音效
            m_ExplosionAudio.Play();

            // 一旦粒子完成，摧毁他们所在的游戏对象。
            ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
            Destroy(m_ExplosionParticles.gameObject, mainModule.duration);

            // 消灭物体
            Destroy(gameObject);
        }
    }

    //计算伤害
    private float CalculateDamage(Vector3 targetPosition)
    {
        // 创建一个外壳到目标的向量
        Vector3 explosionToTarget = targetPosition - transform.position;

        // 计算炮弹到目标的距离
        float explosionDistance = explosionToTarget.magnitude;

        // 计算目标离开最大距离的比例
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

        //按最大伤害的比例计算伤害
        float damage = relativeDistance * m_MaxDamage;

        // 确保最小伤害是0
        damage = Mathf.Max(0f, damage);

        return damage;
    }
}
