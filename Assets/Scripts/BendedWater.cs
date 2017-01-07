using UnityEngine;
using System.Collections;

public class BendedWaterController : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject.Find("WaterFlask").GetComponent<WaterFlask>().OnWhipAttack(other);
    }
}
