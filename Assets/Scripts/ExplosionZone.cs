using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionZone : MonoBehaviour, IDeathListener {

    [SerializeField]
    private GameObject explosion;
    [SerializeField]
    private Vector2 explosionZoneSize;
    [SerializeField]
    private int explosionCount;
    [SerializeField]
    private float explosionDelay;

    public void OnDeath ()
    {
        StartCoroutine(Explosion());
    }
	
    private IEnumerator Explosion ()
    {
        int explosionIdx = 0;
        while (explosionIdx < explosionCount)
        {
            float x = Random.Range(-explosionZoneSize.x, explosionZoneSize.x);
            float y = Random.Range(-explosionZoneSize.y, explosionZoneSize.y);
            Vector3 position = transform.position + Vector3.right * x + Vector3.up * y;
            Instantiate(explosion, position, Quaternion.identity);
            explosionIdx++;
            yield return new WaitForSeconds((explosionCount - explosionIdx) * explosionDelay / explosionCount);
        }
        Destroy(gameObject);
    }
}
