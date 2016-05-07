using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterSourceController : MonoBehaviour {

    private BoxCollider2D box;
    private float area;

    public float spawnDensity;

    void Awake ()
    {
        box = GetComponent<BoxCollider2D>();
        area = box.size.x * box.size.y;
    }
	
    public void EmitWater(Vector3 attractorPosition, float attractorSqrRange, List<Vector3> waterParticlePositions)
    {
        float halfWidth = box.size.x;
        float halfHeight = box.size.y;

        int spawnCount = Mathf.Max(1, Mathf.RoundToInt(spawnDensity * area));

        for(int spawnIdx = 0; spawnIdx < spawnCount; spawnIdx++)
        {
            float x = Random.Range(-halfWidth, halfWidth) + box.offset.x;
            float y = Random.Range(-halfHeight, halfHeight) + box.offset.y;
            Vector3 spawnPosition = transform.position + Vector3.right * x + Vector3.up * y;
            if((spawnPosition - attractorPosition).sqrMagnitude < attractorSqrRange)
            {
                waterParticlePositions.Add(spawnPosition);
            }
        }
    }
}
