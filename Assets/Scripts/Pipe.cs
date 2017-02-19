﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour {

    [SerializeField]
    private Vector2[] chokePoints;

    private AudioSource fumeSound;
    private ParticleSystem fumes;

    void Awake ()
    {
        fumes = GetComponent<ParticleSystem>();
        fumeSound = GetComponent<AudioSource>();
    }

    public bool IsBlocked (Collider2D[] colliders)
    {
        foreach (Vector2 chokePoint in chokePoints)
        {
            Vector2 point = transform.TransformPoint(chokePoint);
            bool blocked = false;
            int colliderIdx = 0;
            while (!blocked && colliderIdx < colliders.Length)
            {
                blocked = colliders[colliderIdx].OverlapPoint(point);
                ++colliderIdx;
            }
            if (!blocked)
            {
                return false;
            }
        }
        return true;
    }

    public void StartEmitFumes ()
    {
        if (!fumes.isPlaying)
        {
            fumes.Play();
        }
        if (!fumeSound.isPlaying)
        {
            fumeSound.Play();
        }
    }

    public void StopEmitFumes()
    {
        if (fumes.isPlaying)
        {
            fumes.Stop();
        }
        if (fumeSound.isPlaying)
        {
            fumeSound.Stop();
        }
    }
}
