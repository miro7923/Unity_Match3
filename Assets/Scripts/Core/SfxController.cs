using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//효과음을 관리하는 클래스 
public class SfxController : MonoBehaviour
{
    private static SfxController m_Instance;
    public static SfxController GetInstance
    {
        get
        {
            SetSingleton();
            return m_Instance;
        }
    }

    private AudioSource m_SfxPlayer;

    [SerializeField] AudioClip m_ClearClip;

    [SerializeField] Slider m_SfxSlider;

    private float m_fSfxVol = 1f;

    private void Awake()
    {
        SetSingleton();

        m_SfxPlayer = GetComponent<AudioSource>();

        m_fSfxVol = PlayerPrefs.GetFloat("SfxVol", 1f);
        m_SfxSlider.value = m_fSfxVol;
        m_SfxPlayer.volume = m_SfxSlider.value;
    }

    public static void SetSingleton()
    {
        if (null == m_Instance)
        {
            GameObject obj = GameObject.Find("SfxController");

            if (null == obj)
            {
                obj = new GameObject { name = "SfxController" };
                obj.AddComponent<SfxController>();
            }

            DontDestroyOnLoad(obj);
            m_Instance = obj.GetComponent<SfxController>();
        }
    }

    public void PlayClearClip()
    {
        m_SfxPlayer.PlayOneShot(m_ClearClip);
    }

    public void SetSfxVolume()
    {
        m_SfxPlayer.volume = m_SfxSlider.value;

        m_fSfxVol = m_SfxSlider.value;
        PlayerPrefs.SetFloat("SfxVol", m_fSfxVol);
    }
}
