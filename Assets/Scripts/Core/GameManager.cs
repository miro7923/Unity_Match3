using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_Instance;
    public static GameManager GetInstance 
    { 
        get
        {
            SetSingleton();
            return m_Instance; 
        } 
    }

    private Coordinate m_FirstBlockCoord = new Coordinate(0, 0);
    private Coordinate m_TargetBlockCoord = new Coordinate(0, 0);

    //Board 초기값 9x9로 설정 
    private int m_iColumns = 9;
    private int m_iRows = 9;

    private bool m_bTouchDown = false;

    private int m_iPlayScore = 0;

    private int m_iTimer = Constants.TIMER_MAX;

    private PROCESS m_eProcess = PROCESS.TITLE;

    private bool m_bBehaviourInScore = false;

    private void Awake()
    {
        SetSingleton();
        InitGame();
    }

    private void Update()
    {
        switch (m_eProcess)
        {
            case PROCESS.TITLE:
                BgmController.GetInstance.PlayBGM("Title");
                break;
            case PROCESS.READY:
                BgmController.GetInstance.StopBGM();
                ResetPlayInfo();
                break;
            case PROCESS.INGAME:
                BgmController.GetInstance.PlayBGM("Play");
                UIManager.GetInstance.UpdateTimerSlider(m_iTimer);
                PlayerInput();
                break;
            case PROCESS.SCORE:
                if (!m_bBehaviourInScore)
                    BehaviourInScore();
                break;
            case PROCESS.MESSAGE:
                BgmController.GetInstance.StopBGM();
                break;
            case PROCESS.SETTING:
                BgmController.GetInstance.SetBgmVolume();
                SfxController.GetInstance.SetSfxVolume();
                break;
        }
    }

    private static void SetSingleton()
    {
        if (null == m_Instance)
        {
            GameObject obj = GameObject.Find("GameManager");

            if (null == obj)
            {
                obj = new GameObject { name = "GameManager" };
                obj.AddComponent<GameManager>();
            }

            DontDestroyOnLoad(obj);
            m_Instance = obj.GetComponent<GameManager>();
        }
    }

    private void BehaviourInScore()
    {
        m_bBehaviourInScore = true;

        StopAllCoroutines();

        PlayGame(false);

        BgmController.GetInstance.PlayBGM("Score");

        UIManager.GetInstance.SetActiveGameoverUI(true);
        UIManager.GetInstance.UpdateFinalScoreText(m_iPlayScore);
        UIManager.GetInstance.UpdateBestScoreText();
    }

    public void SetProcess(string strProcess)
    {
        if (strProcess.Equals("Title"))
            m_eProcess = PROCESS.TITLE;
        else if (strProcess.Equals("Ready"))
            m_eProcess = PROCESS.READY;
        else if (strProcess.Equals("Ingame"))
            m_eProcess = PROCESS.INGAME;
        else if (strProcess.Equals("Score"))
            m_eProcess = PROCESS.SCORE;
        else if (strProcess.Equals("Message"))
            m_eProcess = PROCESS.MESSAGE;
        else if (strProcess.Equals("Setting"))
            m_eProcess = PROCESS.SETTING;
    }

    private void PlayerInput()
    {
        //클릭상태 플래그 false 이고 input down true 일 때 첫번째 블럭 좌표를 구함 
        if (!m_bTouchDown && InputManager.GetInstance.isInputDown())
        {
            m_bTouchDown = true;//클릭 상태 플래그 true
            m_FirstBlockCoord = InputManager.GetInstance.FirstCoord;//블럭 좌표 저장
            m_TargetBlockCoord = m_FirstBlockCoord;
        }
        else if (m_bTouchDown)
        {
            //클릭 상태 플래그 true이고 input up true 일 때만 up 이벤트 처리 
            if (INPUTUP_RESULT.IN == InputManager.GetInstance.isInputUp())
            {
                m_TargetBlockCoord = InputManager.GetInstance.TargetCoord;

                StartCoroutine(InspectBlocks());
            }
            //첫번째 블럭을 선택할 때엔 보드판 안이었는데 스와이프 후 손을 뗐을 때 위치가 보드판 밖인 경우 
            else if (INPUTUP_RESULT.OUT == InputManager.GetInstance.isInputUp())
            {
                m_bTouchDown = false;
                m_TargetBlockCoord = m_FirstBlockCoord;
            }
        }
    }

    private IEnumerator InspectBlocks()
    {
        //처음에 설정된 좌표와 타겟좌표 대상으로 이동 수행
        yield return Board.GetInstance.SetMovingPos(m_FirstBlockCoord, m_TargetBlockCoord);

        //최초 이동 수행 후 이동 대상이었던 두 블록을 대상으로 각각 가로세로 매치검사를 한다.
        //둘 다 매치가 되지 않으면 처음 이동했던 블록들을 원래 자리로 돌아가게 하기
        if (!Board.GetInstance.isMatched(m_FirstBlockCoord) && !Board.GetInstance.isMatched(m_TargetBlockCoord))
        {
            yield return Board.GetInstance.MoveBackBlocks(m_FirstBlockCoord, m_TargetBlockCoord);
        }
        //위 조건에 들어가지 않으면 매치가 생긴 것
        else
        {
            //매치된 블록들을 제거하고 위에 있는 블록들을 아래로 드롭 후
            //맨 윗줄에 생긴 빈칸에 새 블록들을 생성하고 이동시키는데 
            //이 과정에서 또 매치되는 블록들이 생길 수 있다.
            //그렇기 때문에 위의 과정이 한 번 끝난 후 전체 블록을 대상으로 하는 매칭 검사가 필요하다. 

            //Board가 더 이상 매치되는 것이 없는 상태로 채워질 때까지 반복되어야 하기 때문에 반복문 안에서 위 과정을 실행 
            while (Board.GetInstance.InspectAllBlocks())
            {
                yield return MatchAndDropBlocks();

                yield return Board.GetInstance.RefillPositions();
            }

            //만약 이동해서 매치되는 블록이 없다면 생길 때까지 섞어주기 
            while (!Board.GetInstance.CanMatchOnBoard())
            {
                UIManager.GetInstance.SetActiveShuffleUI(true);

                Board.GetInstance.ShuffleBlocks(true);

                yield return Board.GetInstance.SwapPosition();

                //블록들이 매치되는 경우가 생길 수도 있기 때문에 매칭검사 실시 
                while (Board.GetInstance.InspectAllBlocks())
                {
                    yield return MatchAndDropBlocks();

                    yield return Board.GetInstance.RefillPositions();
                }
            }

            UIManager.GetInstance.SetActiveShuffleUI(false);
        }

        m_bTouchDown = false;//클릭 상태 플래그 false  
    }

    private IEnumerator MatchAndDropBlocks()
    {
        while (Board.GetInstance.InspectAllBlocks())
        {
            UpdateScore(Board.GetInstance.GetScore());

            SfxController.GetInstance.PlayClearClip();

            Board.GetInstance.DeactivateBlocks();
            Board.GetInstance.SetDropPos();

            yield return Board.GetInstance.SwapPosition();
        }
    }

    private void UpdateScore(int iNewScore)
    {
        m_iPlayScore += iNewScore;
        UIManager.GetInstance.UpdateScoreText(m_iPlayScore);
    }

    private void InitGame()
    {
        SetActiveObjects(false);

        Board.GetInstance.SetGridSize(m_iColumns, m_iRows);

        InputManager.GetInstance.Init(m_iColumns, m_iRows);
    }

    public void ResetPlayInfo()
    {
        m_iTimer = Constants.TIMER_MAX;
        UIManager.GetInstance.InitTimer();

        m_bTouchDown = false;
        m_bBehaviourInScore = false;

        m_iPlayScore = 0;
        UpdateScore(m_iPlayScore);

        m_FirstBlockCoord = new Coordinate(0, 0);
        m_TargetBlockCoord = new Coordinate(0, 0);
    }

    private void SetActiveObjects(bool b)
    {
        Board.GetInstance.gameObject.SetActive(b);

        InputManager.GetInstance.gameObject.SetActive(b);
    }

    private IEnumerator UpdateTimer()
    {
        while (PROCESS.INGAME == m_eProcess)
        {
            UIManager.GetInstance.UpdateTimerText(m_iTimer);

            yield return new WaitForSecondsRealtime(1.0f);

            if (0 >= m_iTimer)
                m_eProcess = PROCESS.SCORE;

            m_iTimer--;
        }
    }

    public void PlayGame(bool b)
    {
        SetActiveObjects(b);

        IEnumerator TimerCoroutine = UpdateTimer();

        switch (b)
        {
            case true:
                m_eProcess = PROCESS.INGAME;
                StartCoroutine(TimerCoroutine);
                break;
            case false:
                m_bTouchDown = false;
                m_FirstBlockCoord = new Coordinate(0, 0);
                m_TargetBlockCoord = new Coordinate(0, 0);
                StopCoroutine(TimerCoroutine);
                break;
        }
    }
}
