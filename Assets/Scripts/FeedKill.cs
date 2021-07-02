using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedKill : MonoBehaviour
{
    public float m_DestroyTime = 4f;

    void OnEnable()
    {
        Destroy(gameObject, m_DestroyTime);
    }
}
