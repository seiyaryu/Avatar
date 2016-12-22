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

public class FireballShooter : MonoBehaviour {

    [SerializeField]
    private ProjectileController fireBall;

    [SerializeField]
    private Transform firingOrigin;

    [SerializeField]
    private float angularDeviation = 5f;
    [SerializeField]
    private float speedDeviation = 0.4f;

    public void ShootFireballAt(Vector2 direction)
    {
        ProjectileController fireBallInstance = (ProjectileController)Instantiate(fireBall, firingOrigin.position, fireBall.transform.rotation);

        fireBallInstance.Direction = direction.Rotate(Random.Range(-angularDeviation, angularDeviation));
        fireBallInstance.Speed = fireBallInstance.Speed * Random.Range(1f - speedDeviation, 1f + speedDeviation);
    }
}
