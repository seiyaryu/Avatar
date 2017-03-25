using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonCannon : MonoBehaviour, IShooter {

    public Transform Player
    {
        set { player = value; }
    }
    private Transform player;

    [Header("Aiming")]

    [SerializeField]
    private Tank tank;

    [SerializeField]
    private float harpoonAimDuration;
    [SerializeField]
    private float harpoonWaitDuration;

    private float harpoonTimer;

    [Header("Movement")]

    [SerializeField]
    private float cannonMinAngle = 150f;
    [SerializeField]
    private float cannonMaxAngle = 255f;
    [SerializeField]
    private float cannonStoredPosition;
    [SerializeField]
    private float cannonAimingPosition;
    [SerializeField]
    private float cannonAxialSpeed = 5f;
    [SerializeField]
    private float cannonRotationSpeed = 200f;

    [Header("Audio")]

    [SerializeField]
    private AudioSource cannonAimingSound;
    [SerializeField]
    private float angularThreshold;
    private bool movedThisFrame;

    [SerializeField]
    private AudioSource cannonFiringSound;

    private Animator animator;
    private int cannonSortingOrder = 0;

    void Awake ()
    {
        Vector3 position = transform.localPosition;
        position.x = cannonStoredPosition;
        transform.localPosition = position;
        harpoonTimer = harpoonWaitDuration + harpoonAimDuration;

        animator = GetComponent<Animator>();

        Transform cannonTube = transform.GetChild(0);
        if (cannonTube)
        {
            SpriteRenderer cannonTubeRenderer = cannonTube.GetComponent<SpriteRenderer>();
            if (cannonTubeRenderer)
            {
                cannonSortingOrder = cannonTubeRenderer.sortingOrder;
            }
        }
    }

    void Update ()
    {
        if (movedThisFrame)
        {
            if (!cannonAimingSound.isPlaying)
            {
                cannonAimingSound.Play();
            }
        }
        else
        {
            if (cannonAimingSound.isPlaying)
            {
                cannonAimingSound.Pause();
            }
        }
        movedThisFrame = false;
    }

    float NormalizeAngle(float angle)
    {
        return angle - 360f * Mathf.RoundToInt(angle / 360f);
    }

    public void AimAt(Vector2 direction)
    {
        float orientation = tank.Orientation;
        direction *= orientation;
        float target = NormalizeAngle(Vector2.Angle(Vector2.right, direction) * Mathf.Sign(direction.y));
        float angle = NormalizeAngle(transform.rotation.eulerAngles.z);
        float delta = NormalizeAngle(target - angle);
        float rotation = Time.deltaTime * cannonRotationSpeed;
        float aim = (Mathf.Abs(delta) < rotation) ? target : NormalizeAngle(angle + Mathf.Sign(delta) * rotation);

        float maxAngle = (orientation > 0f) ? cannonMaxAngle : -cannonMinAngle;
        float minAngle = (orientation > 0f) ? cannonMinAngle : -cannonMaxAngle;
        aim = Mathf.Min(maxAngle, Mathf.Max(minAngle, aim));

        if(Mathf.Abs(aim - angle) > angularThreshold)
        {
            movedThisFrame = true;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, aim);
    }

    public bool GetTarget(out Vector2 target)
    {
        if (tank.CurrentState == Tank.ChargeState.None)
        {
            if (transform.localPosition.x < cannonAimingPosition)
            {
                float move = Mathf.Min(cannonAxialSpeed * Time.deltaTime, cannonAimingPosition - transform.localPosition.x);
                transform.localPosition += Vector3.right * move;
                movedThisFrame = true;
            }
            else if (player)
            {
                target = player.position;
                Vector2 position = transform.position;
                Vector2 toTarget = target - position;
                AimAt(toTarget);

                if (harpoonTimer > harpoonWaitDuration)
                {
                    harpoonTimer -= Time.deltaTime;
                    return false;
                }
                else
                {
                    target = transform.TransformPoint(Vector3.right);
                    return true;
                }
            }
        }
        else
        {
            OnReload();
        }
        target = Vector2.zero;
        return false;
    }

    public void OnShoot(Projectile projectile)
    {
        animator.SetTrigger("Firing");
        SpriteRenderer projectileRenderer = projectile.GetComponent<SpriteRenderer>();
        if (projectileRenderer)
        {
            projectileRenderer.sortingOrder = cannonSortingOrder - 1;
        }
        cannonFiringSound.Play();
    }

    public void OnReload()
    {
        if (harpoonTimer > 0f && harpoonTimer <= harpoonWaitDuration)
        {
            harpoonTimer -= Time.deltaTime;
        }
        else if (transform.rotation.eulerAngles.z != 0f)
        {
            float orientation = Mathf.Sign(transform.InverseTransformVector(Vector3.right).x);
            AimAt(Vector2.right * orientation);
        }
        else if (transform.localPosition.x > cannonStoredPosition)
        {
            float move = Mathf.Min(cannonAxialSpeed * Time.deltaTime, transform.localPosition.x - cannonStoredPosition);
            transform.localPosition += Vector3.left * move;
            movedThisFrame = true;
        }
        else if (harpoonTimer <= 0f)
        {
            harpoonTimer = harpoonAimDuration + harpoonWaitDuration;
        }
    }

    public void OnReadyToShoot ()
    {

    }
}
