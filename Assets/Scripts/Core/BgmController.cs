using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//배경음악을 관리하는 클래스 
public class BgmController : MonoBehaviour
{
    private static BgmController m_Instance;
    public static BgmController GetInstance
    {
        get
        {
            SetSingleton();
            return m_Instance;
        }
    }

    private AudioSource m_BgmPlayer;

    private AudioClip[] m_BgmList;
    private string m_strCurBgmName = "";

    [SerializeField] Slider m_BgmSlider;
    private float m_fBgmVol = 1f;

    private void Awake()
    {
        SetSingleton();

        m_BgmPlayer = GetComponent<AudioSource>();
        m_BgmPlayer.loop = true;
        m_BgmList = Resources.LoadAll<AudioClip>("Sounds/BGM");

        m_fBgmVol = PlayerPrefs.GetFloat("BgmVol", 1f);
        m_BgmSlider.value = m_fBgmVol;
        m_BgmPlayer.volume = m_BgmSlider.value;
    }

    public static void SetSingleton()
    {
        if (null == m_Instance)
        {
            GameObject obj = GameObject.Find("BgmController");

            if (null == obj)
            {
                obj = new GameObject { name = "BgmController" };
                obj.AddComponent<BgmController>();
            }

            DontDestroyOnLoad(obj);
            m_Instance = obj.GetComponent<BgmController>();
        }
    }

    public void PlayBGM(string BgmName)
    {
        if (m_strCurBgmName.Equals(BgmName))
            return;

        for (int i = 0; m_BgmList.Length > i; i++)
        {
            if (m_BgmList[i].name.Equals(BgmName))
            {
                m_BgmPlayer.clip = m_BgmList[i];
                m_BgmPlayer.Play();
                m_strCurBgmName = BgmName;
            }
        }
    }

    public void StopBGM()
    {
        m_BgmPlayer.Stop();
        m_strCurBgmName = "";
    }

    public void SetBgmVolume()
    {
        m_BgmPlayer.volume = m_BgmSlider.value;

        m_fBgmVol = m_BgmSlider.value;
        PlayerPrefs.SetFloat("BgmVol", m_fBgmVol);
    }
}
