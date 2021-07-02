using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string m_Version = "beta";
    [SerializeField] private TMP_Text m_VersionDisplay;

    [Header("Menu")] 
    [SerializeField] private GameObject m_MenuCanvas;
    [SerializeField] private Button m_ToSettingsButton;
    [SerializeField] private TMP_InputField m_UsernameInput;
    [SerializeField] private TMP_InputField m_JoinGameInput;
    [SerializeField] private Button m_JoinGameButton;

    [Header("Settings")] 
    [SerializeField] private GameObject m_SettingsCanvas;
    [SerializeField] private Button m_ToMenuButton;
    [SerializeField] private Slider m_MusicSlider;
    [SerializeField] private Slider m_SoundSlider;
    [SerializeField] private Toggle m_ShakeToggle;
    [SerializeField] private Toggle m_HurtToggle;
    [SerializeField] private TMP_Text m_MusicPercentage;
    [SerializeField] private TMP_Text m_SoundPercentage;
    [SerializeField] private AudioClip[] m_TestClips;
    
    private void Awake()
    {
        m_VersionDisplay.text = "v" + m_Version;
        PhotonNetwork.ConnectUsingSettings(m_Version);
    }

    private void Start()
    {
        m_MenuCanvas.SetActive(true);
        m_SettingsCanvas.SetActive(false);
        
        MusicPercentageDisplay();
        SoundPercentageDisplay();
        m_ShakeToggle.isOn = UniversalManager.instance.m_ScreenShake;

        m_UsernameInput.text = UniversalManager.instance.m_UserName;
        AllowButtonPress();
    }

    private void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("Connected!");
    }

    public void AllowButtonPress()
    {
        if (m_UsernameInput.text.Length > 0 && m_JoinGameInput.text.Length > 0)
        {
            m_JoinGameButton.interactable = true;
        }
        else
        {
            m_JoinGameButton.interactable = false;
        }
    }

    public void SetUserName()
    {
        PhotonNetwork.playerName = m_UsernameInput.text;
        UniversalManager.instance.SetUsername(m_UsernameInput.text);
    }

    public void JoinGame()
    {
        SetUserName();
        RoomOptions m_RoomOptions = new RoomOptions();
        m_RoomOptions.maxPlayers = 10;
        PhotonNetwork.JoinOrCreateRoom(m_JoinGameInput.text, m_RoomOptions, TypedLobby.Default);
    }

    private void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    public void EnableMenu()
    {
        m_MenuCanvas.SetActive(true);
        m_SettingsCanvas.SetActive(false);
    }
    
    public void EnableSettings()
    {
        m_MenuCanvas.SetActive(false);
        m_SettingsCanvas.SetActive(true);
    }

    public void AdjustMusic()
    {
        UniversalManager.instance.SetMusicVolume(m_MusicSlider.value);
        MusicPercentageDisplay();
    }

    void MusicPercentageDisplay()
    {
        m_MusicPercentage.text = (UniversalManager.instance.m_MusicVolume * 100).ToString("F0") + "%";
        m_MusicSlider.value = UniversalManager.instance.m_MusicVolume;
    }
    
    public void AdjustSound()
    {
        UniversalManager.instance.SetSoundVolume(m_SoundSlider.value);

        int i = Random.Range(0, m_TestClips.Length);
        UniversalManager.instance.audioManager.PlaySound(m_TestClips[i]);
        
        SoundPercentageDisplay();
    }

    void SoundPercentageDisplay()
    {
        m_SoundPercentage.text = (UniversalManager.instance.m_SoundVolume * 100).ToString("F0") + "%";
        m_SoundSlider.value = UniversalManager.instance.m_SoundVolume;
    }

    public void AdjustShake()
    {
        UniversalManager.instance.SetScreenShake(m_ShakeToggle.isOn);
    }

    public void PlayConfirm()
    {
        UniversalManager.instance.PlayConfirmAudio();
    }

    string PasswordGenerator(int length)
    {
        string s = "";
        
        if (m_Version == "beta")
        {
            s = "test";
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                int n = Random.Range(0, 10);
                s = s + n;
            }
        }
        
        return s;
    }
}
