using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//블록이 떨어질 때 보드판 바깥에 대기하고 있다가 떨어지는데
//블록이 보드판 바깥에선 보이지 않다가 보드판 안쪽으로 들어왔을 때 보이는 효과를 보다 연출하기 위해
//Background 위에 Background의 절반을 자른 그림을 Background 위에 두어서 보드판 바깥에 있는 블록들을 가리고자 했다. 
//그렇기 때문에 BackgroundCover는 항상 Background 보다 상위에 있는 레이어에 속해야 한다.
//SpriteRenderer 컴포넌트 상에서 설정해도 되지만 Custom package를 이용해 컴퓨터를 옮겨가면서 작업하는 경우
//설정한 Sorting Layer가 해제되어 있는 경우를 종종 접했기 때문에
//코드를 통해서 Sorting Layer를 설정하고 해당되는 오브젝트들에 컴포넌트로 추가했다. 
public class BaseBackground : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;

    private void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        m_SpriteRenderer.sortingLayerName = "Background";
    }
}
