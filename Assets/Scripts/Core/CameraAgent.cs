using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAgent : MonoBehaviour
{
    [SerializeField] Camera m_TargetCamera;//크기를 설정할 대상 카메라 
    [SerializeField] float m_BoardUnit;//카메라 넓이인데 원점을 기준으로 설정하기 때문에 넓이의 절반값을 입력한다. 
    //예를 들어 9x9 보드판 기준으로 블록 하나의 크기를 1이라고 할 때 9개의 블록이 화면에 모두 보이려면
    //가로 길이는 최소 9 이상이 되어야 하며 약간 여유있게 보기 좋은 그림을 연출하려면 10 정도가 되어야 한다.
    //이런 경우에 m_BoardUnit을 5로 설정한다. 

    void Start()
    {
        //Board 크기에 맞춰 카메라 높이를 자동으로 조절해주는 계산식
        //넓이를 화면 비율로 나눠주면 높이를 구할 수 있다. 
        m_TargetCamera.orthographicSize = m_BoardUnit / m_TargetCamera.aspect;
    }
}
