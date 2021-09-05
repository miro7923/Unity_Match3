using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//플레이어에게 받은 입력을 처리할 클래스 
public class InputManager : MonoBehaviour
{
    private static InputManager m_Instance;
    public static InputManager GetInstance
    {
        get
        {
            SetSingleton();
            return m_Instance;
        }
    }

    private Vector2 m_vecStartTouchPos = Vector2.zero;
    private Vector2 m_vecEndTouchPos = Vector2.zero;

    private Rect m_BoardRect;
    private int m_iColumns;
    private int m_iRows;

    public Coordinate FirstCoord { get; private set; }
    public Coordinate TargetCoord { get; private set; }

    public DIRECTION SwipeDirection { get; private set; }

    private void Awake()
    {
        SetSingleton();
    }

    private static void SetSingleton()
    {
        if (null == m_Instance)
        {
            GameObject obj = GameObject.Find("InputManager");

            if (null == obj)
            {
                obj = new GameObject { name = "InputManager" };
                obj.AddComponent<InputManager>();
            }

            DontDestroyOnLoad(obj);
            m_Instance = obj.GetComponent<InputManager>();
        }
    }

    public void Init(int col, int row)
    {
        m_BoardRect = Board.GetInstance.GetBoardSize();
        m_iColumns = col;
        m_iRows = row;
    }

    private Vector2 GetWorldPosition()
    {
        //Screen position -> World position
        Vector2 vecWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        return vecWorldPos;
    }

    public bool isInputDown()
    {
        Vector2 vecInputDown = GetWorldPosition();
        if (Input.GetMouseButtonDown(0) && isValid(vecInputDown))
        {
            m_vecStartTouchPos = vecInputDown;
            m_vecEndTouchPos = m_vecStartTouchPos;
            FindFirstCoordOnBoard(m_vecStartTouchPos);

            return true;
        }
        else
            return false;
    }

    public INPUTUP_RESULT isInputUp()
    {
        Vector2 vecInputUp = GetWorldPosition();
        if (Input.GetMouseButtonUp(0))
        {
            if (isValid(vecInputUp))
            {
                m_vecEndTouchPos = vecInputUp;
                GetSwipeDirection();
                Debug.Log("direction: " + this.SwipeDirection);
                FindTargetCoord();

                return INPUTUP_RESULT.IN;
            }
            else
                return INPUTUP_RESULT.OUT;
        }
        else
            return INPUTUP_RESULT.NOT_SELECTED;
    }

    private void GetSwipeDirection()
    {
        //Swipe direction 확인을 위해 시작-종료 지점간 벡터 길이 구하기 
        Vector2 vecDragDirection = m_vecEndTouchPos - m_vecStartTouchPos;

        //이동길이가 너무 짧으면 스와이프를 하지 않은 것으로 간주 
        if (0.5f >= vecDragDirection.magnitude)
            this.SwipeDirection = DIRECTION.NONE;
        else
        {
            float fRadian = Mathf.Atan2(vecDragDirection.y, vecDragDirection.x);
            float iDegree = Mathf.RoundToInt(fRadian * Mathf.Rad2Deg);

            //계산된 각도에 따라 방향 판단 
            if ((135f < iDegree && 180f >= iDegree) || (-135f < iDegree && -180f >= iDegree))
                this.SwipeDirection = DIRECTION.LEFT;
            else if ((0f <= iDegree && 45f > iDegree) || (0f <= iDegree && -45f > iDegree))
                this.SwipeDirection = DIRECTION.RIGHT;
            else if (45f < iDegree && 135f > iDegree)
                this.SwipeDirection = DIRECTION.UP;
            else if (-135f < iDegree && -45f > iDegree)
                this.SwipeDirection = DIRECTION.DOWN;
            else
                this.SwipeDirection = DIRECTION.NONE;
        }
    }

    private bool isValid(Vector2 vecInputPos)
    {
        //입력받은 위치가 Board 안인지 확인
        return m_BoardRect.Contains(vecInputPos);
    }

    private void FindFirstCoordOnBoard(Vector2 vecInputPos)
    {
        //입력받은 위치를 이용해서 Board에서의 좌표를 찾음 
        float fCellWidth = Board.GetInstance.GetCellSize.fWidth;
        float fCellHeight = Board.GetInstance.GetCellSize.fHeight;

        //반복문에서 Board rect 시작점부터 cell 한 칸 크기씩 더해가면서 
        //입력받은 위치가 해당 바운더리에 들어오는지 확인 
        float fxMin = m_BoardRect.xMin;
        float fyMin = m_BoardRect.yMin;

        for (int y = 0; m_iColumns > y; y++)
        {
            float fyMax = fyMin + fCellHeight;
            if (fyMin < vecInputPos.y && fyMax > vecInputPos.y)
            {
                for (int x = 0; m_iRows > x; x++)
                {
                    float fxMax = fxMin + fCellWidth;
                    if (fxMin < vecInputPos.x && fxMax > vecInputPos.x)
                    {
                        this.FirstCoord = new Coordinate(x, y);
                        return;
                    }
                    fxMin += fCellWidth;
                }
            }
            fyMin += fCellHeight;
        }
    }

    private void FindTargetCoord()
    {
        int x = 0;
        int y = 0;

        //스와이프 방향에 따라 바꿀 대상이 되는 블록 위치 구하기 
        switch (this.SwipeDirection)
        {
            case DIRECTION.LEFT:
                x = this.FirstCoord.x - 1;
                y = this.FirstCoord.y;
                break;
            case DIRECTION.RIGHT:
                x = this.FirstCoord.x + 1;
                y = this.FirstCoord.y;
                break;
            case DIRECTION.UP:
                x = this.FirstCoord.x;
                y = this.FirstCoord.y + 1;
                break;
            case DIRECTION.DOWN:
                x = this.FirstCoord.x;
                y = this.FirstCoord.y - 1;
                break;
            default:
                x = this.FirstCoord.x;
                y = this.FirstCoord.y;
                break;
        }

        Coordinate coord = new Coordinate(x, y);
        //오작동을 방지하기 위해 위에서 구한 좌표가 현 스테이지 범위 안인지 확인 후
        //범위 안이면 목표 위치로 설정
        //아니라면 이동을 하지 않도록 목표 위치를 처음 위치와 같게 설정 
        if (coord.isValid(m_iColumns, m_iRows))
            this.TargetCoord = new Coordinate(x, y);
        else
            this.TargetCoord = this.FirstCoord;
    }
}
