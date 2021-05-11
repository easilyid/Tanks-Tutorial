using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDirectionControl : MonoBehaviour
{
    //这个类是用来确保世界空间的UI
    //健康栏等元素指向正确的方向

    public bool m_UseRelativeRotation = true;     //使用相对旋转是否应用于这个游戏对象


    private Quaternion m_RelativeRotation;       //场景开始时的局部旋转

    private void Start()
    {
        m_RelativeRotation = transform.parent.localRotation;
    }

    private void Update()
    {
        if(m_UseRelativeRotation)
        {
            transform.rotation = m_RelativeRotation;
        }
    }
}
