using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalManager : MonoBehaviour
{
    [Header("Essentials")] 
    public static UniversalManager instance;
    public AudioManager audioManager;
    private int gameInitialized;

    [Header("Settings")] 
    public string m_UserName;
    public float m_MusicVolume;
    public float m_SoundVolume;
    public bool m_ScreenShake;
    public float m_ColorRed;
    public float m_ColorGreen;
    public float m_ColorBlue;

    [Header("Audio")] 
    public AudioClip m_Confirm;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        m_MusicVolume = volume;
        audioManager.m_MusicPlayer.volume = volume;
        SaveSettings();
    }
    
    public void SetSoundVolume(float volume)
    {
        m_SoundVolume = volume;
        audioManager.m_SoundPlayer.volume = volume;
        SaveSettings();
    }

    public void SetUsername(string name)
    {
        m_UserName = name;
        SaveSettings();
    }

    public void SetScreenShake(bool state)
    {
        m_ScreenShake = state;
        SaveSettings();
    }

    public void SetColours(float r, float g, float b)
    {
        m_ColorRed = r;
        m_ColorGreen = g;
        m_ColorBlue = b;
        SaveSettings();
    }

    public void PlayConfirmAudio()
    {
        audioManager.PlaySound(m_Confirm);
    }

    void SaveSettings()
    {
        PlayerPrefs.SetString("userName", m_UserName);
        
        PlayerPrefs.SetFloat("musicVolume", m_MusicVolume);
        PlayerPrefs.SetFloat("soundVolume", m_SoundVolume);
        
        int screenShake = BoolToInt(m_ScreenShake);
        PlayerPrefs.SetInt("screenShake", screenShake);
        
        PlayerPrefs.SetFloat("colorRed", m_ColorRed);
        PlayerPrefs.SetFloat("colorGreen", m_ColorGreen);
        PlayerPrefs.SetFloat("colorBlue", m_ColorBlue);
    }

    void LoadSettings()
    {
        gameInitialized = PlayerPrefs.GetInt("gameStarted");
        if (gameInitialized == 1)
        {
            m_UserName = PlayerPrefs.GetString("userName");
            
            m_MusicVolume = PlayerPrefs.GetFloat("musicVolume");
            m_SoundVolume = PlayerPrefs.GetFloat("soundVolume");
            
            m_ScreenShake = IntToBool(PlayerPrefs.GetInt("screenShake"));

            m_ColorRed = PlayerPrefs.GetFloat("colorRed");
            m_ColorGreen = PlayerPrefs.GetFloat("colorGreen");
            m_ColorBlue = PlayerPrefs.GetFloat("colorBlue");
        }
        else
        {
            PlayerPrefs.SetInt("gameStarted", 1);
        }
    }

    int BoolToInt(bool b)
    {
        int i;
        if (b == true)
        {
            i = 1;
        }
        else
        {
            i = 0;
        }

        return i;
    }

    bool IntToBool(int i)
    {
        bool b;
        if (i < 1)
        {
            b = false;
        }
        else
        {
            b = true;
        }

        return b;
    }
}
