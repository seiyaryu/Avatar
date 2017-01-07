using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPilot : FirebenderTargeter
{

    [SerializeField]
    private Transform pilot;

    private float pilotTimer;

    [SerializeField]
    private Animator pilotAnimator;


    [SerializeField]
    private float pilotUpPosition;
    [SerializeField]
    private float pilotDownPosition;
    [SerializeField]
    private float pilotSpeed = 5f;

    private GameObject player;

    void Start()
    {
        player = GameController.GameManager.Player;
    }

    protected override void Awake()
    {
        Vector3 position = pilot.transform.localPosition;
        position.y = pilotDownPosition;
        pilot.transform.localPosition = position;
    }

    public override bool GetTarget(out Vector2 target)
    {
        if (pilot.localPosition.y < pilotUpPosition)
        {
            float move = Mathf.Min(pilotSpeed * Time.deltaTime, pilotUpPosition - pilot.localPosition.x);
            pilot.localPosition += Vector3.up * move;
        }
        else
        {
            return base.GetTarget(out target);
        }
        target = Vector2.zero;
        return true;
    }

    public override void OnShoot()
    {
        pilotAnimator.SetTrigger("Firing");
    }

    public override void OnReload()
    {
    }
}