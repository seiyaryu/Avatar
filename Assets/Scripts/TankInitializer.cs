using System.Collections;
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
    
    void Start ()
    {
        Initialize(GameController.GameManager.Player.transform, GameController.GameManager.MainCanvas.transform, -9f, 40);
        Destroy(this);
    }

    public void Initialize(Transform player, Transform gameCanvas, float sceneLeftBound, float sceneRightBound)
    {
        tank.Player = player;
        tank.SceneLeftBound = sceneLeftBound;
        tank.SceneRightBound = sceneRightBound;

        harpoonCannon.Player = player;

        turret.Player = player;

        engine.Player = player;
        Damageable engineHP = engine.GetComponent<Damageable>();
        if (engineHP)
        {
            engineHP.GameCanvas = gameCanvas;
        }

        pilot.Player = player;
        Damageable pilotHP = pilot.GetComponent<Damageable>();
        if (pilotHP)
        {
            pilotHP.GameCanvas = gameCanvas;
        }
    }
}
