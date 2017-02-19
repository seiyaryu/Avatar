﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Popup : MonoBehaviour {

    public Transform origin;
    private Camera viewpoint;

    [SerializeField]
    private Text text;

    public string Text
    {
        set
        {
            text.text = value;
        }
    }

    void Start ()
    {
        viewpoint = GameController.GameManager.Viewpoint;
    }

    void Update ()
    {
        transform.position = viewpoint.WorldToScreenPoint(origin.position);
	}
}