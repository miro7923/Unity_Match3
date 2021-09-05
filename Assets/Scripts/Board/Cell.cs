using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//블록의 배경 클래스 
public class Cell : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;

    public void Init(Vector3 Pos)
    {
        transform.position = Pos;

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sortingLayerName = "Cell";
    }

    public Vector3 GetPos()
    {
        return transform.position;
    }

    public Size GetSize()
    {
        //Board 크기를 구할 용도로 구하는 Cell 하나의 크기 
        var sprite = GetComponent<SpriteRenderer>();
        Size size = new Size(0, 0);
        if (null != sprite)
        {
            float fImageWidth = sprite.bounds.size.x;
            float fImageHeight = sprite.bounds.size.y;
            size = new Size(fImageWidth, fImageHeight);
        }

        return size;
    }
}
