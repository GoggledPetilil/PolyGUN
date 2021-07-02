using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : Photon.MonoBehaviour
{
    [Header("Parameters")] 
    public TMP_Text m_UserName;
    public int m_Health;
    public int m_MaxHP;
    public int m_Attack;
    public int m_Speed;
    public int m_Kills;
    public int m_Deaths;

    [Header("Components")] 
    public AudioListener m_listener;
    public AudioSource m_aud;
    public BoxCollider2D m_col;
    public Camera m_cam;
    public ParticleSystem m_ps;
    public PhotonView photonView;
    public Rigidbody2D m_rb;
    public SpriteRenderer m_sr;

    [Header("Physics")] 
    public Vector2 m_MovDir;
    public bool m_Dead;
    public bool m_CanBeDamaged;
    public Player m_LastHit;

    [Header("Shooting")] 
    public GameObject m_BulletPrefab;
    public bool m_CanShoot;
    private float m_Cooldown; // The amount of time left until the player can shoot again
    private bool m_IsShooting;

    [Header("Audio")] 
    public AudioClip m_ShootingSFX;
    public AudioClip m_DyingSFX;

    private void Awake()
    {
        m_Health = m_MaxHP;
        m_aud.volume = UniversalManager.instance.m_SoundVolume;
        
        if (photonView.isMine)
        {
            m_cam.gameObject.SetActive(true);
            m_listener.enabled = true;
            m_UserName.text = PhotonNetwork.playerName;
            gameObject.name = PhotonNetwork.playerName + photonView.viewID;
           
            GameManager.instance.m_LocalPlayer = this.gameObject;
        }
        else
        {
            m_UserName.text = photonView.owner.name;
            gameObject.name = photonView.owner.name + photonView.viewID;
        }
    }   

    // Update is called once per frame
    void Update()
    {
        if (photonView.isMine && !m_Dead)
        {
            Move();

            if (Input.GetMouseButtonDown(0))
            {
                m_IsShooting = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_IsShooting = false;
            }
            
            if (Input.GetMouseButton(0) && m_CanShoot)
            {
                Shoot();
            }
        
            if (m_Cooldown > 0.0f)
            {
                m_Cooldown -= 1 * Time.deltaTime;
            }
            else if (m_Cooldown <= 0.0f && m_CanShoot == false)
            {
                m_CanShoot = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (m_IsShooting)
        {
            m_rb.velocity = m_MovDir * (m_Speed / 2);
        }
        else
        {
            m_rb.velocity = m_MovDir * m_Speed;
        }
        
    }

    void Move()
    {
        m_MovDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    void Shoot()
    {
        m_CanShoot = false;
        Vector3 mousePos = (m_cam.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        mousePos.Normalize();
        float rotZ = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, rotZ);

        GameObject obj =
            PhotonNetwork.Instantiate(m_BulletPrefab.name, this.transform.position, rot, 0);
        
        BulletBehaviour bb = obj.GetComponent<BulletBehaviour>();
        bb.m_Owner = this;
        m_Cooldown = bb.m_Reload; 
        photonView.RPC("PlayShootSFX", PhotonTargets.AllBuffered);

        float r = m_sr.color.r;
        float g = m_sr.color.g;
        float b = m_sr.color.b;
        bb.photonView.RPC("SetColor", PhotonTargets.AllBuffered, r, g, b);
    }

    [PunRPC]
    public void ReduceHealth(int amount, string source)
    {
        if (m_CanBeDamaged)
        {
            m_Health -= amount;
            m_LastHit = GameObject.Find(source).GetComponent<Player>();
            if (photonView.isMine && m_Health <= 0)
            {
                photonView.RPC("Death", PhotonTargets.AllBuffered);
                GameManager.instance.EnableRespawn();
                float x = transform.position.x;
                float y = transform.position.y;
                float r = m_sr.color.r;
                float g = m_sr.color.g;
                float b = m_sr.color.b;
                GameManager.instance.SpawnExplosion(x, y, r, g, b);
            }
        }
    }

    [PunRPC]
    void Death()
    {
        photonView.RPC("PlayDeathSFX", PhotonTargets.AllBuffered);
        GameManager.instance.CamShake(1f, 1f, this.transform.position);
        m_col.enabled = false;
        m_sr.enabled = false;
        m_ps.Stop();
        m_Dead = true;
        m_Deaths++;
        
        Player p = m_LastHit;
        p.GetComponent<PhotonView>().RPC("IncreaseKillCount", PhotonTargets.AllBuffered);
        GameManager.instance.KillMessage(p.m_UserName.text, m_UserName.text);
    }
    
    [PunRPC]
    void Respawn()
    {
        m_col.enabled = true;
        m_sr.enabled = true;
        m_ps.Play();
        m_Dead = false;
        m_Health = m_MaxHP;

        int i = (int)GameManager.instance.RandomRange(0, GameManager.instance.m_RespawnPoints.Length);
        Transform t = GameManager.instance.m_RespawnPoints[i];
        transform.position = t.position;
    }

    [PunRPC]
    public void IncreaseKillCount()
    {
        m_Kills++;
    }

    [PunRPC]
    public void SetColor(float r, float g, float b)
    {
        Color c = new Color(r, g, b, 1f);
        m_sr.color = c;
        m_ps.startColor = c;
    }
    
    [PunRPC]
    public void PlayShootSFX()
    {
        m_aud.clip = m_ShootingSFX;
        m_aud.Play();
    }
    
    [PunRPC]
    public void PlayDeathSFX()
    {
        m_aud.clip = m_DyingSFX;
        m_aud.Play();
    }
}
