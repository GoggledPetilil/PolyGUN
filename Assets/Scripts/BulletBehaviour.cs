using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : Photon.MonoBehaviour
{
    [Header("Parameters")]
    public Player m_Owner;
    public int m_Attack;
    public int m_Speed;
    public float m_Reload;

    [Header("Components")] 
    public BoxCollider2D m_col;
    public ParticleSystem m_ps;
    public SpriteRenderer m_sr;

    void Start()
    {
        Destroy(gameObject, 10f);
    }
    
    void FixedUpdate()
    {
        Vector3 dir = new Vector3(1 * m_Speed * Time.deltaTime, 0, 0);
        transform.Translate(dir);
    }
    
    [PunRPC]
    public void SetColor(float r, float g, float b)
    {
        Color c = new Color(r, g, b, 1f);
        GetComponent<ParticleSystem>().startColor = c;
    }

    [PunRPC]
    public void KillSelf()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        GameManager.instance.SpawnExplosion(x, y, 1f, 1f, 1f);
        GameManager.instance.CamShake(0.2f, 0.4f, this.transform.position);
        
        m_Speed = 0;
        m_col.enabled = false;
        m_sr.enabled = false;
        m_ps.Stop(); // When all particles are gone, the object will be destroyed
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!photonView.isMine)
        {
            return;
        }
        else
        {
            PhotonView target = collision.gameObject.GetComponent<PhotonView>();
            if (target != null && (!target.isMine || target.isSceneView))
            {
                if (target.CompareTag("Player"))
                {
                    target.RPC("ReduceHealth", PhotonTargets.AllBuffered, m_Attack, m_Owner.gameObject.name);
                }
                this.GetComponent<PhotonView>().RPC("KillSelf", PhotonTargets.AllBuffered);
            }
        }

        if (collision.CompareTag(("Obstacle")))
        {
            this.GetComponent<PhotonView>().RPC("KillSelf", PhotonTargets.AllBuffered);
        }
    }
}
