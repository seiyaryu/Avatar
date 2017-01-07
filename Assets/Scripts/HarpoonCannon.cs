using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonCannon : MonoBehaviour, IShooter {

    [SerializeField]
    private Transform cannon;

    private float harpoonTimer;

    [SerializeField]
    private float harpoonAimDuration;
    [SerializeField]
    private float harpoonWaitDuration;
    [SerializeField]
    private float cannonMinAngle = 150f;
    [SerializeField]
    private float cannonMaxAngle = 255f;

    [SerializeField]
    private Animator cannonAnimator;


    [SerializeField]
    private float cannonStoredPosition;
    [SerializeField]
    private float cannonAimingPosition;
    [SerializeField]
    private float cannonAxialSpeed = 5f;
    [SerializeField]
    private float cannonRotationSpeed = 200f;

    private GameObject player;

    void Start ()
    {
        player = GameController.GameManager.Player;
    }

    void Awake ()
    {
        Vector3 position = cannon.transform.localPosition;
        position.z = cannonStoredPosition;
        cannon.transform.localPosition = position;
        harpoonTimer = harpoonWaitDuration + harpoonAimDuration;
    }

    float NormalizeAngle(float angle)
    {
        return angle - 360f * Mathf.RoundToInt(angle / 360f);
    }

    public void AimAt(Vector2 direction)
    {
        float orientation = Mathf.Sign(transform.localScale.x);
        direction *= orientation;
        float aimAngle = NormalizeAngle(Vector2.Angle(Vector2.right, direction) * Mathf.Sign(direction.y));
        float angle = NormalizeAngle(cannon.transform.rotation.eulerAngles.z);
        float delta = NormalizeAngle(aimAngle - angle);
        float rotation = Time.deltaTime * cannonRotationSpeed;
        angle = (Mathf.Abs(delta) < rotation) ? aimAngle : NormalizeAngle(angle + Mathf.Sign(delta) * rotation);

        float maxAngle = (orientation > 0f) ? cannonMaxAngle : -cannonMinAngle;
        float minAngle = (orientation > 0f) ? cannonMinAngle : -cannonMaxAngle;
        angle = Mathf.Min(maxAngle, Mathf.Max(minAngle, angle));

        cannon.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public bool GetTarget(out Vector2 target)
    {
        if(cannon.localPosition.x < cannonAimingPosition)
        {
            float move = Mathf.Min(cannonAxialSpeed * Time.deltaTime, cannonAimingPosition - cannon.localPosition.x);
            cannon.localPosition += Vector3.right * move;
        }
        else if (player)
        {
            target = player.transform.position;
            Vector2 position = cannon.position;
            Vector2 toTarget = target - position;
            AimAt(toTarget);

            if (harpoonTimer > harpoonWaitDuration)
            {
                harpoonTimer -= Time.deltaTime;
                return false;
            }
            else
            {
                return true;
            }
        }
        target = Vector2.zero;
        return false;
    }

    public void OnShoot()
    {
        cannonAnimator.SetTrigger("Firing");
    }

    public void OnReload()
    {
        if (harpoonTimer <= 0f)
        {
            if (cannon.rotation.eulerAngles.z != 0f)
            {
                AimAt(Vector2.right * Mathf.Sign(transform.localScale.x));
            }
            else if (cannon.localPosition.x > cannonStoredPosition)
            {
                float move = Mathf.Min(cannonAxialSpeed * Time.deltaTime, cannon.localPosition.x - cannonStoredPosition);
                cannon.localPosition += Vector3.left * move;
            }
            else
            {
                harpoonTimer = harpoonAimDuration + harpoonWaitDuration;
            }
        }
        else if (harpoonTimer <= harpoonWaitDuration)
        {
            harpoonTimer -= Time.deltaTime;
        }
    }

}
