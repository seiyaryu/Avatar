using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebenderBase : MonoBehaviour, IShooter {

    protected Transform player;
    protected WaterFlask water;

    public Transform Player
    {
        set
        {
            player = value;
            water = value.GetComponentInChildren<WaterFlask>();
        }
    }

    protected Damageable damageable;

    [Header("Shooting")]

    [SerializeField]
    protected float firingMinRange = 2f;
    [SerializeField]
    protected float firingMaxRange = 5f;
    [SerializeField]
    protected float rangeVariation = 0.2f;
    [SerializeField]
    protected float aimAtPlayer = 0.1f;

    [Header("Pain")]

    [SerializeField]
    private AudioSource painSound;

    [SerializeField]
    private AudioClip[] painClips;
    [SerializeField]
    private AudioClip[] deathClips;

    protected virtual void Awake()
    {
        damageable = GetComponent<Damageable>();
        firingMinRange *= Random.Range(1f - rangeVariation, 1f + rangeVariation);
        firingMaxRange *= Random.Range(1f - rangeVariation, 1f + rangeVariation);
    }

    public virtual bool GetTarget(out Vector2 target)
    {
        if (!damageable.Stunned)
        {
            float aggroDist = Random.Range(firingMinRange, firingMaxRange);
            float aggroSqrDist = aggroDist * aggroDist;

            Vector2 position = transform.position;
            target = player.position;
            float sqrDist = (target - position).sqrMagnitude;

            float directShot = Random.Range(0f, 1f);
            if (directShot < aimAtPlayer && sqrDist < aggroSqrDist)
            {
                return true;
            }

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
            
            return sqrDist < aggroSqrDist;
        }
        else
        {
            target = new Vector2();
            return false;
        }
    }

    public virtual void OnShoot(Projectile projectile)
    {

    }

    public virtual void OnReadyToShoot()
    {

    }

    public virtual void OnReload()
    {

    }

    void PlayHurtSound()
    {
        painSound.clip = painClips[Random.Range(0, painClips.Length)];
        painSound.Play();
    }

    void PlayDeathSound()
    {
        painSound.clip = painClips[Random.Range(0, deathClips.Length)];
        painSound.Play();
    }
}
