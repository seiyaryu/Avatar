using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebenderTargeter : MonoBehaviour, IShooter {

    private GameObject player;
    private WaterFlask water;

    public GameObject Player
    {
        set
        {
            player = value;
            water = value.GetComponentInChildren<WaterFlask>();
        }
    }

    [SerializeField]
    private float firingMinRange = 2f;
    [SerializeField]
    private float firingMaxRange = 5f;
    [SerializeField]
    private float rangeVariation = 0.2f;

    private Damageable damageable;
    private Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
        firingMinRange *= Random.Range(1f - rangeVariation, 1f + rangeVariation);
        firingMaxRange *= Random.Range(1f - rangeVariation, 1f + rangeVariation);
    }

    public virtual bool GetTarget(out Vector2 target)
    {
        if (!damageable.Stunned)
        {
            animator.SetBool("Firing", false);

            Vector2 position = transform.position;
            target = player.transform.position;
            float sqrDist = (target - position).sqrMagnitude;

            float sqrNorm = (water.GetDropPosition() - position).sqrMagnitude;
            if (sqrNorm < sqrDist)
            {
                target = water.GetDropPosition();
                sqrDist = sqrNorm;
            }

            GameObject[] projectiles = GameObject.FindGameObjectsWithTag("PlayerProjectile");
            foreach (GameObject projectile in projectiles)
            {
                Rigidbody2D projectileBody = projectile.GetComponent<Rigidbody2D>();
                if (projectileBody)
                {
                    Vector2 toThis = transform.position - projectile.transform.position;
                    Vector2 aim = projectileBody.velocity.normalized;
                    float deviation = Vector2.Dot(toThis, aim);
                    sqrNorm = toThis.sqrMagnitude;
                    float threshold = Mathf.Sqrt(sqrNorm - 1f);
                    if (deviation >= threshold && sqrNorm < sqrDist)
                    {
                        target = projectile.transform.position;
                        sqrDist = sqrNorm;
                    }
                }
            }
            float aggroDist = Random.Range(firingMinRange, firingMaxRange);
            return aggroDist * aggroDist > sqrDist;
        }
        else
        {
            target = new Vector2();
            return false;
        }
    }

    public virtual void OnShoot()
    {
        animator.SetBool("Firing", true);
    }

    public virtual void OnReload()
    {

    }
}
