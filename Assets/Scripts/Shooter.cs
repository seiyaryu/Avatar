using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float angle)
    {
        float sin = Mathf.Sin(angle * Mathf.Deg2Rad);
        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}

public class Shooter : MonoBehaviour {

    [SerializeField]
    private Projectile projectile;

    [SerializeField]
    private Transform firingOrigin;

    [SerializeField]
    private float firingCooldown = 1.5f;

    private float firingTimer = 0f;

    [SerializeField]
    private float angularDeviation = 5f;
    [SerializeField]
    private float speedDeviation = 0.4f;

    [SerializeField]
    private MonoBehaviour targeter;
    private IShooter controller;

    void Awake ()
    {
        controller = targeter as IShooter;
    }

    void Update ()
    {
        if (controller != null)
        {
            if (firingTimer > 0f)
            {
                firingTimer -= Time.deltaTime;
                controller.OnReload();
                if (firingTimer <= 0f)
                {
                    controller.OnReadyToShoot();
                }
            }
            else
            {

                Vector2 target;
                if (controller.GetTarget(out target))
                {
                    controller.OnShoot(ShootAt(target));
                    firingTimer = firingCooldown;
                }
            }
        }
    }

    Projectile ShootAt (Vector2 target)
    {
        Vector2 origin = firingOrigin.position;
        Projectile instance = Instantiate(projectile, origin, projectile.transform.rotation, GameController.GameManager.Root);

        instance.Direction = (target - origin).Rotate(Random.Range(-angularDeviation, angularDeviation));
        instance.Speed = instance.Speed * Random.Range(1f - speedDeviation, 1f + speedDeviation);

        return instance;
    }

    public Transform FiringOrigin
    {
        get { return firingOrigin; }
    }

    public bool CoolingDown
    {
        get { return firingTimer > 0f; }
    }
}
