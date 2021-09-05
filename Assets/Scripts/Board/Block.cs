using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    //블록 클래스 

    private float m_fSpeed = 5f;

    //2차원 배열에서의 블록 좌표
    public Coordinate Coord { get; private set; }

    //기본 블록 prefab들을 저장하는 Board class의 m_ListPrefabBlocks 배열에서의 인덱스 번호
    //인덱스 번호 기준으로 3매치 검사를 수행할 것임 

    public BLOCKSTATUS BlockStatus { get; private set; }

    private Vector3 m_vecDestination;
    private Vector3 m_vecPrevPos;

    private Coordinate m_coordPrev;
    public Coordinate CoordDestination { get; private set; }

    private SpriteRenderer m_spriteRenderer;

    public void Init(Coordinate coord, Vector3 Pos)
    {
        this.BlockStatus = BLOCKSTATUS.NORMAL;

        this.Coord = coord;
        CoordDestination = this.Coord;
        m_coordPrev = this.Coord;

        transform.position = Pos;
        m_vecDestination = transform.position;
        m_vecPrevPos = transform.position;

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_spriteRenderer.sortingLayerName = "Block";
        m_spriteRenderer.sortingOrder = 0;
    }

    public void SetPos(Coordinate TargetCoord, Vector3 TargetPos)
    {
        //shuffle 할 때 위치를 정해줄 함수

        //이동 전 현위치 저장(타겟 위치로 이동할 필요가 없어서 이전 위치로 돌아가야 할 때 사용할 예정)
        m_coordPrev = this.Coord;
        m_vecPrevPos = transform.position;

        this.Coord = TargetCoord;

        transform.position = TargetPos;

    }

    public void SetDestination(Coordinate TargetCoord, Vector3 TargetPos)
    {
        //이동 전 현위치 저장(매치되지 않아서 다시 돌아가야 할 때 사용할 예정)
        m_coordPrev = this.Coord;
        m_vecPrevPos = transform.position;

        //목적지를 정해준다. 
        CoordDestination = TargetCoord;
        m_vecDestination = TargetPos;

        //움직일 예정이기 때문에 이 함수가 호출되었음 -> 블록의 상태를 MOVE로 설정 
        BlockStatus = BLOCKSTATUS.MOVE;
    }

    public void MovePosition()
    {
        //정해진 목적지로 이동 수행
        transform.position = Vector3.MoveTowards(transform.position, m_vecDestination, m_fSpeed * Time.deltaTime);

        //이동이 끝나면 블록의 상태를 NORMAL로 설정하고 블록이 가진 좌표도 바꿔준다. 
        if (m_vecDestination == transform.position)
        {
            this.Coord = CoordDestination;

            BlockStatus = BLOCKSTATUS.NORMAL;
        }
    }

    public void GoBackToPrevPos(bool bMoving)
    {
        switch (bMoving)
        {
            case true://블록이 실제 움직일 경우 
                SetDestination(m_coordPrev, m_vecPrevPos);
                break;
            case false://이동 가능 블록검사 등 실제로 움직이지는 않을 경우 
                SetPos(m_coordPrev, m_vecPrevPos);
                break;
        }
    }

    public void SetStatus(BLOCKSTATUS eStatus)
    {
        this.BlockStatus = eStatus;
    }
}
