using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f;              //相机对焦的时间
    public float m_ScreenEdgeBuffer = 4f;        //顶部/底部目标和屏幕边缘之间的空间。
    public float m_MinSize = 6.5f;              //摄像机所能达到的最小正投影尺寸。

    [HideInInspector] public Transform[] m_Targets;               //摄像机需要覆盖的所有目标。

    private Camera m_Camera;                   //用于定位相机
    private float m_ZoomSpeed;                 //为正交尺寸的平滑阻尼的参考速度。
    private Vector3 m_MoveVelocity;            //参考速度为平滑阻尼的位置。
    private Vector3 m_DesiredPosition;         // 摄像机移动的位置

    private void Awake()
    {
        m_Camera = GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        //移动到相机想要的位置
        Move();

        //改变相机的大小为基础。
        Zoom();
    }

    private void Move()
    {
        //找出目标的平均位置。
        FindAverAgePosition();

        //平稳地过渡到那个位置。
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }

    private void FindAverAgePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0;
        //寻找所有目标，把它们的位置加在一起。
        for (int i=0; i<m_Targets.Length;i++)
        {
            //如果目标不是活动的，继续到下一个。
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            //将平均值相加并增加平均值中的目标数目
            averagePos += m_Targets[i].position;
            numTargets++;
        }

        //如果目标数量大于0，将求出平均值
        if (numTargets > 0)
            averagePos /= numTargets;

        //保持y值不变
        averagePos.y = transform.position.y;

        //理想位置等于平均位置
        m_DesiredPosition = averagePos;

    }

    private void Zoom()
    {
        //根据需要的位置找到所需的尺寸，并平稳地过渡到该尺寸。
        float requiredSize = FindRequiredSize();
        m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
    }

    private float FindRequiredSize()       //查找所需尺寸
    {
        //在局部空间中找到摄像机移动的位置。
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

        //从0开始计算相机的大小
        float size = 0f;

        //检查所有目标
        for(int i=0;i<m_Targets.Length;i++)
        {
            //如果他们活跃就继续下一个目标
            if (!m_Targets[i].gameObject.activeSelf)
                continue;

            //否则，在相机的局部空间中找到目标的位置。
            Vector3 targetLocalPos = transform.InverseTransformPoint(m_Targets[i].position);

            //从相机的局部空间的期望位置找到目标的位置
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            //选择最大的当前尺寸和距离的坦克'上'或'下'从相机。
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            //从当前的尺寸中选择最大的尺寸，并根据坦克在相机的左边或右边计算出尺寸。
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
        }

            //将边缘缓冲区加到大小中
            size += m_ScreenEdgeBuffer;

            //确保相机的尺寸，不低于最小尺寸
            size = Mathf.Max(size, m_MinSize);

            return size;
        
        
    }

    public void SetStartPositionAndSize()
    {
        //找到理想的位置
        FindAverAgePosition();

        //设置相机想要的位置，没有阻碍
        transform.position = m_DesiredPosition;

        //查找和设置所需的相机大小
        m_Camera.orthographicSize = FindRequiredSize();
    }
}
