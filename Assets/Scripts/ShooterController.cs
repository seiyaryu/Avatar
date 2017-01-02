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

public class ShooterController : MonoBehaviour {

    [SerializeField]
    private ProjectileController projectile;

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

    void Update()
    {
        IShooterController controller = targeter as IShooterController;
        if (controller != null)
        {
            if (firingTimer > 0f)
            {
                firingTimer -= Time.deltaTime;
                controller.OnReload();
            }
            else if (targeter)
            {

                Vector2 target;
                if (controller.GetTarget(out target))
                {
                    ShootAt(target);
                    controller.OnShoot();
                    firingTimer = firingCooldown;
                }
            }
            else
            {
                targeter = null;
            }
        }
    }

    void ShootAt(Vector2 target)
    {
        ProjectileController instance = (ProjectileController)Instantiate(projectile, firingOrigin.position, projectile.transform.rotation);

        instance.Direction = (target - (Vector2)firingOrigin.position).Rotate(Random.Range(-angularDeviation, angularDeviation));
        instance.Speed = instance.Speed * Random.Range(1f - speedDeviation, 1f + speedDeviation);
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
