using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour, IShooter {

    public Transform Player
    {
        set { player = value; }
    }
    private Transform player;

    [SerializeField]
    private Tank tank;
    [SerializeField]
    private float minFiringAngle;
    [SerializeField]
    private float maxFiringAngle;

    private bool isPilotAlive;

    void Awake ()
    {
        isPilotAlive = true;
    }

    public void OnPilotDeath ()
    {
        isPilotAlive = false;
    }

    public void OnShoot (Projectile projectile)
    {

    }

    public void OnReadyToShoot ()
    {

    }

    public void OnReload ()
    {

    }

    public bool GetTarget (out Vector2 target)
    {
        if (isPilotAlive && !tank.Charging)
        {
            Vector2 toTarget = transform.InverseTransformVector(transform.position - player.position).normalized;
            float angle = Mathf.Acos(Vector2.Dot(Vector2.left, toTarget)) * Mathf.Rad2Deg;
            if (minFiringAngle < angle && angle < maxFiringAngle)
            {
                target = player.position;
                return true;
            }
        }
        target = Vector2.zero;
        return false;
    }
}
