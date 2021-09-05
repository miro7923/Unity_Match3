using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    private static Board m_Instance;
    public static Board GetInstance
    {
        get
        {
            SetSingleton();
            return m_Instance;
        }
    }

    private int m_iColumns;
    private int m_iRows;

    //2차원 배열의 좌표에 블록들을 저장한 후 이동이 생겼을 때 저장된 객체들을 바꾸는 것 보다는 
    //블록 하나가 자신의 위치와 좌표를 가지고 있고 이동이 생기면 각 블록들이 위치와 좌표를 바꾸는 것이 더 낫다는 판단 하에 
    //실제로 움직일 블록들은 2차원 배열이 아닌 1차원 배열에 저장(블록 하나가 좌표를 가지고 있기 때문에 2차원 배열에 저장할 필요가 없음)
    //매치 검사를 수행할 때 블록 배열을 순회하면서 해당 위치를 기준으로 상하좌우에 연속되는 블록이 있는지 검사할 것임 
    private List<Block> m_ListBlocksOnBoard = new List<Block>();//움직일 블록들의 배열

    //매치 검사를 수행하면서 매치되는 블록들을 다른 배열에 임시로 저장한 후 매치 검사가 끝나면 제거 
    private List<Block> m_ListMatchedBlocks = new List<Block>();//매치된 블록들을 저장할 배열

    //제거되어 비활성화된 블록들을 저장할 배열(스테이지가 진행되는 동안 돌려쓸 것)
    private List<Block> m_ListBlocksToRefill = new List<Block>();

    //블록이 새로 생성되어야 할 위치를 저장할 배열
    private List<Coordinate> m_ListEmptyPos = new List<Coordinate>();

    //블록의 배경을 저장할 배열 
    private List<Cell> m_ListCells = new List<Cell>();

    private GameObject[] m_arrPrefabBlocks;//Default prefabs를 저장할 배열
    private GameObject m_PrefabCell;//블록의 배경에 있을 cell prefab을 저장할 변수 

    private List<Coordinate> m_ListCoordOnBoard = new List<Coordinate>();

    //InputManager에서 플레이어 입력 위치를 계산할 때 필요해서 만든 프로퍼티 
    public Size GetCellSize { get { return m_ListCells.First().GetSize(); } }

    public int Score { get; private set; }

    private void Awake()
    {
        SetSingleton();
        LoadPrefabs();
    }

    private static void SetSingleton()
    {
        if (null == m_Instance)
        {
            GameObject obj = GameObject.Find("Board");

            if (null == obj)
            {
                obj = new GameObject { name = "Board" };
                obj.AddComponent<Board>();
            }

            DontDestroyOnLoad(obj);
            m_Instance = obj.GetComponent<Board>();
        }
    }

    void LoadPrefabs()
    {
        m_arrPrefabBlocks = Resources.LoadAll<GameObject>("Prefabs/Candies");

        m_PrefabCell = Resources.Load<GameObject>("Prefabs/Cell");
    }

    public void SetGridSize(int col, int row)
    {
        m_iColumns = col;
        m_iRows = row;

        InitCells();
        CreateBlocks();
    }

    public Rect GetBoardSize()
    {
        //입력 유효 범위 계산을 위한 Cell 이미지 크기 구하기 
        float fCellWidth = m_ListCells.First().GetSize().fWidth;
        float fCellHeight = m_ListCells.First().GetSize().fHeight;

        //Board Rect 크기 구하기
        //Rect 구조체의 1,2번째는 사각형이 시작될 좌표 x,y이기 때문에 시작점을 구해야 하고 
        //3, 4번째 매개변수가 width와 height이기 때문에 총 길이를 구해서 넣어줘야 한다.
        //그래서 Cell Grid의 [0,0] 객체의 위치(중심점)에서 cell 이미지 크기의 절반을 빼면 시작점이 되고
        //cell 이미지 하나의 크기를 가로세로 길이만큼 곱하면 총 길이가 된다. 
        float fLeft = m_ListCells.First().GetPos().x - (fCellWidth * 0.5f);//시작점 x
        float fDown = m_ListCells.First().GetPos().y - (fCellHeight * 0.5f);//시작점 y
        float fRight = fCellWidth * m_iRows;//width
        float fUp = fCellHeight * m_iColumns;//height 

        Rect rect = new Rect(fLeft, fDown, fRight, fUp);

        return rect;
    }

    private void CreateBlocks()
    {
        //기본 블록들을 보드판에 들어가는 갯수보다 많게 생성해서 리필리스트에 저장 후
        //게임이 새로 시작될 때마다 여기에서 추출해서 사용한다. 
        //매치된 블록이 제거되고 나서 새로운 블록을 생성해야 할 때에도 
        //리필리스트에서 추출해서 사용할 것이다. 

        for (int i = 0; ; i++)
        {
            if (m_arrPrefabBlocks.Length <= i)
                i = 0;

            Coordinate Coord = new Coordinate(0, 0);

            Vector3 Pos = new Vector3(0, 0, 0);

            Block block = Instantiate(m_arrPrefabBlocks[i], transform).GetComponent<Block>();
            block.Init(Coord, Pos);
            block.gameObject.SetActive(false);
            m_ListBlocksToRefill.Add(block);

            if (m_arrPrefabBlocks.Length * Constants.TIMES_CREATEBLOCK <= m_ListBlocksToRefill.Count)
                break;
        }
    }

    private Vector3 GetPosOnCoord(Coordinate Coord)
    {
        //기준 좌표에서 보드판의 가로세로 길이를 반으로 나눈 값을 빼면 해당 좌표의 실제 위치값을 구할 수 있다.
        //블록간 간격을 1씩 설정 
        int x = Coord.x - (int)(m_iRows * 0.5f);
        int y = Coord.y - (int)(m_iColumns * 0.5f);

        Vector3 Pos = new Vector3(x, y, 0f);

        return Pos;
    }

    private void InitCells()
    {
        //보드판의 정 가운데를 영점으로 정하고 블록들의 위치(transform.position)를 정해줄 것인데
        //그러려면 한가운데에 있는 블록의 위치가 0,0이 될 것이다. 
        //그러면 한가운데에 있는 블록의 아래에 있는 블록들의 위치값은 음수(-)값을 가질 것이고 위에 있는 블록들은 양수값을 가질 것이다. 
        //9x9 크기를 가진 보드판의 가로세로 길이를 2로 나눈 몫만 구하면 4
        //영점을 기준으로 아래에 있는 블록들의 위치는 음수값을 가지고 위에 있는 블록들은 양수값을 가지기 때문에
        //위치값이 -4,-4 ~ 4,4까지 나올 수 있다. 
        //그런데 블록의 위치값과 좌표값을 동일하게 설정하면 수식은 간단해지겠지만 보기에 직관적이지 않기 때문에
        //좌표값은 0,0부터 시작하되 해당 좌표에 맞는 위치값을 구해서 각 블록에 넣어준다. 

        for (int y = 0; m_iColumns > y; y++)
        {
            for (int x = 0; m_iRows > x; x++)
            {
                Coordinate CurCoord = new Coordinate(x, y);

                Cell cell = Instantiate(m_PrefabCell, transform).GetComponent<Cell>();
                cell.Init(GetPosOnCoord(CurCoord));

                m_ListCells.Add(cell);

                //shuffle 때 쓸 좌표도 저장 
                m_ListCoordOnBoard.Add(CurCoord);
            }

        }
    }

    private void SaveBlocksInMatchedList(List<Block> ListBlocks)
    {
        //블록 배열을 순회하면서 모든 개별 블록을 대상으로 매칭검사를 진행하기 때문에 
        //중복되는 블록이 생길 수 있다. -> 중복검사 후 매치리스트에 저장 
        foreach (var block in ListBlocks)
        {
            if (!m_ListMatchedBlocks.Exists((obj) => obj.Equals(block)))
                m_ListMatchedBlocks.Add(block);
        }
    }

    private bool InspectHorizon(string tag, Coordinate coord)
    {
        //가로방향 검사
        //검사 시작 기준 위치를 제외하고 카운트하기 때문에 양방향 검사 카운트가 2 이상이면 3개 이상 매치된 것이라 볼 수 있다. 

        List<Block> ListLeftBlocks = new List<Block>();
        List<Block> ListRightBlocks = new List<Block>();

        ListLeftBlocks = CountMatchedBlocks(tag, coord, -1, 0);//왼쪽검사
        ListRightBlocks = CountMatchedBlocks(tag, coord, 1, 0);//오른쪽 검사 

        if (2 <= ListLeftBlocks.Count + ListRightBlocks.Count)
        {
            SaveBlocksInMatchedList(ListLeftBlocks);
            SaveBlocksInMatchedList(ListRightBlocks);

            return true;
        }
        return false;
    }

    private bool InspectVertical(string tag, Coordinate coord)
    {
        //세로방향 검사 
        //검사 시작 기준 위치를 제외하고 카운트하기 때문에 양방향 검사 카운트가 2 이상이면 3개 이상 매치된 것이라 볼 수 있다.

        List<Block> ListAboveBlocks = new List<Block>();
        List<Block> ListBelowBlocks = new List<Block>();

        ListAboveBlocks = CountMatchedBlocks(tag, coord, 0, -1);//윗방향 검사
        ListBelowBlocks = CountMatchedBlocks(tag, coord, 0, 1);//아랫방향 검사

        if (2 <= ListAboveBlocks.Count + ListBelowBlocks.Count)
        {
            SaveBlocksInMatchedList(ListAboveBlocks);
            SaveBlocksInMatchedList(ListBelowBlocks);

            return true;
        }
        return false;
    }

    public bool isMatched(Coordinate coord)
    {
        //개별 블록을 대상으로 검사 기준 블록의 다음 위치부터 매치 검사 수행
        //검사를 진행하면서 매치가 되는 블록들은 m_ListMatchedBlocks_Tmp에 임시로 저장했다가
        //3개 이상 매치된 것이 확실해지면 m_ListMatchedBlocks에 저장할 것임
        bool bMatched = false;
        Block block = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(coord));

        if (null != block)
        {
            //+ 모양으로 매치되는 경우도 있으니까 가로 검사가 끝나면 세로 방향 검사도 수행
            bool bHorizon = InspectHorizon(block.tag, block.Coord);
            bool bVertical = InspectVertical(block.tag, block.Coord);
            if (bHorizon || bVertical)
            {
                //양방향 검사 결과가 true라면 검사 시작점이 된 블록을 m_ListMatchedBlocks에 저장 후
                //임시 매치리스트에 있는 블록들도 m_ListMatchedBlocks에 저장

                //블록 배열을 순회하면서 모든 개별 블록을 대상으로 매칭검사를 진행하기 때문에 
                //중복되는 블록이 생길 수 있다. -> 중복검사 후 매치리스트에 저장 
                if (!m_ListMatchedBlocks.Exists((obj) => obj.Equals(block)))
                    m_ListMatchedBlocks.Add(block);

                bMatched = true;
            }
        }

        return bMatched;
    }

    public IEnumerator MoveBackBlocks(Coordinate FirstCoord, Coordinate TargetCoord)
    {
        Block FirstBlock = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(FirstCoord));
        Block TargetBlock = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(TargetCoord));

        FirstBlock.GoBackToPrevPos(true);
        TargetBlock.GoBackToPrevPos(true);

        yield return SwapPosition();
    }

    public bool InspectAllBlocks()
    {
        bool bMatched = false;

        //전체 블록을 대상으로 매치 검사 수행 
        foreach (var block in m_ListBlocksOnBoard)
        {
            //블록 배열을 순회하며 개별 블록을 대상으로 매칭 검사 수행하는데
            //생길 수 있는 모든 매칭 모양 확인을 위해서 마지막 블록까지 매칭검사 수행 
            if (isMatched(block.Coord))
                bMatched = true;
        }
        //하나라도 매치된 것이 있으면 true return
        //하나도 매치된 것이 없으면 false return 
        return bMatched;
    }

    private List<Block> CountMatchedBlocks(string tag, Coordinate Coord, int iAddX, int iAddY)
    {
        //매치 검사를 시작할 위치의 다음 위치부터 검사를 시작하도록 좌표(x,y)값을 정해준다. 
        //매개변수 값(iAddX, iAddY)에 따라 한 칸씩 이동하면서 검사
        // iAddX = 1: 오른쪽으로 이동
        // iAddX = -1: 왼쪽으로 이동
        // iAddY = 1: 위로 이동
        // iAddY = -1: 아래로 이동

        List<Block> ListMatchedBlock = new List<Block>();
        for (int x = Coord.x + iAddX, y = Coord.y + iAddY; ; x += iAddX, y += iAddY)
        {
            Coordinate CurCoord = new Coordinate(x, y);
            Block block = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(CurCoord));

            if (null != block && block.CompareTag(tag))
            {
                //해당 좌표에 있는 블록의 태그가 매개변수와 같으면 매치된 것 -> 매치리스트에 저장
                ListMatchedBlock.Add(block);
            }
            else
                break;//매치되지 않는 블록을 만나는 즉시 검사 종료 
        }
        return ListMatchedBlock;
    }

    public IEnumerator SetMovingPos(Coordinate FirstCoord, Coordinate TargetCoord)
    {
        if (FirstCoord.isValid(m_iColumns, m_iRows) && TargetCoord.isValid(m_iColumns, m_iRows))
        {
            //List에서 매개변수 위치의 블록을 찾은 후 각 블록들이 이동해야 할 목적지를 정해준다. 
            Block FirstBlock = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(FirstCoord));
            Block TargetBlock = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(TargetCoord));

            if (null != FirstBlock && null != TargetBlock)
            {
                FirstBlock.SetDestination(TargetCoord, GetPosOnCoord(TargetCoord));
                TargetBlock.SetDestination(FirstCoord, GetPosOnCoord(FirstCoord));

                yield return SwapPosition();
            }
        }
    }

    public IEnumerator SwapPosition()
    {
        //블록 리스트를 순회하면서 이동 상태인 블록 이동
        while (!MovingIsEnd())
        {
            foreach (var block in m_ListBlocksOnBoard)
            {
                if (BLOCKSTATUS.MOVE == block.BlockStatus)
                    block.MovePosition();
            }
            yield return null;
        }
    }

    public bool MovingIsEnd()
    {
        //블록 상태가 하나라도 MOVE면 이동이 아직 끝나지 않은 것임 
        foreach (var block in m_ListBlocksOnBoard)
        {
            if (BLOCKSTATUS.MOVE == block.BlockStatus)
                return false;
        }

        return true;
    }

    public void DeactivateBlocks()
    {
        if (0 < m_ListMatchedBlocks.Count)
        {
            //m_ListMatchedBlocks에 있는 블록들 제거
            //-> 비활성화 후 임시 보관소(m_ListClearedBlocks)에 저장 
            foreach (var block in m_ListMatchedBlocks)
            {
                block.gameObject.SetActive(false);

                m_ListBlocksToRefill.Add(block);

                m_ListBlocksOnBoard.Remove(block);
            }
            m_ListMatchedBlocks.Clear();
        }
    }

    private int GetDropDistance(Coordinate Coord)
    {
        int iCount = 0;
        for (int x = Coord.x, y = Coord.y - 1; 0 <= y; y--)
        {
            Coordinate CurCoord = new Coordinate(x, y);
            Block block = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(CurCoord));
            if (null == block)
                iCount++;
            else
                break;
        }
        return iCount;
    }

    public void SetDropPos()
    {
        //블록 하나의 수직 아래방향으로 빈 칸의 갯수를 구해서 이동해야 하는 거리를 구한다.
        //이동거리가 0보다 큰 블록과 그 수직 윗방향에 있는 블록들의 이동해야 할 위치를 거리만큼 정해준다. 
        foreach (var block in m_ListBlocksOnBoard)
        {
            int iDropDistance = GetDropDistance(block.Coord);
            if (0 < iDropDistance)
            {
                for (int x = block.Coord.x, y = block.Coord.y; m_iColumns > y; y++)
                {
                    Coordinate CurCoord = new Coordinate(x, y);
                    Coordinate DropCoord = new Coordinate(x, y - iDropDistance);

                    Block CurBlock = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(CurCoord));
                    if (null != CurBlock)
                        CurBlock.SetDestination(DropCoord, GetPosOnCoord(DropCoord));
                }
            }
        }
    }

    private void FindEmptyPos()
    {
        for (int y = 0; m_iColumns > y; y++)
        {
            for (int x = 0; m_iRows > x; x++)
            {
                //m_ListBlocksOnBoard에서의 검색결과가 null이면 빈 위치 
                Coordinate CurCoord = new Coordinate(x, y);

                Block block = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(CurCoord));
                if (null == block)
                    m_ListEmptyPos.Add(CurCoord);
            }
        }
    }

    public IEnumerator RefillPositions()
    {
        //빈 칸을 채울 블록 생성 & 드롭 전에 빈 칸 먼저 찾고 시작 
        FindEmptyPos();

        if (0 < m_ListEmptyPos.Count)
        {
            //Board 밖에 있다가 차례대로 안으로 내려오는 효과 연출을 위해
            //새로 생성된 블록의 y좌표는 m_iColumns보다 높은 위치로 정해줄 것인데
            //새로 생성된 블록 중 가장 아래에 있는 블록부터 순서대로 드롭되도록 각 블록들의 대기위치를
            //[이동해야하는 빈 좌표에서 최상단에서 빈 좌표까지의 드롭거리+1] 만큼 정해줄 것이다. 
            //드롭거리는 가장 아래에 있는 빈 칸과 최상단 줄까지의 비어있는 거리를 구하면 되는데 
            //빈 좌표를 찾을 때엔 항상 첫번째 블록부터 시작해서 위로 올라가기 때문에 
            //m_ListEmptyPos의 마지막 좌표가 최상단 줄에 있는 위치라고 볼 수 있다.

            int iDropDistance = GetDropDistance(m_ListEmptyPos.Last());

            foreach (var EmptyCoord in m_ListEmptyPos)
            {
                int iRandomIndex = Random.Range(0, m_ListBlocksToRefill.Count - 1);

                Coordinate CoordToWait = new Coordinate(EmptyCoord.x, EmptyCoord.y + iDropDistance + 1);

                Block NewBlock = m_ListBlocksToRefill[iRandomIndex];
                NewBlock.gameObject.SetActive(true);
                NewBlock.SetPos(CoordToWait, GetPosOnCoord(CoordToWait));
                NewBlock.SetDestination(EmptyCoord, GetPosOnCoord(EmptyCoord));

                m_ListBlocksOnBoard.Add(NewBlock);

                m_ListBlocksToRefill.Remove(NewBlock);
            }

            m_ListEmptyPos.Clear();

            yield return SwapPosition();
        }
    }

    private Coordinate GetNextCoord(Coordinate BaseCoord, DIRECTION eDir)
    {
        //매개변수 방향에 따라 검사대상이 될 옆 블록 좌표 구하기 
        Coordinate NextCoord = new Coordinate(0, 0);

        switch (eDir)
        {
            case DIRECTION.LEFT:
                NextCoord = new Coordinate(BaseCoord.x - 1, BaseCoord.y);
                break;
            case DIRECTION.RIGHT:
                NextCoord = new Coordinate(BaseCoord.x + 1, BaseCoord.y);
                break;
            case DIRECTION.UP:
                NextCoord = new Coordinate(BaseCoord.x, BaseCoord.y + 1);
                break;
            case DIRECTION.DOWN:
                NextCoord = new Coordinate(BaseCoord.x, BaseCoord.y - 1);
                break;
            default:
                NextCoord = BaseCoord;
                break;
        }

        return NextCoord;
    }

    public bool CanMatchOnBoard()
    {
        //개별 블록을 대상으로
        //상하좌우에 있는 옆 블록과 스와이프 했을 때 매치되는 것이 있는지 확인
        //1번 블록의 좌표를 옆 블록과 바꾼다
        //-> 그 상태에서 매칭검사를 한다.
        //-> 매치되는 것이 있으면 true -> 섞을 필요 없음 
        //-> 마지막 블록까지 반복해서 없으면 false -> 전체 블록 섞기 
        //블록 하나에 대한 검사가 끝나면 다시 원위치로 돌리고 다음 블록으로 진행

        bool bCanMatch = false;

        foreach (var CurBlock in m_ListBlocksOnBoard)
        {
            //방향이 4개니까 4번 반복하는데 검사 불가능한 방향은 건너뛰기 
            for (int i = 0; 4 > i; i++)
            {
                //i의 크기에 따라 검사 방향을 정하기...
                DIRECTION eDir = DIRECTION.NONE;
                switch (i)
                {
                    case 0:
                        eDir = DIRECTION.LEFT;
                        break;
                    case 1:
                        eDir = DIRECTION.RIGHT;
                        break;
                    case 2:
                        eDir = DIRECTION.UP;
                        break;
                    case 3:
                        eDir = DIRECTION.DOWN;
                        break;
                }

                Coordinate CurCoord = CurBlock.Coord;
                Coordinate NextCoord = GetNextCoord(CurCoord, eDir);

                //리턴받은 좌표가 Board 안인지 확인 후 검사하기 
                if (NextCoord.isValid(m_iColumns, m_iRows))
                {
                    Block NextBlock = m_ListBlocksOnBoard.Find((obj) => obj.Coord.Equals(NextCoord));

                    CurBlock.SetPos(NextCoord, GetPosOnCoord(NextCoord));
                    NextBlock.SetPos(CurCoord, GetPosOnCoord(CurCoord));

                    if (InspectAllBlocks())
                    {
                        bCanMatch = true;
                        m_ListMatchedBlocks.Clear();
                    }

                    CurBlock.GoBackToPrevPos(false);
                    NextBlock.GoBackToPrevPos(false);

                    if (bCanMatch)
                        break;
                }
            }
        }

        return bCanMatch;
    }

    public void ShuffleBlocks(bool bNeedToDrop)
    {
        //블록들을 섞어주는 함수

        ResetBoard();//만약 보드판에 블록이 있다면 비워주기 위해 호출하는 함수 

        //블록리스트에서 랜덤인덱스 블록을 뽑아서 빈 좌표의 처음부터 차례대로 채우는데
        //해당 위치에 블록을 놓았을 때 매치가 되는지 검사가 필요함
        //-> 매치가 되면 블록을 다시 뽑아야하고 되지 않으면 배치하면 됨
        for (int i = 0; m_ListCoordOnBoard.Count > i;)
        {
            int iRandomIndex = Random.Range(0, m_ListBlocksToRefill.Count);
            Block CurBlock = m_ListBlocksToRefill[iRandomIndex];
            Coordinate CurCoord = m_ListCoordOnBoard[i];

            if (InspectHorizon(CurBlock.tag, CurCoord) || InspectVertical(CurBlock.tag, CurCoord))
            {
                m_ListMatchedBlocks.Clear();//매칭 검사가 끝나면 다음 검사를 위해 매치된 블록들을 저장하는 리스트를 비워준다. 
                continue;
            }

            switch (bNeedToDrop)
            {
                //블록이 섞이는 함수가 호출되는 상황은 게임을 새로 시작할 때와
                //게임 플레이 중 이동 가능한 블록이 없어 섞어줘야 하는 상황인데 두 경우 애니메이션 연출만 다르다.
                //함수를 재사용할 수 있도록 bool 매개변수를 이용한다. 
                case true://아래로 드롭되는 연출이 필요할 경우 배치될 위치를 목적지로 설정해준다. 
                    CurBlock.SetDestination(CurCoord, GetPosOnCoord(CurCoord));
                    break;
                case false://드롭 연출이 필요없는 경우 블록의 초기 위치로 설정해준다. 
                    CurBlock.Init(CurCoord, GetPosOnCoord(CurCoord));
                    break;
            }

            //위의 과정을 통과해서 블록이 보드판에 배치되면 활성화 
            CurBlock.gameObject.SetActive(true);
            //보드판 배열에 블록을 추가하고 Refill List에서는 해당 블록을 삭제한다. 
            m_ListBlocksOnBoard.Add(CurBlock);
            m_ListBlocksToRefill.Remove(CurBlock);
            i++;
        }

        //블록이 위에서 아래로 떨어지는 효과를 연출하기 위해 보드판 바깥으로 현위치 설정
        if (bNeedToDrop)
            SetBlockPosOnTheTop();
    }

    private void SetBlockPosOnTheTop()
    {
        //각 블록들이 보드판의 상단에서 목적지까지 직선으로 떨어지는 효과를 연출하기 위해 
        //각 블록들의 현 위치 x = 목적지 x
        //y = 목적지 y + 세로길이 만큼 설정해준다. 
        //목적지가 0,0이라면 현위치는 0,m_iColumns로 설정해
        //이동이 시작되면 보드판 위에서 목적지까지 떨어지도록 설정
        foreach (var block in m_ListBlocksOnBoard)
        {
            Coordinate Coord = new Coordinate
                (block.CoordDestination.x, block.CoordDestination.y + m_iColumns + 1);

            block.SetPos(Coord, GetPosOnCoord(Coord));
        }
    }

    public int GetScore()
    {
        return m_ListMatchedBlocks.Count * 100;
    }

    public void ResetBoard()
    {
        //게임이 끝나면 재시작시 블록들을 재배치하기위해 리필리스트로 옮긴다.
        //현 보드판에 있는 블록들로만 섞으면 블록별 갯수가 똑같기 때문에 변화를 주기 위해서
        //리필리스트에 보관되어 있는 블록들과 합친 후 일부를 추출해서 보드판에 배치한다.

        foreach (var block in m_ListBlocksOnBoard)
        {
            block.gameObject.SetActive(false);
            m_ListBlocksToRefill.Add(block);
        }

        m_ListBlocksOnBoard.Clear();
    }
}
