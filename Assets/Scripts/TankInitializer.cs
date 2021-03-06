﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankInitializer : MonoBehaviour {

    [SerializeField]
    private Tank tank;
    [SerializeField]
    private HarpoonCannon harpoonCannon;
    [SerializeField]
    private Turret turret;
    [SerializeField]
    private TankPilot pilot;
    [SerializeField]
    private Engine engine;

    public void Initialize(Transform player, float sceneLeftBound, float sceneRightBound)
    {
        tank.Player = player;
        tank.SceneLeftBound = sceneLeftBound;
        tank.SceneRightBound = sceneRightBound;
        harpoonCannon.Player = player;
        turret.Player = player;
        engine.Player = player;
        pilot.Player = player;
        Destroy(this);
    }
}
