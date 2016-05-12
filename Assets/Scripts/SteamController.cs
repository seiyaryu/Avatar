using UnityEngine;
using System.Collections;

public class SteamController : MonoBehaviour {

    public float steamLifetime = 1f;
	
	void Awake ()
    {
        Destroy(gameObject, steamLifetime);
	}   
}
