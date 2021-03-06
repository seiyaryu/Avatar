﻿using UnityEngine;
using System.Collections;

public class IceShard : MonoBehaviour {

    public Transform iceShatterAnimation;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Scene"))
        {
            Instantiate(iceShatterAnimation, transform.position, transform.rotation, GameController.GameManager.Root);
        }
    }
}
