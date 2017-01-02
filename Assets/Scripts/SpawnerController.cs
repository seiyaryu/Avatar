using UnityEngine;
using System.Collections;

[System.Serializable]
public class Spawn
{
    public float time;
    public Vector2 position;
}

[System.Serializable]
public class Wave {
    public Spawn[] spawns;
}

public class SpawnerController : MonoBehaviour {

    public Wave[] waves;

    public GameObject firebender;

    private float time = 0f;
    private int waveIndex = 0;
    private int spawnIndex = 0;
    private int deathCount = 0;

    private int enemyCount = 0;

    public int EnemyCount
    {
        get { return enemyCount; }
    }

    void Awake ()
    {
        enemyCount = 0;
        foreach (Wave wave in waves)
        {
            enemyCount += wave.spawns.Length;
        }
    }

	void Update ()
    {
        if (GameController.GameManager.GameOn)
        {
            if (waveIndex < waves.Length)
            {
                if (spawnIndex < waves[waveIndex].spawns.Length)
                {
                    time += Time.deltaTime;
                    while (spawnIndex < waves[waveIndex].spawns.Length && time >= waves[waveIndex].spawns[spawnIndex].time)
                    {
                        SpawnFirebender();
                        ++spawnIndex;
                    }
                }
                if (deathCount == waves[waveIndex].spawns.Length)
                {
                    ++waveIndex;
                    time = 0f;
                    deathCount = 0;
                }
            }
        }       
	}

    public void NotifyDeath ()
    {
        ++deathCount;
        --enemyCount;
    }

    void SpawnFirebender ()
    {
        GameObject instance = (GameObject) Instantiate(firebender, waves[waveIndex].spawns[spawnIndex].position, Quaternion.identity);
        FirebenderController firebenderController = instance.GetComponent<FirebenderController>();
        if(firebenderController)
        {
            firebenderController.Player = GameController.GameManager.Player;
        }
        DamageableController damageableController = instance.GetComponent<DamageableController>();
        if(damageableController)
        {
            damageableController.gameCanvas = GameController.GameManager.MainCanvas.transform;
        }    
    }
}
