using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{

    public Transform Player
    {
        set { water = value.GetComponentInChildren<WaterFlask>(); }
    }
    private WaterFlask water;

    [Header("Tank Parts")]

    [SerializeField]
    private Tank tank;

    [SerializeField]
    private Pipe pipe;
    private bool pipeBlocked;

    [SerializeField]
    private Hatch hatch;

    [Header("Overheat")]

    [SerializeField]
    private int overheat = 30;
    private int heat;
    [SerializeField]
    private float maxRedRatio = 0.9f;

    [SerializeField]
    private float heatingDelay = 0.1f;
    private float heatingTimer;

    [SerializeField]
    private AudioSource overheatingSound;

    private SpriteRenderer spriteRenderer;
    private Damageable damageable;

    [Header("Engine Sound")]
    
    [SerializeField]
    private AudioSource engineSound;
    [SerializeField]
    private AudioClip engineClip;
    [SerializeField]
    private AudioClip chargingClip;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();

        heatingTimer = heatingDelay;
    }

    void HeatUpEngine ()
    {
        if (heat > 0)
        {
            float ratio = Mathf.Min((float)(heat) / overheat, maxRedRatio);
            spriteRenderer.material.SetFloat("_Flash", ratio);
            overheatingSound.volume = ratio;
            if (!overheatingSound.isPlaying)
            {
                overheatingSound.Play();
            }
            if (heat == overheat)
            {
                damageable.OnHit(1, Vector2.zero);
            }
        }
        else if (overheatingSound.isPlaying)
        {
            overheatingSound.Stop();
        }
    }

    void Update ()
    {
        HeatUpEngine();

        if (heatingTimer > 0f)
        {
            heatingTimer -= Time.deltaTime;
        }
    }

    public void Accelerate ()
    {
        engineSound.Stop();
        engineSound.clip = chargingClip;
        engineSound.Play();
    }

    public void Decelerate ()
    {
        engineSound.Stop();
        engineSound.clip = engineClip;
        engineSound.Play();
    }

    public void SetVolume (float volume)
    {
        engineSound.volume = volume;
    }

    public void EmitFumes ()
    {
        if (!pipeBlocked)
        {
            if (heat > 0)
            {
                --heat;
            }

            pipe.StartEmitFumes();
        }
        else
        {
            if (heat < overheat && heatingTimer <= 0f)
            {
                ++heat;
                heatingTimer = heatingDelay;
            }

            pipe.StopEmitFumes();

            if (hatch.Closed)
            {
                hatch.Unlock();
            }
        }
    }

    public void Restart ()
    {
        pipe.StopEmitFumes();
        if (hatch.Open)
        {
            hatch.Lock();
        }
        engineSound.volume = 1f;
    }

    void FixedUpdate()
    {
        pipeBlocked = CheckPipe();
    }

    bool CheckPipe()
    {
        if (water.Frozen)
        {
            return pipe.IsBlocked(water);
        }
        else
        {
            return false;
        }
    }

    public bool Overheating
    {
        get { return heat > 0 || !damageable.Alive; }
    }
}