using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public AudioSource m_aud;
    public AudioClip m_ExplosionSFX;
    private bool m_Playing;

    void Awake()
    {
        GetComponent<PhotonView>().RPC("PlayAudio", PhotonTargets.AllBuffered);
    }

    void Update()
    {
        if (!m_aud.isPlaying && m_Playing)
        {
            GetComponent<PhotonView>().RPC("KillSelf", PhotonTargets.AllBuffered);
        }
    }
    
    [PunRPC]
    public void ChangeColor(float r, float g, float b)
    {
        GetComponent<ParticleSystem>().startColor = new Color(r, g, b, 1f);
    }

    [PunRPC]
    public void PlayAudio()
    {
        m_aud.clip = m_ExplosionSFX;
        m_aud.volume = UniversalManager.instance.m_SoundVolume;
        m_aud.Play();
        m_Playing = true;
    }

    [PunRPC]
    void KillSelf()
    {
        Destroy(this.gameObject);
    }
}
