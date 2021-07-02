using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource m_MusicPlayer;
    public AudioSource m_SoundPlayer;

    public void PlayMusic(AudioClip clip)
    {
        m_MusicPlayer.clip = clip;
        m_MusicPlayer.volume = UniversalManager.instance.m_MusicVolume;
        m_MusicPlayer.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        m_SoundPlayer.clip = clip;
        m_SoundPlayer.volume = UniversalManager.instance.m_SoundVolume;
        m_SoundPlayer.Play();
    }
}
