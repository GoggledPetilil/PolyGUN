using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random=UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Gameplay")]
    public GameObject m_PlayerPrefab;
    public GameObject m_ExplosionPrefab;
    public GameObject m_CustomCanvas;
    public GameObject m_SceneCam;
    public GameObject m_LocalPlayer;
    public Animator m_HurtAni;
    private GameObject m_FocusedObject;
    private float explosionMagnitude;
    
    [Header("Game Info")]
    public GameObject[] m_AllPlayers;

    [Header("Master Client")]
    public GameObject m_AdminCanvas;
    public TMP_Text m_RoomCode;
    public Button m_StartButton;
    
    [Header("Chat Feed")]
    public GameObject m_PlayerFeed;
    public GameObject m_FeedGrid;
    
    [Header("Character Customization")]
    [SerializeField] private Image m_CharacterImage;
    [SerializeField] private Slider m_SliderHue;
    [SerializeField] private Slider m_SliderSat;
    [SerializeField] private Slider m_SliderVal;
    [SerializeField] private TMP_Text m_HueDisplay;
    [SerializeField] private TMP_Text m_SatDisplay;
    [SerializeField] private TMP_Text m_ValDisplay;
    [SerializeField] private Image m_SatBG;
    [SerializeField] private Image m_ValBG;
    [SerializeField] private Color m_PlayerColour;

    [Header("Respawn")]
    public GameObject m_RespawnCanvas;
    public TMP_Text m_DeathCause;
    public TMP_Text m_RespawnText;
    private float m_RespawnTimer;
    public Transform[] m_RespawnPoints;

    [Header("Leaderboard")] 
    public GameObject m_LeaderboardCanvas;
    public GameObject m_LeaderGrid;
    public GameObject m_LeaderFeed;
    public GameObject m_IconGrid;
    public GameObject m_IconFeed;
    public GameObject m_KillsGrid;
    public GameObject m_DeathsGrid;
    public GameObject m_ScoreGrid;
    public TMP_Text m_PlayersOnlineText;
    
    void Awake()
    {
        instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_CustomCanvas.SetActive(true);
        m_RespawnCanvas.SetActive(false);
        m_StartButton.interactable = false;

        m_RoomCode.text = "Code\n" + PhotonNetwork.room.name;

        float h, s, v;
        float r = UniversalManager.instance.m_ColorRed;
        float g = UniversalManager.instance.m_ColorGreen;
        float b = UniversalManager.instance.m_ColorBlue;
        Color.RGBToHSV(new Color(r, g, b), out h, out s, out v);
        m_SliderHue.value = h;
        m_SliderSat.value = s;
        m_SliderVal.value = v;
        ChangeColourDisplay();
    }

    void Update()
    {
        if (m_RespawnTimer > 0)
        {
            StartRespawn();
        }
    }

    public float RandomRange(float min, float max)
    {
        float r = Random.Range(min, max);
        return r;
    }

    public void GetAllPlayers()
    {
        m_AllPlayers = null;
        m_AllPlayers = GameObject.FindGameObjectsWithTag("Player");
        string plural = " Players";
        if (m_AllPlayers.Length == 1)
        {
            plural = " Player";
        }
        m_PlayersOnlineText.text = m_AllPlayers.Length.ToString() + plural;
    }
    
    public void EnableRespawn()
    {
        m_RespawnTimer = 5f;
        m_RespawnCanvas.SetActive(true);

        Player lp = m_LocalPlayer.GetComponent<Player>();
        Player killer = lp.m_LastHit;
        m_DeathCause.text = "Killed by " + killer.m_UserName.text + "!";

        lp.m_cam.enabled = false;
        m_SceneCam.SetActive(true);
        m_FocusedObject = lp.m_LastHit.gameObject;
    }
    
    void StartRespawn()
    {
        m_RespawnTimer -= Time.deltaTime;
        m_RespawnText.text = "Respawning in...\n" + m_RespawnTimer.ToString("F0");

        m_SceneCam.transform.position = new Vector3(m_FocusedObject.transform.position.x,
            m_FocusedObject.transform.position.y, m_SceneCam.transform.position.z);

        if (m_RespawnTimer <= 0)
        {
            m_LocalPlayer.GetComponent<PhotonView>().RPC("Respawn", PhotonTargets.AllBuffered);
            m_RespawnCanvas.SetActive(false);
            
            Player lp = m_LocalPlayer.GetComponent<Player>();
            lp.m_cam.enabled = true;
            m_SceneCam.SetActive(false);
        }
    }

    public void SpawnPlayer()
    {
        Vector2 pos = new Vector2(0f, 0f);
        GameObject p = PhotonNetwork.Instantiate(m_PlayerPrefab.name, pos, Quaternion.identity, 0);

        float r = m_PlayerColour.r;
        float g = m_PlayerColour.g;
        float b = m_PlayerColour.b;
        p.GetComponent<Player>().photonView.RPC("SetColor", PhotonTargets.AllBuffered, r, g, b);
        UniversalManager.instance.SetColours(r, g, b);
        
        m_CustomCanvas.SetActive(false);
        m_SceneCam.SetActive(false);
        if (p.GetPhotonView().owner.IsMasterClient)
        {
            m_StartButton.interactable = true;
        }
        
        GetAllPlayers();
    }

    public void SpawnExplosion(float x, float y, float r, float g, float b)
    {
        Vector2 pos = new Vector2(x, y);
        
        GameObject e = PhotonNetwork.Instantiate(m_ExplosionPrefab.name, pos, Quaternion.identity, 0);
        
        e.GetPhotonView().RPC("ChangeColor", PhotonTargets.AllBuffered, r, g, b);
    }
    
    [PunRPC]
    public void KillMessage(string playerKiller, string playerVictim)
    {
        GameObject go = Instantiate(m_PlayerFeed, new Vector2(0, 0), Quaternion.identity);
        go.transform.SetParent(m_FeedGrid.transform, false);
        go.GetComponent<TextMeshProUGUI>().text = playerKiller + " killed " + playerVictim + "!";
        go.GetComponent<TextMeshProUGUI>().color = Color.red;
    }

    public void ShowLeaderboard()
    {
        m_LeaderboardCanvas.SetActive(!m_LeaderboardCanvas.activeSelf);

        if (m_LeaderboardCanvas.activeSelf)
        {
            for (int i = 0; i < m_LeaderGrid.transform.childCount; i++)
            {
                // Putting these in the same loop bc they always have the same amount of children
                GameObject.Destroy(m_LeaderGrid.transform.GetChild(i).gameObject);
                GameObject.Destroy(m_IconGrid.transform.GetChild(i).gameObject);
                GameObject.Destroy(m_KillsGrid.transform.GetChild(i).gameObject);
                GameObject.Destroy(m_DeathsGrid.transform.GetChild(i).gameObject);
                GameObject.Destroy(m_ScoreGrid.transform.GetChild(i).gameObject);
            }
            
            GetAllPlayers();
            
            for (int i = 0; i < m_AllPlayers.Length; i++)
            {
                GameObject go = Instantiate(m_LeaderFeed, new Vector2(0, 0), Quaternion.identity);
                go.transform.SetParent(m_LeaderGrid.transform, false);
                go.GetComponent<TextMeshProUGUI>().text = m_AllPlayers[i].GetComponent<Player>().m_UserName.text;
                
                GameObject ic = Instantiate(m_IconFeed, new Vector2(0, 0), Quaternion.identity);
                ic.transform.SetParent(m_IconGrid.transform, false);
                ic.GetComponent<Image>().color = m_AllPlayers[i].GetComponent<Player>().m_sr.color;

                int kills = (m_AllPlayers[i].GetComponent<Player>().m_Kills / 2);
                int deaths = m_AllPlayers[i].GetComponent<Player>().m_Deaths;
                int score = kills - deaths;
                
                GameObject killt = Instantiate(m_LeaderFeed, new Vector2(0, 0), Quaternion.identity);
                killt.transform.SetParent(m_KillsGrid.transform, false);
                killt.GetComponent<TextMeshProUGUI>().text = kills.ToString();
                
                GameObject dedt = Instantiate(m_LeaderFeed, new Vector2(0, 0), Quaternion.identity);
                dedt.transform.SetParent(m_DeathsGrid.transform, false);
                dedt.GetComponent<TextMeshProUGUI>().text = deaths.ToString();
                
                GameObject scot = Instantiate(m_LeaderFeed, new Vector2(0, 0), Quaternion.identity);
                scot.transform.SetParent(m_ScoreGrid.transform, false);
                scot.GetComponent<TextMeshProUGUI>().text = score.ToString();
            }
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        GameObject go = Instantiate(m_PlayerFeed, new Vector2(0, 0), Quaternion.identity);
        go.transform.SetParent(m_FeedGrid.transform, false);
        go.GetComponent<TextMeshProUGUI>().text = player.NickName + " joined the game!";
        go.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        GetAllPlayers();
    }
    
    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        GameObject go = Instantiate(m_PlayerFeed, new Vector2(0, 0), Quaternion.identity);
        go.transform.SetParent(m_FeedGrid.transform, false);
        go.GetComponent<TextMeshProUGUI>().text = player.NickName + " left the game!";
        go.GetComponent<TextMeshProUGUI>().color = Color.yellow;
        GetAllPlayers();
    }
    
    public void ChangeColourDisplay()
    {
        m_PlayerColour = Color.HSVToRGB(m_SliderHue.value, m_SliderSat.value, m_SliderVal.value);
        
        m_SatBG.color = Color.HSVToRGB(m_SliderHue.value, 1f, m_SliderVal.value);
        m_ValBG.color = m_PlayerColour;
        m_CharacterImage.color = m_PlayerColour;

        m_HueDisplay.text = (m_SliderHue.value * 360).ToString("F0");
        m_SatDisplay.text = (m_SliderSat.value * 100).ToString("F0");
        m_ValDisplay.text = (m_SliderVal.value * 100).ToString("F0");
    }

    public void CamShake(float duration, float magnitude, Vector3 sourcePos)
    {
        if (UniversalManager.instance.m_ScreenShake == true)
        {
            float distance = (sourcePos - m_LocalPlayer.transform.position).magnitude;
            float maxDistance = 20f;
            distance = (Mathf.Clamp(distance, 0, maxDistance)) / maxDistance;
            float power = 1f - distance;
            
            explosionMagnitude = (magnitude * power);
            InvokeRepeating("CamIsShaking", 0f, 0.005f);
            Invoke("CamStopShaking", duration);
        }
    }

    void CamIsShaking()
    {
        float x = RandomRange(-1f, 1f) * explosionMagnitude;
        float y = RandomRange(-1, 1f) * explosionMagnitude;
        m_LocalPlayer.GetComponent<Player>().m_cam.transform.localPosition = new Vector3(x, y, -10f);
    }

    void CamStopShaking()
    {
        CancelInvoke("CamIsShaking");
        m_LocalPlayer.GetComponent<Player>().m_cam.transform.localPosition = new Vector3(0f, 0f, -10f);
    }
    
    public void PlayConfirm()
    {
        UniversalManager.instance.PlayConfirmAudio();
    }
}
