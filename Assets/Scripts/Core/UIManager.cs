using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class UIManager : MonoBehaviour
{
    private static UIManager m_Instance;
    public static UIManager GetInstance
    {
        get
        {
            SetSingleton();
            return m_Instance;
        }
    }

    [SerializeField] Text m_ScoreText;

    [SerializeField] Text m_TimerText;
    [SerializeField] Slider m_TimerSlider;

    [SerializeField] GameObject m_GameoverUI;
    [SerializeField] Text m_FinalScoreText;

    [SerializeField] Text m_BestScoreText;
    private int m_iBestScore = 0;
    private string m_strBestScoreKey = "BestScore";

    [SerializeField] GameObject m_ShuffleUI;

    [SerializeField] Button m_QuitButton;
    [SerializeField] Button m_RestartButton;
    [SerializeField] Button m_PauseButton;
    [SerializeField] Button m_PlayButton;

    [SerializeField] GameObject m_WarningMsgQuit;
    [SerializeField] GameObject m_WarningMsgRestart;
    [SerializeField] GameObject m_WarningMsgPause;

    [SerializeField] GameObject m_IngameUIPanel;

    [SerializeField] GameObject m_MainPanel;

    [SerializeField] GameObject m_ReadyUI;

    private void Awake()
    {
        SetSingleton();

        m_iBestScore = PlayerPrefs.GetInt(m_strBestScoreKey, 0);
    }

    private void Start()
    {
        InitTimer();
    }

    private static void SetSingleton()
    {
        if (null == m_Instance)
        {
            GameObject obj = GameObject.Find("UIManager");

            if (null == obj)
            {
                obj = new GameObject { name = "UIManager" };
                obj.AddComponent<UIManager>();
            }

            DontDestroyOnLoad(obj);
            m_Instance = obj.GetComponent<UIManager>();
        }
    }

    public void SetActiveMainPanel(bool b)
    {
        m_MainPanel.SetActive(b);
    }

    public void SetActiveIngameUIPanel(bool b)
    {
        m_IngameUIPanel.SetActive(b);
    }

    public void UpdateScoreText(int iNewScore)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Score : ");
        sb.Append(iNewScore);

        m_ScoreText.text = sb.ToString();
    }

    public void InitTimer()
    {
        m_TimerSlider.maxValue = Constants.TIMER_MAX;
        m_TimerSlider.value = m_TimerSlider.maxValue;
    }

    public void UpdateTimerText(int iNewTime)
    {
        m_TimerText.text = iNewTime.ToString();
    }

    public void UpdateTimerSlider(int iNewTime)
    {
        //남은 시간이 줄어드는 시각적인 효과를 위해 작성 
        m_TimerSlider.value = Mathf.MoveTowards(m_TimerSlider.value, iNewTime, Time.deltaTime * 1f);
    }

    public void SetActiveGameoverUI(bool b)
    {
        m_GameoverUI.SetActive(b);

        m_IngameUIPanel.SetActive(!b);
    }

    public void UpdateFinalScoreText(int iFinalScore)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Final Score : ");
        sb.Append(iFinalScore);

        m_FinalScoreText.text = sb.ToString();

        if (iFinalScore > m_iBestScore)
        {
            //만약 최종 점수가 저장된 최고 점수보다 크다면 최고 점수 갱신
            m_iBestScore = iFinalScore;
            //프로그램을 종료해도 유지되도록 PlayerPrefs에 키와 함께 저장 
            PlayerPrefs.SetInt(m_strBestScoreKey, iFinalScore);
        }
    }

    public void UpdateBestScoreText()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Best Score : ");
        sb.Append(m_iBestScore);

        m_BestScoreText.text = sb.ToString();
    }

    public void SetActiveShuffleUI(bool b)
    {
        m_ShuffleUI.SetActive(b);
    }

    public void PopupPauseMsg(bool b)
    {
        //일시정지 팝업창이 출력되면 나머지 버튼들은 눌러지지 않게 함 
        m_QuitButton.interactable = !b;
        m_RestartButton.interactable = !b;

        //일시정지 버튼은 비활성화되고 플레이 버튼이 활성화되도록 함 
        m_PauseButton.gameObject.SetActive(!b);
        m_PlayButton.gameObject.SetActive(b);

        m_WarningMsgPause.SetActive(b);
    }

    public void PopupQuitMsg(bool b)
    {
        //종료 팝업창이 출력되면 나머지 버튼들은 눌러지지 않도록 함 
        m_RestartButton.interactable = !b;
        m_PauseButton.interactable = !b;

        m_WarningMsgQuit.SetActive(b);
    }

    public void PopupRestartMsg(bool b)
    {
        //재시작 팝업창이 출력되면 나머지 버튼들은 눌러지지 않도록 함 
        m_QuitButton.interactable = !b;
        m_PauseButton.interactable = !b;

        m_WarningMsgRestart.SetActive(b);
    }

    public void SetActiveReadyUI(bool b)
    {
        m_ReadyUI.SetActive(b);
    }
}
